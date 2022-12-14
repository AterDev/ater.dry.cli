[CmdletBinding()]
param (
    [Parameter()]
    [System.Boolean]
    $withStudio = $false
)
$location  = Get-Location
try {
    if ($withStudio -eq $true) {
        # build web project
        Set-Location ../src/AterStudio
        if ([System.IO.File]::Exists(".\publish")) {
            <# Action to perform if the condition is true #>
            Remove-Item .\publish\ -R -Force
        }
        
        dotnet publish -c release -o ./publish
        Compress-Archive -Path .\publish\*  -DestinationPath "../CommandLine/studio.zip" -CompressionLevel Optimal -Force
    }

    Set-Location $location
    Set-Location ../src/CommandLine
    # get package name and version
    $VersionNode = Select-Xml -Path ./CommandLine.csproj -XPath '/Project//PropertyGroup/Version'
    $PackageNode = Select-Xml -Path ./CommandLine.csproj -XPath '/Project//PropertyGroup/PackageId'
    $Version = $VersionNode.Node.InnerText
    $PackageId = $PackageNode.Node.InnerText

    # uninstall old version
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g $PackageId
    Write-Host 'build and pack new version...'
    # build & pack
    dotnet build -c release
    dotnet pack --no-build -c release
    Write-Host 'install new version'
    dotnet tool install -g --add-source ./nupkg $PackageId --version $Version

    Set-Location $location
}
catch {
    Write-Host $_.Exception.Message
}
