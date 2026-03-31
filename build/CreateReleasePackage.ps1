param(
    [string]$ProjectDir,
    [string]$OutputPath
)

# manifest.json からバージョン情報を取得
$manifest = Get-Content (Join-Path $ProjectDir "manifest.json") | ConvertFrom-Json
$version = $manifest.version
$gameVersion = $manifest.gameVersion

# パスの定義
$releaseDir = Join-Path $ProjectDir "Release"
$pluginsDir = Join-Path $releaseDir "Plugins"
$archiveName = "SimpleDimmer_v${version}_bs${gameVersion}.7z"
$archivePath = Join-Path $releaseDir $archiveName

# Plugins フォルダを作成して DLL をコピー
New-Item -ItemType Directory -Force -Path $pluginsDir | Out-Null
Copy-Item (Join-Path $OutputPath "SimpleDimmer.dll") $pluginsDir -Force

# 既存のアーカイブを削除
if (Test-Path $archivePath) { Remove-Item $archivePath -Force }

# 7z.exe のパスを解決（PATH優先、失敗時はデフォルトパス）
$sevenZip = "7z"
try {
    & $sevenZip i 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { throw }
} catch {
    $sevenZip = "C:\Program Files\7-Zip\7z.exe"
}

# 7z で圧縮（LZMA2、最大圧縮率）
# $pluginsDir の親（= $releaseDir）を作業ディレクトリにして "Plugins\" を圧縮する
Push-Location $releaseDir
try {
    & $sevenZip a -t7z -m0=lzma2 -mx=9 $archivePath "Plugins"
    if ($LASTEXITCODE -ne 0) { throw "7z failed with exit code $LASTEXITCODE" }
} finally {
    Pop-Location
}

# 一時フォルダを削除
Remove-Item $pluginsDir -Recurse -Force

Write-Host "Package created: $archivePath"
