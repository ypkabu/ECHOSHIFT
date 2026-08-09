# Phase 5A Selected Revision 2

## Status

Revision 1 Human Review Failを受けた独立Preview。Production Applicationは開始しない。Color Hierarchyと日本語UI CopyはPassとして固定する。Revision 2の自動生成と回帰は完了した。2026-08-09 JSTのHuman ReviewはVisual `FAIL / REVISION REQUIRED`、Audio `PASS WITH MINOR FIXES`、Overall `FAIL / REVISION REQUIRED`となった。

確定状態:

- Selected Revision 1 Automation: Passed
- Selected Revision 1 Human Review: Failed
- Selected Revision 1 High Visual Issues: 4
- Selected Revision 1 Medium Issues: 3
- Selected Revision 2 Automation: Passed
- Selected Revision 2 Human Visual Review: Failed / Revision required
- Selected Revision 2 Human Audio Review: Passed with minor fixes
- Selected Revision 2 Overall Review: Failed / Revision required
- Revision 2.1: Recommended, not started
- Production Application: Not started
- Phase 5A Validation: Pending
- Windows Release Validation: Blocked

## Revision scope

- Logo Revision 2: 読める`ECHO//SHIFT`、O文字を残した分割Seal、背景非依存、64px確認。
- Echo Chamber Revision 2: 床固定Base、Rear Support、金属Outer Frame、内部Hologram Arc、支持Core、埋め込み3 Indicator。
- Actor motif: 突き出し棒を使わず、Body surfaceへ密着したCyan／Violet strip、世代別Tick、欠け位置。
- Goal Revision 2: Plateより大きい床埋め込みOuter Hex Ring＋Inner 3 Segment、入口欠け、解錠時だけLime。
- Real Decal Pass: 区画、Rack、機器、装置、Door、床へ用途別配置。巨大黒板なし。
- Section 3 Revision 2: MaintenanceとObservationを2～3部品から成る設備単位で区別する。
- Audio Evidence: 8個別cue、10.000秒preview、cue開始／終了timeline。

## Generated evidence

- Unity: `6000.4.6f1`、URP `17.4.0`、D3D11。
- Builder: exit code 0、`PHASE5A_SELECTED_REVISION2_OK captures=11;cues=8;graphics=Direct3D11`。
- Capture: 1920×1080 PNG 11/11、contact sheet 1/1。
- Audio: 44.1 kHz／16-bit／monoの個別WAV 8/8、preview 1/1、timeline 1/1。
- 個別cue duration: 0.42～1.75秒。個別cue peak: 0.80～0.84。
- Preview: 10.000秒、peak `0.8600`、SHA-256 `1771DBD230769F659892C2ACAEF839FE2087406400A38831E174F613E37C325C`。
- 8 cueとpreviewはすべて異なるSHA-256。PCM sampleはpeak 0.9未満でclipping 0。

出力先は`Captures/Phase5A/SelectedRevision2/`、Audioは同フォルダの`Audio/`。Capture／WAV／timelineはGit管理しない。

## Automated gates and results

- 11 PNG、1920×1080、D3D11、Cursor／Debug Overlayなし。
- Contact sheet 1枚: 生成済み。
- 個別WAV 8、preview 1、peak 0.89以下、clipping 0: Pass。
- Preview SceneのCollider、Rigidbody、Stable ID、LoopDirector、CharacterMotor: 0件。
- EditMode: 135/135 Pass、failed 0、skipped 0。
- PlayMode: 130/130 Pass、failed 0、skipped 0。
- P3自動完走: Section 1～3 Completed、`ADVANCES=1063`。
- Replay Drift最大値: `0 m`、許容値0.05 m以内。
- 正常解法Interaction: success 4／failure 0。
- C# compiler warning／error、Unhandled、NullReference、MissingReference: 0。
- Production Scene Git差分: 0。P0～P3 SHA-256は以下の基準値と一致。
  - P0: `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5`
  - P1: `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB`
  - P2: `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C`
  - P3: `4ACA5F734DB1C754E7C237CA6EE819B30F8F50B53BB5C37298C31C92F36961E8`
- Gameplay Prefab、Runtime script、ThirdParty原本、Windows crash証跡のGit差分: 0。
- Capture、Builds、Logs、TestResultsの追跡ファイル: 0。

EditModeのScene Builder testが一時的に変更する既知の4ファイルは、結果取得後に基準HEADへ復元してからProduction hashとPlayModeを確認した。

## Human Review criteria

以下は自動Passにせず、2026-08-09 JSTのReviewで個別判定した。

- Logoが一読で`ECHO//SHIFT`と読める。
- Chamberが浮遊Primitiveに見えず、物理部品が接続されている。
- Actor motifがアンテナやDebug markerに見えない。
- GoalがPlate／Socketと区別できる。
- Decalが看板やDebug UIに見えない。
- Maintenance／Observationの用途が画像だけで分かる。
- 無料Asset＋自動配置感が減っている。
- Audio cueが用途ごとに識別できる。

判定結果は次節と`Docs/Phase5ARevision2HumanReview.md`へ記録する。

## Human Review result

詳細は`Docs/Phase5ARevision2HumanReview.md`を参照。

- P0: 0件。
- P1: 3件。Actor motifの突出／浮遊、Goalの通常状態とGameplay距離での認識不足、64px Capture証跡欠落。
- P2: Logo spacing、Chamber密度、Observation主役の弱さ、Decalの馴染み、Capture angle、Audio cue差別化。
- Lime rule: 成功／接続／解錠状態だけに使用され、装飾使用は確認されなかった。
- Production Application allowed: **NO**。
- Final recommendation: **CREATE REVISION 2.1**。

状態: **Revision 2 Automation Passed／Human Review Failed／Revision 2.1未着手／Production Scene未変更**。
