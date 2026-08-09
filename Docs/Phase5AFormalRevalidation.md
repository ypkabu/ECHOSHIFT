# Phase 5A Formal Revalidation

- Revalidation date: 2026-08-09 JST
- Branch: `feature/phase5a-authorship-identity`
- Validation start HEAD: `35bc519acd25fa010756c19a5daf4e0ff1ce6a62`
- Final tested HEAD: `d471010a76f6de14bce42d13c136307b8a6403d3`
- Approved Production commit: `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`
- Human Review finalization commit: `156b28dd7f461928c88cacdad3d6ad6985240185`
- Builder determinism fix: `35bc519acd25fa010756c19a5daf4e0ff1ce6a62`
- Canonical mismatch fail-closed fix: `d471010a76f6de14bce42d13c136307b8a6403d3`
- Unity: `6000.4.6f1 (0b051c2e5d54)`
- URP: `17.4.0`

## Final Verdict

**PHASE 5A FORMAL VALIDATION PASS**

初回Formal ValidationのFAILは`Docs/Phase5AFormalValidation.md`とcommit `3ed79071cd8615cf4802054fc98424098f49534c`に保持する。初回blockerは承認済みProduction状態でもBuilderがRobot Controller、Volume Profile、P3 Echo Prefab、P3 Sceneを不要に再serializeしたことである。

本Revalidationでは全Gateを現在の実装から再実行した。Human Review P0／P1は0、approved Visual／Audioは維持され、Builderのsame-process／cross-process実行は差分0、全EditMode／PlayModeとP3回帰はPassした。Gate 4監査で発見したcanonical mismatch後のlegacy rebuild経路は、Production assetが一部でも存在する場合にwrite前でfail-closedするよう別commitへ分離して修正し、その最終HEADから全自動Gateを取り直した。

Builderについて証明した内容は次に限定する。

> **Approved canonical Production stateに対するBuilder execution is deterministic and idempotent, protected by exact canonical asset validation.**

任意入力、削除済みProduction、historic stateから同一YAMLを完全再生成できることや、Unity serializer自体の完全determinismは主張しない。

## Gate 1 — Git Baseline

- Branch: `feature/phase5a-authorship-identity`。
- Starting HEAD: `35bc519acd25fa010756c19a5daf4e0ff1ce6a62`。
- Worktree entries: `0`。
- Staged: `0`。
- Relevant untracked: `0`。
- Existing `phase5a-*` tags: `0`。
- Residual Unity／UnityCrashHandler process: `0`。
- Verdict: **PASS**。

Gate 4の追加安全修正後は`d471010a76f6de14bce42d13c136307b8a6403d3`を最終検証HEADとし、worktree cleanからGate 2以降を再実行した。

## Gate 2 — Previous FAIL Resolution

対象4 assetを承認済みcanonical hashから3つの独立Unity batchmode processで再実行した。

| Asset | Approved SHA-256 |
|---|---|
| Robot Controller | `EE885A3CE1AA515DC6C1EE40AEE629DA4E51CA9822C982A2D5335639E30D8C92` |
| Volume Profile | `D85DF4E9C7C20D6FADC2DB06899D4B6437E79EEF4BB9B65922257485B51C19C3` |
| P3 Echo Prefab | `0FBB217DCA6A1D6132F17F1F4B512A83124A5C33D47D2F492A4BC08820903B52` |
| P3 Scene | `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854` |

| Run | Exit | Marker | Tracked Production diff | Hash result |
|---|---:|---|---:|---|
| 1 | `0` | `assets=4;writeCount=0` | `0` | 4/4一致 |
| 2 | `0` | `assets=4;writeCount=0` | `0` | 4/4一致 |
| 3 | `0` | `assets=4;writeCount=0` | `0` | 4/4一致 |

- Same-process 3 runs: EditMode determinism test Pass、各Run byte diff `0`。
- Cross-process 3 runs: exit `0`、diff `0`。
- Post-run worktree: clean。
- Original blocker resolved: **YES**。
- Verdict: **PASS**。

## Gate 3 — Canonical Guard Behavior

- Expected hash／actual asset: 4/4 exact match。
- Marker: `PHASE5A_PRODUCTION_CANONICAL_OK assets=4;writeCount=0`。
- writeCount: `0`。
- canonical hit時はP3 Sceneを必要時だけread-onlyでopenする。
- `EnsureFolders`、Phase 4 asset build、Robot Controller dirty/save、Volume override Destroy/Create、Echo Prefab save、Empty Scene生成、P3 Scene save、`SaveAssets`へ入らない。
- Builder実行後の対象4 hashとGit diffは不変。
- Verdict: **PASS**。

## Gate 4 — Canonical Mismatch Failure Behavior

Production assetを変更せず、OS temporary directory内のisolated fixtureへ意図的に誤ったexpected hashを与えた。

- Fixture bytes: ASCII `ECHO//SHIFT`。
- Actual SHA-256: `9071CD811930A2DC0186E6B677B4757B4E43B78AED7164B8C0FA912B88FBEDA6`。
- Rejected expected SHA-256: 64桁の`0`。
- Detection: falseを返し、`mismatch=<path>;expected=<hash>;actual=<hash>`を報告。
- Fixture bytes after validation: unchanged。
- Expected baseline after validation: unchanged。
- Existing Production asset countを同時に取得し、Production assetが1件以上存在するmismatch／partial-missingは`PHASE5A_PRODUCTION_CANONICAL_MISMATCH ...;writeCount=0`でlegacy write path前に例外終了する。
- MismatchをPASS扱いしない。actual hashを自動baselineへ採用しない。
- Isolated targeted test: Pass。
- Production diff: `0`。
- Verdict: **PASS**。

## Gate 5 — Builder Responsibility Audit

### Guaranteed

- Approved canonical Production stateを4 exact SHA-256で検証する。
- canonical stateではsafe no-opになり、不要なasset reserializationを行わない。
- mismatch／partial-missingを検出し、Production write前にfail-closedする。
- canonical状態でのsame-process／cross-process再実行をbyte-identicalに維持する。

### Not claimed

- 4 Production assetがすべて削除された状態からの完全なcanonical regeneration。
- arbitrary historic／modified stateからcanonical YAMLへの収束。
- legacy bootstrap outputのcross-session byte determinism。
- Unity serialization generator自体の完全determinism。

4 assetすべてが存在しない初期bootstrap pathは残るが、Formal PASSの保証対象ではない。この境界はPhase 5A blockerにせず、README／portfolioで完全deterministic generatorと表現しない。

- Verdict: **PASS**。

## Gate 6 — Production Diff Audit

### Approved Visual／Audio

- Logo: approved `ECHO//SHIFT` wordmark。
- Chamber: Floor Base、Rear Support、Metal Outer Frame、restrained Arc、Support Core、3 Indicator。
- Actor motif: Cyan Player／Violet Echo、Chest Strip、Surface Seal、Rear Tick、thin Floor Segment。
- Goal: Locked White／Violet、Unlocked Lime success state。
- Maintenance／Observation／Real Decal: Human Review承認済み構成。
- Audio: approved 8 cue。

### Supporting infrastructure

- Production Application Builder、exact canonical guard、fail-closed mismatch validation。
- Goal presentation adapter、audio cue routing、EditMode／PlayMode tests、validation documents。

### Unexpected

- Builder fix以後のRuntime diff: `0`。
- ThirdParty diff: `0`。
- Packages diff: `0`。
- Project Settings diff: `0`。
- Scene／Prefab／Audio diff: `0`。
- Unrelated Gameplay logic: `0`。
- Verdict: **PASS**。

## Gate 7 — Presentation Ownership Audit

### Goal

- `Phase5AGoalIdentityVisual`は`DoorController.IsOpenRequested`をread-onlyで参照する。
- 書き換えるのはLocked／Unlocked visual childのactive stateだけ。
- unlock判定、Puzzle completion、Door state、Section progressionを所有しない。

### Audio

- `Phase4FeedbackDirector`は既存Loop／Echo／Interaction／Goal eventを受けて`Phase4AudioController.Play`へcueを渡す。
- state mutation、completion decision、Battery ownership、Replay logicを持たない。

### Actor／Environment identity

- Approved environment identity rootのCollider／Rigidbody: `0`。
- Actor Animatorは`P4 Robot Visual` childだけに存在し、root motion OFF。Gameplay Actor rootにAnimatorを追加していない。
- Project-owned primitive visualは生成時にColliderを除去し、imported visual prefabもchild Colliderを除去する。
- Actor motifはRenderer／TransformとPresentation componentだけで、Gameplay state ownerを追加しない。
- Verdict: **PASS**。

## Gate 8 — Cold／Cross-process Validation

- 開始前residual Unity process: `0`。
- Builder、EditMode、PlayMode、D3D11 Captureはそれぞれ新規Unity process。
- Script compile: Pass。
- Builder validation: Pass。
- P3 Scene load: Pass。
- EditMode／PlayMode: Pass。
- 各process後residual Unity process: `0`。
- 既存interactive Editor sessionへの依存: なし。
- Verdict: **PASS**。

## Gate 9 — EditMode

- Total: `151`。
- Passed: `151`。
- Failed: `0`。
- Skipped: `0`。
- Duration: `6.6515738 s`。
- Exact 64px Logo evidence: Pass。
- Locked Goal Lime 0／Unlocked state: Pass。
- Actor motif placement: Pass。
- Audio exact hash／reference: Pass。
- Canonical guard／same-process 3-run idempotence: Pass。
- Isolated mismatch detection: Pass。
- Production composition／evidence exclusion: Pass。
- Verdict: **PASS**。

## Gate 10 — PlayMode

- Total: `131`。
- Passed: `131`。
- Failed: `0`。
- Skipped: `0`。
- Duration: `38.5318821 s`。
- Verdict: **PASS**。

## Gate 11 — Full P3 Gameplay Regression

- Player spawn／movement: Pass。
- Interaction: success `4`／failure `0`。
- Battery acquire／carry／insert: Pass。
- Echo spawn／replay／remove: Pass。
- Goal Locked／Unlocked: Pass。
- Door behavior: Pass。
- Section progression／final completion: Pass。
- P3 all sections complete: Pass。
- Replay Drift maximum: `0 m`。
- Integration advances: `1061`。
- Production identity／Echo lifecycle smoke test: Pass。
- Verdict: **PASS**。

## Gate 12 — Scene Regression

| Scene | SHA-256 | Result |
|---|---|---|
| P0 | `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5` | approved一致 |
| P1 | `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB` | approved一致 |
| P2 | `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C` | approved一致 |
| P3 | `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854` | Phase 5A canonical一致 |

- Verdict: **PASS**。

## Gate 13 — Visual Production Validation

Fresh Production CaptureをDirect3D 11／1920×1080で9枚生成した。command exit `0`、markerは`PHASE5A_PRODUCTION_CAPTURES_OK count=9;graphics=Direct3D11;resolution=1920x1080`。全PNGの実寸は1920×1080。

- Gameplay overview: Section 3 layoutと主要deviceを確認。
- Player／Echo: approved surface motif、Cyan／Violet identity、silhouetteを維持。
- Locked Goal: Lime `0`。White／Violet sealが床面より上に見え、床へ沈んでいない。
- Unlocked Goal: Lime perimeter／segmentが表示され、Lockedとの差が明確。
- Chamber: Metal Outer Frame／Internal Arc／Core hierarchyを維持。
- Maintenance: bay／rack／service equipment identityを維持。
- Observation: monitor／frame／indicator identityを維持。
- Real Decal: surface配置を維持し、巨大黒板型へ戻っていない。
- Human Review承認済みP0: `0`、P1: `0`を変更するVisual asset差分なし。
- Verdict: **PASS**。

これはfresh Production一致確認であり、Human Visual Reviewを再実施したという意味ではない。

## Gate 14 — Audio Production Validation

| Cue | SHA-256 |
|---|---|
| `echo_spawn_01.wav` | `80B9D7EA143D2A04770A3D1674BEC6BEA5E7B8DF922F269E6314EC5CB6AB41C3` |
| `echo_spawn_02.wav` | `191F2017CA1DB75FB29AFDE4BA04BBAF8D0411B5BB08512266631C7A01ACDFF9` |
| `echo_spawn_03.wav` | `E1D8C5624FA94FE07137ABE932132CDB87509F700460D07EC95ABAFB9EA13E58` |
| `echo_remove.wav` | `F29BC367844D9BA17A841C14E1FAD5C7C54C5E66ADDEB9FB80203227B8765694` |
| `loop_end.wav` | `3AFAC17282AB377747F8BBEB36D82B510FE4D42AE54D8832BA478E34C1905C73` |
| `interaction_success.wav` | `311D2C78C190593645628B60BDC8E05B83B74E7B8F6B97A9195D4D10C28C2556` |
| `battery_insert.wav` | `496EE8DAAEB920D51BE26ACE71410DF62700CAAE17A8E614635CD7594FF54528` |
| `section_complete.wav` | `3A0317258FCE13ED0F4BF59F08D32156E3D73EFAAD9B172417082C78B8504B7D` |

- Exact approved hash: `8/8`一致。
- Missing: `0`。
- Wrong routing: `0`。
- generation別Echo spawn、remove、Loop end、Interaction success、Battery insert、Section／Game completion reference: Pass。
- Comparison／Preview WAVのProduction event参照: `0`。
- Human listening: Revision 2.1でPass済み。再実施なし。
- Verdict: **PASS**。

## Gate 15 — Error Audit

Builder 3 process、mismatch targeted test、EditMode、PlayMode、D3D11 Captureのfresh logを監査した。

- Compiler errors: `0`。
- Compiler warnings: `0`。
- Missing Script: `0`。
- Missing Reference: `0`。
- NullReferenceException: `0`。
- Unhandled Exception: `0`。
- AssertionException／Assertion failed: `0`。
- AudioListener warning: `0`。
- Serialization error／failed: `0`。
- Test run failed／Aborting batchmode: `0`。
- Verdict: **PASS**。

## Gate 16 — Repository Hygiene

- Tracked Captures／Builds／Logs／TestResults／Library／Temp／obj: 各`0`。
- Tracked crash dump／Player.log／Editor.log／crash archive: `0`。
- Tracked text内の実ユーザー絶対path: `0`。
- Username: `0`。
- Credential assignment candidate: `0`。
- Production Scene／Prefab／Settings内Preview／comparison marker: `0`。
- Phase 5A scopeの実TODO／FIXME: `0`。既存FAIL文書内の監査結果文言だけが文字列検索へ1件一致した。
- Largest tracked filesは必要なNoto Sans JP font、TMP font asset、Quaternius Robot、Production／Preview Sceneであり、由来または用途を文書化済み。不要なBuild／Capture／archiveは含まない。
- Verdict: **PASS**。

## Gate 17 — Asset Provenance

- Phase 5A固有Visual: project-owned procedural mesh／material／layout。
- Environment: Quaternius Modular Sci-Fi MegaKit Standard FREE、CC0 1.0。公式URL、取得版、archive SHA-256、import subset、noticeを`Docs/ThirdPartyAssets.md`で追跡可能。
- Robot: Quaternius Animated Robot Pack、CC0 1.0。公式配布元、original FBX SHA-256、noticeを追跡可能。
- Audio: Kenney Sci-Fi Sounds 1.0、CC0 1.0をsourceとするproject-owned derived WAV。公式URL、archive SHA-256、import subset、noticeを追跡可能。
- Font: Noto Sans JP、OFL noticeあり。
- ThirdParty／notice／provenance doc unexpected diff: `0`。
- License blocker: `0`。
- Verdict: **PASS**。

## Gate 18 — Public Portfolio Safety

- Credentials／tokens／secrets: `0`。
- Personal information／username／private absolute path: `0`。
- Machine-specific cache／settings／dump: tracked `0`。
- Private Capture／test result／log: tracked `0`。
- Unnecessary large generated files: `0`。
- Unlicensed／source-unknown asset blocker: `0`。
- Production debug junk／Human Review placeholder: `0`。
- Verdict: **PASS**。

## Remaining P2

Human Review承認済みの次の4件だけを維持し、本Revalidationでは変更していない。

1. Logo spacing／badge感。
2. Chamber Violet密度。
3. Observation text依存。
4. Decal新品感／surface馴染み／small serial readability。

Critical／High／P0／P1 blockerではない。

## Final Git Audit

- Safety fixは`fix: fail closed on phase 5A canonical mismatch`としてValidation evidence commitと分離した。
- Revalidation commit対象は本書とPhase 5A implementation planの実行結果だけ。
- Production implementation／Scene／Prefab／Audio／Runtime／ThirdParty／Packages／Project Settingsを混ぜない。
- Generated evidenceはGit管理しない。
- Tagは本工程で作成しない。

## Recommendation

**READY TO CREATE PHASE 5A VALIDATED TAG**

Validated tagは別工程でのみ作成する。
