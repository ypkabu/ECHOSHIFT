# アート方針

## テーマと雰囲気

ECHO//SHIFTの舞台は、時間複製実験のために作られた簡素な近未来研究施設です。管理された清潔な空間の外側に黒い空間を置き、わずかな不安感を加えています。建築にはQuaternius Modular Sci-Fi MegaKitから選定した素材を使い、プロジェクト側のURP Materialと表示用Prefabで統一感を保っています。

## 画面構成の優先順位

1. 装飾より先に、歩ける床とドアの開口部がゲーム用カメラから読み取れること。
2. プレイヤー、Echo、動作中の装置を周囲より高いContrastで示すこと。
3. 水色と橙色の回路で、感圧板／ドアとソケット／ドアの関係を示すこと。
4. 建築の色と形は、キャラクターのSilhouetteを邪魔しないこと。
5. 背景の機械、柱、外周枠で広い施設を想像できるようにし、カメラやゲーム用Colliderを増やさないこと。

## 形状のルール

- 環境：外部素材の床・壁Panel、Charcoal色のくぼみ、垂直な柱、開口枠、Console、数を抑えた小物。
- プレイヤー：プロジェクト所有のPrefabに入れたRobot Mesh。白とAmberを使い、正面のVisorと保持状態を見せます。
- Echo：同じRobotを基に、軌跡、寒色Emission、1〜3個の世代記号、再生終了時の減光を加えます。常時表示する名前Labelは使いません。
- 感圧板：低い水色の段差、1本の回路、1本線のドア記号。
- 電源：向きの分かる橙色のBattery、形を合わせたSocket、2本の回路、2本線のドア記号。
- ゴール：背の高い淡色のPortal Frameと床の光。装置形状や警告用の赤色と区別します。

## ライティングと仕上げ

形状を示すSoft Shadow付きDirectional Lightを1灯使います。各セクションのLocal LightはShadowなしで最大2灯とし、寒色と暖色の経路を分けます。Trilight Ambient、弱いFog、Bloom、Vignette、Color Adjustments、ACES Tonemappingで奥行きを補います。Motion Blur、Chromatic Aberration、ゲームプレイ中のDepth of Field、密度の高いParticle、極端な露出は使用しません。

## 確認範囲

自動テストでは、配布元とライセンスのハッシュ、選定したImport内容、アセット参照、設定値、キャプチャ、ビルド、性能を確認します。実画面では、外部素材との統一感、黒の見え方、動作中のSilhouetteとRobotの足、音量、代表画像としての分かりやすさを確認します。
