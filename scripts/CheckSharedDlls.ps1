[CmdletBinding()]
param()

$repoRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot "..")).Path
$commandLineDir = Join-Path $repoRoot "src/Apps/CommandLine"
$studioDir = Join-Path $repoRoot "src/Apps/Dashboard"
$commandLinePublishPath = Join-Path $commandLineDir "publish"
$studioPublishPath = Join-Path $studioDir "publish"
$shareDllsFile = Join-Path $commandLineDir "ShareDlls.txt"

# 清理旧的 publish 目录，避免把上一次构建的 DLL 纳入比较。
Remove-Item -LiteralPath $commandLinePublishPath -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $studioPublishPath -Recurse -Force -ErrorAction SilentlyContinue

## 构建项目
& dotnet publish (Join-Path $commandLineDir "CommandLine.csproj") -c Release -o $commandLinePublishPath -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE -ne 0) {
    throw "CommandLine publish failed with exit code $LASTEXITCODE."
}

& dotnet publish (Join-Path $studioDir "Dashboard.csproj") -c Release -o $studioPublishPath -p:GeneratePackageOnBuild=false
if ($LASTEXITCODE -ne 0) {
    throw "Dashboard publish failed with exit code $LASTEXITCODE."
}

## 检查共享的 DLL 文件
$files1 = @(Get-ChildItem -LiteralPath $commandLinePublishPath -Filter "*.dll" -File | Select-Object -ExpandProperty Name)
$files2 = @(Get-ChildItem -LiteralPath $studioPublishPath -Filter "*.dll" -File | Select-Object -ExpandProperty Name)

# 转换为 HashSet 以便高效交集
$set2 = [System.Collections.Generic.HashSet[string]]::new()
$files2 | ForEach-Object { [void]$set2.Add($_) }

# 求交集并排序，确保生成结果稳定。
$shareDlls = @($files1 | Where-Object { $set2.Contains($_) } | Sort-Object)

Write-Host "Total common DLL files: $($shareDlls.Count)"

## 保存结果
$shareDlls | Set-Content -LiteralPath $shareDllsFile -Encoding UTF8
Write-Host "Generated shared DLL list: $shareDllsFile"

# common 的总大小
$totalSize = 0
$shareDlls | ForEach-Object {
    $filePath1 = Join-Path $commandLinePublishPath $_

    if (Test-Path -LiteralPath $filePath1) {
        $totalSize += (Get-Item -LiteralPath $filePath1).Length
    }
}
Write-Host "Total size of common DLL files: $([Math]::Round($totalSize / 1MB, 2)) MB"

