param(
    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[A-I]$')]
    [string]$Stage,

    [Parameter(Mandatory = $true)]
    [string]$Executable,

    [int]$SecondsBeforeClose = 3,

    [string]$EvidenceRoot = ""
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (-not $EvidenceRoot) {
    $EvidenceRoot = Join-Path $projectRoot 'Logs/StageRuns'
}
$stageEvidence = Join-Path $EvidenceRoot $Stage
New-Item -ItemType Directory -Force -Path $stageEvidence | Out-Null

Add-Type @"
using System;
using System.Runtime.InteropServices;
public static class ReproWindowClose {
    [DllImport("user32.dll", SetLastError=true)]
    public static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
"@

$rows = @()
for ($run = 1; $run -le 5; $run++) {
    $playerLog = Join-Path $stageEvidence "run-$run-Player.log"
    $arguments = @('-screen-width', '1280', '-screen-height', '720', '-screen-fullscreen', '0', '-logFile', $playerLog)
    $started = Get-Date
    $process = Start-Process -FilePath $Executable -ArgumentList $arguments -PassThru
    if ($Stage -lt 'H') {
        $windowDeadline = (Get-Date).AddSeconds(30)
        do {
            Start-Sleep -Milliseconds 200
            $process.Refresh()
        } while (-not $process.HasExited -and $process.MainWindowHandle -eq [IntPtr]::Zero -and (Get-Date) -lt $windowDeadline)
        Start-Sleep -Seconds $SecondsBeforeClose
        $process.Refresh()
        if (-not $process.HasExited) {
            if ($process.MainWindowHandle -eq [IntPtr]::Zero) {
                throw "Stage $Stage run $run never created a closeable window."
            }
            [ReproWindowClose]::PostMessage($process.MainWindowHandle, 0x0010, [IntPtr]::Zero, [IntPtr]::Zero) | Out-Null
        }
    }
    if (-not $process.WaitForExit(45000)) {
        $process.Kill()
        $process.WaitForExit()
    }
    $process.Refresh()
    $rows += [pscustomobject]@{
        Stage = $Stage
        Run = $run
        Started = $started.ToString('o')
        ExitCode = $process.ExitCode
        PlayerLog = $playerLog
    }
}

$rows | Export-Csv -NoTypeInformation -Encoding UTF8 (Join-Path $stageEvidence 'results.csv')
$rows
