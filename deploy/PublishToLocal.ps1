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
            ".\publish\Microsoft.CodeAnalysis.CSharp.Workspaces.dll",
            ".\publish\Microsoft.CodeAnalysis.Workspaces.MSBuild.dll"
            ".\publish\Microsoft.Build.dll",
            ".\publish\Microsoft.Build.Framework.dll",
            ".\publish\Humanizer.dll",
            ".\publish\LiteDB.dll",
            ".\publish\Microsoft.OpenApi.Readers.dll",
            ".\publish\Microsoft.OpenApi.dll",
            ".\publish\SharpYaml.dll",
            ".\publish\AterStudio.exe",
            ".\publish\CodeGenerator.dll",
            ".\publish\Command.Share.dll",
            ".\publish\Core.dll",
            ".\publish\Datastore.dll",
            ".\publish\NuGet.Versioning.dll",
            ".\publish\PluralizeService.Core.dll",
            ".\publish\swagger.json"
        )

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
    Write-Host 'build and pack new version...'
    # build & pack
    dotnet build -c release
    dotnet pack --no-build -c release
    
    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId

    Write-Host 'install new version'
    dotnet tool install -g --add-source ./nupkg $PackageId --version $Version

    Set-Location $location
}
catch {
    Write-Host $_.Exception.Message
}
