using System.Diagnostics;
using System.Text;

namespace CoreMod.Services;

/// <summary>
/// Result of an external command executed while updating a project template.
/// </summary>
public sealed record CommandExecutionResult(int ExitCode, string Output)
{
    public bool Succeeded => ExitCode == 0;
}

/// <summary>
/// Abstraction over external processes used by the template update workflow.
/// </summary>
public interface ICommandRunner
{
    Task<CommandExecutionResult> RunAsync(
        string command,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default
    );
}

/// <summary>
/// Runs external commands without invoking a shell.
/// </summary>
public sealed class ProcessCommandRunner : ICommandRunner
{
    public async Task<CommandExecutionResult> RunAsync(
        string command,
        IReadOnlyList<string> arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default
    )
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true,
            },
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        cancellationToken.ThrowIfCancellationRequested();

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        if (!process.Start())
        {
            return new(-1, $"Unable to start command '{command}'.");
        }

        using var cancellationRegistration = cancellationToken.Register(
            static state =>
            {
                var runningProcess = (Process)state!;
                try
                {
                    if (!runningProcess.HasExited)
                    {
                        runningProcess.Kill(entireProcessTree: true);
                    }
                }
                catch (InvalidOperationException) { }
                catch (NotSupportedException) { }
            },
            process
        );

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        await Task.WhenAll(standardOutputTask, standardErrorTask);

        var output = CombineOutput(standardOutputTask.Result, standardErrorTask.Result);
        return new(process.ExitCode, output);
    }

    private static string CombineOutput(string standardOutput, string standardError)
    {
        if (string.IsNullOrWhiteSpace(standardError))
        {
            return standardOutput;
        }

        if (string.IsNullOrWhiteSpace(standardOutput))
        {
            return standardError;
        }

        return standardOutput + Environment.NewLine + standardError;
    }
}

/// <summary>
/// Describes a file that exists in the latest template and is new or changed in the project.
/// </summary>
public sealed record TemplateFileChange
{
    private readonly Lazy<TemplateDiffResult> _diff;

    public TemplateFileChange(string relativePath, byte[]? currentBytes, byte[] templateBytes)
    {
        RelativePath = relativePath;
        CurrentBytes = currentBytes?.ToArray();
        TemplateBytes = templateBytes.ToArray();
        CurrentContent = CurrentBytes is null
            ? null
            : TemplateFileEncoding.TryDecode(CurrentBytes, out var currentContent)
                ? currentContent
                : null;
        TemplateContent = TemplateFileEncoding.TryDecode(TemplateBytes, out var templateContent)
            ? templateContent
            : null;
        _diff = new(() => TemplateDiffFormatter.CreateDocument(this));
    }

    public TemplateFileChange(
        string relativePath,
        string? currentContent,
        string templateContent
    )
        : this(
            relativePath,
            currentContent is null ? null : Encoding.UTF8.GetBytes(currentContent),
            Encoding.UTF8.GetBytes(templateContent)
        )
    {
    }

    public string RelativePath { get; }
    public byte[]? CurrentBytes { get; }
    public byte[] TemplateBytes { get; }
    public string? CurrentContent { get; }
    public string? TemplateContent { get; }

    public bool IsNew => CurrentBytes is null;
    public bool IsBinary =>
        TemplateContent is null || (CurrentBytes is not null && CurrentContent is null);

    public TemplateDiffStats DiffStats => _diff.Value.Stats;

    public string Diff => _diff.Value.Content;
}

/// <summary>
/// Summarizes the line changes shown for a template file.
/// </summary>
public sealed record TemplateDiffStats(int AddedLines, int RemovedLines);

internal sealed record TemplateDiffResult(string Content, TemplateDiffStats Stats);

internal static class TemplateFileEncoding
{
    private static readonly byte[] Utf8Preamble = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
        .GetPreamble();
    private static readonly byte[] Utf16LittleEndianPreamble = [0xFF, 0xFE];
    private static readonly byte[] Utf16BigEndianPreamble = [0xFE, 0xFF];

    public static bool AreEqual(byte[] left, byte[] right)
    {
        if (TryDecode(left, out var leftText) && TryDecode(right, out var rightText))
        {
            return string.Equals(
                NormalizeLineEndings(leftText),
                NormalizeLineEndings(rightText),
                StringComparison.Ordinal
            );
        }

        return left.AsSpan().SequenceEqual(right);
    }

    public static bool AreEquivalent(byte[] left, byte[] right)
    {
        if (TryDecode(left, out var leftText) && TryDecode(right, out var rightText))
        {
            return string.Equals(
                NormalizeWhitespace(leftText),
                NormalizeWhitespace(rightText),
                StringComparison.Ordinal
            );
        }

        return left.AsSpan().SequenceEqual(right);
    }

    public static bool TryDecode(byte[] bytes, out string content)
    {
        if (!TryGetEncoding(bytes, out var encoding, out var preambleLength))
        {
            content = string.Empty;
            return false;
        }

        try
        {
            content = encoding.GetString(bytes.AsSpan(preambleLength));
            if (content.Contains('\0', StringComparison.Ordinal))
            {
                content = string.Empty;
                return false;
            }

            return true;
        }
        catch (DecoderFallbackException)
        {
            content = string.Empty;
            return false;
        }
    }

    public static byte[] EncodeLike(string content, byte[] currentBytes)
    {
        if (!TryGetEncoding(currentBytes, out var encoding, out var preambleLength))
        {
            return new UTF8Encoding(encoderShouldEmitUTF8Identifier: false).GetBytes(content);
        }

        var contentBytes = encoding.GetBytes(content);
        if (preambleLength == 0)
        {
            return contentBytes;
        }

        var preamble = currentBytes.AsSpan(0, preambleLength);
        var result = new byte[preambleLength + contentBytes.Length];
        preamble.CopyTo(result);
        contentBytes.CopyTo(result, preambleLength);
        return result;
    }

    public static string NormalizeLineEndings(string content)
    {
        return content.Replace("\r\n", "\n").Replace('\r', '\n');
    }

    public static string NormalizeWhitespace(string content)
    {
        var lines = NormalizeLineEndings(content)
            .Split('\n', StringSplitOptions.None)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(NormalizeWhitespaceLine);
        return string.Join('\n', lines);
    }

    public static string NormalizeWhitespaceLine(string line)
    {
        var builder = new StringBuilder(line.Length);
        var whitespacePending = false;
        foreach (var character in line)
        {
            if (char.IsWhiteSpace(character))
            {
                whitespacePending = true;
                continue;
            }

            if (whitespacePending && builder.Length > 0)
            {
                builder.Append(' ');
            }

            builder.Append(character);
            whitespacePending = false;
        }

        return builder.ToString();
    }

    public static string ConvertLineEndings(string content, string currentContent)
    {
        var newline = currentContent.Contains("\r\n", StringComparison.Ordinal)
            ? "\r\n"
            : currentContent.Contains('\r')
                ? "\r"
                : "\n";
        return NormalizeLineEndings(content).Replace("\n", newline, StringComparison.Ordinal);
    }

    private static bool TryGetEncoding(
        byte[] bytes,
        out Encoding encoding,
        out int preambleLength
    )
    {
        if (bytes.AsSpan().StartsWith(Utf8Preamble))
        {
            encoding = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false,
                throwOnInvalidBytes: true
            );
            preambleLength = Utf8Preamble.Length;
            return true;
        }

        if (bytes.AsSpan().StartsWith(Utf16LittleEndianPreamble))
        {
            encoding = new UnicodeEncoding(
                bigEndian: false,
                byteOrderMark: false,
                throwOnInvalidBytes: true
            );
            preambleLength = Utf16LittleEndianPreamble.Length;
            return true;
        }

        if (bytes.AsSpan().StartsWith(Utf16BigEndianPreamble))
        {
            encoding = new UnicodeEncoding(
                bigEndian: true,
                byteOrderMark: false,
                throwOnInvalidBytes: true
            );
            preambleLength = Utf16BigEndianPreamble.Length;
            return true;
        }

        encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true
        );
        preambleLength = 0;
        return true;
    }
}

/// <summary>
/// Compares and applies the part of a Perigon template that is safe to upgrade automatically.
/// </summary>
public sealed class TemplateComparisonService
{
    private static readonly string[] ServiceDefaultDirectories = ["ServiceDefault", "ServiceDefaults"];

    private static readonly StringComparer RelativePathComparer = StringComparer.OrdinalIgnoreCase;

    private static readonly string[] ExcludedEntityFrameworkFiles =
    [
        "DefaultDbContext.cs",
        "AnalysisDbContext.cs",
        "ReadonlyDbContext.cs",
    ];

    private const string ExcludedWebConstPath =
        "src/Perigon/Perigon.AspNetCore/Constants/WebConst.cs";

    public async Task<IReadOnlyList<TemplateFileChange>> CompareAsync(
        string projectRoot,
        string templateRoot,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedProjectRoot = Path.GetFullPath(projectRoot);
        var normalizedTemplateRoot = Path.GetFullPath(templateRoot);
        var changes = new List<TemplateFileChange>();

        foreach (var templateFile in EnumerateTemplateFiles(normalizedTemplateRoot))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var relativePath = NormalizeRelativePath(
                Path.GetRelativePath(normalizedTemplateRoot, templateFile)
            );
            var projectFile = GetSafePath(normalizedProjectRoot, relativePath);
            var templateBytes = await File.ReadAllBytesAsync(templateFile, cancellationToken);
            byte[]? currentBytes = null;

            if (File.Exists(projectFile))
            {
                currentBytes = await File.ReadAllBytesAsync(projectFile, cancellationToken);
            }

            if (currentBytes is null || !TemplateFileEncoding.AreEquivalent(currentBytes, templateBytes))
            {
                changes.Add(new TemplateFileChange(relativePath, currentBytes, templateBytes));
            }
        }

        return changes
            .OrderBy(change => change.RelativePath, RelativePathComparer)
            .ToArray();
    }

    public async Task ApplyAsync(
        string projectRoot,
        string templateRoot,
        IReadOnlyList<TemplateFileChange> changes,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedProjectRoot = Path.GetFullPath(projectRoot);
        var normalizedTemplateRoot = Path.GetFullPath(templateRoot);
        var pendingFiles = new List<PendingTemplateFile>();

        // Validate all files before copying any of them. This prevents an external edit from
        // leaving the project partially upgraded after the user confirms the plan.
        foreach (var change in changes)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var source = GetSafePath(normalizedTemplateRoot, change.RelativePath);
            var target = GetSafePath(normalizedProjectRoot, change.RelativePath);
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(
                    $"The template file no longer exists: {change.RelativePath}",
                    source
                );
            }

            var templateBytes = await File.ReadAllBytesAsync(source, cancellationToken);
            if (!templateBytes.AsSpan().SequenceEqual(change.TemplateBytes))
            {
                throw new IOException(
                    $"The template file changed after comparison: {change.RelativePath}"
                );
            }

            byte[]? currentBytes = null;
            if (change.IsNew)
            {
                if (File.Exists(target) || Directory.Exists(target))
                {
                    throw new IOException(
                        $"The project file was created after comparison: {change.RelativePath}"
                    );
                }
            }
            else
            {
                if (!File.Exists(target))
                {
                    throw new IOException(
                        $"The project file was removed after comparison: {change.RelativePath}"
                    );
                }

                currentBytes = await File.ReadAllBytesAsync(target, cancellationToken);
                if (change.CurrentBytes is null
                    || !TemplateFileEncoding.AreEqual(currentBytes, change.CurrentBytes))
                {
                    throw new IOException(
                        $"The project file changed after comparison: {change.RelativePath}"
                    );
                }
            }

            var content = change.IsNew
                ? change.TemplateBytes
                : GetUpdatedBytes(change, currentBytes!);
            pendingFiles.Add(new PendingTemplateFile(change, target, currentBytes, content));
        }

        var appliedFiles = new List<PendingTemplateFile>();
        try
        {
            foreach (var pendingFile in pendingFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var targetDirectory = Path.GetDirectoryName(pendingFile.Target);
                if (!string.IsNullOrEmpty(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                appliedFiles.Add(pendingFile);
                await File.WriteAllBytesAsync(
                    pendingFile.Target,
                    pendingFile.Content,
                    cancellationToken
                );
            }
        }
        catch
        {
            Rollback(appliedFiles);
            throw;
        }
    }

    private static byte[] GetUpdatedBytes(
        TemplateFileChange change,
        byte[] currentBytes
    )
    {
        if (change.IsBinary || change.TemplateContent is null || change.CurrentContent is null)
        {
            return change.TemplateBytes;
        }

        var content = TemplateFileEncoding.ConvertLineEndings(
            change.TemplateContent,
            change.CurrentContent
        );
        return TemplateFileEncoding.EncodeLike(content, currentBytes);
    }

    private static void Rollback(IEnumerable<PendingTemplateFile> appliedFiles)
    {
        foreach (var pendingFile in appliedFiles.Reverse())
        {
            try
            {
                if (pendingFile.OriginalBytes is null)
                {
                    if (File.Exists(pendingFile.Target))
                    {
                        File.Delete(pendingFile.Target);
                    }
                }
                else
                {
                    File.WriteAllBytes(pendingFile.Target, pendingFile.OriginalBytes);
                }
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }

    private sealed record PendingTemplateFile(
        TemplateFileChange Change,
        string Target,
        byte[]? OriginalBytes,
        byte[] Content
    );

    private static IEnumerable<string> EnumerateTemplateFiles(string templateRoot)
    {
        var files = new Dictionary<string, string>(RelativePathComparer);

        AddFilesUnder(
            templateRoot,
            Path.Combine(ConstVal.SrcDir, ConstVal.PerigonDir),
            file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                && !NormalizeRelativePath(Path.GetRelativePath(templateRoot, file))
                    .Equals(ExcludedWebConstPath, StringComparison.OrdinalIgnoreCase),
            files,
            excludeBuildOutput: true
        );

        foreach (var serviceDefaultDirectory in ServiceDefaultDirectories)
        {
            AddFilesUnder(
                templateRoot,
                Path.Combine(ConstVal.SrcDir, ConstVal.DefinitionDir, serviceDefaultDirectory),
                file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase),
                files,
                excludeBuildOutput: true
            );
        }

        AddFilesUnder(
            templateRoot,
            Path.Combine(ConstVal.SrcDir, ConstVal.DefinitionDir, ConstVal.ShareName),
            file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase),
            files,
            excludeBuildOutput: true
        );

        AddFilesUnder(
            templateRoot,
            Path.Combine(ConstVal.SrcDir, ConstVal.DefinitionDir, ConstVal.EntityFrameworkName),
            file => file.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                && !IsUnderDirectory(
                    Path.Combine(
                        templateRoot,
                        ConstVal.SrcDir,
                        ConstVal.DefinitionDir,
                        ConstVal.EntityFrameworkName,
                        "Migrations"
                    ),
                    file
                )
                && !ExcludedEntityFrameworkFiles.Contains(
                    Path.GetFileName(file),
                    StringComparer.OrdinalIgnoreCase
                ),
            files,
            excludeBuildOutput: true
        );

        AddFilesUnder(
            templateRoot,
            "scripts",
            file => file.EndsWith(".ps1", StringComparison.OrdinalIgnoreCase),
            files,
            excludeBuildOutput: true
        );

        foreach (var agentDirectory in new[] { ".agent", ".agents" })
        {
            AddFilesUnder(
                templateRoot,
                agentDirectory,
                _ => true,
                files,
                excludeBuildOutput: false
            );
        }

        return files.Values;
    }

    private static void AddFilesUnder(
        string templateRoot,
        string relativeDirectory,
        Func<string, bool> include,
        IDictionary<string, string> files,
        bool excludeBuildOutput
    )
    {
        var directory = GetSafePath(templateRoot, relativeDirectory);
        if (!Directory.Exists(directory))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
        {
            if (excludeBuildOutput && IsBuildOutputPath(directory, file))
            {
                continue;
            }

            if (!include(file))
            {
                continue;
            }

            var relativePath = NormalizeRelativePath(Path.GetRelativePath(templateRoot, file));
            files[relativePath] = file;
        }
    }

    private static bool IsBuildOutputPath(string root, string file)
    {
        var relativePath = Path.GetRelativePath(root, file);
        return relativePath
            .Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries)
            .Any(part => part.Equals("bin", StringComparison.OrdinalIgnoreCase)
                || part.Equals("obj", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsUnderDirectory(string directory, string file)
    {
        var normalizedDirectory = Path.GetFullPath(directory)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var normalizedFile = Path.GetFullPath(file);
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        return normalizedFile.StartsWith(
            normalizedDirectory + Path.DirectorySeparatorChar,
            comparison
        );
    }

    private static string GetSafePath(string root, string relativePath)
    {
        var normalizedRoot = Path.GetFullPath(root)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var platformRelativePath = relativePath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(normalizedRoot, platformRelativePath));
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

        if (!string.Equals(fullPath, normalizedRoot, comparison)
            && !fullPath.StartsWith(normalizedRoot + Path.DirectorySeparatorChar, comparison))
        {
            throw new InvalidOperationException($"Path is outside the root directory: {relativePath}");
        }

        return fullPath;
    }

    private static string NormalizeRelativePath(string path)
    {
        return path.Replace('\\', '/');
    }

}

/// <summary>
/// A prepared update that can be displayed, applied, or discarded.
/// </summary>
public sealed class TemplateUpdatePlan : IDisposable
{
    private bool _disposed;

    public TemplateUpdatePlan(
        string projectRoot,
        string templateRoot,
        IReadOnlyList<TemplateFileChange> changes,
        string workingDirectory
    )
    {
        ProjectRoot = projectRoot;
        TemplateRoot = templateRoot;
        Changes = changes;
        WorkingDirectory = workingDirectory;
    }

    public string ProjectRoot { get; }
    public string TemplateRoot { get; }
    public IReadOnlyList<TemplateFileChange> Changes { get; }
    public string WorkingDirectory { get; }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        try
        {
            if (Directory.Exists(WorkingDirectory))
            {
                Directory.Delete(WorkingDirectory, recursive: true);
            }
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}

/// <summary>
/// Installs the latest Perigon template, prepares a fresh project, and applies an approved plan.
/// </summary>
public sealed class TemplateUpdateService(
    SolutionContext projectContext,
    TemplateComparisonService comparisonService,
    ICommandRunner commandRunner,
    ILogger<TemplateUpdateService> logger
)
{
    public async Task<TemplateUpdatePlan> CreatePlanAsync(
        CancellationToken cancellationToken = default
    )
    {
        var projectRoot = projectContext.SolutionPath;
        if (string.IsNullOrWhiteSpace(projectRoot) || !Directory.Exists(projectRoot))
        {
            throw new DirectoryNotFoundException("The current solution directory does not exist.");
        }

        projectRoot = Path.GetFullPath(projectRoot);
        var workingDirectory = Path.Combine(
            Path.GetTempPath(),
            $"perigon-update-{Guid.NewGuid():N}"
        );
        var templateRoot = Path.Combine(workingDirectory, "template");

        Directory.CreateDirectory(workingDirectory);
        try
        {
            await InstallLatestTemplateAsync(projectRoot, cancellationToken);

            var templateType = SolutionService.IsAOT(projectRoot)
                ? ConstVal.Mini
                : ConstVal.WebApi;
            var projectName = GetTemplateProjectName(projectRoot);
            var arguments = new List<string>
            {
                "new",
                templateType,
                "--output",
                templateRoot,
                "--name",
                projectName,
                "--force",
                "--no-update-check",
            };

            if (HasAngularFrontend(projectRoot))
            {
                arguments.Add("--frontType");
                arguments.Add("Angular");
            }

            var createResult = await commandRunner.RunAsync(
                "dotnet",
                arguments,
                projectRoot,
                cancellationToken
            );
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the latest template project: {FormatCommandOutput(createResult)}"
                );
            }

            if (!Directory.Exists(templateRoot))
            {
                throw new DirectoryNotFoundException(
                    $"The template command did not create '{templateRoot}'."
                );
            }

            var changes = await comparisonService.CompareAsync(
                projectRoot,
                templateRoot,
                cancellationToken
            );
            return new TemplateUpdatePlan(projectRoot, templateRoot, changes, workingDirectory);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to prepare a template update for {ProjectRoot}", projectRoot);
            DeleteWorkingDirectory(workingDirectory);
            throw;
        }
        catch
        {
            DeleteWorkingDirectory(workingDirectory);
            throw;
        }
    }

    public async Task ApplyAsync(
        TemplateUpdatePlan plan,
        CancellationToken cancellationToken = default
    )
    {
        await ApplyAsync(plan, plan.Changes, cancellationToken);
    }

    public async Task BuildAsync(
        TemplateUpdatePlan plan,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(plan);

        var result = await commandRunner.RunAsync(
            "dotnet",
            ["build"],
            plan.ProjectRoot,
            cancellationToken
        );
        if (!result.Succeeded)
        {
            logger.LogError(
                "Failed to build the updated solution {ProjectRoot}. ExitCode={ExitCode}; Output={Output}",
                plan.ProjectRoot,
                result.ExitCode,
                result.Output
            );
            throw new InvalidOperationException(
                $"dotnet build failed: {FormatCommandOutput(result)}"
            );
        }
    }

    public async Task ApplyAsync(
        TemplateUpdatePlan plan,
        IReadOnlyList<TemplateFileChange> changes,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(changes);

        var preparedChanges = plan.Changes.ToDictionary(
            change => change.RelativePath,
            StringComparer.OrdinalIgnoreCase
        );
        foreach (var change in changes)
        {
            if (!preparedChanges.TryGetValue(change.RelativePath, out var preparedChange)
                || !ReferenceEquals(preparedChange, change))
            {
                throw new InvalidOperationException(
                    $"The selected update file was not part of the prepared plan: {change.RelativePath}"
                );
            }
        }

        try
        {
            await comparisonService.ApplyAsync(
                plan.ProjectRoot,
                plan.TemplateRoot,
                changes,
                cancellationToken
            );
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to apply a template update for {ProjectRoot}", plan.ProjectRoot);
            throw;
        }
    }

    private async Task InstallLatestTemplateAsync(
        string projectRoot,
        CancellationToken cancellationToken
    )
    {
        var installResult = await commandRunner.RunAsync(
            "dotnet",
            ["new", "install", ConstVal.TemplatePackageId],
            projectRoot,
            cancellationToken
        );
        if (!installResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to install {ConstVal.TemplatePackageId}: {FormatCommandOutput(installResult)}"
            );
        }
    }

    private static string GetTemplateProjectName(string projectRoot)
    {
        var name = new DirectoryInfo(projectRoot).Name;
        return string.IsNullOrWhiteSpace(name) ? "PerigonProject" : name;
    }

    private static bool HasAngularFrontend(string projectRoot)
    {
        return Directory.Exists(Path.Combine(projectRoot, "src", "ClientApp", "WebApp"))
            || Directory.Exists(Path.Combine(projectRoot, "src", "app"));
    }

    private static string FormatCommandOutput(CommandExecutionResult result)
    {
        return string.IsNullOrWhiteSpace(result.Output)
            ? $"exit code {result.ExitCode}"
            : result.Output.Trim();
    }

    private void DeleteWorkingDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(ex, "Failed to clean up template update directory {Directory}", path);
        }
    }
}

internal static class TemplateDiffFormatter
{
    private const int MaxComparisonCells = 4_000_000;
    private const int ContextLineCount = 3;

    public static TemplateDiffResult CreateDocument(TemplateFileChange change)
    {
        if (!change.IsBinary)
        {
            return CreateTextDocument(
                change.RelativePath,
                change.CurrentContent,
                change.TemplateContent!
            );
        }

        var builder = new StringBuilder();
        var isNew = change.IsNew;
        builder.AppendLine($"--- {(isNew ? "/dev/null" : $"current/{change.RelativePath}")}");
        builder.AppendLine($"+++ template/{change.RelativePath}");
        if (isNew)
        {
            builder.Append($"+ [binary content, {change.TemplateBytes.Length} bytes]");
        }
        else
        {
            builder.Append(
                $"! binary content differs (current: {change.CurrentBytes!.Length} bytes; "
            );
            builder.Append($"template: {change.TemplateBytes.Length} bytes).");
        }

        return new TemplateDiffResult(builder.ToString().TrimEnd(), new(0, 0));
    }

    public static string Create(TemplateFileChange change) => CreateDocument(change).Content;

    public static string Create(
        string relativePath,
        string? currentContent,
        string templateContent
    )
        => CreateTextDocument(relativePath, currentContent, templateContent).Content;

    private static TemplateDiffResult CreateTextDocument(
        string relativePath,
        string? currentContent,
        string templateContent
    )
    {
        var builder = new StringBuilder();
        var isNew = currentContent is null;
        builder.AppendLine($"--- {(isNew ? "/dev/null" : $"current/{relativePath}")}");
        builder.AppendLine($"+++ template/{relativePath}");

        if (isNew)
        {
            var newTemplateLines = SplitLines(templateContent);
            foreach (var line in newTemplateLines)
            {
                builder.Append("+ ").AppendLine(line);
            }

            return new TemplateDiffResult(
                builder.ToString().TrimEnd(),
                new(newTemplateLines.Length, 0)
            );
        }

        var currentLines = RemoveWhitespaceOnlyLines(SplitLines(currentContent!));
        var templateLines = RemoveWhitespaceOnlyLines(SplitLines(templateContent));
        if ((long)currentLines.Length * templateLines.Length > MaxComparisonCells)
        {
            AppendFullDiff(builder, currentLines, templateLines);
            return new TemplateDiffResult(
                builder.ToString().TrimEnd(),
                new(templateLines.Length, currentLines.Length)
            );
        }

        var operations = BuildOperations(currentLines, templateLines);
        var stats = new TemplateDiffStats(
            operations.Count(operation => operation.Kind == DiffLineKind.Added),
            operations.Count(operation => operation.Kind == DiffLineKind.Removed)
        );
        var changedIndexes = operations
            .Select((operation, index) => (operation, index))
            .Where(item => item.operation.Kind != DiffLineKind.Context)
            .Select(item => item.index)
            .ToArray();

        if (changedIndexes.Length == 0)
        {
            builder.AppendLine("! content differs only in whitespace, line endings, or encoding.");
            return new TemplateDiffResult(builder.ToString().TrimEnd(), stats);
        }

        var includedIndexes = new HashSet<int>();
        foreach (var changedIndex in changedIndexes)
        {
            var start = Math.Max(0, changedIndex - ContextLineCount);
            var end = Math.Min(operations.Count - 1, changedIndex + ContextLineCount);
            for (var index = start; index <= end; index++)
            {
                includedIndexes.Add(index);
            }
        }

        var previousIndex = -1;
        foreach (var index in includedIndexes.OrderBy(index => index))
        {
            if (index > previousIndex + 1)
            {
                builder.AppendLine("...");
            }

            var operation = operations[index];
            var prefix = operation.Kind switch
            {
                DiffLineKind.Removed => "- ",
                DiffLineKind.Added => "+ ",
                _ => "  ",
            };
            builder.Append(prefix).AppendLine(operation.Content);
            previousIndex = index;
        }

        return new TemplateDiffResult(builder.ToString().TrimEnd(), stats);
    }

    private static List<DiffLine> BuildOperations(string[] currentLines, string[] templateLines)
    {
        var currentKeys = currentLines.Select(NormalizeLineForDiff).ToArray();
        var templateKeys = templateLines.Select(NormalizeLineForDiff).ToArray();
        var lcs = new int[currentLines.Length + 1, templateLines.Length + 1];
        for (var currentIndex = currentLines.Length - 1; currentIndex >= 0; currentIndex--)
        {
            for (var templateIndex = templateLines.Length - 1; templateIndex >= 0; templateIndex--)
            {
                lcs[currentIndex, templateIndex] = currentKeys[currentIndex] == templateKeys[templateIndex]
                    ? lcs[currentIndex + 1, templateIndex + 1] + 1
                    : Math.Max(lcs[currentIndex + 1, templateIndex], lcs[currentIndex, templateIndex + 1]);
            }
        }

        var operations = new List<DiffLine>(currentLines.Length + templateLines.Length);
        var current = 0;
        var template = 0;
        while (current < currentLines.Length || template < templateLines.Length)
        {
            if (current < currentLines.Length
                && template < templateLines.Length
                && currentKeys[current] == templateKeys[template])
            {
                operations.Add(new(DiffLineKind.Context, currentLines[current++]));
                template++;
            }
            else if (template < templateLines.Length
                && (current == currentLines.Length
                    || lcs[current, template + 1] >= lcs[current + 1, template]))
            {
                operations.Add(new(DiffLineKind.Added, templateLines[template++]));
            }
            else
            {
                operations.Add(new(DiffLineKind.Removed, currentLines[current++]));
            }
        }

        return operations;
    }

    private static string[] RemoveWhitespaceOnlyLines(string[] lines)
    {
        return lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();
    }

    private static string NormalizeLineForDiff(string line) =>
        TemplateFileEncoding.NormalizeWhitespaceLine(line);

    private static void AppendFullDiff(
        StringBuilder builder,
        IEnumerable<string> currentLines,
        IEnumerable<string> templateLines
    )
    {
        foreach (var line in currentLines)
        {
            builder.Append("- ").AppendLine(line);
        }

        foreach (var line in templateLines)
        {
            builder.Append("+ ").AppendLine(line);
        }
    }

    private static string[] SplitLines(string content)
    {
        var normalized = content.Replace("\r\n", "\n").Replace('\r', '\n');
        if (normalized.Length == 0)
        {
            return [];
        }

        if (normalized[^1] == '\n')
        {
            normalized = normalized[..^1];
        }

        return normalized.Split('\n');
    }

    private enum DiffLineKind
    {
        Context,
        Removed,
        Added,
    }

    private readonly record struct DiffLine(DiffLineKind Kind, string Content);
}
