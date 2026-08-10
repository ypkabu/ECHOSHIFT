# 再現手順

影響を受ける実行では`0xC0000005`で終了します。製品版と以前の小さいPlayerから取得したDumpでは、`PlatformAccessibilityManager`の終了処理から呼ばれた`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`が、Null Pointerを基準に`0x138`を読み取っています。

## 環境

- Windows 11 Home、Build 26200
- Unity 6000.4.12f1（`3ca267ce8005`）
- Windows x86_64、Mono、D3D11、Development Build
- 構成BのPackage：URP 17.4.0と解決済みの依存関係

## 構成Bのビルド

1. `MinimalRepro`をUnity 6000.4.12f1で開くか、次のCommandを実行します。

   ```powershell
   .\Automation\BuildStage.ps1 -Stage B -UnityEditor "C:\Path\To\6000.4.12f1\Editor\Unity.exe"
   ```

2. `Builds/B/MinimalExitCrash.exe`が存在することを確認します。繰り返し実行の間は再ビルドしません。
3. 実行前にBuild全体のHashを記録します。確認済み構成BのHashは`229597DCD872E3F09E46FFB35AF0814745F06B4311E1F385861B10DFFC3D8B69`です。

## 画面表示を伴う終了操作

1. `Builds/B/MinimalExitCrash.exe -force-d3d11 -screen-fullscreen 0`を起動します。
2. Player WindowとSceneの読み込み完了を待ちます。
3. Windowを通常操作で閉じます。
4. Processの終了コードを記録し、同じBuildで5回以上繰り返します。

最初の確認では5回中1回が符号付き終了コード`-1073741819`（`0xC0000005`）となりました。その後、ProcDump付き20回と、Debuggerを使わずWER LocalDumpsを有効にした30回はすべて終了コード`0`で、構成BのDumpは取得できませんでした。

## その他の構成

構成定義は`MinimalRepro/StageDefinitions`にあり、AからIへ順に要素を追加します。自動処理は選択したManifestをコピーし、Lock Fileを解決し、Sceneを生成してClean Development Buildを作成します。

- A：空のSceneとCamera
- B：URP
- C：Input System
- D：uGUI／TMP
- E：Volume／Post Processing Component
- F：Audio
- G：製品版に近いWindows Player Settings
- H：重複防止付き同期保存と`Application.Quit(0)`
- I：最小のURP/Lit表示とCollider

A〜Gは通常のWindow Close、H〜IはProjectと同じ自動終了経路を使います。結果は`Evidence/StageMatrix.md`に記録しています。追加順に発生率が増えていないため、Packageの単純な二分探索結果としては扱いません。

## 期待する結果

画面表示を伴うWindows PlayerがNative Cleanupを完了し、終了コード`0`で終了します。

## 実際の結果

影響を受けるBuildは`0xC0000005`で終了します。製品版と以前の小さいPlayerの確認済みDumpでは、`PlatformAccessibilityManager`のRuntime Cleanupから呼ばれた`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`が、Null Pointerを基準に`0x138`を読み取っています。
