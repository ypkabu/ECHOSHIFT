# ECHO//SHIFT Visual Language

## Status

Phase 5A Human-selected direction。Selected Revision 1はAutomation Passed／Human Review Failed。Revision 2のHuman Review前であり、本番Sceneへは未適用。Option Cの静かな分割Sealを造形の基礎とし、Option Aの機能色・明瞭なCopy・Audioだけを用途別に統合する。Option Bは使用しない。

## Canonical identity

- Canonical表記: `ECHO//SHIFT`。既存Project表記との互換が必要な本文では`ECHO SHIFT`も許容する。
- Primary shape: 分割六角Seal。記録された時間単位を表す。
- Secondary shape: 欠けた円環。不完全な現在を表す。
- Tertiary shape: 3本の短いPhase Tick。最大3体のEchoを表す。
- Offset: 1～2px相当または小さな角度差だけを用い、時間のずれを表す。常時glitchは禁止する。
- 色に依存せず、Tick数、欠け位置、segment数を併用する。

## Color hierarchy

| 色 | 意味 | 平常使用 |
| --- | --- | --- |
| Violet | 施設Identity、Seal、Chamber、Decal | 許可 |
| Cyan | Current Player、記録中、通常の時間情報 | 許可 |
| Amber | Battery、操作可能物、注意対象 | 対象限定 |
| Lime | 接続済み、解錠済み、成功 | 平常使用禁止 |
| Red | Error、失敗、危険、閉鎖 | 状態限定 |
| White | 文章、中立情報 | 許可 |

全ObjectのViolet／Lime化、Red／LimeによるPlayer／Echo常時区別、Emission単独識別は禁止する。

## Logo system

- `ECHO//SHIFT`の`O`へ分割Sealを統合する。
- `O`の文字形を消さず、外周だけをSeal化する。`ECH + 記号 + SHIFT`と読ませない。
- 横長版、正方形Icon版、単色版を持つ。
- Echo Chamberや背景Assetなしで成立させる。
- 小型表示では細線を減らし、欠け位置と3 Tickを残す。
- 既存Fontだけを使い、新規外部素材、画像生成AI、既存作品の模倣を禁止する。

## Echo Chamber

構成はOuter Hex Seal、3層Inner Phase Arc、欠番Segment 1個、短いTick 3本、小型Core 1個だけとする。交差棒、垂下棒、均等な装飾突起、Actorを隠す前景部品、巨大矩形frameは禁止する。

Outer Frameは暗い金属筐体としてBase PedestalとRear Supportへ物理接続する。浮遊可能なのはOuter Frame内のHologram Arcだけとし、Coreも支持構造を持たせる。

状態はread-only visual responseとする。

- Echo 0: Outer Sealを微弱点灯。
- Echo 1～3: 対応するInner ArcとTickを順に追加。
- Loop終了: 欠番Segmentへ短い位相offsetだけを表示し、一周animationにしない。
- Gameplay状態、Collider、Stable ID、Replayへ書き込まない。

## Gameplay object identity

- Current Player: Cyanの短いPhase Tick、小型胸部Seal。全身Emission禁止。
- Echo 1～3: Violet系、Tick数1／2／3、世代別の欠け位置。既存Animation／Collider／Gameplay Rootは固定。
- Battery: Amberを維持し、小型刻印だけを追加。LimeはSocket接続後だけ。
- Door: Neutral body。閉鎖時は小型Red、操作可能時はAmber、解錠後だけLime。中央へ小型分割Sealを置く。
- Goal: 床面の分割Seal。入口側Segmentを欠けさせ、解錠時だけ補完してLimeにする。放射状太陽mark、高い棒、Doorを隠す装飾は禁止する。

## Decal system

最大1024×1024 atlas、生成元SVGを保持する。英語は施設／機器、日本語はHUD／操作へ分離する。

1. ECHO SHIFT facility seal
2. RECORD SECTOR 01
3. RECORD SECTOR 02
4. RECORD SECTOR 03
5. TEMPORAL HAZARD
6. ECHO TEST
7. PHASE SYNC
8. BATTERY BAY
9. EXIT GATE
10. MAINTENANCE
11. INSPECTED
12. OUT OF SERVICE
13. Equipment serial
14. Floor direction arrow
15. Missing index

壁面へ密着させ、環境Materialへ馴染ませる。新品の純白、巨大な黒い浮遊看板、全面反復、同一Camera内3個以上の同一Decalは禁止する。

異なる用途の文字を1枚の説明板へ集約しない。区画名、Rack名、検品Stamp、Serial、Sector、床矢印は対応する壁／機器／床面へ分離する。

## Section 3 functional asymmetry

- 左: Maintenance Bay、Battery rack、工具／機器box、小型Maintenance decal、壁接続Console。
- 右: Observation Bay、Chamber観察window／monitor、Record Sector表示、配線終端、広めの余白。
- 中央: Gameplay通路、Plate、Door、Socket、Goal、Actor。装飾を置かない。

外周Assetを床から浮かせず、壁を意味なく斜めにせず、黒背景へ孤立させない。同Prefabの均等配置ではなく用途密度で反復を崩す。Gameplay Transform／Collider／Puzzle解法／Camera framingは固定する。

## UI copy

Stable Text Keyを維持し、次の値を正式候補とする。

- Loop: `記録 02`
- Echo count: `エコー 2/3`
- Remaining: `残り 18.4`
- Section 3: `記録区画 03`
- Objective: `2体のエコーと協力して出口を開く`
- Normal interaction: `[E] 操作`
- Synchronization device: `[E] 同期`
- Battery pickup: `バッテリーを取得`
- Battery insert: `バッテリーを接続`
- Door: `出口ゲート`
- Goal: `回収ポイント`
- Section complete: `記録区画を突破`
- Game complete: `実験完了`

`索引`、`記録体`、`封緘`、`保管区画`、`セル移行`は操作説明に使わない。

## Audio identity

- Option A由来の短く明瞭な上昇Phase Tickを基本とする。
- 最大3音。Echo世代1／2／3で音数を1音ずつ増やす。
- 単純Pitch変更だけにせず、source layer、filter、短いdelayを変える。
- Option C由来の低い余韻はLoop終了／Section完了相当だけへ少量使う。
- UIとWorldを分離し、同時発音上限とGameplay timingを維持する。
- Kenney原本を上書きせず、Derived AssetをProject-owned領域へ分離する。
- Normalize後もclippingを発生させない。

## Explicitly rejected

Option B全体、Coral／Mint、Relay Chevron、軌跡線の床／壁反復、Option Aの大型黄色frameと黄色破片、太陽型Goal、Archive中心HUD語彙、同寸motifの全Object貼付、ランダム斜め壁、浮遊MAINTENANCE看板を使用しない。

## Selected Revision 2 implementation record

Revision 2 Previewでは、Canonical表記のO文字を残して外周Sealだけを重ねた。Chamberの金属Outer Frameは床BaseとRear Supportへ接続し、浮遊表現は内部Hologram Arcへ限定した。Actor motifは胸部／背面surfaceと床面segmentへ統合し、Goalは床埋め込みOuter Hex＋Inner 3 Segmentへ分離した。

MaintenanceはBattery rack、charging slot、conduit、service unit、tool box、wall consoleで構成し、Observationはwindow frame、record monitor、3 indicator、console、meter、cable terminationで構成した。Decalは黒い背景板を持たず、対応する壁、rack、機器、Door、床へ直接配置した。

これは自動構造検証済みのPreview仕様であり、Logoの一読性、Chamberの完成感、Actor motifの自然さ、Goal識別、Decalの馴染み、区画用途の伝達、Audio cueの識別はHuman Review pendingである。
