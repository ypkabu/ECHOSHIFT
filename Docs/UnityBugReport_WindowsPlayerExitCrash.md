# Windows版終了時にUnityPlayer.dll内で発生するAccess Violation

## 概要

画面表示を伴うWindows Standalone Playerの終了時に、`0xC0000005`が発生します。製品版と以前の小さいProjectから取得したSymbol付きFull Dumpでは、`RuntimeCleanup`で`PlatformAccessibilityManager`を破棄する際に呼ばれた`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`が、Null Pointerを基準に読み取っています。

Unity 6000.4.6f1、6000.4.8f1、6000.4.12f1で再現しました。Development Build、Hardware GPU、特定GPU Vendor、特定の終了方法だけに限定されません。`-batchmode -nographics`では再現しません。

## 再現率

製品版と以前の小さいProjectでの結果：

- 6000.4.6f1 製品版Development、自動終了：3/3
- 6000.4.8f1 以前の小さいPlayer：9/10
- 6000.4.12f1 以前の小さいPlayer：9/10
- 6000.4.12f1 製品版Development：自動終了7/10、Pause Quit 8/10、Window Close 2/10
- 6000.4.12f1 製品版Non-Development、自動終了：9/10。2回は通常完了前に外部Timeoutへ到達しましたが、Cleanup中に発生しました。
- 6000.4.12f1 D3D11 Device：NVIDIA 4/5、Intel 5/5、WARP 5/5
- 6000.4.12f1 `-batchmode -nographics`：0/5

比較用Projectでの結果：

- A：空のScene＋Camera：0/5
- B：A＋URP 17.4.0とPipeline Asset：1/5
- C〜I：各0/5
- 同じ構成BをProcDump付きで実行：0/20、Dumpなし
- 同じ構成BをUser Level WER LocalDumps付きで実行：0/30、Dumpなし

構成Bは、比較用ProjectでAccess Violationを観測した最小候補ですが、安定した最小再現ではありません。後続構成で発生率が増えていないため、URPを直接原因とは判断できません。監視、Timing、Serialization、Build Layoutが発生率へ影響する可能性があります。

## 確認したUnityバージョン

| Version | Changeset | 結果 |
| --- | --- | --- |
| 6000.4.6f1 | `0b051c2e5d54` | 製品版Full Dumpで再現 |
| 6000.4.8f1 | `f8b72d3d7343` | 以前の小さいPlayerで再現 |
| 6000.4.12f1 | `3ca267ce8005` | 製品版と以前の小さいPlayerで再現。比較用Projectも作成 |

最初に問題が発生したUnity Versionと、修正済みVersionは特定できていません。

## 例外とSymbol付きCall Stack

- 例外コード：`0xC0000005`
- 操作：読み取り
- 不正なAddress：`0x0000000000000138`
- Register：`RAX = 0`
- 障害関数：`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`
- 終了処理：`PlatformAccessibilityManager`のNative Runtime Static Cleanup

```text
UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9
UnityPlayer!RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy+0x3b
UnityPlayer!RegisterRuntimeInitializeAndCleanup::ExecuteCleanup+0xed
UnityPlayer!RuntimeCleanup+0x27
UnityPlayer!UnityMainImpl+0x1300
UnityPlayer!UnityMain+0xb
<player-exe>!__scrt_common_main_seh+0x106
kernel32!BaseThreadInitThunk+0x17
ntdll!RtlUserThreadStart+0x2c
```

確認した3つのUnity VersionはBinary Offset（`0x1d2f39`、`0x1d3e49`、`0x1d4a89`）が異なりますが、正規化したUnity関数順は同じです。`UnityMain`より下にある最初のUnity以外のModuleは、生成済みPlayer EXEのCRT Entryです。障害時のCall StackにUser DLL、Native Plugin、GPU Driver、managed Callbackはありません。

正規化したCall StackのSHA-256：`2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`

構成Bの失敗時はDumpを取得できていません。構成Bの1件を、終了コードだけで製品版と同じCall Stackの障害として扱うことはできません。

## 最小構成の再現手順

1. `BugReports/UnityWindowsExitCrash/MinimalRepro`をUnity 6000.4.12f1で開きます。
2. Git管理している`Packages/manifest.json`と`packages-lock.json`は構成B（URP 17.4.0）です。
3. `Automation/BuildStage.ps1 -Stage B -UnityEditor <Unity.exe path>`を1回実行します。
4. Build Hashを記録し、繰り返し実行の間は再ビルドしません。
5. `Builds/B/MinimalExitCrash.exe -force-d3d11 -screen-fullscreen 0`を起動します。
6. Windowの読み込み完了後、通常操作で閉じます。
7. 同じBuildで繰り返します。最初の5回では1回再現しましたが、後続確認では再現しませんでした。

A〜IのManifestで同じ累積比較を行えます。構成Bは「低頻度で再現」と記載し、決定的な再現構成とはしません。

## 期待する結果

画面表示を伴うWindows Playerが終了処理を完了し、終了コード`0`で終了します。

## 実際の結果

影響を受ける実行は、`UnityPlayer.dll`のNative Cleanup中に`0xC0000005`で終了します。製品版ではゲームと同期テレメトリー保存が先に完了し、Player Logにmanaged側の未処理例外、Missing Script／Reference、NullReferenceはありません。

## ビルド設定・GPU・終了方法との関係

- Development BuildとNon-Development Buildで再現します。
- NVIDIA、Intel、D3D11 WARPで再現します。
- ポーズ画面からの終了、`Application.Quit(0)`、Window Closeで再現します。
- 画面付きPlayerで再現し、`-batchmode -nographics`では再現しません。
- Recorder初期化と録画の有無に依存しません。

## 発生開始バージョンの確認状況

不明です。確認した6000.4.6f1、6000.4.8f1、6000.4.12f1では同じUnity関数順を確認しました。6000.4より前のEditorは未検証です。

## 回避策の確認状況

画面付きPlayerで安全に使える回避策は見つかっていません。`-batchmode -nographics`は問題の経路を通りませんが、通常のゲーム配布には使えません。確認した6000.4系Patchへの更新でも解消しません。Processの強制終了、`Application.Quit`の置き換え、終了時のSleep、終了コードの書き換えは使用していません。

## 添付内容

- A〜IのManifestと構成BのLock Fileを含むソースコードだけの比較Project
- 再現手順と全比較結果
- Unity Version／Build／GPU表
- Full DumpのSHA-256一覧。Dump File自体は必要に応じて別途提供
- Symbol付きCall Stackと正規化したHash
- Player Logの終了順とSystem情報
- Package Manifest／Lock FileとProject Settingsの比較
