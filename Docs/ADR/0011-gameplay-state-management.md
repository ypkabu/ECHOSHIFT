# ADR 0011：ゲーム状態の管理

## 状態

採用済み。

## 採用した方法

通常のC#クラス`GameplayStateController`に、`Booting`、`Playing`、`LoopTransition`、`SectionTransition`、`Paused`、`Completed`を明示します。許可した遷移だけを表にまとめ、重複や不正な遷移を拒否します。シーン側の管理処理が状態に合わせて`LoopDirector`の停止・再開とUIを更新し、シミュレーション側はPanel、Time Scale、有効なGameObjectから状態を推測しません。

`Completed`ではシミュレーションとセクション進行を終了しますが、完了画面を表示するため`Paused`へ移れます。一時停止元が`Playing`か`Completed`かを記録し、再開時は元の状態へ戻します。完了後は最終セクションを再開せず、最初から再開と2段階の終了確認を利用できます。

## 理由

明示的な状態遷移により、二重遷移、完了後のEcho生成、一時停止の時間処理、再開動作をFrameworkやGlobal Singletonなしでテストできます。

## 検討した別案

- Boolean Flagの組み合わせ：矛盾する状態を作れます。
- Animatorによる進行：表示側がシミュレーションのライフサイクルを持つことになります。
- 汎用State Machine Package：現在の規模には不要です。

## 現在の制約

一時停止はシーン内だけの状態で永続化しません。完了画面は別のFrontend状態ではなく、ゲーム処理を再開しません。Loading画面とセーブ状態の復元はありません。

## 見直す条件

非同期読み込みや永続化するFrontend状態に階層的・並行な状態が必要になった場合に置き換えます。
