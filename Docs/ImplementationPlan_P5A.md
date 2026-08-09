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

## Approved Revision 2.1 Production Application plan

Human Review確定commit `156b28dd7f461928c88cacdad3d6ad6985240185`を基準に、Revision 2.1で承認されたIdentityだけを製品`P3_PlayableGreybox`へ移植する。本工程は新RevisionでもFormal Validationでもなく、P2 polish、追加Asset、Gameplay変更、tag作成を含めない。

### Source → Production対応

- Logo: `SelectedRevision2/P5A_R2_Logo_Wordmark.svg`の承認済み本体だけをProduction UIへ配置する。64px label、4x inspection、比較layout、annotationは除外する。
- Chamber: Revision 2のFloor Base／Rear Support／Metal Outer Frame／Support Core／3 Indicatorへ、Revision 2.1の抑制済みArc Material／寸法をそのまま適用し、Section 3のVisual-only環境rootへ配置する。
- Actor: Revision 2.1のChest Strip、Surface Seal、Rear Tick、薄型Floor SegmentをPlayer／EchoのVisual Childへ適用する。Cyan Player／Violet Echoを維持し、Gameplay Transform、Collider、Replay、pose制御は変更しない。
- Goal: Revision 2.1のLocked／Unlocked表現をVisual-only childとして適用する。Locked Lime Rendererは0、全Door解錠時だけ承認済みLime perimeter／center Segmentを表示する。
- Maintenance／Observation／Real Decal: 承認済み設備構成とRevision 2.1 monitor contrastだけをSection 3 Visual rootへ配置し、追加装飾やP2改善を行わない。
- Audio: 承認済み8 WAVをProduction audio folderへ複製し、Echo generation別spawn、Echo remove、Loop end、Interaction success、Battery insert、Section completeへ明示配線する。比較WAVはEvidence専用として除外する。

### 実装と検証

1. 再生成可能な`Phase5AProductionApplicationBuilder`を追加し、`P3SceneBuilder`のPhase 4適用後にだけ呼び出す。
2. Production-owned Material／Logo／Audio assetとVisual-only runtime adapterを追加する。ThirdParty原本、P0～P2 Scene、Project Settings、Packageは変更しない。
3. Builderをbatchmode実行してP3 Sceneを再生成し、Evidence-only object／audio 0、Missing 0、承認済み構成とaudio referenceを検査する。
4. EditMode／PlayMode全件、P3自動完走、Drift `0.05 m`以内、Interaction failure 0をSmoke Validationとして実行する。結果はFormal Validation完了と扱わない。
5. `Captures/Phase5A/ProductionApplication/`へProduction確認用9枚を生成し、Git管理しない。
6. Expected Production Application差分とUnexpected差分を分類し、Unexpected 0の場合だけ単一commit `feat: apply approved phase 5A identity to production`を作成する。tagは作成しない。

## Approved Revision 2.1 Production Application execution result

- Production Application Builder: Unity `6000.4.6f1`／URP `17.4.0`、exit code `0`。
- 適用: 承認済みLogo、Chamber、Actor surface motif、Locked／Unlocked Goal、Maintenance、Observation、Real Decal、Audio 8 cue。
- Evidence-only除外: 64px label、inspection／comparison UI、Preview Camera、comparison WAV、review helperはいずれもProductionへ未混入。
- EditMode: **149/149 Pass**。PlayMode: **131/131 Pass**。
- P3自動完走: Pass。最大Drift `0 m`。Interaction success `4`／failure `0`。
- P0～P2 Scene hash: 基準一致。P3は承認済みProduction applicationによるexpected差分だけ。
- compiler error／warning、Missing Script／Reference、NullReferenceException、Unhandled Exception: `0`。
- Production Capture: D3D11／1920×1080 PNG **9/9**。
- Unexpected差分: `0`。ThirdParty、Project Settings、Packages、Gameplay rulesは変更なし。
- 状態: **Production Application Passed／Ready for Phase 5A Formal Validation**。Formal Validationとtag作成は未実施。

## Production Builder determinism fix plan

Formal Validation commit `3ed79071cd8615cf4802054fc98424098f49534c`では、承認済みProduction状態から`P3SceneBuilder`を再実行した際にRobot Controller、Volume Profile、P3 Echo Prefab、P3 Sceneのtracked YAMLが再serializeされるため、Builder determinism／idempotenceだけがFailとなった。Visual、Audio、Gameplay、ThirdParty、Packages、Project Settingsは固定する。

1. 修正前に独立Unity processで3回再現し、4ファイルのexact hashとYAML差分を保存する。
2. fileIDとYAML document順を正規化した内容を比較し、semantic changeとserialization-only changeを分離する。
3. 承認済み4ファイルのexact serialized hashをEditor-only canonical stateとして検証する。全件一致時はPhase 4／P3の破壊的再生成経路へ入らず、assetをload、dirty、saveしない。
4. canonical stateでBuilderを3回呼んでも4ファイルがbyte-identicalであるEditMode回帰テストを追加する。
5. 独立batchmode processを最低3回起動し、各回exit code 0、Production asset diff 0、hash不変を確認する。
6. EditMode／PlayMode全件、P3自動完走、Drift、Interaction、compiler／Missing／NullReference／Unhandledを再検証する。
7. Production Visual、8 Audio cue、Runtime、ThirdParty、Packages、Project Settingsの差分0を監査し、Builder infrastructure、test、文書だけをcommitする。tagは作成せず、Formal Validationの判定は更新しない。

## Formal Revalidation execution result

- Validation start HEAD: `35bc519acd25fa010756c19a5daf4e0ff1ce6a62`。
- Gate 4監査でcanonical mismatch後にlegacy rebuildへ進む経路を発見し、isolated temporary fixture testを追加した。Production assetが一部でも存在するmismatch／partial-missingはwrite前にfail-closedする修正を`d471010a76f6de14bce42d13c136307b8a6403d3`へ分離した。
- Final tested HEAD: `d471010a76f6de14bce42d13c136307b8a6403d3`。
- Builder: same-process 3/3、cross-process 3/3でexit／test Pass、Production diff `0`、approved 4 hash一致、`writeCount=0`。
- EditMode: **151/151 Pass**。PlayMode: **131/131 Pass**。
- P3: 全Section完走、Interaction success `4`／failure `0`、maximum Drift `0 m`。
- Fresh Production Capture: D3D11／1920×1080、9/9生成・構成確認。
- Scene hash: P0～P3 approved hash一致。Audio: 8/8 approved hashとrouting一致。
- compiler warning／error、Missing、NullReference、Unhandled、Assertion、AudioListener、serialization error: `0`。
- Runtime、Scene、Prefab、Audio、ThirdParty、Packages、Project Settings unexpected diff: `0`。Generated tracking: `0`。
- Human Review: P0 `0`、P1 `0`を維持。承認済みP2 4件だけを残す。
- Formal Revalidation: **PASS**。初回FAIL記録は`Docs/Phase5AFormalValidation.md`へ保持する。状態は`READY TO CREATE PHASE 5A VALIDATED TAG`だが、本工程ではtagを作成しない。
