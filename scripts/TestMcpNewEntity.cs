using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

var cts = new CancellationTokenSource(TimeSpan.FromSeconds(120));

var psi = new ProcessStartInfo
{
    FileName = "dotnet",
    Arguments = "./src/Apps/CommandLine/bin/Debug/net10.0/CommandLine.dll mcp start",
    WorkingDirectory = Directory.GetCurrentDirectory(),
    UseShellExecute = false,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true,
};

using var proc = new Process { StartInfo = psi };
if (!proc.Start())
{
    Console.Error.WriteLine("Failed to start MCP process.");
    return 1;
}

var stderrLines = new List<string>();
var startedSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
var stderrTask = Task.Run(async () =>
{
    while (true)
    {
        var line = await proc.StandardError.ReadLineAsync();
        if (line is null)
        {
            break;
        }

        if (!string.IsNullOrWhiteSpace(line))
        {
            lock (stderrLines)
            {
                stderrLines.Add(line);
            }

            if (line.Contains("MCP server started over stdio", StringComparison.OrdinalIgnoreCase)
                || line.Contains("Application started", StringComparison.OrdinalIgnoreCase))
            {
                startedSignal.TrySetResult(true);
            }
        }
    }
});

try
{
    using var startCts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
    await startedSignal.Task.WaitAsync(startCts.Token);

    await WriteMessageAsync(proc.StandardInput.BaseStream, new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = 1,
        ["method"] = "initialize",
        ["params"] = new JsonObject
        {
            ["protocolVersion"] = "2024-11-05",
            ["capabilities"] = new JsonObject(),
            ["clientInfo"] = new JsonObject
            {
                ["name"] = "dotnet-file-client",
                ["version"] = "0.1.0"
            }
        }
    }, cts.Token);

    var initRes = await ReadMessageAsync(proc.StandardOutput.BaseStream, cts.Token);

    await WriteMessageAsync(proc.StandardInput.BaseStream, new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = 2,
        ["method"] = "tools/list",
        ["params"] = new JsonObject()
    }, cts.Token);

    var toolsRes = await ReadMessageAsync(proc.StandardOutput.BaseStream, cts.Token);

    var tools = toolsRes["result"]?["tools"]?.AsArray() ?? [];
    var newEntityTool = tools.FirstOrDefault(t =>
    {
        var name = t?["name"]?.GetValue<string>() ?? string.Empty;
        return name.Contains("new", StringComparison.OrdinalIgnoreCase)
               && name.Contains("entity", StringComparison.OrdinalIgnoreCase);
    });

    if (newEntityTool is null)
    {
        Console.Error.WriteLine("Tool NewEntity not found in tools/list.");
        DumpStderr(stderrLines);
        return 2;
    }

    var toolName = newEntityTool["name"]?.GetValue<string>() ?? string.Empty;

    await WriteMessageAsync(proc.StandardInput.BaseStream, new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = 3,
        ["method"] = "tools/call",
        ["params"] = new JsonObject
        {
            ["name"] = toolName,
            ["arguments"] = new JsonObject
            {
                ["prompt"] = "Create an Order entity with Number and CreatedTime fields."
            }
        }
    }, cts.Token);

    var callRes = await ReadMessageAsync(proc.StandardOutput.BaseStream, cts.Token);
    var text = callRes["result"]?["content"]?[0]?["text"]?.GetValue<string>() ?? "(no text)";

    Console.WriteLine("=== INIT OK ===");
    Console.WriteLine(initRes.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    Console.WriteLine("=== TOOL NAME ===");
    Console.WriteLine(toolName);
    Console.WriteLine("=== TOOL CALL RESULT (first 800 chars) ===");
    Console.WriteLine(text.Length > 800 ? text[..800] : text);

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine("=== ERROR ===");
    Console.Error.WriteLine(ex.ToString());
    DumpStderr(stderrLines);
    return 1;
}
finally
{
    try
    {
        proc.StandardInput.Close();
    }
    catch { }

    try
    {
        if (!proc.HasExited)
        {
            proc.Kill(entireProcessTree: true);
        }
    }
    catch { }

    try
    {
        await stderrTask;
    }
    catch { }

    DumpStderr(stderrLines);
}

static async Task WriteMessageAsync(Stream stdin, JsonNode payloadObj, CancellationToken ct)
{
    var json = payloadObj.ToJsonString();
    var line = Encoding.UTF8.GetBytes(json + "\n");
    await stdin.WriteAsync(line, ct);
    await stdin.FlushAsync(ct);
}

static async Task<JsonNode> ReadMessageAsync(Stream stdout, CancellationToken ct)
{
    var json = await ReadLineAnyNewlineAsync(stdout, ct);
    while (string.IsNullOrWhiteSpace(json))
    {
        json = await ReadLineAnyNewlineAsync(stdout, ct);
    }

    return JsonNode.Parse(json) ?? throw new InvalidOperationException("Failed to parse MCP JSON body.");
}

static async Task<string> ReadLineAnyNewlineAsync(Stream stream, CancellationToken ct)
{
    var ms = new MemoryStream();
    while (true)
    {
        var one = new byte[1];
        var n = await stream.ReadAsync(one, 0, 1, ct);
        if (n == 0)
        {
            throw new EndOfStreamException("Unexpected EOF while reading line.");
        }

        var b = one[0];
        if (b == (byte)'\n')
        {
            break;
        }

        if (b != (byte)'\r')
        {
            ms.WriteByte(b);
        }
    }

    return Encoding.ASCII.GetString(ms.ToArray());
}

static void DumpStderr(List<string> lines)
{
    lock (lines)
    {
        if (lines.Count == 0)
        {
            return;
        }

        Console.WriteLine("=== MCP STDERR (last 20 lines) ===");
        foreach (var line in lines.TakeLast(20))
        {
            Console.WriteLine(line);
        }
    }
}
