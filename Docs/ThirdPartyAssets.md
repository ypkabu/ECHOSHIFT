# Third-Party Asset Register

取得日: 2026-07-21 (JST)

この文書はECHO//SHIFTへ取り込んだ外部原本、公式配布元、ライセンス、選別理由、変更内容、Archive hashを固定する。`ExternalDownloads/`のArchiveと展開物はGit管理外であり、ゲームBuildに必要な選別ファイルだけを`Unity/Assets/_Project/ThirdParty/`へ保存する。原本を直接編集せず、Material、Prefab、Audio cue設定などのゲーム側加工物は`Unity/Assets/_Project/Art/`および`Unity/Assets/_Project/Audio/`へ生成する。

## Quaternius — Modular Sci-Fi MegaKit, Standard FREE

- Asset名: Modular Sci-Fi MegaKit `[Standard]`
- 制作者: Quaternius (`@Quaternius`)
- 公式配布元: <https://quaternius.com/packs/modularscifimegakit.html>
- 公式ダウンロード: <https://quaternius.itch.io/modular-sci-fi-megakit>
- 取得日: 2026-07-21
- 配布版: Standard FREE、46 MB、upload id `13454739`、2025-04-22版
- License: CC0 1.0 Universal / Public Domain Dedication
- 商用利用: 可。公式ページはpersonal / educational / commercial projectsで無料利用可能と明記する。
- Build同梱: 可。CC0のため、選別したモデル・textureを変換してゲームBuildへ同梱できる。
- 再配布条件: CC0に帰属表示義務はない。ECHO//SHIFTでは由来と原文を任意表示として残す。原Archiveは再配布しない。
- 生成AI: 公式itch.io配布ページのContent欄は`No generative AI was used`と明記する。
- 元Archive: `Modular SciFi MegaKit Standard.zip`
- 元Archive SHA-256: `6FAE60CF5189E44DFF0BD91097F094A765ACC6D57D64A85A0CC0DD56E03035E3`
- License原文: `ThirdPartyNotices/Quaternius-ModularSciFiMegaKit-CC0.txt`
- Imported Fileの場所: `Unity/Assets/_Project/ThirdParty/Quaternius/ModularSciFiMegaKit/`
- Unity側変更: FBX material importを無効化し、Mesh Compression Medium、Read/Write OFF、Rig None、camera/light/animation import OFFとする。Textureは最大2048、normal mapを明示し、project-owned URP/Lit Materialから参照する。見た目用MeshにはColliderを追加しない。

### 使用した原本ファイル

- Models/Platforms: `Door_DarkMetal.fbx`, `Door_Frame_A.fbx`, `Door_Simple.fbx`, `Platform_3Plates.fbx`, `Platform_CenterPlate.fbx`, `Platform_Metal.fbx`, `Platform_Rails_4Wide.fbx`, `Platform_Squares.fbx`
- Models/Walls: `BottomMetal_Straight.fbx`, `ShortWall_AccentStrip_Straight.fbx`, `ShortWall_WhitePlate2_Straight.fbx`, `WallAstra_Straight.fbx`, `WallAstra_Straight_Flat.fbx`, `WallBand_Straight.fbx`
- Models/Columns: `Column_Astra.fbx`, `Column_MetalSupport.fbx`, `Column_Simple.fbx`
- Models/Props: `Prop_AccessPoint.fbx`, `Prop_Computer.fbx`, `Prop_Crate4.fbx`, `Prop_ItemHolder.fbx`, `Prop_Light_Floor.fbx`, `Prop_Light_Wide.fbx`, `Prop_Vent_Wide.fbx`
- Textures: `T_Trim_01_BaseColor.png`, `T_Trim_01_Normal.png`, `T_Trim_01_Emissive.png`, `T_Trim_02_BaseColor.png`, `T_Trim_02_Normal.png`, `T_Trim_03_BaseColor.png`, `T_Trim_03_Normal.png`, `T_Trim_03_Cables.png`

### 使用していない原本ファイル

- Archive内のFBX 354点（選別24点を除く）、OBJ/MTL 382点、glTF/BIN 380点、preview PNG 3点はImportしない。
- Alien、Decal、不要なplatform/wall variation、未使用propはImportしない。
- `T_PaddedWall_*`, `T_Decals.png`, trim color variation、detail mask、ORMはImportしない。URPへ不適切なchannel解釈を推測しないため、base color/normal/emissive/cable maskだけを使用する。
- Pro/Source有料版は取得もImportもしない。

## Quaternius — Animated Robot Pack

- Asset名: Animated Robot Pack
- 制作者: Quaternius (`@Quaternius`)
- 公式配布元: <https://quaternius.com/packs/animatedrobot.html>
- 公式ダウンロード: 上記公式ページからリンクされた公開Google Drive folder
- 取得日: 2026-07-21
- 公開日: 2018-10
- License: CC0 1.0 Universal / Public Domain Dedication
- 商用利用: 可。公式ページはpersonal / commercial projectsで無料利用可能と明記する。
- Build同梱: 可。
- 再配布条件: CC0。ECHO//SHIFTでは由来と原文を任意表示として残す。
- 生成者情報: License原文は`LowPoly Models by @Quaternius`、公式ページはQuaternius制作として公開する。2018年公開の原作者配布物であり、転載物や権利者不明素材ではない。
- 元Archive SHA-256: 該当なし。公式配布はfolder内の個別ファイルで、Archiveを提供しない。
- 取得原本 SHA-256: `Robot.fbx` = `38AFB56DB7FB17A74D30F0AFC8ADB5F00441A94E65D6B8AC1958732480F79EB8`
- Preview SHA-256: `C4B2FA71F64FF5E57C98E9AE6695BA51744DD225FAA46E22ED3E1DCAD5C3E558`
- License原文: `ThirdPartyNotices/Quaternius-AnimatedRobot-CC0.txt`
- Imported Fileの場所: `Unity/Assets/_Project/ThirdParty/Quaternius/AnimatedRobot/Robot.fbx`
- Unity側変更: Generic Rig、Mesh Compression Medium、Read/Write OFF、material import OFF。Gameplay Collider/Motorとは分離したVisual child wrapperでのみ評価し、Animator root motionは常にOFFとする。
- 採用条件: 1920x1080 gameplay-camera captureで比率、Battery carry、Player/Echo silhouette、generation markが読め、animationがActor rootを移動しないこと。満たさない場合はSceneから除外し、既存compound actorを改修する。
- 使用していないファイル: Blend folder、OBJ folder、Preview.gifはUnityへImportしない。

## Kenney — Sci-Fi Sounds 1.0

- Asset名: Sci-Fi Sounds 1.0
- 制作者: Kenney
- 公式配布元: <https://kenney.nl/assets/sci-fi-sounds>
- 取得日: 2026-07-21
- 配布物記載のcreation date: 2020-10-11
- License: Creative Commons Zero (CC0 1.0 Universal)
- 商用利用: 可。配布Licenseはpersonal / educational / commercial projectsで無料利用可能と明記する。
- Build同梱: 可。
- 再配布条件: CC0。creditは任意。ECHO//SHIFTでは由来と原文を残す。原Archiveは再配布しない。
- 元Archive: `kenney_sci-fi-sounds.zip`
- 元Archive SHA-256: `119340F351A5098AD814F78719438C0DA355A9CE8A4C8A3AF6A8D48AA3D49E04`
- License原文: `ThirdPartyNotices/Kenney-SciFiSounds-CC0.txt`
- Imported Fileの場所: `Unity/Assets/_Project/ThirdParty/Kenney/SciFiSounds/Audio/`
- Unity側変更: Vorbis、quality 0.60、preload ON、mono化なし。project-owned `Phase4AudioCueSet.asset`から参照し、runtimeでclipを加工しない。

### 使用した原本ファイル

- `doorOpen_001.ogg`
- `forceField_000.ogg`
- `forceField_004.ogg`
- `impactMetal_001.ogg`
- `laserSmall_000.ogg`
- `lowFrequency_explosion_001.ogg`

### 使用していない原本ファイル

- 上記6音を除くOGG 64音、`desktop.ini`, `Kenney.url`, `Patreon.url`はImportしない。

## Import集計

- ThirdParty原本: 39ファイル、9,984,796 bytes（Unity `.meta`を除く）
- 内訳: Quaternius MegaKit 24 FBX + 8 PNG、Quaternius Robot 1 FBX、Kenney 6 OGG
- Archive/展開物: `ExternalDownloads/`にのみ保存し、Git管理・Build同梱の対象外
- Optional Secondary Props: Kenney Space Station Kit / Modular Space Kitは不採用。Primary Environmentに不足する大型形状はなく、Art Style混在を避ける。
- Optional Textures: Poly Havenは不採用。MegaKitの2K trim textureで必要なsurface variationを満たすため。
