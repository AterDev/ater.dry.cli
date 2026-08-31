[CmdletBinding()]
param (
    [Parameter()]
    [System.Boolean]
    $withStudio = $false
)
$scriptRoot = (Resolve-Path -LiteralPath $PSScriptRoot).Path
$repoRoot = (Resolve-Path -LiteralPath (Join-Path $scriptRoot "..")).Path
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

$dotnetVersion = "net10.0"
$commandLinePath = Join-Path $repoRoot "src/Apps/CommandLine"
$studioPath = Join-Path $repoRoot "src/Apps/Dashboard"
$artifactsPath = Join-Path $repoRoot "artifacts"
$packageWorkspacePath = Join-Path $commandLinePath "nupkg"
$shareDllsFile = Join-Path $commandLinePath "ShareDlls.txt"
$agentTemplatePath = Join-Path (Join-Path (Split-Path -Parent $repoRoot) "Perigon.template") "ApiStandard"
$agentRepoRoot = (Resolve-Path -LiteralPath $agentTemplatePath -ErrorAction SilentlyContinue).Path
$agentZipPath = Join-Path $commandLinePath "agent.zip"
$agentStagingPath = Join-Path $commandLinePath ".agent-temp"

try {
    Set-Location $repoRoot

    # Run the full test project before changing or publishing any build artifacts.
    # PowerShell does not automatically stop when an external process returns a
    # non-zero exit code, so check LASTEXITCODE explicitly.
    $testProjectPath = Join-Path $repoRoot "tests/StudioMod.Tests/CoreMod.Tests.csproj"
    Write-Host 'Running tests before publish...'
    & dotnet test $testProjectPath -c Release -p:GeneratePackageOnBuild=false --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        throw "Tests failed with exit code $LASTEXITCODE. Publish was cancelled."
    }
    Write-Host 'Tests passed. Continuing with publish...'

    $commandLineProjectPath = Join-Path $commandLinePath "CommandLine.csproj";
    # get package name and version
    $VersionNode = Select-Xml -Path $commandLineProjectPath -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path $commandLineProjectPath -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # sync studio version
    $studioProjectPath = Join-Path $studioPath "Dashboard.csproj"
    $xml = [xml](Get-Content $studioProjectPath)
    $propertyGroup = $xml.Project.PropertyGroup[0]
    Write-Host "Current Version:"$Version
    if ($null -eq $propertyGroup.Version) {
        $version = $xml.CreateElement("Version")
        
        $version.InnerText = "$Version"
        $propertyGroup.AppendChild($version)
    }
    else {
        $propertyGroup.Version = "$Version"
    }
    $xml.Save($studioProjectPath)


    # build web project
    if ($withStudio -eq $true) {
        $studioPublishPath = Join-Path $studioPath "publish"
        if (Test-Path -LiteralPath $studioPublishPath) {
            Remove-Item -LiteralPath $studioPublishPath -Recurse -Force
        }
        
        dotnet publish $studioProjectPath -c Release -o $studioPublishPath -p:GenerateDocumentationFile=false -p:DebugType=None
        # 移除部分 dll文件，减少体积
        # 读取ShareDlls.txt，获取dll列表
        if (Test-Path $shareDllsFile) {
            $shareDlls = Get-Content -Path $shareDllsFile
            foreach ($dll in $shareDlls) {
                $dllPath = Join-Path $studioPublishPath $dll
                if (Test-Path -LiteralPath $dllPath) {
                    Remove-Item -LiteralPath $dllPath -Force
                }
            }
        }
        $pathsToRemove = @(
            (Join-Path $studioPublishPath "BuildHost-net472"),
            (Join-Path $studioPublishPath "BuildHost-netcore"),
            (Join-Path $studioPublishPath "runtimes"),
            (Join-Path $studioPublishPath "Dashboard.exe"),
            (Join-Path $studioPublishPath "Dashboard")
        );
        foreach ($path in $pathsToRemove) {
            if (Test-Path -LiteralPath $path) {
                Remove-Item -LiteralPath $path -Recurse -Force
            }
        }

        # remove pdb and xml files
        $files = Get-ChildItem -Path $studioPublishPath -Recurse -Include *.pdb, *.xml, *.map
        foreach ($file in $files) {
            Remove-Item -LiteralPath $file.FullName -Force
        }
        $zipPath = Join-Path $commandLinePath "studio.zip";
        if (Test-Path -LiteralPath $zipPath) {
            Remove-Item -LiteralPath $zipPath -Force
        }
        Compress-Archive -Path (Join-Path $studioPublishPath "*") -DestinationPath $zipPath -CompressionLevel Optimal -Force

        # 删除发布目录
        Remove-Item -LiteralPath $studioPublishPath -Recurse -Force
    }

    if ($agentRepoRoot) {
        if (Test-Path -Path $agentZipPath) {
            Remove-Item -Path $agentZipPath -Force
        }

        if (Test-Path -Path $agentStagingPath) {
            Remove-Item -Path $agentStagingPath -Recurse -Force
        }

        New-Item -ItemType Directory -Path $agentStagingPath -Force | Out-Null

        $agentEntries = @(
            ".agents/skills/perigon",
            ".agents/skills/code-review",
            ".agents/skills/commit-message",
            ".agents/skills/delivery-loop",
            ".agents/skills/docs",
            ".agents/skills/test",
            "docs"
        )

        foreach ($entry in $agentEntries) {
            $sourcePath = Join-Path $agentRepoRoot $entry
            if (Test-Path -Path $sourcePath) {
                $targetPath = Join-Path $agentStagingPath $entry
                New-Item -ItemType Directory -Path (Split-Path -Parent $targetPath) -Force | Out-Null
                Copy-Item -Path $sourcePath -Destination $targetPath -Recurse -Force
            }
        }

        Compress-Archive -Path (Join-Path $agentStagingPath "*") -DestinationPath $agentZipPath -CompressionLevel Optimal -Force
        Remove-Item -Path $agentStagingPath -Recurse -Force
    }

    Write-Host 'Packing new version...'

    # pack
    dotnet build $commandLineProjectPath -c Release
    dotnet pack $commandLineProjectPath -c Release -o $packageWorkspacePath
    $newPackName = $PackageId + "." + $Version + ".nupkg"
    $packagePath = Join-Path $packageWorkspacePath $newPackName
    $zipPackPath = [System.IO.Path]::ChangeExtension($packagePath, ".zip")
    $expandedPackagePath = Join-Path $packageWorkspacePath $Version
    $finalPackagePath = Join-Path $artifactsPath $newPackName

    New-Item -ItemType Directory -Path $artifactsPath -Force | Out-Null

    # 将nupkg修改成zip，并解压
    Rename-Item -LiteralPath $packagePath -NewName ([System.IO.Path]::GetFileName($zipPackPath))
    Expand-Archive -LiteralPath $zipPackPath -DestinationPath $expandedPackagePath

    ## 移除pdb文件
    $toolPackagePath = Join-Path $expandedPackagePath "tools/$dotnetVersion/any"
    $files = Get-ChildItem -Path $toolPackagePath -Recurse -Include *.pdb
    foreach ($file in $files) {
        Remove-Item -LiteralPath $file.FullName -Force
    }

    # 删除 BuildHost-net472
    $buildHostPath = Join-Path $toolPackagePath "BuildHost-net472"
    if (Test-Path -LiteralPath $buildHostPath) {
        Remove-Item -LiteralPath $buildHostPath -Recurse -Force
    }

    # 重新将文件压缩，不包含最外层目录
    Compress-Archive -Path (Join-Path $expandedPackagePath "*") -DestinationPath $finalPackagePath -CompressionLevel Optimal -Force

    # 获取并输出文件大小
    $fileSize = (Get-Item -LiteralPath $finalPackagePath).Length
    Write-Host "New package size: $([Math]::Round($fileSize / 1MB, 2)) MB"

    # 删除临时文件
    Remove-Item -LiteralPath $expandedPackagePath -Recurse -Force
    Remove-Item -LiteralPath $zipPackPath -Force

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId

    Write-Host 'install new version:'$PackageId $Version
    dotnet tool install -g --add-source $artifactsPath $PackageId --version $Version

    Set-Location $repoRoot
}
catch {
    Set-Location $repoRoot
    Write-Host $_.Exception.Message
    exit 1
}
