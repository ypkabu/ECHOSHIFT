# Windows版終了時クラッシュの調査資料

このFolderには、Unity Bug Report用に作成したソースコードだけの比較Projectと、確認結果を収録しています。生成した`Builds`、`Library`、未整理の`Logs`、Crash Archive、Symbol、PDB、DumpはGit管理に含めません。

## 現在の分類

製品版と、以前に作成した小さいPlayerでは、Unityの画面付きNative Shutdown処理でAccess Violationが繰り返し発生しました。このFolderのA〜I比較では、最初の5回中、構成Bだけで1回発生しましたが、構成BのDumpは取得できませんでした。したがって構成Bは観測できた最小候補であり、安定した再現構成でも、URPが直接原因である根拠でもありません。

> 画面表示を伴うWindows PlayerのUnity Native Shutdown処理で発生する再現性の高いエンジン側不具合。ゲームコードが発生条件となる可能性は完全には否定できませんが、Access Violation自体は`UnityPlayer.dll`のNative Cleanup内で発生しています。

## 内容

- `MinimalRepro/`：Unity 6000.4.12f1向けの比較用ソースProject
- `ReproductionSteps.md`：ビルドと実行手順
- `Evidence/StageMatrix.md`：A〜Iと構成B追加確認の結果
- `Evidence/VersionAndBuildMatrix.md`：Unity、Graphics、Development設定、Build Hash
- `Evidence/SymbolicatedStack.md`：製品版／以前の小さいProjectのDump解析と、構成Bの制約
- `Evidence/DumpHashes.md`：DumpのHash。Dump File自体は非収録
- `Evidence/PlayerLogExcerpts.md`：終了前後の抜粋。未整理のLogは非収録
- `Evidence/EventViewer.md`：保存したEventの情報と構成Bの制約
- `Evidence/SystemInformation.md`：確認PCとTool Version
- `Evidence/ProjectSettingsComparison.md`：構成ごとの設定／Package差分

## 提供時の注意

Git管理している構成Bの`manifest.json`と`packages-lock.json`は、Access Violationを観測した中で最も小さい構成です。最初のWindow Closeでは`1/5`回、ProcDump付きでは`0/20`回、User Level WER LocalDumps付きでは`0/30`回でした。構成BのCall Stackは製品版Dumpと比較できていません。構成Bを再現できない場合はVersion／Build表とSymbol付きCall Stackを照合し、構成BだけからURPのRegressionと判断しないでください。
