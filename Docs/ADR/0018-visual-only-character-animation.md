# ADR 0018：表示専用のProcedural Animation

## 状態

採用済み。

## 背景

Quaternius Robotでキャラクターの形は改善しましたが、読み込んだFBXには名前付きのIdle、Walk、Carry、Interact、Stopped ClipとAvatarがなく、T-poseのままでした。ゲーム上の移動とReplay Driftはfixed tickで動くルートが既に担当しています。

## 採用した方法

ゲーム処理を持つルートより下に`Phase4RobotPoseController`を置き、プロジェクト所有のRobot Wrapper Boneを動かします。ルートの移動量、保持状態、インタラクション成功数、Echoの再生終了を読み、ゲーム状態を書き換えずに6種類の表示用姿勢を補間します。

Animatorとプロジェクト所有のControllerも表示用の子だけに置き、Root Motionを無効にします。Animatorには6姿勢と同名の空Stateを用意し、実際のBone OffsetはAnimator更新後にProcedural Componentが反映します。Animation Eventとゲーム用Transform Curveは使用しません。

## 理由

- 第三者製FBXとImporter Metadataを変更しません。
- 意図と境界が不明なSource TakeからClipを推測しません。
- fixed tickのキャラクタールートがリプレイと衝突判定を担当し続けます。
- 状態名と参照をScene Builderから再生成し、テストできます。

## 検討した別案

- FBXのTakeを分割：元のClipの意図と境界が定義されていません。
- Root Motion：`CharacterMotor`と記録済みの位置・姿勢へ干渉します。
- Animation Rigging Package：現在の簡易表示には依存が大きすぎます。
- T-poseのまま使用：動作中の読みやすさを損ないます。

## 制約と見直す条件

簡易的な動きで、最終品質の接地やIKは行いません。最終Character Artにライセンスを確認した名前付きClipが揃った場合に置き換えます。置き換え後もゲーム処理を持つルートを移動させないことを検証します。
