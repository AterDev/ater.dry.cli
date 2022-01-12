
try {
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
    dotnet pack -c release
    Write-Host 'install new version'
    dotnet tool install -g --add-source ./nupkg $PackageId --version $Version

}
catch {
    Write-Host $_.Exception.Message
}
