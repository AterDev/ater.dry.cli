[CmdletBinding()]
param (
    [Parameter()]
    [System.String]
    $relativePath = "../../ater.web"
)
$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8
$deployPath = Get-Location
$templatePath = (Join-Path $deployPath $relativePath)

$entityPath = Join-Path $templatePath "templates" "apistd" "src" "Core" "Entities"

# 获取模块实体文件
$entityFiles = Get-ChildItem -Path $entityPath -Filter "*.cs" -Recurse |`
    Select-String -Pattern "Module" -List |`
    Select-Object -ExpandProperty Path

# 目标路径
$destPath = Join-Path $deployPath ".." "src" "CommandLine" "Modules"

# define string array
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
    $entityDestDir = Join-Path $destPath $moduleName "Entities"
    if (!(Test-Path $entityDestDir)) {
        New-Item -ItemType Directory -Path $entityDestDir | Out-Null
    }
    Copy-Item $file $entityDestDir -Force 
    write-host "copy $file `nto $entityDestDir"
} 

# 模块的copy
foreach ($moduleName in $modulesNames) {
    $modulePath = Join-Path $templatePath "templates" "apistd" "src" "Modules" $moduleName
    $destModulePath = Join-Path $destPath $moduleName

    Write-Host $modulePath $destModulePath
    Copy-Item $modulePath $destModulePath -Recurse -Force

    # delete obj and bin dir 
    $pathsToRemove = @("obj", "bin") | ForEach-Object { Join-Path $destModulePath $_ }
    Remove-Item $pathsToRemove -Recurse -Force -ErrorAction SilentlyContinue

}




    




