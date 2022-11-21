$location  = Get-Location
try {
    # build web project
    # cd ../src/AterStudio
    # dotnet publish -c release -o ./publish
    # Compress-Archive -Path .\publish\*  -DestinationPath "../CommandLine/studio.zip" -CompressionLevel Optimal -Force
    # rm .\publish\ -R -Force

    cd $location
    cd ../src/CommandLine
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

    cd $PSScriptRoot
}
catch {
    Write-Host $_.Exception.Message
}
