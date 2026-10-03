# 検証結果の要約

## 最新の再検証（2026-09-29）

公開ソース `263e34ec5f0212e6cd6d4b8dae3fbbd9d64f65ca` をUnity `6000.4.6f1` / Windowsで検証しました。以下は今回の実行結果であり、下段の開発時記録とは分けています。

| 確認項目 | 今回の結果 |
| --- | --- |
| シーン生成 | 終了コード0、canonical assets=4 / writeCount=0 |
| EditMode | 151件中146成功・5失敗・0 Skip、5.1491039秒、終了コード2 |
| PlayMode | 131成功・0失敗・0 Skip、30.9540531秒、終了コード0 |
| Windows x86_64 Development Build | BuildReport成功、エラー0・警告0、終了コード0 |
| 画面付きD3D11 Player | 自動入力で全3セクションを1,070 advancesで完走 |
| インタラクション / Replay Drift | 4成功・0失敗 / 最大0 m |
| 完走Playerログのmanaged異常 | Missing / NullReference / Unhandled Exceptionの一致なし |
| Player終了 | `-1073741819 (0xC0000005)`。正常終了せず、配布不可 |

Pause / Restart / Quit確認フローはPlayModeテストに含まれます。今回の画面付きPlayerは自動完走・自動Quitを確認したもので、手動操作・ゲームパッド・別PCの再確認を完了したという意味ではありません。

### EditModeの5失敗

次のテストが必要とする `Captures/Phase5A/` は `.gitignore` により公開ソースに含まれません。失敗は削除・Skip化しておらず、テスト一式の成功とは判断しません。

- `Phase5AIdentityPreviewTests.GeneratedCapturesAndAudioMeetDeliveryBounds`
- `Phase5ASelectedRevisionTests.SelectedAtlasCapturesAndAudioMeetDeliveryBounds`
- `Phase5ASelectedRevision2Tests.Revision2CapturesAndEightIndividualCuesMeetDeliveryBounds`
- `Phase5ASelectedRevision21Tests.Revision21CapturesMeetCountAndExactResolution`
- `Phase5ASelectedRevision21Tests.Revision21AudioChangesOnlySectionCompleteAndDoesNotClip`

再生成メニューと注意事項は[テスト計画](TestPlan.md)を参照してください。Editor初回ImportではShader GraphのAPI更新後にコンパイルが成功しました。またEditor起動時のPackageCache検査にUnity CollectionsテストDLLのDirectoryNotFound診断があり、今回のEditorログ全体が異常0とは記載しません。

## 開発時の検証記録（今回のクリーン実行とは別）

以下は既存資料に残る開発時の値です。画像・音声成果物を含む当時の環境での結果であり、クリーンcloneで `151/151` を再現できたという意味ではありません。

| 確認項目 | 結果 |
| --- | --- |
| EditModeテスト | `151/151` 成功 |
| PlayModeテスト | `131/131` 成功 |
| プロジェクト内の全シーン読み込み | 成功 |
| Missing Component / Reference | `0` |
| 全3セクションの自動完走 | 成功 |
| インタラクション | `4`回成功 / `0`回失敗 |
| 最大Replay Drift | `0 m` |
| コンパイラーのエラー / 警告 | `0 / 0` |
| Missing / NullReference / 未処理例外 | `0 / 0 / 0` |

検証コマンド、対象シーン、許容値は[テスト計画](TestPlan.md)に記録しています。

## 既知の課題

画面表示を伴うWindows版は、終了時に`UnityPlayer.dll`のnative cleanup内で`0xC0000005`を再現します。ゲームプレイ中のmanaged exceptionではありませんが、正式配布前の未解決事項として扱っています。再現条件と調査範囲は[Windows版終了時クラッシュの調査](Phase4WindowsExitCrashInvestigation.md)を参照してください。
