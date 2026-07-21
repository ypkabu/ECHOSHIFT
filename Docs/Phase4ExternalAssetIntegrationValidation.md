# Phase 4.1 External Asset Integration Validation

**Status: Phase 4.1 Automation Passed; Human Visual Review Pending**

検証日: 2026-07-21～2026-07-22 (Asia/Tokyo)
Branch: `feature/phase4-external-asset-integration`
Baseline: `e043b160a945939d5f862a4648d3df728a40a49e` / `phase4-automation-passed`
Unity Editor: `6000.4.6f1`
URP: `17.4.0`

## Assetとライセンス

Buildへ取り込む外部原本は39ファイル、9,984,796 bytes（Unity `.meta`除外）に限定した。内訳はQuaternius Modular Sci-Fi MegaKit 24 FBX + 8 PNG、Quaternius Animated Robot 1 FBX、Kenney Sci-Fi Sounds 6 OGGである。いずれも公式配布元のCC0素材で、商用利用とBuild同梱が可能である。Archive、展開物、Preview、未使用variationは`ExternalDownloads/`に隔離し、GitおよびBuildへ含めない。

| Package | Official source | Source hash | Imported subset |
|---|---|---|---:|
| Quaternius Modular Sci-Fi MegaKit Standard FREE | <https://quaternius.com/packs/modularscifimegakit.html> / <https://quaternius.itch.io/modular-sci-fi-megakit> | Archive SHA-256 `6FAE60CF5189E44DFF0BD91097F094A765ACC6D57D64A85A0CC0DD56E03035E3` | 24 FBX + 8 PNG |
| Quaternius Animated Robot | <https://quaternius.com/packs/animatedrobot.html> | `Robot.fbx` SHA-256 `38AFB56DB7FB17A74D30F0AFC8ADB5F00441A94E65D6B8AC1958732480F79EB8` | 1 FBX |
| Kenney Sci-Fi Sounds 1.0 | <https://kenney.nl/assets/sci-fi-sounds> | Archive SHA-256 `119340F351A5098AD814F78719438C0DA355A9CE8A4C8A3AF6A8D48AA3D49E04` | 6 OGG |

権利、原文notice、選別内容、未使用ファイル、Unity Import設定は`ThirdPartyAssets.md`へ固定した。RobotはRig None、animation import OFFのstatic visualとして使用し、Animator/root motionを含めない。外部Meshはpresentation childだけに置き、Gameplay Collider、Stable ID、Motor、Carry Socketを変更しない。

## Scene Builderと視覚修正

`P3SceneBuilder.BuildFromCommandLine`が外部Asset catalog、project-owned URP Material、wrapper Prefab、P3 Sceneを再生成する。同じ入力から2回生成したRobot wrapper SHA-256はともに`624853FC7D9BFCC3A8D77D2B6ACE626CE578B44976A46C9F251DB6408B9F8A40`で、idempotenceを確認した。

- EnvironmentをQuaterniusのfloor、wall、column、support、console、crate、lightで再構成した。
- Player/Echoは共通Robot meshをGameplay rootの子として採用し、Player/Echo 1～3を色以外のcompact mark、visor、trailで識別する。
- Doorは外部frame/panelとcircuit badge、Goalは外部portal frame、glow、particle、日本語`出口ゲート`で再構成した。
- 初回Visual ReviewのHighであった暗部潰れ、頭上の黒いbeam、常時world label、Pause Menuの階層とfocusを修正した。Pause中はGameplay HUDを隠す。
- P0～P2 Scene SHA-256はP0 `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5`、P1 `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB`、P2 `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C`のまま変化していない。

Import時にQuaternius Robotの`Foot.L`へself-intersecting polygonの除去diagnosticが1件出る。BuildReport warning/error、テスト、captureで可視欠損は確認されなかったが、両足の実機目視をHuman Visual Review項目として残す。

## Automated tests

| Suite | Total | Passed | Failed | Skipped | Duration |
|---|---:|---:|---:|---:|---:|
| EditMode | 93 | 93 | 0 | 0 | 28.2191387 s |
| PlayMode | 93 | 93 | 0 | 0 | 24.4194633 s |

追加した15件（EditMode 9、PlayMode 6）はlicense/source/hash、Import設定、2K以下texture、URP対応shader、Missing Material/Texture、Collider/Animator/root motion不在、wrapper idempotence、Stable ID/collider保持、world label/overhead beam不在、Pause HUD/focus、外部visual、Robot transformを検証する。既存テストは削除、無効化、許容値緩和を行っていない。

実Scene統合ではP0～P3最大Replay Driftがすべて`0 m`、P1 interaction `2/0`、P2 `2/0`、P3 `4/0`（成功/失敗）で、P3全3 Sectionを完了した。compiler warning/error、MissingReference、NullReference、未処理Exceptionの最終log一致件数は0である。

## Before / After captures

無変更の`phase4-automation-passed`を一時worktreeから生成したBeforeと、現在のPhase 4.1 Afterを、同じ8状態・1920x1080・Debug Overlay OFFで`Captures/Phase4_1/Before`と`Captures/Phase4_1/After`へ保存した。各folderは8 PNGで、captureはGit管理外である。

1. `01_section1_facility.png`
2. `02_player_echo1.png`
3. `03_battery_carry.png`
4. `04_socket_door_open.png`
5. `05_echo1_echo2_roles.png`
6. `06_goal_arrival.png`
7. `07_gameplay_hud.png`
8. `08_pause_menu.png`

自動検査は個数、解像度、非空、hashを確認した。After合計は7,650,168 bytes。表示品質の合否は自動captureから推測せず、新しいHuman Visual Reviewで判定する。

## Performance

Development Player、1920x1080、D3D11、NVIDIA GeForce RTX 5070 Laptop GPU、120fps cap、最大3 Echo、steady 600 framesの結果:

| Metric | Result |
|---|---:|
| Average / p95 / max frame | 8.337547 / 8.336699 / 8.838301 ms |
| Main Thread average / max | 8.329742 / 8.832500 ms |
| Loop Transition frame / Main max | 15.721394 / 15.705700 ms |
| Steady GC max / total / non-zero frames | 0 B / 0 B / 0 of 600 |
| Maximum used memory | 168,041,477 B |
| Texture memory | 40,882,741 B |
| Final / required Echoes | 3 / 3 |

GPU Frame Time、SetPass、Triangles、VerticesのRecorderはavailableを返したが値は0、Draw Callsはunavailableだった。これらを実測0とは扱わず、interactive Profiler / Frame Debugger確認をHuman Visual Reviewへ残す。

## Standalone builds

| Build | BuildReport | Distribution | EXE SHA-256 |
|---|---|---|---|
| `Builds/Phase4_1/ECHOSHIFT_Phase4_1.exe` (Development) | Success, warning 0, error 0, 204,271,910 B | 291 files / 204,467,042 B | `098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938` |
| `Builds/Phase4_1_NonDevelopment/ECHOSHIFT_Phase4_1_NonDevelopment.exe` | Success, warning 0, error 0, 140,590,037 B | 182 files / 140,785,117 B | `65D646C9285BF2CBBAB784992E3AD5AE9012BEF5E8A6B4FFA46209592AF9DDA2` |

両BuildのEditor processはexit 0で、EXEとData folderが存在する。Developmentのstartup probeはP3 Playing、`ja-JP`、packaged Noto、glyphs true、HUD true、telemetry JSON生成、自然終了0、Missing/Null/unhandled一致0を確認した。

## `0xC0000005` investigation

過去のPhase 3/初回Phase 4 Buildには`UnityPlayer.dll`、exception `0xc0000005`、fault offset `0x00000000001d2f39`（UnityPlayer 6000.4.6.1308）のWindows Application Error履歴がある。Phase 4.1ではDevelopment/Non-Development、D3D11/D3D12、audio有無、auto-quit、Pause Menu二段階Quitの8条件と、表示Windowへの通常Close 1条件を検証し、全9条件がexit 0、Missing/Null/unhandled 0だった。Window Closeはforce terminateを使わず、PlayerConnection cleanupまで完了した。直近2時間の新規Windows Application Errorは0件だった。

従来のhidden Window Close試行はwindow handleを得られず強制終了したため判定から除外した。現在Buildで再現しないことは確認したが、過去crashの根本原因を特定したとは扱わない。

## Gate decision

Asset、license、Import、Scene、回帰、capture、performance、Development/Non-Development Build、startup、Quit matrixの自動ゲートは通過した。`phase4-assets-automation-passed`はこの結果を示す軽量タグであり、`phase4-validated`ではない。外部Assetを用いた見た目、動作中の識別、音量、Robot足、GPU/Draw metrics、非16:9レイアウトは人間が確認する必要がある。

**Final state: Phase 4.1 Automation Passed; Human Visual Review Pending.**

## Post-tag perimeter visual correction

The `phase4-assets-automation-passed` capture was subsequently found to contain floating cyan/orange wall lamps, partial wall rails, and diagonal supports. The tag remains unchanged as evidence of that baseline. The Builder-source correction, exact generated-object inventory, new 94th EditMode test, refreshed same-camera comparison, and 94/94 EditMode + 93/93 PlayMode regression are recorded in `Phase4FloatingVisualAudit.md`. This correction remains subject to human visual recheck and does not create or authorize `phase4-validated`.
