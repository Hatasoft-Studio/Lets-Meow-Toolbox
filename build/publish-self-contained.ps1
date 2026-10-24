# ---- Let's Meow Toolbox ----
# 单文件发布：自包含（Self-Contained），目标机无需安装 .NET
# 产物：dist/self-contained/LetsMeowToolbox.exe + plugins/

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$Out  = Join-Path $Root "dist\self-contained"

Write-Host "[1/3] 清理旧产物..." -ForegroundColor Cyan
if (Test-Path $Out) { Remove-Item $Out -Recurse -Force }

Write-Host "[2/3] 发布主程序（单文件 · 自包含）..." -ForegroundColor Cyan
& dotnet publish (Join-Path $Root "src\LetsMeowToolbox.App\LetsMeowToolbox.App.csproj") `
    -c Release -r win-x64 -p:PublishSingleFile=true --self-contained true -o $Out
if ($LASTEXITCODE -ne 0) { Write-Error "发布失败"; exit 1 }

Write-Host "[3/3] 构建高危插件并复制到 plugins 目录..." -ForegroundColor Cyan
& dotnet build (Join-Path $Root "src\LetsMeowToolbox.Plugins.HighRisk\LetsMeowToolbox.Plugins.HighRisk.csproj") -c Release
$PluginDll = Join-Path $Root "src\LetsMeowToolbox.Plugins.HighRisk\bin\Release\net8.0-windows\LetsMeowToolbox.Plugins.HighRisk.dll"
$PluginsDir = Join-Path $Out "plugins"
New-Item -ItemType Directory -Force -Path $PluginsDir | Out-Null
Copy-Item $PluginDll (Join-Path $PluginsDir "LetsMeowToolbox.Plugins.HighRisk.dll") -Force

Write-Host "" -ForegroundColor Cyan
Write-Host "完成！产物目录：$Out" -ForegroundColor Green
Get-ChildItem $Out | Select-Object Name, Length
