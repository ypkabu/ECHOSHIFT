# Phase 5A Selected Revision

## Human decision

2026-08-02 JST、Human選定完了。Option Cの分割六角Seal、欠けた記録環、欠番Segment、3層構造、Seal／Index系施設Decal、用途に基づく静かな非対称性を採用する。Option Aからは既存Cyan／Amber中心の機能色、明瞭な日本語Copy、上昇Phase Tick Audio方針だけを採用する。Option Bは不採用であり、再比較しない。

これはA／B／Cのランダムmixではない。形状は施設Identity、色はGameplay state、Copyは操作理解、Audioは短いfeedbackという用途階層へ分離して統合する。

## Selected Revision deliverables

本番Sceneへ反映する前の独立Preview 1案をUnity `6000.4.6f1`、URP `17.4.0`、D3D11で生成した。

- `01_revised_logo.png`
- `02_simplified_echo_chamber.png`
- `03_player_echo_hierarchy.png`
- `04_door_battery_goal.png`
- `05_section3_overview.png`
- `06_ui_copy.png`
- `07_decal_closeup.png`
- `revised_audio_preview.wav`

出力先は`Captures/Phase5A/SelectedRevision/`。Capture／WAVはGit管理しない。生成元SVG、Material、Builder、独立Preview Sceneだけを追跡する。

Builder結果: `PHASE5A_SELECTED_REVISION_OK captures=7;decals=15;graphics=Direct3D11`、終了コード0、compiler warning／error 0。PNGは7/7が1920×1080。Audioは44.1 kHz／16-bit／mono、10.000秒、peak `0.8600`、SHA-256 `E3DB1D1DB95EEB4DC9BA88426F89A28036AA33B408BDE4DD171345AE5E27C834`。

## Automated validation

- EditMode全件: 129/129 Pass。
- PlayMode全件: 130/130 Pass。
- Selected Preview Scene内のCollider、Rigidbody、Stable ID、LoopDirector、CharacterMotor: 0件。
- 15 Decal、3 Logo variant、7 Capture、1 Audio preview: 生成済み。
- P0～P3 SHA-256: Phase 5A開始時の4値と一致。生成時hash manifestをProject-owned assetとして保持する。
- Gameplay Prefab、Stable ID、Collider、Text Catalog stable key、Phase 4 audio原本、ThirdParty原本、Windows crash証跡: Git差分0。
- Production Scene: Git差分0。EditMode内で既存Scene Builderテストが作る一時差分は検証後に基準HEADへ復元した。
- Capture、WAV、Logs、TestResults、Library: Git追跡対象外。

自動検証は出力数、解像度、構造、非侵襲性、既存回帰だけを保証する。Logoの記憶性、Cの固有性、寄せ集め感、Chamberの造形品質、Goal識別、無料Asset感の低減はHuman Reviewでのみ確定する。

## Frozen production scope

- `P0_ReplayLab`～`P3_PlayableGreybox`
- Gameplay Prefab、Stable ID、Collider、Puzzle座標、Replay、Interaction
- Character animation、Camera framing、HUD情報量、Text Catalog stable key
- Quaternius／Kenney／Noto Sans JP原本
- Windows graphical Player shutdown crashのcode／evidence

PreviewはGameplay component、Collider、Rigidbody、Stable IDを持たず、製品Sceneをロード／保存しない。

## Human Review gate

- Cの固有性が残っている。
- A／B／Cの寄せ集めに見えない。
- Limeが成功状態だけに限定される。
- UIが理解できる。
- ChamberがPrimitiveの乱雑な集合に見えない。
- 浮いた壁／看板がない。
- Logoが装置なしで成立する。
- Goalが太陽markではない。
- Gameplay対象が明確で、無料Asset感が減っている。

## Human Review result

2026-08-02 JST、Revision 1 Human ReviewはFail。

- High 4件: Logo、Echo Chamber、Decal、Section 3 Identity。
- Medium 3件: Player／Echo motif、Goal識別、Audio cue証跡。
- Pass: Color Hierarchy、日本語UI Copy、Audio技術仕様。
- Production Application: 未開始。

Revision 1は比較証跡として保持し、本番へ適用しない。次の判定対象は独立したRevision 2 Previewである。

状態: **Selected Revision 1 Automation Passed／Human Review Failed／Production Scene未変更**。

## Revision 2 handoff

Revision 1のHigh 4件とMedium 3件を対象にした独立Revision 2 Previewを生成した。Color Hierarchyと日本語UI CopyのPass結果は維持し、Production Sceneへは適用していない。

- Builder: exit code 0、D3D11、11 Capture、8個別cue。
- EditMode: 135/135 Pass。
- PlayMode: 130/130 Pass。
- P3: 全Section完走、Drift `0 m`、Interaction success 4／failure 0。
- P0～P3 hash: 基準値一致。Production Scene差分0。
- Human Visual／Audio Review: 未判定。

詳細とHuman判定項目は`Docs/Phase5ASelectedRevision2.md`へ分離した。
