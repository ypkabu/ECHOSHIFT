# ADR 0007：決定的なtick処理順

## 状態

採用済み。

## 採用した方法

`LoopDirector`が各tickを明示的に並べます。前のtickで確定したドア状態を反映し、Echoの記録とプレイヤー入力を取得し、古いEchoから新しいEcho、最後にプレイヤーを移動させます。その後に`Physics.SyncTransforms`、感圧板とインタラクションセンサーの更新、要求の収集・解決、ドア条件の確定、位置・姿勢とReplay Driftの記録、ゴールとループ終了の判定を行います。

tick Nで確定したドア条件は、tick N+1の冒頭で衝突判定へ反映します。感圧板とゴールは通常利用向けのTrigger Callbackを残し、統合シーンでは決められた段階で上限付きの`OverlapBoxNonAlloc`による更新も行います。

## 理由

MonoBehaviourの`Update`、Trigger Callback、登録順、GameObject生成順だけでは、複数キャラクターを含む一連の処理順を保証できません。シーン単位の管理処理で順番を明示しつつ、移動、操作、装置、リプレイの責務は分離します。

## 検討した別案

- Script Execution Order：順番がComponentの設定へ分散し、追いにくくなります。
- 全ゲーム規則を持つ巨大なSimulation Manager：無関係な処理まで集中します。
- 別のUnity Physics Scene：kinematic主体の現在のプロトタイプには過剰です。

## 現在の制約

Transformを使うCapsuleCastと明示的な物理同期は、このシーンと環境で再現性がありますが、環境をまたいでbit単位に一致する物理エンジンではありません。運搬中の見た目は`LateUpdate`で更新しますが、所有や挿入の状態はその表示更新に依存しません。

## 見直す条件

自由なRigidbodyのリプレイ、Network Lockstep、環境をまたぐbit単位の一致、Additive Sceneでのシミュレーションが必要になった場合に置き換えます。
