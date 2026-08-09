# Phase 5A Authorship Audit

監査日: 2026-08-02 JST
対象: commit `4d0269ef50b9e03d2712fc95596c7e0bfb227529`のP3／Phase 4.3 presentation
判定方法: Scene Builder、Prefab wrapper、Material、Text Catalog、Audio cue、Phase 4.3 Capture 8枚の読み取り。Visual／Gameplay実装は変更していない。

## 結論

Phase 4.3は可読性と機能説明には成功している。一方、床・外周・装置・発光・文言・音がすべて「同じ規則で丁寧に並んだ汎用研究施設」へ収束し、ECHO SHIFT固有の記憶／反復／時間差という題材が、Player／Echo marker以外へ十分展開されていない。Criticalは0、Highは2、Mediumは5、Lowは3と分類する。いずれもPhase 4の合格済み構造を変えず、Preview候補で比較できる。

## Concrete inventory

| 領域 | 現在の生成物／参照 | 監査所見 |
| --- | --- | --- |
| Scene shell | `Section 3 - 過去との協力/Environment/P4 Facility`、`External Floor x-z`、`External Wall side-z`、`External Column side-z`、`Facility Machine Bank` | `P4X_Floor_01`～`04`の交互反復と左右対称bankが規則的で、作者固有の焦点が弱い。 |
| Environment prefabs | `P4X_Wall_01/02`、`P4X_Column_01/02`、`P4X_Prop_01`～`07` | Quaternius wrapperは統一されているが、Section固有の用途や履歴を示す配置差が少ない。 |
| Player／Echo | `P4 Robot Visual/Quaternius Robot Model`、`Compact Visor`、`Chest Time Core`、`Player Circular Floor Marker`、`Echo Segmented Hex Marker`、`Echo Replaying Mark`、`Echo Stopped Mark` | 識別は合格済み。形状はvendor robot中心で、ゲーム固有motifが小型markerへ限定される。 |
| Door | `P4 Doorway Frame`、`External Door Frame`=`P4X_DoorFrame`、`P4 Split Door Visual Assembly`、`External Door Left/Right Panel`=`P4X_DoorPanel`、`Door Closed Center Emission`、`Door Circuit Symbol` | 左右分割とcyan/orange系統は明瞭。中央記号とframeが汎用的で、反復／記録の意味が弱い。 |
| Plate | `P4 Plate Visual/Plate Housing/Pressure Pad/Plate Corner 1..4`、`PlateAccent` | 機能は明瞭だが、四隅＋正方形という標準的な圧力板記号で、他作品との差が小さい。 |
| Battery | `P4 Battery Visual/Energy Core/Battery Endcap ±1/Battery Direction Key/Battery Insertion Tip`、`BatteryAccent` | Phase 4.3のcarry寸法は合格。円筒＋orangeという汎用energy-cell表現に留まる。 |
| Socket | `P4 Socket Visual/Socket Plinth/Socket Power Ring/Socket Recess/Socket Key` | Batteryとの対応は読めるが、project固有symbol languageがない。 |
| Goal | `P4 Goal Portal/External Goal Frame/Portal Rear Glow/Goal Vertical Particles/出口ゲート Sign`、`GoalAccent` | Exit readabilityは高いが、白面と汎用door frameが強く、ECHO SHIFTのクライマックスとしての固有性が弱い。 |
| Wiring | `P4 Floor Circuit/Circuit Segment`、Plate cyan／Socket orange | 床拘束と直角化は合格。色分けが一般的で、時間差やEcho系統を示すgrammarへ発展していない。 |
| HUD | `P4 Section Intro Panel`、Loop／Echoes／Timer／Carry、`Loop Timer Bar`、`P4 Pause Card`、`Selection Fill/Arrow` | 占有と階層は合格。角丸暗色panel＋cyan focusという既視感が残る。 |
| Materials | `FacilityPanels`、`FacilityDark`、`FacilityTrim`、`PlayerAccent`、`PlateAccent`、`BatteryAccent`、`GoalAccent`、`Echo1/2/3`、`DangerAccent`、`FacilityGlass` | 全装置を同じ金属／emission作法で揃えているため、意図的な“手がかりの偏り”が少ない。 |
| VFX | `P4_Pulse.prefab`／`P4_SoftPulse.mat`、`Phase 4 Feedback` pool、`Goal Vertical Particles` | 白飛びは修正済み。pulse／particleは汎用成功feedbackで、Echo固有の残像・欠落・位相差が弱い。 |
| Audio | `Phase4AudioCueSet`: `laserSmall_000`、`lowFrequency_explosion_001`、`impactMetal_001`、`doorOpen_001`、`forceField_000`、`forceField_004` | Kenney原音の直接割当が多く、個々の効果は分かるがゲーム固有の短い音型へ統合されていない。 |
| Text | `Phase3TextCatalog.asset`の`section1Name`／`section2Name`／`section3Name`、`section1Objective`～`section3Objective`、`interactKeyboard`、`loopLabel`、`timeLabel`、`echoLabel`、`batteryCarried`、`gameCompleted`、`paused`、`restartSection`、`quit` | 「セクション」「装置」「協力」「実験」など仕様説明語が多く、施設の作者性や時間テーマを示すvoiceが弱い。stable text keyは変更せず、値だけを候補比較する。 |
| Phase 4.3 captures | `01_player_idle.png`、`02_player_walk_turn.png`、`03_player_echo_pose.png`、`04_battery_carry_idle.png`、`05_battery_carry_walk.png`、`06_battery_insertion.png`、`07_split_door_open.png`、`08_two_echo_roles.png` | Character／Carry／Door／Wiringの可読性は合格。一方、8枚を横断する固有logo、Hero object、decal grammar、sonic identityはまだない。 |

## Severity findings

### High 1 — 固有のHero object／記号体系がない

`P4 Facility`の中心視線はPlate、Door、Socket、Goalへ分散し、タイトルと結び付く一つの形がない。`Echo Segmented Hex Marker`は有効だがActor足元だけに留まり、logo、Door、Battery、HUD、decalへ反復されていない。Steam画像の縮小表示で「ECHO SHIFTの一枚」と識別する根拠が弱い。

提案: 二重輪、欠けた位相、ずれたframe、記録軌跡のいずれかをHero Echo Chamberから全カテゴリへ展開する。Gameplay colliderを持たないVisual child／decalだけで実現する。

### High 2 — 固有のcopy／sonic signatureがない

`section3Name=セクション3：過去との協力`、`objective3=2体のエコーと協力して出口へ進む`、`batteryCarried=電池を保持中：紫の電源へ運ぶ`は正確だが、チュートリアル仕様書のvoiceに近い。Audioも`EchoSpawn=forceField_000.ogg`、`Door=doorOpen_001.ogg`など原音単発で、複数eventを束ねる3音以下の識別音がない。

提案: stable keyを変えずに短い日本語copyを三系統比較し、Kenney原本を非破壊でlayer／pitch／EQ加工した3音以下のmotifをOptionごとに提示する。

### Medium 1 — Section 3が均等密度で、Goalへの視線階層が弱い

`External Floor x-z`は4種類を規則交互に敷き、両側の`Facility Machine Bank`も同密度で続く。Goalだけを静かに見せる余白、観察室、保守帯の役割差が薄い。

### Medium 2 — Device silhouetteが共通primitive grammarへ寄り過ぎる

Plateの四角、Batteryの円筒、Socketの同心円、Doorの左右板は機能的だが、共通の“記録された時間”symbolを持たない。既存形状を崩さず、small crest／key／seam／decalで個性を足す余地がある。

### Medium 3 — Emission色が機能分類だけを担う

cyan=plate/Echo、orange=battery/socket、yellow=Playerは読める。一方、色以外の線数、切れ目、位相offsetがカテゴリ横断で統一されず、monochrome時に意味が失われやすい。

### Medium 4 — Goalの白面が最終到達点として平板

`Portal Rear Glow`と`出口ゲート Sign`は認識しやすいが、白い面の面積が大きく、echo記録の集積や完了を示す固有motion／symbolがない。Previewでは白面を増やさず、輪郭と負空間を比較する。

### Medium 5 — VFXとAudioが個別eventの通知に留まる

`P4_Pulse`と単発cueはGameplayを妨げないが、Loop End→Echo Spawn→Door/Goalを一連の“記録機構”として感じさせない。event timingは固定し、visual layeringと短い共有motifだけを候補化する。

### Low

1. `Section {number} Identity Edge {side}`は細い色線だけで、Section固有の履歴や用途を示さない。
2. `出口ゲート Sign`だけが大きなworld textとして残り、他のdevice grammarと表現方式が異なる。
3. Pauseの`Selection Arrow`とfacility motifが無関係で、UIとworldの作者性が分離している。

## 変更禁止対象の監査結果

- Section 3 Stable IDは`p3-s3-battery-3aaf6a48`、`p3-s3-socket-852fdd89`で、Option Previewは参照も複製もせず独立表示とする。
- `Gate A - Plate`、`Gate B - Battery`のDoorController／collider timing、PressurePlate、CarryableBattery、PowerSocket、GoalVolumeは変更しない。
- `P4 Robot Visual`のPhase 4.3 procedural pose、carry socket、split Door、floor circuitの本体実装は変更しない。
- Product Scene、Text Catalog asset、Phase 4 audio cue asset、ThirdParty原本、shutdown evidenceはPhase 5Aの書込み対象外とする。

## Phase 5Aで比較する問い

1. どのmotifが小型アイコン、monochrome、emission、Actor、Door、Battery、HUDへ最も一貫して展開できるか。
2. どのEcho ChamberがSection 3のHeroとしてGoalを奪わず、Loop／Echoを一目で語れるか。
3. どのcopyが日本語で短く自然であり、説明不足や過剰な詩的表現を避けられるか。
4. どの3音以下のsonic motifがKenney原音感を薄め、通知の明瞭さを維持できるか。
5. Steam thumbnail、ES技術紹介、production実装費、過剰演出リスクのどこにOptionごとのtrade-offがあるか。
