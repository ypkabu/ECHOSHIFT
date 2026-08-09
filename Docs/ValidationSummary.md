# Validation Summary

現行のPlayable prototypeに対して、実Sceneを含む回帰検証を行っています。

| Check | Result |
| --- | --- |
| EditMode tests | `151/151` passed |
| PlayMode tests | `131/131` passed |
| P0-P3 Scene load | Passed |
| Missing Component / Reference scan | `0` |
| P3 automated completion | Passed |
| P3 interactions | `4` success / `0` failure |
| Maximum replay drift | `0 m` |
| Compiler error / warning matches | `0 / 0` |
| Missing / NullReference / unhandled exception matches | `0 / 0 / 0` |

検証コマンド、対象Scene、許容値は[Test Plan](TestPlan.md)に記録しています。

## Known limitation

Windowsの画面付きStandaloneは、終了時に`UnityPlayer.dll`のnative cleanup内で`0xC0000005`を再現します。Gameplay中のmanaged exceptionではありませんが、正式配布前の未解決事項として扱っています。再現条件と調査境界は[Windows Player Exit Crash Investigation](Phase4WindowsExitCrashInvestigation.md)を参照してください。
