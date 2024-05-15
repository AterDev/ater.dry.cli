[CmdletBinding()]
param (
    [Parameter()]
    [System.Boolean]
    $withStudio = $false
)
$location = Get-Location
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8
try {
    Set-Location $location
    # get package name and version
    $VersionNode = Select-Xml -Path ../src/CommandLine/CommandLine.csproj -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path ../src/CommandLine/CommandLine.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # 支持的runtimes
    $supportRuntimes = @(
        "linux-arm64",
        "linux-x64",
        "win-x64",
        "win-arm64",
        "osx-x64",
        "osx-arm64"
    );

    # sync studio version
    Set-Location $location
    $xml = [xml](Get-Content ../src/AterStudio/AterStudio.csproj)
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
    $path = Join-Path  $location "../src/AterStudio/AterStudio.csproj"
    $xml.Save($path)

    # pack modules  use PackModules.ps1
    & "./PackTemplate.ps1"

    # build web project
    if ($withStudio -eq $true) {
        Set-Location ../src/AterStudio
        if (Test-Path -Path ".\publish") {
            Remove-Item .\publish -R -Force
        }
        
        dotnet publish -c release -o ./publish
        # 移除部分 dll文件，减少体积
        $pathsToRemove = @(
            ".\publish\Microsoft.CodeAnalysis.CSharp.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.dll",
            ".\publish\Microsoft.CodeAnalysis.dll",
            ".\publish\Microsoft.EntityFrameworkCore.dll",
            ".\publish\Microsoft.EntityFrameworkCore.Relational.dll",
            ".\publish\Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
            ".\publish\Humanizer.dll",
            ".\publish\Microsoft.IdentityModel.Tokens.dll",
            ".\publish\Microsoft.EntityFrameworkCore.Sqlite.dll",
            ".\publish\Microsoft.OpenApi.dll",
            ".\publish\CodeGenerator.dll",
            ".\publish\SharpYaml.dll",
            ".\publish\Microsoft.Data.Sqlite.dll",
            ".\publish\Mapster.dll",
            ".\publish\Microsoft.OpenApi.Readers.dll",
            ".\publish\Definition.dll",
            ".\publish\Microsoft.IdentityModel.JsonWebTokens.dll",
            ".\publish\Command.Share.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost.dll"
            ".\publish\System.IdentityModel.Tokens.Jwt.dll",
            ".\publish\Microsoft.Extensions.DependencyModel.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.MSBuild.dll"
            ".\publish\NuGet.Versioning.dll",
            ".\publish\System.Composition.TypedParts.dll",
            ".\publish\PluralizeService.Core.dll",
            ".\publish\System.Composition.Hosting.dll",
            ".\publish\System.Composition.Convention.dll",
            ".\publish\SQLitePCLRaw.core.dll",
            ".\publish\Ater.Web.Abstraction.dll",
            ".\publish\Microsoft.Build.Locator.dll",
            ".\publish\Microsoft.IdentityModel.Logging.dll",
            ".\publish\Ater.Web.Core.dll",
            ".\publish\SQLitePCLRaw.provider.e_sqlite3.dll",
            ".\publish\Mapster.Core.dll",
            ".\publish\System.Composition.Runtime.dll",
            ".\publish\System.Composition.AttributedModel.dll",
            ".\publish\Microsoft.IdentityModel.Abstractions.dll",
            ".\publish\Microsoft.Bcl.AsyncInterfaces.dll",
            ".\publish\SQLitePCLRaw.batteries_v2.dll",

            ".\publish\AterStudio.exe",
            ".\publish\swagger.json"
        );

        if (Test-Path -Path ".\publish\runtimes") {
            # 移除runtime目录
            Remove-Item .\publish\runtimes -Recurse -Force
        }

        foreach ($path in $pathsToRemove) {
            if (Test-Path $path) {
                Remove-Item $path -Force
            }
        }
        # remove pdb and xml files
        $files = Get-ChildItem -Path .\publish -Recurse -Include *.pdb, *.xml
        foreach ($file in $files) {
            Remove-Item $file.FullName -Force
        }
        if (Test-Path -Path "../CommandLine/studio.zip") {
            Remove-Item "../CommandLine/studio.zip" -Force
        }
        Compress-Archive -Path .\publish\*  -DestinationPath "../CommandLine/studio.zip" -CompressionLevel Optimal -Force
    }

    Set-Location $location
    Set-Location ../src/CommandLine
    Write-Host 'Packing new version...'

    # pack
    dotnet build -c release
    dotnet pack --no-build -c release

    $newPackName = $PackageId + "." + $Version + ".nupkg"

    # 将nupkg修改成zip，并解压
    $zipPackName = $newPackName.Replace(".nupkg", ".zip")
    Rename-Item -Path "./nupkg/$newPackName" -NewName "$zipPackName"
    Expand-Archive -Path "./nupkg/$zipPackName" -DestinationPath "./nupkg/$Version"

    # 移除 tools\net8.0\any\runtimes 中不需要的文件
    $runtimes = Get-ChildItem -Path "./nupkg/$Version/tools/net8.0/any/runtimes" -Directory
    foreach ($runtime in $runtimes) {
        if ($supportRuntimes -notcontains $runtime.Name) {
            Remove-Item -Path $runtime.FullName -Recurse -Force
        }
    }
    ## 移除pdb xml文件
    $files = Get-ChildItem -Path "./nupkg/$Version/tools/net8.0/any" -Recurse -Include *.pdb
    foreach ($file in $files) {
        Remove-Item $file.FullName -Force
    }

    # 重新将文件压缩，不包含最外层目录
    Compress-Archive -Path "./nupkg/$Version/*" -DestinationPath "./nupkg/$newPackName" -CompressionLevel Optimal -Force

    # 删除临时文件
    Remove-Item -Path "./nupkg/$Version" -Recurse -Force
    Remove-Item -Path "./nupkg/$zipPackName" -Force

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId

    Write-Host 'install new version:'$PackageId $Version
    dotnet tool install -g --add-source ./nupkg $PackageId --version $Version

    Set-Location $location
}
catch {
    Set-Location $location
    Write-Host $_.Exception.Message
}
