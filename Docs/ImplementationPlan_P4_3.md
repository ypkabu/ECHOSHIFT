# Phase 4.3 Implementation Plan — Character and Device Presentation Pass

## 目的と完了条件

Phase 4.2で合格したCamera、HUD、Pause Menu、Actor識別、Capture構図を維持し、Human Visual Reviewで残ったCharacter／Battery／Door／Floor Wiringの見た目だけを改善する。Gameplay Transform、Collider、Stable ID、固定tick、Replay、Interaction、Puzzle座標、Section構造は変更しない。

完了条件は、Idle／Walk／Carry Idle／Carry Walk／Interact／Echo Stoppedが画面上で区別できること、Batteryが手元へ収まること、床配線が床面に接続された直角経路として読めること、Doorが巨大な一枚板に見えないこと、D3D11 Capture 8枚と30秒Presentation Probeが生成できること、および全回帰・Performance・Standalone Buildゲートを通過することである。

## 利用可能なAnimationと採用判断

- 原本: `Assets/_Project/ThirdParty/Quaternius/AnimatedRobot/Robot.fbx`
- FBX内には単一の`RobotArmature`／`MultiTake`スタックしかなく、Idle、Walk、Carry、Interact、Stoppedへ分割された名前付きClipはない。
- Importerは`importAnimation: 0`、`animationType: 0`、Avatarなしであり、既存のCC0原本・metaを変更してClipを推測分割することは再現性と第三者原本隔離規則に反する。
- したがって、Project-owned wrapperの既存骨（Upper／Lower Arm、Upper／Lower Leg、Head、Torso）へVisual-only procedural poseを適用する。
- Animatorは`P4 Robot Visual`配下にのみ置き、Root Motion OFF、Animation Event 0件、Gameplay曲線0件のProject-owned controllerで6状態を明示する。短いBlendと実際の骨Poseは専用componentが担当する。

## RootとVisualの分離

- `Player`／`Echo`のGameplay rootはCharacterMotor、Collider、Rigidbody、LoopActor、Interactor、Replayの所有者として維持する。
- `P4 Robot Visual`のみが拡縮・Pose・Emissionを受ける。
- 移動状態はGameplay rootのフレーム間の表示上の位置差から読み取り、fixed tick、入力、Motor、Replay dataへ書き戻さない。
- Carry状態は既存Interactorの読み取り専用状態、Interactは既存成功カウンタ、Echo Stoppedは既存Playback tickから決定する。
- Animator、Pose controller、骨、表示markerはColliderやStable IDを持たない。

## Robot状態とPose

| 状態 | 表示 | 遷移 |
| --- | --- | --- |
| Idle | 腕を体側へ下ろし、弱い呼吸／頭部変化 | 0.12秒 |
| Walk | 距離駆動の左右腕脚スイング | 0.10秒 |
| Carry Idle | 両腕を胸前Batteryへ向ける | 0.12秒 |
| Carry Walk | Carry腕を維持し、脚を小さく振る | 0.10秒 |
| Interact | 既存Interaction成功時に0.32秒の短い腕動作 | 0.08秒 |
| Echo Stopped | 通常Idleと異なる非対称な停止姿勢 | 0.14秒 |

Walk phaseは移動距離から進め、Root MotionとAnimation Eventは使用しない。Visual Child以外のTransformは一切変更しない。

## Battery比率とCarry Pose

- Battery Visual全長: Robot表示高の約36%。
- Battery Visual直径: Robot胴体幅の約44%。
- 原本のBattery root、Collider、Stable ID、初期位置、reset位置は維持する。
- 表示meshだけを縮小し、前後を示すkeyed tipを追加する。
- 既存Carry SocketはInteractorの所有関係を変えず、胸／腹部前方へ表示が収まる位置へ調整する。PlayerとEcho prefabで同一ローカル値を使用する。
- Carry Idle／Walkでは両Upper／Lower ArmをBattery方向へ寄せる。
- Socket挿入のGameplay確定tickは維持し、Battery Visualだけを0.25秒で確定位置へ補間する。

## Door Visual

- DoorController、open source、open offset、Collider、1-tick確定規則は変更しない。
- Visual assemblyをDoor gameplay rootの移動から分離し、閉位置に固定したDoor Frameの子として保持する。
- 一枚panelを左右2枚へ分割し、0.10秒の準備点灯後、0.35秒でFrame左右へ収納する。
- 閉鎖は逆再生し、中央seamとFrame内側のEmissionで状態を示す。
- Plate Doorはcyan単線、Socket Doorはorange二重線の記号を維持する。

## Floor Wiring

- P3が生成するLineRendererはPhase4SceneBuilderで生成元からProject-owned thin meshへ置換する。
- 高さは床面から0.012m、幅は0.06m、経路は既存start／corner／endの直角gridを維持する。
- Plate系cyan、Socket系orangeを維持し、Collider／Trigger／Raycast対象を追加しない。
- Actor通路またはDoor正面を横切る区間は、端点近傍から外側laneへ寄せてからDoor Frameへ接続する。
- Builder再実行時は既存`P4 Floor Circuit`を削除して再生成し、LineRendererと重複meshを残さない。

## Scene Builder再現性

- Robot controller／Pose参照、Carry Socket、Battery visual、split Door、floor meshを毎回同一名・同一値で生成する。
- P4 wrapper・material・controller／clipを決め打ちAsset pathで更新し、重複を作らない。
- Quaternius／KenneyのThirdParty原本は変更しない。
- P0～P2 Scene hash、P3 Stable ID／Collider／Puzzle座標／Replay pathを前後比較する。

## CaptureとPresentation Probe

- `Captures/Phase4_3/`へD3D11、1920×1080、Debug Overlay OFF、cursor OFFで8枚を生成する。
- Idle、Walk、Player／Echo pose差、Carry Idle、Carry Walk、Battery挿入、Door開放、Echo 2役割を別構図で記録する。
- `-nographics`は使用しない。
- 正規Replay routeを使うrendered Standalone probeで最低30秒間、Idle→移動→Echo→Battery→Socket→Door→Stoppedのtimestampとscreenshotを記録する。

## Replay／Gameplay回帰リスク

- Visual poseがGameplay rootへ伝播するリスク: component参照とテストでVisual descendantへの書込みだけに限定する。
- Carry Socket調整がinteraction結果を変えるリスク: Stable ID解決・Interaction rangeはActor position基準のまま。成功4／失敗0と自動完走で検証する。
- Door visual分離がCollider状態を隠すリスク: DoorController／Collider hash、1-tick規則、open／close表示同期を別々に検証する。
- Scene Builderの再実行がduplicateを作るリスク: 同名rootの削除と2回生成比較テストを追加する。
- procedural poseのframe依存がReplay driftへ影響するリスク: Gameplay Transform前後不変とDrift 0.05m以内を検証する。

## 変更予定ファイル

- `Assets/_Project/Scripts/Runtime/Presentation/Phase4RobotPoseController.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4ActorVisual.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4BatteryVisual.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/DoorVisualFeedback.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4FloorCircuitVisual.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4PerformanceProbe.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4PresentationProbe.cs`
- `Assets/_Project/Scripts/Runtime/Presentation/Phase4CapturePreset.cs`
- `Assets/_Project/Scripts/Editor/Phase4AssetBuilder.cs`
- `Assets/_Project/Scripts/Editor/Phase4SceneBuilder.cs`
- `Assets/_Project/Scripts/Editor/Phase4CapturePipeline.cs`
- Phase 4.3 EditMode／PlayMode tests
- `Docs/ADR/0018-visual-only-character-animation.md`
- `Docs/ADR/0019-door-visual-separation.md`
- `Docs/Phase4CharacterPresentationValidation.md`および関連Validation／Architecture／TestPlan文書
- Builderが再生成するP3 Scene、Actor prefab、Project-owned presentation assets

## Test方針

- EditMode: FBX原本不変、Animator配置／Root Motion／Event／state、骨参照、visual-only曲線、Battery比率、Carry Socket一致、Door split、wiring height／width／直角、duplicate 0、Collider／Stable ID／hash不変、capture presetを検証する。
- PlayMode: 6状態遷移、移動距離に応じたWalk、Carry pose、Interact時間、Echo stopped、Gameplay Transform不変、挿入補間、Door 1-tick／open／close visual、配線状態、Builder生成実Scene、P0～P3統合、自動完走、Drift、Interaction、Missing／Exceptionを検証する。
- Performance: Main／Frame／GC／Animator(Pose)／Camera／UI、最大Echo 3体、Door transition spike、Carry transition spikeを計測する。steady GC 0 B/frameと60fps目標を維持する。
- Build: Windows x86_64 Development Build、warning／error 0、P3完走、Telemetry、Pause／Quit、自然終了code 0を確認する。

## 明示的な非対象

Camera、HUD、Pause Menuの再設計、Puzzle Layout、Section構造、Gameplay座標、Replay仕様、Interaction仕様、Collider、Stable ID、新規Gameplay system、敵AI、環境asset追加、production animation pipeline、Root Motionは変更しない。

## 実施結果

2026-07-23に自動ゲートを完了した。

- Quaternius FBXの未定義`MultiTake`は採用せず、Visual Child配下のProject-owned boneへ6状態のprocedural poseを実装した。Root Motion、Animation Event、Gameplay Transform curveは使用していない。
- Battery Visualは全長0.78m、直径0.44mへ縮小し、Player／Echo共通の胸前Carry Socketと両腕Poseを使用する。
- 旧LineRenderer配線は床上0.012m、幅0.06mの直角meshへ置換した。
- Doorは左右分割panelをframe内へ格納するVisualへ変更し、Gameplay root、Collider、1-tick規則は維持した。
- Scene Builder再実行、EditMode 116/116、PlayMode 127/127、P0～P3実Scene、P3全Section、Drift 0m、Interaction 4成功／0失敗がPassした。
- 最大Echo 3体・600 frameで平均8.342ms、p95 8.388ms、steady GC 0 B/frameを記録した。Door/Loop transitionの単発最大は18.038msであり、16.6msを1.438ms上回るが継続負荷ではない。
- D3D11 1920×1080 Capture 8枚と、通常描画30.025秒Presentation Probeの9状態画像を生成した。
- Windows x86_64 Development Build、P3完走、Telemetry、Pause／Quit、自然終了code 0を確認した。

自動化は状態、参照、投影範囲、寸法、性能、回帰を確認した。足滑りの知覚、Carry Poseの自然さ、Door動作の自然さ、代表画像としての完成度はHuman Visual Review未確認であり、Pass扱いしない。
