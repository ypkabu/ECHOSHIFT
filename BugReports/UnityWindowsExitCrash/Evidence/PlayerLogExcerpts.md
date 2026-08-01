# Curated Player log evidence

Raw generated logs remain under ignored local `Logs` directories and are not committed. The relevant confirmed ordering from the product Player is:

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

The access violation occurs after normal gameplay completion and after managed/engine shutdown messages. The saved logs contain no compiler error, Missing Script/Reference, `NullReferenceException`, managed unhandled exception, or failed recorded interaction before the native fault.

For the earlier 6000.4.12f1 minimal dump, initialization completes, the minimal quit request is logged, and the dump records the AV in native cleanup. For staged Stage B run 4, the Player log exists locally but no dump was created; the log and signed exit code alone do not establish its fault function.
