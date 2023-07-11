[CmdletBinding()]
param (
    [Parameter()]
    [System.String]
    $relativePath = "../../ater.web"
)
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8
# 路径定义
$deployPath = Get-Location
$rootPath = [IO.Path]::GetFullPath("$deployPath/..")
$templatePath = (Join-Path $deployPath $relativePath)
$entityPath = Join-Path $templatePath "templates" "apistd" "src" "Entity"
$commandLinePath = Join-Path $rootPath "src" "CommandLine"
$destModulesPath = Join-Path $commandLinePath "Modules" 

# 目标目录
if (!(Test-Path $destModulesPath)) {
    New-Item -ItemType Directory -Path $destModulesPath | Out-Null
}

# 获取模块实体文件
$entityFiles = Get-ChildItem -Path $entityPath -Filter "*.cs" -Recurse |`
    Select-String -Pattern "Module" -List |`
    Select-Object -ExpandProperty Path

# 模块名称
$modulesNames = @()

# 获取模块名称
$regex = '\[Module\("(.+?)"\)\]';
foreach ($file in $entityFiles) {
    $content = Get-Content $file
    $match = $content | Select-String -Pattern $regex -AllMatches | Select-Object -ExpandProperty Matches
    $moduleName = $match.Groups[1].Value

    # add modulename  to modulesNames if not exist 
    if ($modulesNames -notcontains $moduleName) {
        $modulesNames += $moduleName
    }
    
    # 实体的copy
    $entityDestDir = Join-Path $destModulesPath $moduleName "Entities"
    if (!(Test-Path $entityDestDir)) {
        New-Item -ItemType Directory -Path $entityDestDir | Out-Null
    }
    Copy-Item $file $entityDestDir -Force 
    $fileName = [System.IO.Path]::GetFileName($file)
    write-host "ℹ️ $fileName to $entityDestDir"
} 

# 模块的copy
foreach ($moduleName in $modulesNames) {
    $modulePath = Join-Path $templatePath "templates" "apistd" "src" "Modules" $moduleName
    Copy-Item $modulePath $destModulesPath -Recurse -Force
    
    # delete obj and bin dir 
    $destModulePath = Join-Path $destModulesPath $moduleName
    $pathsToRemove = @("obj", "bin") | ForEach-Object { Join-Path $destModulePath $_ }
    Remove-Item $pathsToRemove -Recurse -Force -ErrorAction SilentlyContinue
}

# zip
$zipPath = Join-Path $commandLinePath "modules.zip"
Compress-Archive -Path $destModulesPath -DestinationPath $zipPath -CompressionLevel Optimal -Force
Write-Host "🗜️ $zipPath"

# remove modules
Remove-Item $destModulesPath -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "🗑️ $destModulesPath"

