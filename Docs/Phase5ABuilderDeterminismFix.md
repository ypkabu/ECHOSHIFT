# Phase 5A Production Builder Determinism Fix

- Date: 2026-08-09 JST
- Branch: `feature/phase5a-authorship-identity`
- Starting commit: `3ed79071cd8615cf4802054fc98424098f49534c`
- Approved Production commit: `80dfa68bd2e5537794b5219e38c61d6bc379b8e8`
- Unity: `6000.4.6f1`
- Invocation: `EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine`
- Formal Validation status: **FAILEDのまま**。本工程はblocker修正だけであり、Formal Validationの再実行ではない。

## Original failure

承認済みProduction bytesから旧Builderを別Unity processで3回実行すると、毎回exit code `0`だがtracked Production asset 4件が変化した。Robot ControllerはRun 1後に安定したが、Volume Profile、Echo Prefab、P3 SceneはRunごとにexact hashが変化した。

| Asset | Canonical hash | Run 1 | Run 2 | Run 3 |
|---|---|---|---|---|
| Robot Controller | `EE885A3CE1AA515DC6C1EE40AEE629DA4E51CA9822C982A2D5335639E30D8C92` | `97F808F6F8F76E67C5BDDDA141A7B5538B2F2FFB69FAB326115404FED8E7E3AD` | same as Run 1 | same as Run 1 |
| Volume Profile | `D85DF4E9C7C20D6FADC2DB06899D4B6437E79EEF4BB9B65922257485B51C19C3` | `21753D9551A2D199C38DC508DF09CAC5874C87481CAFA4B412EA4AE93C4255CC` | `1C90AF9ADEC1B1C6DF6DBE51A55DF0E66CEA97EE9D08F32F4E4C08B620980140` | `26295E6A7305819B3FA365DF0855C9A802C51C14670E00D990CB0B5B19E506A2` |
| P3 Echo Prefab | `0FBB217DCA6A1D6132F17F1F4B512A83124A5C33D47D2F492A4BC08820903B52` | `8728D34F018D417EC664BB6D00DFF5C2064BD62638AB8BF3FD8C75AFBA4D9474` | `AEF4A2928D99F49065A6D65F76CCA02969D0F104EFAAF9EECF5DDDAD3C677AED` | `56F066B801E8BD88751A9E2448932D1BAF6B3B070E90E04AEE0DA9652925BA80` |
| P3 Scene | `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854` | `41B7500DFDC99DB001182B60AECFFB1C61B1D893445D7B3DFE12FDEFC37E2E29` | `14133A4EB95498646AAD171CD62522EB2DF359ECB4CA28E73461702332EE0EE9` | `99A7F4658C68C8A5FAA9865FCCD1B522DE6EF6B2C7E36A0A284ECC34E31003FE` |

保存したpre-fix patch sizeはRun 1 `4,102,215 bytes`、Run 2 `4,075,398 bytes`、Run 3 `4,197,647 bytes`。証跡はignoredな`Logs/Phase5A_BuilderDeterminism/`に置き、Git管理しない。

## Per-file YAML analysis

### Robot Controller

- Exact diff: 6 Animator Stateの`m_Tag`、`m_SpeedParameter`、`m_MirrorParameter`、`m_CycleOffsetParameter`、`m_TimeParameter`が、空値末尾空白なしから末尾空白ありへ変化した。
- Run behavior: Run 1だけhashが変化し、Run 2／3はRun 1と同一。
- Confirmed cause: `Phase4AssetBuilder.BuildRobotAnimatorController`が既存Controllerへ同じmotion／`writeDefaultValues`／default stateを再設定し、内容が同じでもControllerをdirtyにして保存する。Unity serializerが空文字列fieldを別表記で書き直す。
- Classification: **B — serialization-only**。

### Volume Profile

- Exact diff: `components`の4 reference、各Tonemapping／Bloom／ColorAdjustments／Vignette subassetのYAML anchor local fileID、document orderが毎回変化した。override型、順序、float値は同一。
- Run behavior: Run 1／2／3で別hash。
- Confirmed cause: `BuildVolumeProfile`が既存4 componentをlistから除去して`DestroyImmediate(..., true)`し、`profile.Add<T>`と`AddObjectToAsset`で毎回作り直すため、新しいlocal identifierが割り当てられる。
- Classification: **B — serialization-only**。

### P3 Echo Prefab

- Exact diff: GameObject／Transform／Renderer／MonoBehaviour document anchor、component reference、parent／child reference、YAML document orderが再採番された。name、GUID、Transform、material／script reference、serialized gameplay valueは同一。
- Run behavior: Run 1／2／3で別hash。
- Confirmed cause: P3 BuilderがPrefab contentsをsaveし、その後`Phase4SceneBuilder.PolishEchoPrefab`が`P4 Robot Visual`を`DestroyImmediate`して全Visual childを再生成し、同じPrefabへ`SaveAsPrefabAsset`する。
- Classification: **B — serialization-only**。

### P3 Scene

- Exact diff: Scene内GameObject／Component／Transformのlocal fileID、cross-reference、root／document orderが全面的に再採番された。name、GUID、Transform、Stable ID、serialized gameplay valueは同一。
- Run behavior: Run 1／2／3で別hash。
- Confirmed cause: `P3SceneBuilder.BuildScene`が毎回`NewScene(EmptyScene)`から全Hierarchyを新規作成し、Phase 4／5A visual rootsもdestroy／createして同じpathへ`SaveScene`する。Unityの新規object local IDはsession間のcanonical identifierではない。
- Classification: **B — serialization-only**。

### Semantic comparison method

各canonical fileとpre-fix Run 3を、YAML document anchorと全`fileID`をplaceholderへ置換し、行末空白を除去してline multisetを比較した。4件すべて一致した。GUID change、float drift、Quaternion／Transform微小値、timestamp、locale依存format、random gameplay valueは検出されなかった。既存Visual／Audio／Gameplay testも同じsemantic stateを確認した。

## Fix

`P3SceneBuilder.BuildScene`のwrite pathへ入る前に、承認済み4 assetのexact SHA-256を比較するEditor-only canonical guardを追加した。

- 4件がすべてbyte-identicalなら、既存P3 Sceneを必要時だけread-onlyで開き、Phase 4 asset build、Volume override再作成、Prefab save、Scene再生成、`SaveAssets`を実行しない。
- 1件でもmissing／mismatchなら、pathとexpected／actual hashをログへ記録し、従来bootstrap pathへ進む。dirty flagだけを抑える処理、実行後のcheckout／restore、`.gitignore`による隠蔽は行わない。
- `BuildScene()`を直接呼ぶ既存Editor testにも同じ境界を適用したため、test suite内の再実行もProductionをdirtyにしない。
- canonical hit時の明示markerは`PHASE5A_PRODUCTION_CANONICAL_OK assets=4;writeCount=0`。
- Visual、Audio、Runtime Gameplay、ThirdParty、Packages、Project Settingsは変更していない。

この修正は、承認済みProductionがBuilderの入力Source of Truthであり、Phase 5A identity適用時にPhase 4 Controller／Volume／全P3 Hierarchyを再作成する必要がないというwrite-scope修正である。既存stateの「近似比較」ではなくexact serialized bytesを比較するため、異なる状態を同一扱いしない。

## Determinism test

`Phase5AProductionApplicationEditModeTests.ProductionCommandLineBuilderIsByteStableForThreeConsecutiveRuns`を追加した。

1. 開始時に4件がapproved exact hashであることを確認する。
2. 同一Editor processでProduction command-line Builderを3回呼ぶ。
3. 各Run後に4件のexact SHA-256が開始snapshotと一致することを確認する。
4. 各Run後にcanonical guardが引き続き成立することを確認する。

Targeted result: `1/1 Pass`、failed `0`、skipped `0`。Full EditModeにも含まれる。

## Post-fix proof

### Same process

| Run | Exit／result | Production diff |
|---|---|---|
| 1 | Pass | `0` |
| 2 | Pass | `0` |
| 3 | Pass | `0` |

### Cross process

Unity／UnityCrashHandler residual process `0`から開始し、同じLibrary、同じsource、同じ4 assetを使用して3つの独立batchmode processを順番に起動した。Buildは作り直していない。

| Process | Exit code | Marker | Production diff |
|---|---:|---|---:|
| Run 1 | `0` | `assets=4;writeCount=0` | `0` |
| Run 2 | `0` | `assets=4;writeCount=0` | `0` |
| Run 3 | `0` | `assets=4;writeCount=0` | `0` |

3 Run後も4 exact hashはcanonical tableと一致し、residual Unity processは`0`。

## Production preservation

- P0: `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5`
- P1: `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB`
- P2: `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C`
- P3: `A1887A182ECD17E4E1712E2926765230DCB9D1F6DF9B71D748EBDE49A086B854`
- Logo、Actor、Locked／Unlocked Goal、Chamber、Maintenance、Observation、Decal: Production application EditMode tests Pass、asset diff `0`。
- Audio 8 cue: approved SHA-256 `8/8`一致、routing test Pass、WAV diff `0`。
- Player、Echo、Replay、Battery、Door、Goal、Section progression: PlayMode regression Pass、Runtime diff `0`。
- ThirdParty、Packages、Project Settings: diff `0`。

## Automated validation

- EditMode: **150/150 Pass**、failed `0`、skipped `0`、duration `6.3066577 s`。
- PlayMode: **131/131 Pass**、failed `0`、skipped `0`、duration `36.6229044 s`。
- P3 real Scene completion: Pass。
- Phase 4 presentation full P3 route: Pass。
- Interaction: success `4`／failure `0`。
- Maximum Replay Drift: `0 m`。
- compiler error／warning: `0`。
- Missing Script／Reference: `0`。
- NullReferenceException: `0`。
- Unhandled Exception: `0`。

## Diff audit

### Included

- Editor Builder infrastructure: `P3SceneBuilder.cs`、`Phase5AProductionApplicationBuilder.cs`。
- EditMode determinism regression: `Phase5AProductionApplicationEditModeTests.cs`。
- Plan／root-cause／evidence documentation。

### Excluded and unchanged

- Production Scene、Prefab、Controller、Volume Profile: final diff `0`。
- Runtime、Audio WAV、ThirdParty、Packages、Project Settings: diff `0`。
- Captures、Builds、Logs、TestResults、Library: tracked file `0`。
- `Docs/Phase5AFormalValidation.md`: FAIL判定を維持し、本工程では変更しない。
- Tag: 作成しない。

## Remaining risks and replacement condition

- Exact canonical hashesはApproved Revision 2.1 Productionに意図的に固定している。将来、Human承認済みProduction assetを正当に変更するmilestoneでは、新しい正式状態を検証したうえで4 hashを同じchange set内で更新する必要がある。
- missing／mismatch時のlegacy bootstrap pathは空Project生成用として残るが、その生成YAML自体のcross-session canonicalizationは本修正の保証対象外である。承認済みProductionからの再実行はexact mismatchを黙って無視しない。
- Phase 5A Formal Validationは過去結果を流用せず、次工程で全Gateを最初から再実行する必要がある。

## Git

- Planned message: `fix: make phase 5A production builder deterministic`
- Tag: `NO`

## Result

**READY TO RERUN PHASE 5A FORMAL VALIDATION**
