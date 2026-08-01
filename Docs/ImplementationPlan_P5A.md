# Phase 5A Implementation Plan — Authorship & Identity Pass

## 目的

Phase 4.3で合格したCamera、HUD可読性、Character pose、Battery carry、Door、床配線を固定したまま、Human選定済みのECHO SHIFT固有Identityを独立Previewで確定する。Option Cの分割六角Sealを造形の基礎、Option Aの既存機能色・明瞭な日本語Copy・上昇Phase Tick Audioを機能層として統合する。Option Bは不採用であり、再比較しない。

## 固定境界

- 製品Scene `P0_ReplayLab`～`P3_PlayableGreybox`、Prefab、Stable ID、Collider、Puzzle座標、Section解法、Goal、Telemetryを変更しない。
- fixed tick、Replay、Echo、Interaction、CharacterMotor、Battery ownership、Socket、Door collider timingを変更しない。
- Phase 4.3のRobot animation構造、Camera framing、HUD情報量を変更しない。
- Windows graphical Player shutdown crashの調査・修正・証跡変更を行わない。
- Quaternius／Kenney／Noto Sans JPの原本を変更しない。新しい外部素材と画像生成AIは使わない。
- Previewは独立したEditor生成Scene／Assetと`Captures/Phase5A/`だけへ出力する。

## 基準状態

- Branch開始点: `feature/phase4-windows-exit-crash`
- 基準commit: `4d0269ef50b9e03d2712fc95596c7e0bfb227529`
- 作業branch: `feature/phase5a-authorship-identity`
- `phase4-validated`: 存在しない。
- P0 SHA-256: `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5`
- P1 SHA-256: `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB`
- P2 SHA-256: `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C`
- P3 SHA-256: `4ACA5F734DB1C754E7C237CA6EE819B30F8F50B53BB5C37298C31C92F36961E8`

## Human選定済み方向

- Primary shape: 分割六角Seal。
- Secondary shape: 欠けた円環。
- Tertiary shape: 3本の短いPhase Tick。
- Violetは施設Identity、CyanはCurrent Player／記録、AmberはBattery／操作対象、Limeは成功状態のみ、Redは失敗／閉鎖状態のみ、Whiteは文章／中立情報へ固定する。
- LogoはOption Cのsealを基礎に、既存Canonical表記`ECHO//SHIFT`単体で成立する横長版、正方形Icon版、単色版を作る。
- Echo ChamberはOuter Seal、3層Phase Arc、欠番Segment、3 Tick、小型Coreだけへ簡略化する。
- Goalの太陽型markを廃止し、入口側が欠けた床面Sealへ置き換える。
- EchoはViolet＋世代別Tick数／欠け位置、PlayerはCyan＋小型Seal、BatteryはAmber、DoorはNeutral＋状態色をPreview上で示す。
- 15種の施設Decalを最大1024 atlasへまとめる。巨大看板、浮遊看板、ランダム斜め壁、全床への同一Seal反復は禁止する。
- Section 3 Previewは左Maintenance Bay、右Observation Bay、中央Gameplay通路という用途差で非対称性を作る。
- UIはStable Keyを変えず、明瞭な日本語候補だけを表示する。
- AudioはOption A系の最大3音の上昇Phase Tickを基本とし、低い余韻はLoop／Section完了相当に限定する。

不採用要素はOption B全体、Coral／Mint、Relay Chevron、軌跡線反復、Option Aの大型黄色frame／黄色破片、太陽型Goal、Archive中心HUD語彙である。

## Preview出力

選定Revision 1案だけをD3D11のURP Editor render、1920×1080で出力する。

1. Revised Logo
2. Simplified Echo Chamber
3. Player／Echo Color Hierarchy
4. Door／Battery／Goal
5. Section 3全景
6. UI Copy
7. Decal close-up

Captureと`revised_audio_preview.wav`は`Captures/Phase5A/SelectedRevision/`へ保存し、Git管理しない。生成元Builder、SVG logo／decal atlas、独立Preview SceneはProject-owned assetとして追跡する。本番P3 Sceneへ適用しない。

## 検証

- Selected Revision Builderをbatchmodeで実行し、7 PNGが1920×1080、1 WAVが15秒以内であることを確認する。
- 15 decal、3 logo variant、簡略Chamber、Goal floor seal、color hierarchy、用途別Section 3 layout、UI copyを検証する。
- Production SceneのGit diff 0とP0～P3 SHA-256一致を確認する。
- Stable ID、Collider、Replay、P3自動完走、Drift 0.05m以内、Interaction失敗0の既存回帰を実行する。
- Text Catalog、Kenney／Quaternius原本、Phase 4 tag、shutdown evidenceのdiff 0を確認する。
- Capture、audio preview、Build、Log、Library、TestResultsが追跡対象外であることを確認する。

## 最終Human Reviewゲート

自動工程は選定仕様の成立、出力仕様、非侵襲性だけを確認する。Cの固有性、寄せ集め感の有無、Lime制限、Chamberの整理、Logo単体成立、Goal識別、無料Asset感の低減は最終Human Review対象である。承認まで製品Sceneへ適用せず、Phase 5 tagを作成しない。

## 実行結果

- Selected Revision Builder: Unity `6000.4.6f1`／URP `17.4.0`／D3D11、exit code 0。
- 出力: 1920×1080 PNG 7/7、10.000秒WAV 1/1、15 Decal atlas、Logo 3 variant。
- EditMode: 129/129 Pass。
- PlayMode: 130/130 Pass。
- P0～P3: 基準SHA-256一致、Production Scene／Gameplay Prefab差分0。
- 生成対象: Project-ownedなSelected Revision Material／SVG／hash manifest／Preview Scene／Builder／testのみ。
- 状態: 自動工程完了。Human Review待ち。本番P3への適用、tag作成、Phase 4 blocker変更は未実施。
