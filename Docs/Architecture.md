# 設計

## Assemblyの依存方向

```text
EchoShift.Runtime
    ^
    +-- EchoShift.Editor
    +-- EchoShift.Tests.EditMode
    +-- EchoShift.Tests.PlayMode
```

Runtime側はEditor用・テスト用のAssemblyに依存しません。`Unity.InputSystem`を参照するのは入力アダプターと入力アセットだけで、シミュレーション、移動、リプレイ、インタラクションは`IInputSource`に依存する構成です。

## fixed tickと入力

`LoopDirector.Update`から、時間尺度の影響を受けない経過時間を`SimulationClock`へ渡します。1 tickでは、前のtickで確定したドア状態の反映、古いEchoから順に記録フレームを取得、プレイヤー入力の取得、移動、物理状態とセンサーの同期、インタラクション要求の収集と解決、装置状態の確定、位置・姿勢とReplay Driftの記録、ゴールとループ終了の判定を順番に処理します。

実際の入力には`EchoShiftControls.inputactions`の`Gameplay`アクションマップを使う`InputSystemInputSource`を使用します。テストでは同じ`IInputSource`へ決められた入力を渡し、1 tickずつ進められます。長い停止後に1フレームへ処理が集中しないよう、追いつき処理の回数には上限を設けています。

## キャラクターの処理順と要求の調停

`LoopActor`はリプレイの世代を保持します。`ActorSimulationOrder`は古いEcho、新しいEcho、現在のプレイヤーの順に並べます。`InteractionConflictResolver`はtick、キャラクターの順序、Stable ID、操作種別で要求を整列し、同じtickで最初に要求された対象を予約します。同じ対象への後続要求は`TargetBusy`として1回だけ失敗し、近くの別の対象で代用したり、次のtickで再試行したりしません。

同時に扱う要求は、最大3体のEchoとプレイヤー1体に合わせて4件に固定しています。詳しい判断理由は[ADR 0008](ADR/0008-interaction-conflict-resolution.md)に記録しています。

## リプレイ処理の責務

`ReplayRecorder`は、次の2つの領域をあらかじめ確保します。

- 入力と期待する位置・姿勢を毎tick保存する移動フレーム
- tick、操作種別、対象のStable ID、検証位置を保存する成功済みインタラクション

ループ確定時は、実際に記録した範囲だけを`ReplayRecording`と`InteractionRecording`へコピーします。利用側はインデックスによる読み取りだけができ、記録元の領域は取得・変更できません。途中で終了した記録では、移動フレームの範囲外にあるインタラクションを受け付けず、再生もしません。

## 記録したインタラクションの流れ

`InteractionSensor`は、あらかじめ確保した配列を使って`Physics.OverlapSphereNonAlloc`を実行します。候補は物理演算の戻り順に依存せず、利用可能か、距離の二乗、正面方向との一致、Stable IDの順で比較します。`Interactor`は現在の候補に操作を行い、成功した場合だけ確定後に変更できない入力として返します。

記録対象の`StableId`は、シーン単位の`InteractionRegistry`へ有効化時に登録し、無効化・破棄時に解除します。Echoは記録したtickでStable IDを1回だけ解決して操作します。対象がない、無効、使用中、範囲外などの場合は理由とともに失敗を1件記録します。シーン全体を検索したり、近くの対象を選び直したり、対象へ自動移動したり、毎tick再試行したりはしません。

## バッテリー・ソケット・ドア

`CarryableBattery`が、保持者と挿入先ソケットの状態を管理します。保持中のバッテリーはキャラクター階層の外に置き、Colliderを無効にしたうえで`LateUpdate`にCarry Socketの位置・姿勢を反映します。これにより、Echoの階層を破棄してもバッテリーまで失われません。挿入時はPowerSocketの挿入位置へ取り付けます。自由なRigidbodyの速度はリプレイ対象にしません。

`PowerSocket`は挿入状態と通電状態を持ち、`IDoorOpenSource`を実装します。`DoorController`は複数の開放条件を受け取れます。統合シーンでは、tick Nで確定した条件をtick N+1の冒頭でドアのTransformと衝突判定へ反映します。

## ループ切り替え

ループ切り替えは同期処理とし、多重実行を防ぎます。

```text
シミュレーションを停止
移動とインタラクションの記録を確定
プレイヤーとEchoが保持する物体を解放
バッテリー、ソケット、ドア、感圧板、ゴールを登録順に復元
既存のEchoをtick 0へ戻す
必要なら最も古いEchoを削除し、新しいEchoを作成
プレイヤーを開始位置へ戻す
新しい記録をtick 0から開始
シミュレーションを再開
```

通常のループ切り替えではシーンを再読み込みせず、Trigger Exitや無効化時のコールバック順に状態復元を依存させません。

## シーン設定とループ履歴

短い検証用シーンは600 tick、協力動作を確認するシーンは900 tickの設定を使います。記録領域は現在のシーン設定に合わせて確保し、誤って大きな領域を確保しないよう36,000 tickを超える設定を拒否します。

`LoopHistory`は最大16件の集計結果だけを保持し、`ReplayRecording`やUnityオブジェクトへの参照は持ちません。古いEchoが削除された後も値として結果を確認でき、上限を超えた履歴は古いものから削除します。

## 衝突処理とUnity側の境界

`CharacterMotor`はEnvironmentレイヤーへX方向、Z方向の順にCapsuleCastを行い、Transformを直接更新します。プレイヤーとEchoには別々の非Triggerレイヤーとkinematic Rigidbodyを使用します。Layer Collision Matrixでプレイヤー同士、プレイヤーとEcho、Echo同士の接触を無効にし、環境、InteractionTrigger、InteractionTargetとの関係は残します。

時計、入力、記録、候補比較、Replay Driftの計算は通常のC#クラスが担当します。MonoBehaviourはInput System、Transform、Collider／Trigger、ライフサイクル、Gizmos、OnGUIとの接続を担当します。Stable IDの生成・修復、レイヤー設定、シーン生成、ビルド処理は`EchoShift.Editor`に分離しています。

## ゲーム進行

`SectionTransitionCoordinator`がシーン全体の進行を管理し、各`PuzzleSectionController`がセクションごとの`LoopDirector`、プレイヤー、ゴール、開始位置、ルートを持ちます。状態は`Booting`、`Playing`、`LoopTransition`、`SectionTransition`、`Paused`、`Completed`に限定し、重複した遷移や許可されていない遷移を拒否します。

セクション切り替えでは、保持中の参照とEchoを解放し、現在の`LoopDirector`を停止し、登録済みの状態を復元してから次のルートだけを有効にします。シーンの再読み込みは行いません。完了後に一時停止画面を開いても最終シミュレーションは再開せず、セクション再開ボタンを無効にします。

## 表示・演出と入力案内

`SectionCameraController`はCinemachineを追加せず、減衰を設定できる見下ろしカメラを提供します。セクションごとの表示範囲、進行方向への先読み、プレイヤーとEchoを収める範囲でのズームを行いますが、ゲーム上の座標は変更しません。

`GameplayHud`、`PauseMenuController`、`TutorialGuide`は`Phase3TextCatalog`にまとめた文字列を使用します。`InputSystemInputSource`は最後に使用したキーボード／マウスまたはゲームパッドの種別を記録し、ゲームへの入力を変えずに案内表示を切り替えます。MaterialPropertyBlock、LineRenderer、Echoの軌跡で対象、配線、ドア状態、Echoの世代、再生終了を示し、毎フレームのマテリアル生成を避けています。

## ローカル計測

`PlaytestTelemetry`は、ビルド／Unityバージョン、セクション時間、ループ回数、終了理由、再開回数、インタラクション結果、最大Replay Drift、最終セクション、完走結果だけを集計します。JSONは`Application.persistentDataPath/EchoShiftPlaytests`へ保存します。リプレイ本体、tickごとの入力、位置、Stable ID、Unityオブジェクト参照、個人情報は保存しません。

## ゲーム処理と表示・演出の分離

表示・演出は、ゲーム処理を持つルートの子または隣接オブジェクトとして構成します。装置用アダプターは`PressurePlate`、`DoorController`、`CarryableBattery`、`PowerSocket`、`GoalVolume`の状態を読み、表示用Transformと`MaterialPropertyBlock`だけを変更します。`LoopDirector.EchoRemoved`は演出通知であり、リプレイ、処理順、Echo削除、リセットには影響しません。

`Phase4FeedbackDirector`はイベント購読、セクション完了演出の重複防止、ドア状態の監視、固定数のParticleSystemと8個の再利用可能なAudioSourceを管理します。一時的な演出もプールし、同時に動作するParticleSystemは最大4個です。実行中のマテリアル生成、上限のないParticle生成、毎フレームの階層検索は禁止しています。

プレイヤーとEchoのルートには、従来どおりCollider、kinematic Rigidbody、レイヤー、移動、Carry Socket、Stable IDを置きます。頭・胴体・手足・バイザー・軌跡・世代表示を持つColliderなしの表示用階層は、その下に分離しています。`Phase4RobotPoseController`は移動量、保持状態、操作成功数、Echoの再生終了を読み、表示用Boneだけを動かします。Root Motion、Animation Event、ゲーム処理を動かすAnimation Curveは使用しません。

`DoorVisualFeedback`は左右へ格納する表示用パネルを動かしますが、`DoorController`とColliderは従来の1 tick単位の規則を保ちます。床の回路表示もColliderなしの薄いMeshで、ゲーム上の判定には参加しません。

## 外部素材との境界

選定したCC0素材とライセンス表示は`Assets/_Project/ThirdParty`と`ThirdPartyNotices`に保存し、`Phase4ExternalAssetCatalog`から使用範囲を限定します。Import処理ではRig None、Animation／Root Motion無効、Collider自動生成なし、Texture上限、プロジェクト所有のURP Material利用を固定します。

`Phase4ExternalAssetBuilder`は`Assets/_Project/Art`へ表示用Prefabを生成します。外部FBXやMaterialがStable ID、レイヤー、`CharacterMotor`、Trigger、保持状態、リセット処理を持たない構成にしています。

## フォント、ライティング、描画

日本語表示には、必要な文字を収録した固定のTextMeshPro Atlasと、既存のUnity UI／TextMesh用に同梱した`Font`を使います。実行環境のOSフォント検索は行いません。Noto Sans JPの原本とOFL表示はプロジェクト内に保存しています。

ライティングはSoft Shadowを使うDirectional Light 1灯、セクションごとにShadowなしのLocal Lightを最大2灯、環境光とFog、URP Volume 1つに制限しています。VolumeにはACES Tonemapping、Color Adjustments、弱いBloomとVignetteを設定し、Motion Blur、Chromatic Aberration、ゲームプレイ中のDepth of Fieldは使用しません。

`GameplayHud`は変更のない値を再利用し、計測前にタイマー文字列を用意することで、定常時の毎フレーム文字列生成を避けます。性能計測では最大3体のEchoを準備し、定常時と遷移時を分けて記録します。

## 検証範囲

2026-10-03に画像・音声を再生成し、古い検査baselineとContact Sheet生成の不整合を修正した後、EditMode `151/151`、PlayMode `131/131`を確認しました。再生成前の2026-09-29はEditMode `146成功 / 5失敗`で、5件はGit管理外の成果物に依存します。全3セクションの自動完走、インタラクション`4`回成功／`0`回失敗、最大Replay Drift `0 m`、Windows BuildReport成功は再確認しましたが、画面付きPlayerのnative終了時クラッシュは未解決です。[最新の検証範囲](ValidationSummary.md)と[テスト計画](TestPlan.md)を参照してください。
