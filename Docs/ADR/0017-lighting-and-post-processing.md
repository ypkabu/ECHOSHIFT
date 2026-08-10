# ADR 0017：URPライティングとPost-processing

## 状態

採用済み。

## 採用した方法

Soft Shadowを使うDirectional Lightを1灯、各セクションにShadowなしのLocal Lightを最大2灯、Trilight AmbientとFog、全体用のURP Volumeを1つ使用します。VolumeにはACES Tonemapping、弱いColor Adjustments、しきい値付きBloom、弱いVignetteを設定します。Motion Blur、Chromatic Aberration、ゲームプレイ中のDepth of Fieldは使用しません。

## 理由

- キャラクターと装置の形状はEmissionだけでなく実際のLightで示します。
- Light数を固定し、Module追加による実時間Lightの増加を防ぎます。
- 弱いPost-processingで全体の色を整えながら、Echoの世代と日本語HUDを読み取れる状態に保ちます。
- Scene Builderから同じ設定を再生成できます。

## 検討した別案

- Emissive／Unlitだけの表示：奥行きとSilhouetteが弱くなります。
- 多数のPoint／Spot Light：GPUとShadowの負担が増えます。
- 強いBloom、DOF、Motion Blur、Chromatic Aberration：Puzzle TargetとUIが見にくくなります。
- 全面Bake：頻繁に再生成する現在のシーンには不要です。

## 現在の制約

自動キャプチャと設定値テストだけでは、主観的な露出、黒つぶれ、眩しさ、画面差を判断できません。この環境ではDraw Callsを取得できず、SetPassは`0`を返したため有効な計測値として扱っていません。

## 見直す条件

実画面確認または対象PCでの計測により調整します。Volume設定、Shadow付きLight、Lightmap、Renderer Featureを追加する場合は、3解像度の画像、性能、全回帰テストを確認します。
