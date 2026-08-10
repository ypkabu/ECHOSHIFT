# Playerログの抜粋

生成した未整理のLogはGit管理外の`Logs`に保存し、Commitしません。製品版Playerで確認できた終了前後の順序は次のとおりです。

```text
PHASE3_GAME_COMPLETED
PHASE4_STANDALONE_PROBE_PASS ... drift=0 ... interactionSuccess=4 ... interactionFailure=0
PHASE3_TELEMETRY_SAVED ...
[Physics::Module] Cleanup current backend.
Input System module state changed to: ShutdownInProgress.
Input Polling Thread exited.
Input System module state changed to: Shutdown.
Cleanup mono
CodeReloadManager destroyed
```

Access Violationはゲームの正常完了後、managed側とEngine側の終了Messageより後に発生します。保存したLogでは、Native障害より前にCompiler Error、Missing Script／Reference、`NullReferenceException`、managed側の未処理例外、記録済みインタラクションの失敗はありません。

以前の小さい6000.4.12f1 Projectでは、初期化と終了要求のLogが完了した後、Native Cleanup内のAccess ViolationをDumpで確認できました。構成Bの4回目はPlayer Logだけが端末に残り、Dumpは作成されませんでした。Logと終了コードだけでは、障害関数を特定できません。
