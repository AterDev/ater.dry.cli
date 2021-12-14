# uninstall old version
try {
    Write-Host 'uninstall old version'
    dotnet tool uninstall -g commandline
    Write-Host 'packing new version...'
    dotnet pack 
    # get package version
    $Node = Select-Xml -Path ./src/CommandLine.csproj -XPath '/Project//PropertyGroup/Version'
    $Version = $Node.Node.InnerText

    Write-Host 'install new version'
    dotnet tool install -g --add-source ./src/nupkg CommandLine --version $Version
}
catch {
    Write-Host $_.Exception.Message
}
