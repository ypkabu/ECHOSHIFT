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

## シンボル付きコールスタック

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

## 公開ソースからの再検証（2026-09-29）

上段は以前のDump解析・環境比較です。以下は公開ソース `263e34ec5f0212e6cd6d4b8dae3fbbd9d64f65ca` を別のクリーンな作業コピーでビルドした今回の結果です。以前のDumpを新しい実行から取得したDumpとして扱いません。

- Unity `6000.4.6f1`、Windows 11、NVIDIA GeForce RTX 5070、driver `32.0.16.1692`、D3D11。
- シーン生成：終了コード0、`PHASE5A_PRODUCTION_CANONICAL_OK assets=4;writeCount=0`。
- Windows x86_64 Development Build：終了コード0、`PHASE4_3_BUILD_OK`、BuildReport警告0・エラー0、291 files / 204,674,787 bytes（BuildReport値）。
- EXE SHA-256：`098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938`。
- UnityPlayer.dll SHA-256：`4B0E50D9F9438D6184576036C3E36653E1FD98533D33A6B42C8435F6AD0A9B2A`。

### 再現方法と観測

ビルド後、画面を表示したまま次の既存自動完走probeを使用しました。Unity Editorなしで起動できるPlayerですが、正常終了できる配布物という意味ではありません。

```powershell
& ".\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -force-d3d11 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -phase4AutoCompleteProbe -logFile ".\Logs\player-autocomplete-05.log"
```

`PHASE4_3_STANDALONE_COMPLETE_OK advances=1070;interactionSuccess=4;interactionFailure=0;drift=0` とテレメトリー保存完了を確認しました。このPlayerログにはMissing、NullReference、Unhandled Exceptionの一致はありません。続いてUnity cleanupのメッセージが出た後、実processの終了コード `-1073741819` を取得しました。完走・保存完了マーカーだけでは正常終了を確認できません。

Windows Applicationイベント1000でも、17:37:03および17:39:22（JST）の実行で `UnityPlayer.dll / 0xc0000005 / offset 0x1d2f39` を確認しました。offsetは以前の6000.4.6f1解析と一致しますが、**今回の実行では新しいDumpを取得しておらず、同じCall Stackだったとまでは断定しません**。

Hidden起動および別起動方式ではboot後に完走しない試行もありました。その2件は検証用Playerだけを停止し、正常終了・native crashの集計には含めていません。画面表示・起動方式は再現条件の一部として残します。

### 切り分けの判断

今回の自動完走はゲーム進行・同期テレメトリー保存が終了まで達することを示します。既存の終了処理確認、Native Plugin不使用、以前のUnity native stackとの照合によりUnity cleanup内の障害という分類は維持します。一方、ゲームのどの設定・終了順がUnity内部の条件を成立させるかは未特定です。外部素材やURPを原因として断定する修正は行いません。

2026-10-03にUnity公式Issue Trackerも照合しました。Accessibility関連の[UUM-126552](https://issuetracker.unity.com/issues/21472/application-crash-when-setting-assistivesupportactivehierarchy-with-narrator-enabled-on-windows-10)はNarrator有効時の `ucrtbase.dll / 0xc0000409` が条件で、本件の終了時 `UnityPlayer.dll / 0xc0000005` とは一致しません。検索で本件と一致する修正を確認できなかったことは、Unity全版で未修正という証明ではありません。

### 配布判断・残る確認

**Windows ZIPおよび新しいゲーム配布Releaseは作成しません。** Quit無効化、強制終了、例外隠蔽で配布条件を緩和していません。別PC、IL2CPP、正常終了が確認できるUnity更新版、今回の再現に対する新Dumpは未確認です。正常終了の検証後にのみ、手動Pause / Restart / Quit、通常のクリア、非Developmentビルド、配布ファイル・署名状態を含む配布チェックを再開します。
