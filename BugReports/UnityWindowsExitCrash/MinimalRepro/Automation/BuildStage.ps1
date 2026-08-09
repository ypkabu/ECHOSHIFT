param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-I]$')]
    [string]$Stage,

    [Parameter(Mandatory = $true)]
    [string]$UnityEditor,

    [string]$OutputRoot = ""
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$manifest = Join-Path $projectRoot "StageDefinitions/$Stage.manifest.json"
$packageManifest = Join-Path $projectRoot 'Packages/manifest.json'
$packageLock = Join-Path $projectRoot 'Packages/packages-lock.json'
if (-not $OutputRoot) {
    $OutputRoot = Join-Path $projectRoot 'Builds'
}
$output = Join-Path $OutputRoot "$Stage/MinimalExitCrash.exe"
$log = Join-Path $projectRoot "Logs/Build-$Stage.log"

Copy-Item -LiteralPath $manifest -Destination $packageManifest -Force
if (Test-Path -LiteralPath $packageLock) {
    Remove-Item -LiteralPath $packageLock -Force
}

$tmpResources = Join-Path $projectRoot 'Assets/Resources'
$tmpSettings = Join-Path $tmpResources 'TMP Settings.asset'
$tmpSettingsMeta = "$tmpSettings.meta"
if ($Stage -ge 'D') {
    New-Item -ItemType Directory -Force -Path $tmpResources | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'StageDefinitions/Support/TMP Settings.asset') -Destination $tmpSettings -Force
    Copy-Item -LiteralPath (Join-Path $projectRoot 'StageDefinitions/Support/TMP Settings.asset.meta') -Destination $tmpSettingsMeta -Force
} else {
    if (Test-Path -LiteralPath $tmpSettings) { Remove-Item -LiteralPath $tmpSettings -Force }
    if (Test-Path -LiteralPath $tmpSettingsMeta) { Remove-Item -LiteralPath $tmpSettingsMeta -Force }
}

$arguments = @(
    '-batchmode',
    '-nographics',
    '-quit',
    '-projectPath', $projectRoot,
    '-executeMethod', 'UnityWindowsExitCrashRepro.Editor.BuildStagedExitCrashRepro.BuildFromCommandLine',
    '-reproStage', $Stage,
    '-reproOutput', $output,
    '-logFile', $log
)
$process = Start-Process -FilePath $UnityEditor -ArgumentList $arguments -Wait -PassThru
if ($process.ExitCode -ne 0) {
    throw "Unity stage $Stage build failed with exit code $($process.ExitCode). See $log"
}

Write-Output $output
