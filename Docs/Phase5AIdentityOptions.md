# Phase 5A Identity Options

## 状態

Phase 5AのHuman選定は2026-08-02 JSTに完了した。以下のA／B／C比較は選定に至った履歴として保持し、再比較や再選定には使用しない。正式方向はOption Cの造形を基礎とし、Option Aから既存機能色、明瞭な日本語Copy、上昇Phase Tick Audio方針だけを用途階層として統合する。Option Bは不採用である。

選定結果:

- 採用Shape: 分割六角Seal、欠けた記録環、欠番Segment、3層Phase Arc、短い3 Tick。
- 採用Color hierarchy: Violet=施設Identity、Cyan=Current Player／記録、Amber=Battery／操作対象、Lime=成功のみ、Red=失敗／閉鎖のみ、White=中立情報。
- 採用Copy／Audio: Option Aの明瞭さと上昇Phase Tickを基礎にする。
- 不採用: Option B、Coral／Mint、Relay Chevron、軌跡線反復、Option Aの大型黄色frame／破片、太陽型Goal、Archive中心HUD語彙。
- 次の成果物: `Captures/Phase5A/SelectedRevision/`の選定Revision 1案。
- 状態: 最終Human Review前。本番P3 Scene未変更。

共通の製品境界:

- `P0_ReplayLab`～`P3_PlayableGreybox`、Gameplay Prefab、Stable ID、Collider、Puzzle座標、Replay、Interaction、Camera framing、HUD情報量は変更しない。
- Option Sceneは`Assets/_Project/Scenes/Preview/Phase5A_Option_A|B|C.unity`へ分離する。
- Preview Actorは`P3_Echo.prefab`から`P4 Robot Visual`だけを抽出する。`CharacterMotor`、`LoopDirector`、`StableId`、`Rigidbody`、`Collider`を含まない。
- Echo Chamber、device motif、decal、copy、audioは採用後もVisual／Presentation層だけへ接続する想定で、Loop eventを変更しない。

## Option A — Phase Lattice

| 項目 | 内容 |
| --- | --- |
| Concept | 記録された時間を、欠けた二重輪と規則的な位相tickで可視化する精密な実験設備。 |
| 固有motif | 二重の欠けた時間輪、一定間隔のtick、左右でずれる短いindex bar。monochromeでも切れ目と線数で識別する。 |
| Palette | Background `#08121F`、Panel `#10283A`、Primary `#67E8F9`、Secondary `#FFD166`、Accent `#E8F3F5`。 |
| Logo | `ECHO SHIFT` wordmarkへ欠けた二重輪を組み合わせる。front／small／monochrome／emissionへ同じ輪郭を展開可能。 |
| Echo Chamber | 三層のphase ringとtickが記録信号を蓄積する中央装置。Goalより手前に置く場合も輪郭中心で、白い面を増やさない。 |
| Decal | `LATTICE`／`SYNC`系。logo、Section 01～03、hazard、echo test、route、maintenance、serial、caution、inspected、out of serviceの12種。 |
| UI copy | 「記録層 03」「軌跡を重ね、出口を解錠」「記録 02」「残響 02」「[E] 接続」「同期出口」「記録完了」。説明性を残しつつ記録語彙へ統一する。 |
| Audio | 1.00／1.26／1.50の上昇3音。high-pass、短いreverse tail、90 ms delayで明るいlattice signatureを作る。 |
| 強み | 時間・反復が最も直接伝わり、小型markやmonochromeでも構造が残る。既存cyan/orangeの機能色との互換性が高い。 |
| 弱み | 円とcyanはSF UIで一般的。適用量が多いと「計器盤」へ寄り、現在の装置群を均一化し直す危険がある。 |
| 実装コスト | 低～中。既存ring／emission／floor circuitを再利用しやすい。 |
| Steam／ES適性 | Steam thumbnailでは明快、ESではシステム説明との接続が強い。独自性は配置と欠けのgrammarを徹底できるかに依存する。 |
| 借り物感低減 | vendor meshへ共通のphase crestを重ねるため効果は中～高。 |
| 過剰演出リスク | 中。発光輪を増やし過ぎるとGameplay targetより目立つ。 |

Preview: `Captures/Phase5A/Options/A/`

## Option B — Afterimage Relay

| 項目 | 内容 |
| --- | --- |
| Concept | Playerの行動が残像へ受け渡され、次の役目へ進むrelay施設。時間そのものより「過去の自分との協力」を前面に出す。 |
| 固有motif | ずれた二本の軌跡、前方chevron、遅れて重なるframe。線のoffsetと方向性でmonochrome識別する。 |
| Palette | Background `#10131C`、Panel `#24273A`、Primary `#FF6B6B`、Secondary `#72F1B8`、Accent `#F6E7CB`。 |
| Logo | `ECHO SHIFT`へ二本のoffset traceとrelay chevronを付加する。small markは二本線＋矢頭へ簡略化できる。 |
| Echo Chamber | coral／mintのframeが時間差で奥へ送られるgate状構成。Actorの移動方向とEchoの引き継ぎを一枚で説明しやすい。 |
| Decal | `RELAY`／`TRACE`系。同じ12カテゴリを矢印・引き継ぎ線で構成する。 |
| UI copy | 「反復 03」「二つの残像へ役目を渡す」「走査 02」「残像 02」「[E] 引き継ぐ」「帰還点」「軌跡を接続」。 |
| Audio | 1.34／1.10／0.92の下降3音。band-limitしたtransient、2段delay、warm impact layerで受け渡しを表す。 |
| 強み | Player→Echo→Goalの順序と役割分担を最も物語的に示せる。coral／mintは現状のcyan/orange依存を崩し、比較時の差が大きい。 |
| 弱み | 暖色Player、寒色Echoという既存意味と再調整が必要。chevronを多用すると物流施設やレース表現に見える。 |
| 実装コスト | 中。Actor／Door／HUDへoffset grammarを個別調整する必要がある。 |
| Steam／ES適性 | Steamでは動きと協力が伝わりやすく、ESではReplayの役割分担を説明しやすい。静止logo単体の時間テーマはAより弱い。 |
| 借り物感低減 | 色とframe配置がvendor meshの印象を強く上書きするため高い。 |
| 過剰演出リスク | 中～高。方向線がGameplay配線や誘導線と競合し得る。 |

Preview: `Captures/Phase5A/Options/B/`

## Option C — Archive Seal

| 項目 | 内容 |
| --- | --- |
| Concept | Echoを複製ではなく、欠番を含む「保存された記録体」として扱う静かなarchive施設。 |
| 固有motif | 分割六角seal、欠けたindex、1～3本の不均一bar。色がなくてもsegment数と欠番で世代差を残す。 |
| Palette | Background `#11130F`、Panel `#242A24`、Primary `#B89CFF`、Secondary `#B7F36B`、Accent `#ECE7D9`。 |
| Logo | `ECHO SHIFT`へ六角sealと欠番indexを併置する。small／monochromeは6分割seal、emissionは欠番だけを点灯する。 |
| Echo Chamber | 六角sealを複数層で封緘する保管装置。記録数に応じてindexだけが増えるread-only visual responseを想定する。 |
| Decal | `ARCHIVE`／`INDEX`系。12カテゴリを保管番号、検品、欠番の語彙で統一する。 |
| UI copy | 「保管区画 03」「二つの記録体で封鎖を解除」「索引 02」「記録体 02」「[E] 照合」「搬出口」「記録を封緘」。 |
| Audio | 0.78／1.00／1.19の低い3音。low-pass body、抑制したreverse tail、140 ms単発echoで封緘感を作る。 |
| 強み | 静かで固有の世界観を作りやすく、六角形の世代差は既存Echo markerとも接続できる。UI／decalのvoiceが最も統一される。 |
| 弱み | archive語彙は保存・文書管理を連想し、即時の物理パズル感を弱める可能性がある。紫／黄緑は既存装置色との再整理が必要。 |
| 実装コスト | 中。copy reviewと既存color semanticsの調整が主になる。 |
| Steam／ES適性 | Steamでは雰囲気と固有性が強い。ESでは実装意図の説明が必要だが、借り物のSF施設との差は最も出やすい。 |
| 借り物感低減 | vendor形状を「保管システム」の記号と文言で再文脈化するため高い。 |
| 過剰演出リスク | 低～中。静かな一方、記号を増やし過ぎると意味不明な管理ラベルになる。 |

Preview: `Captures/Phase5A/Options/C/`

## 横断比較

| 観点 | A: Phase Lattice | B: Afterimage Relay | C: Archive Seal |
| --- | --- | --- | --- |
| 一読できる主題 | 時間／位相 | 過去との役割継承 | 記録／封緘 |
| Logo縮小耐性 | 高 | 中 | 高 |
| Character展開 | crestとring | traceとfin／chevron | index barとseal |
| Door／Battery展開 | seamと同期tick | 受け渡し方向 | seal keyとindex |
| HUD互換性 | 高 | 中 | 中 |
| 既存機能色との互換 | 高 | 中 | 中 |
| Steamの瞬発力 | 高 | 高 | 中～高 |
| ESの説明力 | 高 | 高 | 中 |
| 無料Asset感の低減 | 中～高 | 高 | 高 |
| Productionコスト | 低～中 | 中 | 中 |
| 過剰設計リスク | 中 | 中～高 | 低～中 |

## Preview deliverables

各Optionは同じ5構図と10秒audioを持つ。

1. `01_logo_echo_chamber.png` — LogoとEcho Chamber
2. `02_section3_overview.png` — Section 3 preview overview
3. `03_player_echo_motif.png` — Current Player、Echo、固有motif
4. `04_door_battery_decal.png` — Door、Battery、decal board
5. `05_hud_goal.png` — UI copyとGoal
6. `audio_preview.wav` — 44.1 kHz／16-bit／mono、10.000秒

出力はすべて`Captures/Phase5A/Options/{A|B|C}/`にあり、PNGは1920×1080、D3D11、Debug Overlay OFF、Cursor OFF。CaptureとWAVは比較用生成物でGit管理しない。

Audio実測値:

| Option | 長さ | Peak | SHA-256 |
| --- | ---: | ---: | --- |
| A | 10.000 s | 0.8800 | `A044CDA30BACCB8412242D081A5C0DD9DD6C590ECF2CD8CF6F5A9885A409C3F2` |
| B | 10.000 s | 0.8800 | `616100C0B6B125EAE5C4D7FABAF9C7A00021A8E7165B3F3453D3C8A737E48ADE` |
| C | 10.000 s | 0.8800 | `F2B13080EBD9E1487D55FBF8255C422092B04466ED621348674939A612B55D0F` |

## 自動検証

- Unity Editor: `6000.4.6f1`
- URP: `17.4.0`
- Builder: exit code 0、`PHASE5A_PREVIEWS_OK options=3;captures=15;graphics=Direct3D11`
- Phase 5A EditMode: 4/4 Pass
- EditMode全件: 124/124 Pass
- PlayMode全件: 130/130 Pass
- P3実Scene自動完走: Pass、Section 1～3完了
- Replay Drift最大値: `0 m`（許容値`0.05 m`以内）
- 正規解法Interaction: 成功4／失敗0
- Option Scene内のGameplay／Physics component: 0件
- P0～P3製品Scene: Phase 5A開始時SHA-256と一致
- Text Catalog、Phase 4 audio cue、ThirdParty原本、shutdown crash evidence: 変更0

自動検証が保証するのは、3案の成立、出力仕様、非侵襲性、既存Gameplay回帰である。Steam画像としての強さ、文言の好み、音の印象、過剰演出の有無、最終的な採用案はHuman Reviewでのみ決定する。

## Human選定項目

Human Reviewでは、次をA／B／Cごとに比較する。

1. 160～320px程度へ縮小したLogoがECHO SHIFTとして記憶に残るか。
2. Echo ChamberがLoop／Echoを説明し、GoalやGameplay targetを奪わないか。
3. Player、Echo、Door、Battery、decal、HUDが同じ作者の言語に見えるか。
4. 日本語copyが短く自然で、説明不足または過剰に詩的でないか。
5. 10秒audioが通知として明瞭で、Kenney原音の寄せ集めに聞こえないか。
6. Steam代表画像、ES技術紹介、実装費のバランスが目的に合うか。

HumanがA／B／Cまたは要素の組合せを選ぶまで、製品Sceneへの適用、Phase 5タグ作成、Phase 4 release blockerの変更は行わない。
