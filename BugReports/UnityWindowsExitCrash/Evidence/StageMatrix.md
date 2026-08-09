# Staged minimal-project matrix

All builds used Unity 6000.4.12f1, D3D11, Windows x86_64, Mono, Development, and clean build cache. Every build completed with warning `0` and error `0`.

| Stage | Added delta | Exit route | Runs | AV | Exit 0 | Result |
| --- | --- | --- | ---: | ---: | ---: | --- |
| A | Empty Scene + Camera | Window Close | 5 | 0 | 5 | No AV observed |
| B | URP 17.4.0 + Pipeline/Renderer assets | Window Close | 5 | 1 | 4 | Intermittent candidate (`1/5`) |
| C | Input System 1.19.0 + PlayerInput | Window Close | 5 | 0 | 5 | No AV observed |
| D | uGUI 2.0.0 + Canvas/CanvasScaler/TMP | Window Close | 5 | 0 | 5 | No AV observed |
| E | Global Volume component | Window Close | 5 | 0 | 5 | No AV observed |
| F | Audio module + AudioSource | Window Close | 5 | 0 | 5 | No AV observed |
| G | Product-like Windows settings | Window Close | 5 | 0 | 5 | No AV observed |
| H | Guard + synchronous JSON save + Application.Quit | Application.Quit | 5 | 0 | 5 | No AV observed |
| I | Minimum URP-lit cube + Collider | Application.Quit | 5 | 0 | 5 | No AV observed |

Initial A-I total: `1/45` AV. The B failure was run 4, started `2026-08-01T23:50:26.6081892+09:00`, signed exit code `-1073741819` (`0xC0000005`). A Stage B-specific dump was not captured.

## Stage B follow-up

| Condition | Same B build | Runs | AV | Dumps | Interpretation |
| --- | --- | ---: | ---: | ---: | --- |
| ProcDump first-chance AV filter | Yes | 20 | 0 | 0 | Monitoring may affect timing; no stack obtained |
| User-level WER LocalDumps, no debugger | Yes | 30 | 0 | 0 | All exit code 0; temporary WER key removed afterward |

The heterogeneous observations must not be pooled into a single stable rate. The result is fixed as follows:

> Stage BはA～Iで唯一Access Violationを観測した最小構成候補。ただし1/5の低頻度であり、URPを直接原因とは断定できない。後続Stageで単調に再現率が上がらないため、タイミングまたはBuild layout依存の可能性がある。

## Confirmed, unconfirmed, and inferred

- Confirmed: B is the only A-I stage that produced an AV in the initial five-run set.
- Confirmed: the B follow-up did not reproduce or produce a dump.
- Unconfirmed: B's fault function, native stack, stack hash, and UnityPlayer offset.
- Unconfirmed: whether B's one AV is the same defect as the product crash.
- Inference only: monitoring, timing, or serialized/build layout can change the reproduction rate.
- Not concluded: URP is the root cause.

The larger A/B/H/product repetition matrix was intentionally not run because its prerequisite—confirming that Stage B has the same stack as the product—was not met.
