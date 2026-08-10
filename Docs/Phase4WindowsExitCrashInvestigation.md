# Windows版終了時クラッシュの調査

## 結論

画面表示を伴うWindows Playerの終了処理で、`UnityPlayer.dll`内の`0xC0000005`が再現します。確認できたDumpでは、`PlatformAccessibilityManager`を破棄するUnityのnative cleanup内でNull Pointerからの読み取りが発生しています。ゲーム側の設定が発生条件へ影響する可能性は残りますが、Access Violation自体はUnityのNative処理内で発生しています。

## 確認できた事実

- Unity 6000.4.6f1、6000.4.8f1、6000.4.12f1の画面付きWindows Playerで再現しました。
- Development BuildとNon-Development Buildの両方で再現しました。
- D3D11のNVIDIA、Intel、WARPで再現しました。
- ポーズ画面からの終了、`Application.Quit`、Window Closeで再現しました。
- `-batchmode -nographics`は`5/5`回正常終了しました。
- 製品版3件と、以前に作成した小さいProjectの6000.4.8f1／6000.4.12f1のDumpは、Unity内の同じ関数順になりました。
- 例外は`RAX = 0`の状態で`0x138`を読み取る`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`です。
- 呼び出し元は`RegisterRuntimeInitializeAndCleanup::ExecuteCleanup`と`RuntimeCleanup`配下の`RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy`です。
- 障害発生前にゲームは完走しており、Replay Drift `0 m`、インタラクション`4`回成功／`0`回失敗、テレメトリー保存完了、managed側のCompiler／Missing／Null／Unhandled一致`0`を確認しました。

## 最小構成の比較

確認用Projectは`BugReports/UnityWindowsExitCrash/MinimalRepro`にあります。A〜IはUnity 6000.4.12f1で警告`0`、エラー`0`の状態でビルドし、各5回実行しました。

| 構成 | 終了方法 | Access Violation / 実行回数 |
| --- | --- | ---: |
| A：空のシーン＋Camera | Window Close | 0/5 |
| B：A＋URP | Window Close | 1/5 |
| C：B＋Input System | Window Close | 0/5 |
| D：C＋uGUI／TMP | Window Close | 0/5 |
| E：D＋Volume | Window Close | 0/5 |
| F：E＋Audio | Window Close | 0/5 |
| G：F＋製品に近いPlayer Settings | Window Close | 0/5 |
| H：G＋Projectと同じ終了処理 | `Application.Quit` | 0/5 |
| I：H＋最小限の表示／Component | `Application.Quit` | 0/5 |

構成BはA〜Iの中で唯一Access Violationを観測した最小候補ですが、`1/5`回だけで、URPを直接の原因とは断定できません。同じBinaryをProcDump付きで`20`回、User Level WER LocalDumps付きで`30`回追加実行した結果は`0/20`と`0/30`で、Dumpを取得できませんでした。そのため構成Bが製品版と同じCall Stackかは未確認です。

## Symbol付きCall Stack

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

取得できたのは9フレームです。`UnityMain`と障害箇所の間に、ユーザー製DLL、Native Plugin、GPU Driverのフレームはありません。

Unity関数順を正規化したSHA-256は`2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`です。

| Unity | UnityPlayer Offset | 確認状況 |
| --- | --- | --- |
| 6000.4.6f1 | `0x1d2f39` | 製品版3/3で同じ |
| 6000.4.8f1 | `0x1d3e49` | 小さいProjectで同じ関数順 |
| 6000.4.12f1 | `0x1d4a89` | 小さいProjectで同じ関数順 |
| 構成B 6000.4.12f1 | 取得できず | 比較不可 |

## 終了処理のコード確認

`Application.quitting`のHandlerと`OnApplicationQuit`実装はありません。終了管理処理は重複要求を防ぎ、シミュレーションを停止し、テレメトリーを同期保存し、保存先をログへ出してから`Application.Quit(0)`を呼びます。表示・演出で調べたEvent購読には対応する解除処理があります。

通常のゲーム処理では、独自Thread、Task、NativeArray／Job、P/Invoke、Native Plugin、外部Recorder／FFmpeg Process、Background Disposeを作成しません。AudioとParticleのPoolはUnity管理のシーンオブジェクトで、障害時のNative Call Stackにも現れていません。この確認により明白なmanaged側の二重解放や終了後Callbackの可能性は下がりますが、ゲーム設定がUnity内部の破棄順へ影響しないことまでは証明できません。

## 環境別の結果

- NVIDIA D3D11：`4/5`回再現
- Intel D3D11：`5/5`回再現
- WARP D3D11：`5/5`回再現
- `-batchmode -nographics`：`0/5`回
- Mono：再現
- IL2CPP：Module未導入のため未検証
- 別PC：利用できる2台目がなく未検証

## 現在の扱い

Access ViolationがUnityPlayerのNative Cleanup内にあることは確認できていますが、発生率がビルド内容、実行時刻、監視方法で変わるため、ゲーム側の発生条件は特定できていません。URPが直接原因であるという根拠もありません。

`-batchmode -nographics`は画面表示を伴う処理を通らないため回避しますが、通常のゲーム配布には使えません。Processの強制終了、`Environment.Exit`、終了コードの書き換え、待ち時間の追加、Crashの非表示化は回避策として採用していません。画面付きWindows版は、この問題が解決するまで正式配布の対象外です。
