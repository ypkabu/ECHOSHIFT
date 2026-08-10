# ADR 0018：外部ビジュアル素材の採用範囲

## 状態

採用済み。[ADR 0014](0014-phase4-art-direction.md)の基本形状中心の案を置き換えます。

## 採用した方法

Quaternius Modular Sci-Fi MegaKit、Quaternius Animated Robot、Kenney Sci-Fi Soundsから、Manifestで指定した少数のCC0素材だけを使用します。未変更の原本は`Assets/_Project/ThirdParty`へ保存し、配布元、ライセンス、ハッシュを記録します。プロジェクト所有のURP Materialと表示用Prefabを`Assets/_Project/Art`へ生成し、ゲーム処理を持つルートにはそのPrefabだけを組み合わせます。

外部Mesh、Skeleton、Material、名前、階層はゲーム処理を持ちません。Stable ID、Collider、Layer、`CharacterMotor`、保持・リセット状態、リプレイ、セクション解法は既存のルートへ残します。RobotはRig None、Animation／Root Motion無効で読み込み、環境とキャラクターの表示用の子にはColliderを付けません。

## 理由

- 基本形状だけでは不足していたTopologyとTextureを補い、ゲーム処理は変更しません。
- 主な外部Kitを1つに絞って統一感を保ち、未使用素材を大量に取り込みません。
- 公式のCC0配布元を使用し、商用利用とビルド同梱を確認できます。
- プロジェクト所有のPrefabを境界にし、UnityのImport差や外部素材の更新からゲーム処理を分離できます。
- Import、ライセンス、Material、Collider、Stable ID、再生成、実シーンをテストできます。

## 検討した別案

- 基本形状だけを調整し続ける：形状と質感の不足を解消できませんでした。
- 無料Package全体をImport：容量、Shader／Material、未使用内容、確認範囲が増えます。
- 複数のAsset Store Packageを混在：ライセンス、Account依存、見た目の統一が難しくなります。
- 外部RobotのAnimatorをゲーム処理に使用：Animation／Root Motionがキャラクタールートを動かし、Replay Driftへ影響する可能性があります。
- 原本を直接変更：出所が追いにくく、再Import時の再現性が下がります。

## 現在の制約

- Robotは表示用の簡易姿勢を使用し、最終的なLocomotion、Interaction Animation、IKはありません。
- 外部Importerは`Foot.L`に自己交差するPolygonを1件報告します。自動確認で表示・ビルド上の問題は見つかっていません。
- Draw Calls、GPU Frame Time、SetPass、Triangle、Vertexはこの環境で有効な値を取得できませんでした。

## 見直す条件

追加・差し替え時は、公式配布元、再配布可能なライセンス、ハッシュ、Manifest、ライセンス表示、Import設定、プロジェクト所有Prefab、Colliderなしの表示境界、Stable IDとゲーム用Colliderの維持、シーン自動完走、Replay Drift、GC／性能、ビルドを再確認します。
