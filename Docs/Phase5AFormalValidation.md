# Phase 5A Formal Validation

- Validation date: 2026-08-09 JST
- Git branch: `feature/phase5a-authorship-identity`
- Validated Production commit: `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`
- Production Application commit: `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`
- Human Review finalization commit: `156b28dd7f461928c88cacdad3d6ad6985240185`
- Unity: `6000.4.6f1`
- URP: `17.4.0`
- Human Review: `PASS WITH P2 POLISH — PRODUCTION CANDIDATE`

## Final Verdict

**PHASE 5A FORMAL VALIDATION FAILED**

Approved Visual、Audio routing、全EditMode／PlayMode、Gameplay regression、repository hygiene、provenanceは合格した。一方、cleanなProduction commitへ`P3SceneBuilder`を再実行するとtracked asset 4件が変化し、同じ生成後状態から2回目を実行しても3件のhashが再変化した。Builder reproducibility／idempotenceは明示的なPASS条件であるため、他Gateが合格していてもFormal ValidationはFAILとする。

Production／Gameplay／Visual／Audioは本Validation中に変更していない。Builderが生成した差分は証跡取得後に対象4ファイルだけをvalidated commitへ復元した。問題を隠す修正、P2 polish、tag作成は行っていない。

## Finding

### FV-01 — Production Builder is not deterministic or idempotent

- Severity: **Blocking**
- Starting state: commit `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`、worktree clean、P3 SHA-256 `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854`。
- Invocation: `EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine`、batchmode／nographics／quit。
- First execution: exit code `0`だが次のtracked 4件が変更された。
  - `Assets/_Project/Art/Phase4/Animation/P4_RobotVisual.controller`
  - `Assets/_Project/Art/Phase4/Phase4VolumeProfile.asset`
  - `Assets/_Project/Prefabs/Actors/P3_Echo.prefab`
  - `Assets/_Project/Scenes/P3_PlayableGreybox.unity`
- First generated hashes:
  - Robot controller: `97F808F6F8F76E67C5BDDDA141A7B5538B2F2FFB69FAB326115404FED8E7E3AD`
  - Volume profile: `87F37EB17E31B19763566497AF981E2D67F6847DF1210F8F7F0463CA2E23E371`
  - Echo prefab: `7D441ADD2834F3679D91DCC2AC896D7FD1D4EF28A769A46813DCCC6A9BE89759`
  - P3 Scene: `68F7FAA0C93E8D9BDC8E46C419EEF6EBC0455693DEBB5F0C53219B669363CB8A`
- Second execution from the first generated state: exit code `0`。Robot controllerだけ同一で、他3件が再変化した。
  - Volume profile: `0CA788E5B123AF0E4C21766E676B7DFFF6DB252DF3D9E7DA976DAB4B13BFB3EA`
  - Echo prefab: `CC7B53373A0BD42D33F6CE924B801588B338C7D1F015E6867C8FC9656F603790`
  - P3 Scene: `C7A817C00202408D7A251C9A961C68716D785195FF4350E1EFA4247ED168F550`
- Diff character: Scene／Prefabのserialized object ID／ordering再生成と、controller／Volume serialization差分。first runは63,631 additions／63,631 deletionsを発生させた。
- Cold EditMode suite内のBuilder実行後にも同じ4 tracked差分が残った。
- Deterministic: **NO**。
- Idempotent: **NO**。
- Production state reproducible without manual restore: **NO**。
- Validation action: Production修正は禁止されているため原因修正は実施せず、差分をvalidated commitへ復元してFAILを記録した。

## Gate 1 — Repository / Git State

- Branch: `feature/phase5a-authorship-identity`。
- Starting HEAD: `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`。
- Starting worktree: clean。
- Staged changes: `0`。
- Relevant untracked files: `0`。
- `phase5a-*` tag: `0`。
- Production Application commit: 履歴上確認済み。
- Verdict: **PASS**。

## Gate 2 — Production Diff Audit

### Approved Production Changes

- Logo identity: Pause UIへ承認済みwordmarkを適用。
- Chamber: Floor Base、Rear Support、Metal Outer Frame、抑制済みArc、Support Core、3 Indicator。
- Actor motif: Chest Strip、Surface Seal、Rear Tick、薄型Floor Segment。
- Goal: Locked White／VioletとUnlocked Lime state。
- Maintenance／Observation／Real Decal: 承認済みSection 3設備Identityとsurface配置。
- Audio: Human Listening済み8 cue。

### Supporting Production Infrastructure

- `Phase5AProductionApplicationBuilder`: approved sourceからProduction構成とcue setを生成するEditor入口。
- `Phase5AGoalIdentityVisual`: Doorの既存open-request状態を読み、Goal visual childを切り替えるPresentation adapter。
- Audio routing: generation別Spawn、Echo removal、Loop、Interaction、Battery、Section completion eventをapproved cueへ配線。
- Tests: approved composition、Actor placement、Goal Lime rule、audio hash／routing、evidence exclusion、P0～P2 hash、PlayMode lifecycleを検証。
- Docs: Human Review、Production Application、Formal Validationの境界と実測値を記録。

### Unexpected Application Changes

- ThirdParty: `0`。
- Packages: `0`。
- Project Settings: `0`。
- P0～P2 Scene: `0`。
- P3以外のScene: `0`。
- Echo以外のPrefab: `0`。
- 無関係Gameplay logic: `0`。
- Application commit自体のunexpected change: `0`。
- Verdict: **PASS**。

Builder再実行が生成するunexpected tracked diffはGate 4のFV-01として分離する。

## Gate 3 — Presentation Layer Ownership

### Goal

- Gameplay state owner: `DoorController`と既存Puzzle／Section progression。
- Presentation responsibility: `Phase5AGoalIdentityVisual`は`DoorController.IsOpenRequested`をread-onlyで確認し、Locked／Unlocked childのactive stateだけを切り替える。
- Goal unlock、Door unlock、Puzzle completion、Section progressionの決定処理は持たない。
- Verdict: **PASS**。

### Audio

- Gameplay state owner: 既存Loop、Interaction、Battery、Door、Section coordinator。
- Presentation responsibility: `Phase4FeedbackDirector`が既存eventを受け、`Phase4AudioController.Play`へcueを渡す。
- Audio componentからGameplay state、Battery ownership、Replay、Section completionを書き換える経路はない。
- Verdict: **PASS**。

### Actor / Chamber

- Actor motifは`P4 Robot Visual`配下のVisual Childであり、専用Collider／Rigidbodyを持たない。Gameplay root、movement、collision、Interaction、Replay transform、Echo stateを所有しない。
- Chamber／Maintenance／Observation／Decal rootにもCollider／Rigidbodyはなく、Gameplay geometryやPuzzle stateを所有しない。
- Verdict: **PASS**。

## Gate 4 — Builder Reproducibility

- Cold invocation: 実施。
- First execution exit code: `0`。
- Second execution exit code: `0`。
- Post-first-builder tracked diff: `4 files`。
- Post-second-builder hash stability: `1/4`のみ同一。
- Deterministic: **NO**。
- Idempotent: **NO**。
- Verdict: **FAIL — FV-01**。

## Gate 5 — Clean / Cold Validation

- Unity process before invocation: `0`。
- Builder、EditMode、PlayModeはそれぞれ新規batchmode processで起動。
- Production Scene load／compile／tests: Pass。
- Library完全削除は未実施。既存Editor sessionだけに依存しないcold process境界は確認した。
- Verdict: **PASS**。

## Gate 6 — EditMode

- Total: `149`。
- Passed: `149`。
- Failed: `0`。
- Skipped: `0`。
- Duration: `51.5333269 s`。
- Exact Logo dimensions: `Revision21LogoEvidenceContainsSeparateExact64AndFourTimesSamples` Pass。
- Locked Lime 0／Unlocked Lime: Production testとRevision 2.1 test Pass。
- Actor attachment／placement: Production test Pass。
- Audio existence／exact hash／routing: Production test Pass。
- Expected Production composition／evidence exclusion: Production test Pass。
- Verdict: **PASS**。

Test assertionsは全件Passしたが、suite内のBuilder実行がtracked assetをdirtyにする問題はFV-01として検出した。

## Gate 7 — PlayMode

- Total: `131`。
- Passed: `131`。
- Failed: `0`。
- Skipped: `0`。
- Duration: `39.2035666 s`。
- Verdict: **PASS**。

## Gate 8 — Full Gameplay Regression

- Player spawn／movement: Pass。
- Interaction: success `4`／failure `0`。
- Battery pickup／carry／insert: Pass。
- Echo spawn／replay／remove: Pass。
- Goal Locked／Unlocked: Pass。
- Door behavior: Pass。
- Section progression／final completion: Pass。
- P3全Section完走: Pass。
- Maximum Replay Drift: `0 m`。
- Verdict: **PASS**。

## Gate 9 — Scene Regression

| Scene | SHA-256 | Result |
|---|---|---|
| P0 | `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5` | unchanged |
| P1 | `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB` | unchanged |
| P2 | `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C` | unchanged |
| P3 approved state | `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854` | restored and verified |

- Verdict: P0～P2 **PASS**。P3 approved commit state **PASS**、Builder regeneration **FAIL**。

## Gate 10 — Visual Production Validation

- Evidence: `Captures/Phase5A/ProductionApplication/`。
- Graphics: Direct3D 11。
- Resolution: `1920 × 1080`。
- PNG: `9/9`、全件寸法一致。
- Actor: surface motifとCyan Player／Violet Echo識別を維持し、大型debug markerへ戻っていない。
- Locked Goal: Lime 0、White／Violet outlineで床から分離。
- Unlocked Goal: success時Limeが明確。
- Chamber: Metal Outer FrameがArc hierarchyを維持。
- Maintenance／Observation: 承認済みarea identityを維持。
- Real Decal: surface配置を維持し、giant boardへ戻っていない。
- Verdict: **PASS**。

Human Reviewの主観判定を再実施したものではなく、Formal Validation時点のProduction移植一致を確認した結果である。

## Gate 11 — Audio Production Validation

- Production WAV: `8/8`存在。
- Exact approved SHA-256: `8/8`一致。
- Missing: `0`。
- Wrong routing: `0`。
- `echo_spawn_01/02/03`: generation 1／2／3へ個別配線。
- `echo_remove`: oldest Echo removal eventへ配線。
- `loop_end`: Loop end eventへ配線。
- `interaction_success`: successful Interaction eventへ配線。
- `battery_insert`: Battery insertion eventへ配線。
- `section_complete`: Section／Game completionへ配線。
- Comparison／Preview WAVのProduction event参照: `0`。
- Human Listening: Revision 2.1で既にPassしており、再要求していない。
- Verdict: **PASS**。

## Gate 12 — Error / Warning Audit

Builder 2回、EditMode、PlayModeの全Formal logを監査した。

- Compiler errors: `0`。
- Compiler warnings: `0`。
- Missing Script: `0`。
- Missing Reference: `0`。
- NullReferenceException: `0`。
- Unhandled Exception: `0`。
- Assertion failure／AssertionException: `0`。
- AudioListener warning: `0`。
- Serialization error: `0`。
- Verdict: **PASS**。

## Gate 13 — Repository Hygiene

- Tracked Captures／Builds／Logs／TestResults／Library／Temp／obj: 各`0`。
- Tracked user-profile absolute path／username: `0`。
- Credential-like content in Phase 5A scope: `0`。
- TODO／FIXME／temporary debug text in Phase 5A scope: `0`。
- Evidence-only object／comparison audio in Production: `0`。
- Machine-specific setting／cache／dump: `0`。
- Largest changed Production fileはP3 Scene約2.63 MBで、承認済みSceneとして必要。8 WAVは約37～172 KB／fileで、巨大不要fileではない。
- Verdict: **PASS**。

## Gate 14 — Asset / License Audit

- Phase 5A固有Visual: project-owned Builderによるprocedural mesh／material／layout。新規外部assetなし。
- Environment／Robot underlying assets: Quaternius CC0 1.0。由来、公式配布元、archive hash、noticeを`Docs/ThirdPartyAssets.md`と`ThirdPartyNotices/`で追跡可能。
- Audio 8 cue: Kenney Sci-Fi Sounds CC0原音をproject-owned Builderでmix／pitch／filter／timing処理したderived WAV。Revision 2.1では`section_complete`終端だけを同じCC0 sourceから再構成。
- Kenney license notice: `ThirdPartyNotices/Kenney-SciFiSounds-CC0.txt`。
- ThirdParty tracked diff: `0`。
- Portfolio／公開Repository利用上のlicense blocker: `0`。
- Verdict: **PASS**。

## Gate 15 — Public / Portfolio Safety

- API key／token／private credential: `0`。
- Personal information／absolute private path: `0`。
- Copyright／license blocker: `0`。
- Crash dump／private screenshot／local cache／machine-specific config: tracked `0`。
- Generated Captureはignored evidenceでありcommit対象外。
- Verdict: **PASS**。

## Gate 16 — Final Git Audit

- Builder／testが生成した4 tracked差分は対象を確認後、validated commitへ復元した。
- Production implementation diff after cleanup: `0`。
- Formal Validationで追加するtracked change: 本文書のみ。
- Validation commitへProduction修正を混ぜない。
- Tag: 作成しない。

## Remaining P2

Human Reviewで許容済みの次のP2だけを維持する。

- Logo spacing／badge感。
- Chamber Violet密度。
- Observation text依存。
- Decal新品感／surface馴染み／small serial readability。

FV-01はP2 polishではなく、Formal Validationの再現性blockerである。

## Required Next Action

1. Production Visual／Gameplayを変えず、Builderが同一serialized outputを再生成するようにEditor infrastructureだけを修正する。
2. cleanなProduction commit相当からBuilderを最低2回実行し、tracked diff `0`と全対象hash安定を確認する。
3. EditMode内Builder testがworktreeをdirtyにしないことを回帰testへ追加する。
4. 全Formal Gateを再実行する。
5. Formal Validation PASS後、別工程でのみPhase 5A tagを作成する。

## Final Recommendation

**PHASE 5A FORMAL VALIDATION FAILED**

Phase 5A validated tagを作成してはいけない。
