# 外部素材一覧

取得日: 2026-07-21 (JST)

この文書では、ECHO//SHIFTへ取り込んだ外部素材の原本、公式配布元、ライセンス、選定理由、変更内容、配布アーカイブのハッシュを記録します。`ExternalDownloads/`の配布アーカイブと展開物はGit管理外とし、ゲームのビルドに必要なファイルだけを`Unity/Assets/_Project/ThirdParty/`へ保存します。原本は直接編集せず、Material、Prefab、音響設定などの加工物は`Unity/Assets/_Project/Art/`と`Unity/Assets/_Project/Audio/`へ生成します。

## Quaternius — Modular Sci-Fi MegaKit, Standard FREE

- Asset名: Modular Sci-Fi MegaKit `[Standard]`
- 制作者: Quaternius (`@Quaternius`)
- 公式配布元: <https://quaternius.com/packs/modularscifimegakit.html>
- 公式ダウンロード: <https://quaternius.itch.io/modular-sci-fi-megakit>
- 取得日: 2026-07-21
- 配布版: Standard FREE、46 MB、upload id `13454739`、2025-04-22版
- ライセンス: CC0 1.0 Universal / Public Domain Dedication
- 商用利用: 可。公式ページに個人・教育・商用Projectで無料利用できる旨が記載されています。
- ビルドへの同梱: 可。選定したモデル・Textureを変換してゲームへ同梱できます。
- 再配布条件: CC0に帰属表示義務はありません。ECHO//SHIFTでは由来と原文を任意表示として残し、配布アーカイブ全体は再配布しません。
- 生成AI: 公式itch.io配布ページのContent欄は`No generative AI was used`と明記する。
- 元の配布アーカイブ: `Modular SciFi MegaKit Standard.zip`
- 配布アーカイブのSHA-256: `6FAE60CF5189E44DFF0BD91097F094A765ACC6D57D64A85A0CC0DD56E03035E3`
- ライセンス原文: `ThirdPartyNotices/Quaternius-ModularSciFiMegaKit-CC0.txt`
- 読み込み先: `Unity/Assets/_Project/ThirdParty/Quaternius/ModularSciFiMegaKit/`
- Unity側の変更: FBXのMaterial Importを無効にし、Mesh Compression Medium、Read/Write OFF、Rig None、Camera／Light／Animation Import OFFとします。Textureは最大2048、Normal Mapを明示し、プロジェクト所有のURP/Lit Materialから参照します。表示用MeshにはColliderを追加しません。

### 使用した原本ファイル

- Models/Platforms: `Door_DarkMetal.fbx`, `Door_Frame_A.fbx`, `Door_Simple.fbx`, `Platform_3Plates.fbx`, `Platform_CenterPlate.fbx`, `Platform_Metal.fbx`, `Platform_Rails_4Wide.fbx`, `Platform_Squares.fbx`
- Models/Walls: `BottomMetal_Straight.fbx`, `ShortWall_AccentStrip_Straight.fbx`, `ShortWall_WhitePlate2_Straight.fbx`, `WallAstra_Straight.fbx`, `WallAstra_Straight_Flat.fbx`, `WallBand_Straight.fbx`
- Models/Columns: `Column_Astra.fbx`, `Column_MetalSupport.fbx`, `Column_Simple.fbx`
- Models/Props: `Prop_AccessPoint.fbx`, `Prop_Computer.fbx`, `Prop_Crate4.fbx`, `Prop_ItemHolder.fbx`, `Prop_Light_Floor.fbx`, `Prop_Light_Wide.fbx`, `Prop_Vent_Wide.fbx`
- Textures: `T_Trim_01_BaseColor.png`, `T_Trim_01_Normal.png`, `T_Trim_01_Emissive.png`, `T_Trim_02_BaseColor.png`, `T_Trim_02_Normal.png`, `T_Trim_03_BaseColor.png`, `T_Trim_03_Normal.png`, `T_Trim_03_Cables.png`

### 使用していない原本ファイル

- 配布アーカイブ内のFBX 354点（選定24点を除く）、OBJ/MTL 382点、glTF/BIN 380点、Preview PNG 3点はImportしません。
- Alien、Decal、不要なPlatform／Wall Variation、未使用PropはImportしません。
- `T_PaddedWall_*`、`T_Decals.png`、Trim Color Variation、Detail Mask、ORMはImportしません。URPでChannelを推測して誤用しないよう、Base Color、Normal、Emissive、Cable Maskだけを使用します。
- Pro／Source有料版は取得・Importしません。

## Quaternius — Animated Robot Pack

- Asset名: Animated Robot Pack
- 制作者: Quaternius (`@Quaternius`)
- 公式配布元: <https://quaternius.com/packs/animatedrobot.html>
- 公式ダウンロード: 上記公式ページからリンクされた公開Google Drive folder
- 取得日: 2026-07-21
- 公開日: 2018-10
- ライセンス: CC0 1.0 Universal / Public Domain Dedication
- 商用利用: 可。公式ページはpersonal / commercial projectsで無料利用可能と明記する。
- ビルドへの同梱: 可。
- 再配布条件: CC0。ECHO//SHIFTでは由来と原文を任意表示として残す。
- 生成者情報: License原文は`LowPoly Models by @Quaternius`、公式ページはQuaternius制作として公開する。2018年公開の原作者配布物であり、転載物や権利者不明素材ではない。
- 配布アーカイブのSHA-256: 該当なし。公式配布はFolder内の個別ファイルで、アーカイブは提供されていません。
- 取得原本 SHA-256: `Robot.fbx` = `38AFB56DB7FB17A74D30F0AFC8ADB5F00441A94E65D6B8AC1958732480F79EB8`
- Preview SHA-256: `C4B2FA71F64FF5E57C98E9AE6695BA51744DD225FAA46E22ED3E1DCAD5C3E558`
- ライセンス原文: `ThirdPartyNotices/Quaternius-AnimatedRobot-CC0.txt`
- 読み込み先: `Unity/Assets/_Project/ThirdParty/Quaternius/AnimatedRobot/Robot.fbx`
- Unity側の変更: 表示用の静的MeshとしてRig None、Animation Import OFF、Mesh Compression Medium、Read/Write OFF、Material Import OFFにします。ゲーム用のCollider／Motorと分離した表示用の子だけで使用し、Animator／Root Motionをビルドへ含めません。
- 採用条件: 1920x1080のゲーム用カメラで比率、Batteryの保持、プレイヤー／EchoのSilhouette、世代記号を読み取れ、表示用の子Transformがキャラクタールートを移動しないこと。
- 使用していないファイル: Blend Folder、OBJ Folder、`Preview.gif`はUnityへImportしません。

## Kenney — Sci-Fi Sounds 1.0

- Asset名: Sci-Fi Sounds 1.0
- 制作者: Kenney
- 公式配布元: <https://kenney.nl/assets/sci-fi-sounds>
- 取得日: 2026-07-21
- 配布物記載のcreation date: 2020-10-11
- ライセンス: Creative Commons Zero (CC0 1.0 Universal)
- 商用利用: 可。配布ライセンスに個人・教育・商用Projectで無料利用できる旨が記載されています。
- ビルドへの同梱: 可。
- 再配布条件: CC0でCreditは任意です。ECHO//SHIFTでは由来と原文を残し、配布アーカイブ全体は再配布しません。
- 元の配布アーカイブ: `kenney_sci-fi-sounds.zip`
- 配布アーカイブのSHA-256: `119340F351A5098AD814F78719438C0DA355A9CE8A4C8A3AF6A8D48AA3D49E04`
- ライセンス原文: `ThirdPartyNotices/Kenney-SciFiSounds-CC0.txt`
- 読み込み先: `Unity/Assets/_Project/ThirdParty/Kenney/SciFiSounds/Audio/`
- Unity側の変更: Vorbis、Quality 0.60、Preload ON、Mono化なし。プロジェクト所有の`Phase4AudioCueSet.asset`から参照し、実行中にClipを加工しません。

### 使用した原本ファイル

- `doorOpen_001.ogg`
- `forceField_000.ogg`
- `forceField_004.ogg`
- `impactMetal_001.ogg`
- `laserSmall_000.ogg`
- `lowFrequency_explosion_001.ogg`

### 使用していない原本ファイル

- 上記6音を除くOGG 64音、`desktop.ini`, `Kenney.url`, `Patreon.url`はImportしない。

## 読み込み内容の集計

- 外部素材の原本: 39ファイル、9,984,796 bytes（Unity `.meta`を除く）
- 内訳: Quaternius MegaKit 24 FBX + 8 PNG、Quaternius Robot 1 FBX、Kenney 6 OGG
- 配布アーカイブ／展開物: `ExternalDownloads/`にだけ保存し、Git管理とビルド同梱の対象外
- 追加の小物: Kenney Space Station Kit / Modular Space Kitは不採用。主な環境素材だけで必要な大型形状を満たし、アート方向の混在を避けます。
- 追加Texture: Poly Havenは不採用。MegaKitの2K Trim Textureで必要な表面の変化を作れるためです。
