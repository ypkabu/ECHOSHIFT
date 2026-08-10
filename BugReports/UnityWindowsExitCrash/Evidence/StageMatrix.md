# 最小プロジェクトの比較結果

全BuildでUnity 6000.4.12f1、D3D11、Windows x86_64、Mono、Development BuildとClean Build Cacheを使用しました。全Buildが警告`0`、エラー`0`で完了しています。

| 構成 | 追加内容 | 終了方法 | 回数 | AV | Exit 0 | 結果 |
| --- | --- | --- | ---: | ---: | ---: | --- |
| A | 空のScene＋Camera | Window Close | 5 | 0 | 5 | AVなし |
| B | URP 17.4.0＋Pipeline／Renderer Asset | Window Close | 5 | 1 | 4 | 低頻度の候補（`1/5`） |
| C | Input System 1.19.0＋PlayerInput | Window Close | 5 | 0 | 5 | AVなし |
| D | uGUI 2.0.0＋Canvas／CanvasScaler／TMP | Window Close | 5 | 0 | 5 | AVなし |
| E | Global Volume Component | Window Close | 5 | 0 | 5 | AVなし |
| F | Audio Module＋AudioSource | Window Close | 5 | 0 | 5 | AVなし |
| G | 製品版に近いWindows設定 | Window Close | 5 | 0 | 5 | AVなし |
| H | 重複防止＋同期JSON保存＋Application.Quit | Application.Quit | 5 | 0 | 5 | AVなし |
| I | 最小のURP/Lit Cube＋Collider | Application.Quit | 5 | 0 | 5 | AVなし |

最初のA〜I合計は`1/45`回でした。構成Bの4回目は`2026-08-01T23:50:26.6081892+09:00`に開始し、符号付き終了コード`-1073741819`（`0xC0000005`）でした。構成B専用のDumpは取得できませんでした。

## Stage Bの追加確認

| 条件 | 同じ構成B | 回数 | AV | Dump | 解釈 |
| --- | --- | ---: | ---: | ---: | --- |
| ProcDump First-chance AV Filter | はい | 20 | 0 | 0 | 監視がTimingへ影響した可能性。Call Stackなし |
| User Level WER LocalDumps、Debuggerなし | はい | 30 | 0 | 0 | 全て終了コード0。仮のWER Keyは確認後に削除 |

条件の異なる結果をまとめて1つの安定した発生率とはしません。結論は次のとおりです。

> Stage BはA～Iで唯一Access Violationを観測した最小構成候補。ただし1/5の低頻度であり、URPを直接原因とは断定できない。後続Stageで単調に再現率が上がらないため、タイミングまたはBuild layout依存の可能性がある。

## 確認済み・未確認・推定

- 確認済み：最初の5回でAVが出たのはA〜IのうちBだけです。
- 確認済み：Bの追加確認では再現せず、Dumpも作成されませんでした。
- 未確認：Bの障害関数、Native Call Stack、Stack Hash、UnityPlayer Offset。
- 未確認：Bの1件が製品版と同じ障害かどうか。
- 推定：監視、Timing、Serialized Data／Build Layoutが発生率へ影響する可能性があります。
- 判断していないこと：URPが直接原因であること。

構成Bが製品版と同じCall Stackであることを確認できなかったため、A／B／H／製品版の大規模な繰り返し比較は実施していません。
