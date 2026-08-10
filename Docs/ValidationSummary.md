# 検証結果の要約

現行のプレイ可能なプロトタイプに対して、実際のシーンを含む回帰検証を行っています。

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
