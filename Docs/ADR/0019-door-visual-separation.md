# ADR 0019：ドアのゲーム処理と表示の分離

## 状態

採用済み。

## 背景

`DoorController`は、明示した1 tick単位の規則でゲーム用ドアルートとColliderを動かします。そのルートに1枚の外部Panelを置くと、開いた位置で大きな板のように見え、ゲームの時間処理を変えずに自然な開閉を表現できませんでした。

## 採用した方法

`DoorController`、開放条件、ゲーム用ルート、Collider、1 tick単位の状態は変更しません。閉じたドア位置に、プロジェクト所有の表示用Assemblyを別オブジェクトとして置きます。左右のPanelは短い予告Lightingの後、時間尺度に影響されない一定時間で左右のFrame内へ格納します。閉じる場合は逆向きに動かします。

`DoorVisualFeedback`は確定済みのドア状態を読み、表示用Assembly、Panel Renderer、FrameのEmissionだけを変更します。感圧板とソケットで開くドアは異なる回路記号を使います。表示用MeshにColliderはなく、RaycastとStable ID解決にも参加しません。

## 理由

- ゲームの時間処理と衝突判定を独立してテストできます。
- 分割Panelは、ゲーム用Colliderの開いた位置まで付いていきません。
- 表示用の開閉を調整しても、新しいゲーム状態を追加せずに済みます。
- Scene Builderから同じ構成を再生成できます。

## 検討した別案

- `DoorController`でPanelもAnimation：表示とシミュレーションの責務が混ざります。
- Colliderを徐々に動かす：検証済みの1 tick単位のパズル規則が変わります。
- 古いPanelを即座に隠す：見た目の問題は消えますが、開いたことが伝わりません。

## 制約と見直す条件

短いTransform Animationで、最終的なVFX、音響、機械Rigではありません。最終ドア素材に開閉Partsが含まれる場合も、既存の1 tick単位のゲーム状態を表示側が読む境界を保ちます。
