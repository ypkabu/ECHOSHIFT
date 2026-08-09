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

## Selected Revision 2 plan

Revision 1は2026-08-02 JSTのHuman ReviewでFailとなった。HighはLogo、Echo Chamber、Decal、Section 3 Identityの4件、MediumはActor motif、Goal識別、Audio cue証跡の3件。Color Hierarchyと日本語UI CopyはPassとして固定し、別案比較へ戻さない。

Revision 2は`Assets/_Project/Scenes/Preview/Phase5A_SelectedRevision2.unity`、`Assets/_Project/Art/Phase5A/SelectedRevision2/`、`Captures/Phase5A/SelectedRevision2/`だけへ生成する。

1. `ECHO//SHIFT`を全文字で残し、Oの外周だけを分割Seal化した背景非依存Wordmarkを作る。
2. Chamberを床固定Base、Rear Support、厚い金属Outer Frame、内部Hologram Arc、支持されたCore、埋め込みIndicatorへ再構成する。
3. Actorの突き出し記号を廃止し、Body surfaceへ密着したStrip／Seal／足元Segmentへ置換する。
4. GoalをPlateより大きい床埋め込み二重Sealとし、Socket形状を持たせない。
5. Decalを区画壁、Rack、機器角、装置側面、Door上、床へ用途別に分散し、巨大黒板を廃止する。
6. MaintenanceはRack／charging slot／conduit／service unit、ObservationはChamber向きframe／monitor／3 indicator／consoleを設備単位で構成する。
7. 8 cueの個別WAV、cue timing付きpreview、timelineを生成する。
8. 11枚の1920×1080 D3D11 Captureとcontact sheetを生成し、Human Review項目は未判定のまま停止する。

Production Scene、Gameplay Prefab、Stable ID、Collider、Replay、Text Catalog stable key、Phase 4 audio原本、ThirdParty原本、Windows crash証跡は変更しない。

## Selected Revision 2 execution result

- Revision 2 Builder: Unity `6000.4.6f1`／URP `17.4.0`／D3D11、exit code 0。
- 生成: 1920×1080 PNG 11/11、contact sheet 1/1、個別WAV 8/8、10.000秒preview 1/1、timeline 1/1。
- Audio: 個別cue 0.42～1.75秒、peak 0.80～0.84。Preview peak 0.86。clipping 0。
- EditMode: 135/135 Pass。PlayMode: 130/130 Pass。
- P3自動完走: Pass。最大Drift `0 m`。Interaction success 4／failure 0。
- P0～P3: 基準SHA-256一致。Production Scene、Gameplay Prefab、Runtime、ThirdParty、Windows crash証跡のGit差分0。
- Capture／Audio／Logs／TestResultsはgitignore対象、追跡ファイル0。
- 状態: Revision 2自動工程完了。Human Visual／Audio Review待ち。本番適用とtag作成は未実施。

## Selected Revision 2.1 plan

Revision 2は2026-08-09 JSTのHuman ReviewでVisual `FAIL / REVISION REQUIRED`、Audio `PASS WITH MINOR FIXES`となった。P0は0件、P1はActor motifのSurface統合、Locked／Unlocked GoalのGameplay距離での識別、正式Capture内の64px Logo証跡の3件である。Revision 2でPassした色階層、日本語UI、設備Identity、Decal配置方針は固定し、全面再設計を行わない。

Revision 2.1は`Assets/_Project/Scenes/Preview/Phase5A_SelectedRevision21.unity`、`Assets/_Project/Art/Phase5A/SelectedRevision21/`、`Captures/Phase5A/SelectedRevision21/`だけへ生成する。旧Revision 2 Scene、Art、Capture、Audio証跡は上書きしない。

1. ActorのChest Strip／Surface Seal／Rear TickをRobot surfaceへ埋め込み、Floor Segmentを床面へ薄く寝かせる。Gameplay Prefab、Actor Transform、Robot silhouette、Cyan／Violet分類は変更しない。
2. Locked GoalはViolet／White／value contrast／線幅だけで床模様から分離し、Limeを追加しない。Unlocked GoalはLimeを成功状態に限定したまま、perimeterと内部Segmentの変化面積を広げる。
3. 正式Preview内に64×64の実寸Icon、64px高の実寸wordmark、4倍inspectionを重ならないScreen-space領域へ配置し、RectTransform寸法とCapture解像度を自動確認する。
4. P1と干渉しない低リスクP2として、Chamber内部Arcの発光を抑えてOuter Frameを優先し、isometric Cameraだけを調整する。Observation monitorはlocal contrastとindicator visibilityだけを上げる。
5. Logo O spacingとDecalはP1可読性を悪化させる変更を避け、追加変更の効果が自動で保証できない場合は未修正理由を記録する。
6. Audioは`section_complete`だけの終端へmechanical transientを追加し、`echo_spawn_03`と近接比較できるR2.1専用WAVを生成する。旧8 cueは再生成しない。
7. D3D11／1920×1080で14枚の専用Captureを生成し、Actor、Goal、64px Evidence、Chamber、Observation、Decal、Gameplay距離を比較可能にする。
8. EditMode／PlayMode全件、P3完走、Drift、Interaction、compiler／Missing／Unhandled、P0～P3 hash、Production Scene／Prefab／Runtime／ThirdParty／Windows crash証跡の差分0を再確認する。

自動工程の完了状態は`READY FOR REVISION 2.1 HUMAN REVIEW`までとする。Production Application、Phase 5A正式Validation、tag作成へは進めず、Actor silhouette、Goalの一秒認識、64px perception、Chamber／Observation hierarchy、Audioイベント識別はHuman Review未判定として残す。

## Selected Revision 2.1 execution result

- Revision 2.1 Builder: Unity `6000.4.6f1`／URP `17.4.0`／D3D11、exit code 0。
- 生成: 1920×1080 PNG 14/14、contact sheet 1/1、比較用WAV 3/3、audio evidence 1/1。
- Audio: `echo_spawn_03` 1.050秒、`section_complete` 1.950秒、比較5.000秒。全ファイルpeak `0.8400`、clipping 0。
- EditMode: 143/143 Pass。PlayMode: 130/130 Pass。
- P3自動完走: Pass。最大Drift `0 m`。Interaction success 4／failure 0。
- compiler warning／error、Missing Script／Reference、NullReferenceException、Unhandled Exception: 0。
- P0～P3: 基準SHA-256一致。Production Scene、Gameplay Prefab、Runtime、ThirdParty、Windows crash証跡のGit差分0。
- Capture／Audio／Logs／TestResults／Libraryはgitignore対象、追跡ファイル0。
- Human Visual Review: **PASS WITH P2 POLISH**。Revision 2のP1 3件（Actor motif、Goal readability、64px Logo）はすべてClosed。Visual P0／P1は0件。
- Human Audio Review: **PASS**。人間が`spawn03_section_complete_comparison.wav`を実聴し、Echo Spawn 03とSection Completeの識別、Section Completeの上位event感、終端mechanical transient／confirmation tone、小音量での差を確認した。
- 状態: **Revision 2.1 Human Review Passed with P2 Polish／Production Candidate**。Revision 2.2はProduction Application前に不要。Production Application、Phase 5A正式Validation、tag作成は未実施。
