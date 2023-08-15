# 打包模板中的内容，主要包含模块以及基础设施项目
[CmdletBinding()]
param (
    [Parameter()]
    [System.String]
    $relativePath = "../../ater.web"
)
# 路径定义
$deployPath = Get-Location
$rootPath = [IO.Path]::GetFullPath("$deployPath/..")
$templatePath = Join-Path $deployPath $relativePath
$entityPath = Join-Path $templatePath "templates" "apistd" "src" "Entity"
$commandLinePath = Join-Path $rootPath "src" "CommandLine"
$destPath = Join-Path $commandLinePath "template"
$destModulesPath = Join-Path $destPath "Modules" 
$destInfrastructure = Join-Path $destPath "Infrastructure"

# 移动模块到临时目录
function CopyModule([string]$solutionPath, [string]$moduleName, [string]$destModulesPath) {
    Write-Host "copy module files:"$moduleName

    # 实体的copy
    $entityDestDir = Join-Path $destModulesPath $moduleName "Entities"
    if (!(Test-Path $entityDestDir)) {
        New-Item -ItemType Directory -Path $entityDestDir | Out-Null
    }

    $entityPath = Join-Path $solutionPath "./src/Entity" $moduleName"Entities"

    if (Test-Path $entityPath) {
        Copy-Item -Path $entityPath\* -Destination $entityDestDir -Force

        # move store to tmp
        $entityFrameworkPath = Join-Path $solutionPath "./src/Database/EntityFramework"
        $applicationDestDir = Join-Path $destModulesPath $moduleName "Application"
    
        if (!(Test-Path $applicationDestDir)) {
            New-Item -Path $applicationDestDir -ItemType Directory -Force | Out-Null
        }
    
        $entityNames = Get-ChildItem -Path $entityPath -Filter "*.cs" | ForEach-Object { $_.BaseName }
        foreach ($entityName in $entityNames) {
            $queryStorePath = Join-Path $entityFrameworkPath "QueryStore" $entityName"QueryStore.cs"
            $commandStorePath = Join-Path $entityFrameworkPath "CommandStore" $entityName"CommandStore.cs"

            if ((Test-Path $queryStorePath)) {
                Copy-Item -Path $queryStorePath -Destination $applicationDestDir -Force
            }
            if ((Test-Path $commandStorePath)) {
                Copy-Item -Path $commandStorePath -Destination $applicationDestDir -Force
            }
        }
    }
}


$OutputEncoding = [System.Console]::OutputEncoding = [System.Console]::InputEncoding = [System.Text.Encoding]::UTF8

# 目标目录
if (!(Test-Path $destModulesPath)) {
    New-Item -ItemType Directory -Path $destModulesPath -Force | Out-Null
}

# 获取模块实体文件
$entityFiles = Get-ChildItem -Path $entityPath -Filter "*.cs" -Recurse |`
    Select-String -Pattern "\[Module" -List |`
    Select-Object -ExpandProperty Path

# 模块名称
$modulesNames = @("CMSMod", "FileManagerMod")

# 获取模块名称
$regex = '\[Module\("(.+?)"\)\]';
foreach ($file in $entityFiles) {
    $content = Get-Content $file
    $match = $content | Select-String -Pattern $regex -AllMatches | Select-Object -ExpandProperty Matches
    if ($match.Count -eq 0) {
        continue
    }
    $moduleName = $match.Groups[1].Value

    # add modulename  to modulesNames if not exist 
    if ($modulesNames -notcontains $moduleName) {
        $modulesNames += $moduleName
    }
} 

# 模块的copy
foreach ($moduleName in $modulesNames) {
    Write-Host "copy module:"$moduleName

    $modulePath = Join-Path $templatePath "templates" "apistd" "src" "Modules" $moduleName
    Copy-Item $modulePath $destModulesPath -Recurse -Force
    
    # delete obj and bin dir 
    $destModulePath = Join-Path $destModulesPath $moduleName
    $pathsToRemove = @("obj", "bin") | ForEach-Object { Join-Path $destModulePath $_ }
    Remove-Item $pathsToRemove -Recurse -Force -ErrorAction SilentlyContinue

    # copy module entity and store
    $solutionPath = Join-Path $templatePath "templates" "apistd"
    CopyModule $solutionPath $moduleName $destModulesPath
}

# copy Infrastructure
$infrastructurePath = Join-Path $templatePath "templates" "apistd" "src" "Infrastructure"
Copy-Item $infrastructurePath $destInfrastructure -Recurse -Force
Remove-Item "$destInfrastructure/**/obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item "$destInfrastructure/**/bin" -Recurse -Force -ErrorAction SilentlyContinue

# zip
$zipPath = Join-Path $commandLinePath "template.zip"
Compress-Archive -Path $destModulesPath, $destInfrastructure -DestinationPath $zipPath -CompressionLevel Optimal -Force
Write-Host "🗜️ $zipPath"

# remove modules
Remove-Item $destPath -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "🗑️ $destPath"

