# テスト計画

この文書では、現在のプロトタイプに対する回帰テスト、実際のシーンを使った完走確認、性能計測、Windowsビルドの確認方法をまとめます。終了コードだけで判断せず、テスト結果XMLとログの内容も確認します。

## EditModeテスト

2026-09-29のクリーンな公開ソースでは`146成功 / 5失敗 / 0 Skip`でした。開発時の`151/151`とは区別します。失敗5件はGit管理外の画像・音声成果物を検査するテストです。主な確認範囲は次のとおりです。

- fixed tick、入力記録、記録上限、短い記録、確定後のデータ変更防止
- Stable IDの未設定・重複検出と、記録したIDによる対象の一意な解決
- インタラクションの記録順、記録範囲、競合時の優先順位と`TargetBusy`
- バッテリーの重複所有防止、バッテリー／ソケット／ドアのリセット整合性
- ループ履歴の上限、Echoの世代順、ドア状態を次のtickで反映する規則
- ゲーム状態の正しい遷移、不正遷移の拒否、一時停止中のシミュレーション停止
- シーン再生成の再現性、Stable ID、Build Settings、Missing Script / Reference
- 日本語表示、入力案内、フォント、画面幅に応じた折り返し、ポーズ画面の収まり
- 外部素材の出典・ハッシュ・Import設定、表示用オブジェクトにColliderがないこと
- VFX、AudioSource、マテリアル、カメラ、HUDの個数・参照・割り当て上限
- シーン生成前後のアセットSHA-256確認と、想定外の変更がある場合の停止

## PlayModeテスト

現在のPlayModeテストは`131/131`件が成功しています。主な確認範囲は次のとおりです。

- プレイヤーとEchoの移動、短い記録の終端、Echo上限、衝突レイヤー
- 感圧板、バッテリーの取得・運搬・挿入、ソケット、ドア、ゴール
- プレイヤー／Echo間の所有競合、対象の無効化・再有効化、ループ時の状態復元
- 記録したStable IDだけを使用し、近くの別の対象で代用しないこと
- 3セクションをシーン再読み込みなしで進行し、全区間を自動完走できること
- 一時停止、セクション再開、最初から再開、完了後のメニュー、2段階の終了確認
- 日本語の目的表示、状況別の入力案内、失敗理由、デバッグ表示の初期状態
- カメラ、HUD、ロボットの姿勢、ドア表示、回路表示がゲーム処理を変更しないこと
- 最大3体のEcho、世代表示、VFX・音響のプール上限、重複再生の抑制
- プロジェクト内の全シーン読み込み、Missing Component / Reference、予期しないログ

## 実行コマンド

PowerShellでリポジトリのルートから実行します。メニュー名、クラス名、引数名は実装上の識別子であるため、そのまま記載しています。

WindowsのGUIアプリは呼び出し方によって呼び出し元が先に戻ります。下記の各実行後、次の工程へ進む前に対象Unity processの終了、XML、ログを確認してください。今回の自動検証は `Start-Process -PassThru -WindowStyle Hidden` で取得したprocessを `WaitForExit()` で待ち、実際の終了コードを取得しました。**Playerの画面付き検証ではHiddenを使わず**、完走マーカーだけを正常終了の根拠にしません。

```powershell
$unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.6f1\Editor\Unity.exe'
& $unity -batchmode -nographics -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.P3SceneBuilder.BuildFromCommandLine -logFile "$PWD\Logs\scene-builder.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform EditMode -testResults "$PWD\TestResults\editmode.xml" -logFile "$PWD\Logs\editmode.log"
& $unity -batchmode -nographics -projectPath "$PWD\Unity" -runTests -testPlatform PlayMode -testResults "$PWD\TestResults\playmode.xml" -logFile "$PWD\Logs\playmode.log"
& $unity -batchmode -force-d3d11 -quit -projectPath "$PWD\Unity" -executeMethod EchoShift.Editor.Phase4BuildPipeline.BuildPhase4ThreeWindowsDevelopment -logFile "$PWD\Logs\build.log"
& "$PWD\Builds\Phase4_3\ECHOSHIFT_Phase4_3.exe" -batchmode -force-d3d11 -screen-width 1920 -screen-height 1080 -phase4PerfProbe -logFile "$PWD\Logs\performance.log"
```

Unity Test FrameworkはXMLを書き出した後に終了するため、テスト実行コマンドには`-quit`を付けません。シーン生成とビルドは終了コードに加えてログ内の警告・エラー・完了マーカーを確認します。性能計測はNull Graphics Deviceを使わず、取得できないカウンターを推定値で補いません。

## 最新の再検証結果（2026-09-29）

[検証結果の要約](ValidationSummary.md)に対象commit、実行時間、失敗したテスト名とEditor起動時診断を記録しています。

| 確認項目 | 結果 |
| --- | --- |
| EditMode | 146成功 / 5失敗 / 0 Skip、終了コード2 |
| PlayMode | 131成功 / 0失敗 / 0 Skip、終了コード0 |
| Windows x86_64 Development Build | BuildReport Success、エラー0・警告0、終了コード0 |
| 画面付きPlayer | 全3セクション完走、インタラクション4/0、Replay Drift 0 m |
| Player終了 | `-1073741819 (0xC0000005)`、配布不可 |

### Phase5A成果物依存テストの準備

`Captures/Phase5A/` はGit管理外です。開発時の成果物検査を実行するには、描画可能なEditorで以下の既存メニューから画像・音声を生成する必要があります。

1. `ECHO SHIFT/Phase 5A/Generate Identity Options` → `Options/A`、`B`、`C`
2. `ECHO SHIFT/Phase 5A/Generate Selected Revision` → `SelectedRevision`
3. `ECHO SHIFT/Phase 5A/Generate Selected Revision 2` → `SelectedRevision2`
4. `ECHO SHIFT/Phase 5A/Generate Selected Revision 2.1` → `SelectedRevision21` と `Audio`

実装入口は[Phase5AIdentityPreviewBuilder](../Unity/Assets/_Project/Scripts/Editor/Phase5AIdentityPreviewBuilder.cs)と同ディレクトリのSelectedRevision各Builderです。これらはキャプチャだけでなくプレビューScene・Materialも再保存します。作業前に差分がない検証用コピーを使い、生成後の差分と本番canonical hashを確認してください。今回の公開ソース再検証では、納品済みの本番アセットを不用意に再保存しないため、この再生成を実行していません。**上記の準備手順を含む151件の全成功は今回未確認**です。

## 開発時の検証結果

以下の成功数・性能値・動画寸法は既存の開発時記録です。今回、性能計測や動画の再録画を実施したものではありません。

| 確認項目 | 結果 |
| --- | --- |
| EditMode | `151/151` 成功 |
| PlayMode | `131/131` 成功 |
| 全3セクションの自動完走 | 成功 |
| インタラクション | `4`回成功 / `0`回失敗 |
| 最大Replay Drift | `0 m` |
| Missing Component / Reference | `0` |
| コンパイラーのエラー / 警告 | `0 / 0` |
| Missing / NullReference / 未処理例外 | `0 / 0 / 0` |
| Windows x86_64 Development Build | BuildReport Success |

最大3体のEchoを出したD3D11計測では、600フレームの平均／95パーセンタイル／最大が`8.342 / 8.405 / 8.639 ms`、Main Threadの平均／最大が`8.337 / 8.637 ms`、定常時GCが`0 B/frame`でした。遷移時の最大値は初回`16.887 ms`、再計測`18.038 ms`で、定常時とは分けて記録しています。Draw Callsなど、この環境で取得できなかった値は未計測として扱っています。

表示・演出では、VFXを最大4システムに制限し、パルスの最大サイズ`0.32 m`、最大アルファ`0.42`、Bloom強度`0.22`、しきい値`1.35`を確認しています。最終確認動画は1920x1080 / 30 fps、39.966667秒で、必要な12場面を含みます。

## 検証結果の推移

途中の実装で取得した値を履歴として残しています。最新のクリーン実行結果は上段の「最新の再検証結果」を参照してください。

### ゲーム進行と日本語表示

Unity `6000.4.6f1`で2026-07-21に確認しました。

- EditMode `61`成功／`0`失敗／`0`Skip、`1.8848759 s`
- PlayMode `61`成功／`0`失敗／`0`Skip、`16.4244913 s`
- 全検証シーンの最大Replay Drift `0 m`
- 3つのセクションを1,081 tickで完走し、インタラクション`4`回成功／`0`回失敗
- Windows x86_64 Development Build：BuildReport成功、警告`0`、289 File、166,334,282 bytes
- EXE SHA-256：`098A43C3B20762E4BDF938771C36F0FB116126AEC8932B2A77EB403F0CB77938`
- Headless Playerで`language=ja-JP`、`font=Noto Sans JP`、`glyphs=True`、HUD、Schema 1のJSON保存、終了コード`0`を確認
- 画面付きの通し確認は10分53.2秒、各セクション2分08秒／3分33秒／4分24秒、ループ2／3／3、再開`0`

### モジュール式の表示・演出

Unity `6000.4.6f1`、URP `17.4.0`で2026-07-21に確認しました。

- EditMode `84`成功／`0`失敗／`0`Skip、`8.059572 s`
- PlayMode `87`成功／`0`失敗／`0`Skip、`19.2221506 s`
- P0／P1／P2のScene SHA-256は`1411EB0E...24C5`、`4AADD3D3...EBB`、`73EC41AA...22C`のまま変化なし
- 全シーンの最大Replay Drift `0 m`、3セクション完走、インタラクション`4`回成功／`0`回失敗
- BuildReport成功、警告`0`、エラー`0`、291 File、178,845,266 bytes
- D3D11、最大3体のEcho、1920x1080、600フレーム：平均`8.339 ms`、95パーセンタイル`8.359 ms`、最大`8.581 ms`
- Main Thread平均`8.335 ms`、最大`8.596 ms`、ループ切り替え最大`14.093 ms`、定常時GC `0 B/frame`
- 最大使用Memory `105,811,560 B`
- Draw Callsは取得できず、SetPassは`0`を返したため有効値として扱わず
- 1920x1080のキャプチャ8枚について、個数・寸法・File Sizeを確認

この時点の画面付きPlayerは、正常なUnity終了Messageの後に`0xC0000005`で終了しました。ゲーム内容の結果とは分けて、現在もWindows版の既知の課題として扱っています。

### 外部素材の組み込み

Unity `6000.4.6f1`、URP `17.4.0`で2026-07-22に確認しました。

- EditMode `93/93`、`28.2191387 s`
- PlayMode `93/93`、`24.4194633 s`
- 全シーンの最大Replay Drift `0 m`。インタラクション成功／失敗は順に`2/0`、`2/0`、`4/0`
- Development Build：291 File、204,467,042 B
- Non-Development Build：182 File、140,785,117 B
- D3D11、最大3体のEcho、1920x1080、600フレーム：平均`8.337547 ms`、95パーセンタイル`8.336699 ms`
- Main Thread平均`8.329742 ms`、遷移最大`15.721394 ms`、定常時GC `0 B/frame`
- 最大使用Memory `168,041,477 B`、Texture Memory `40,882,741 B`
- Draw Callsは取得できず、GPU Frame Time、SetPass、Triangle、Vertexの`0`は有効値として扱わず
- 外部素材導入前後のキャプチャは各8枚、1920x1080、デバッグ表示なし

床から浮いて見える外周装飾を修正した後は、対象テスト`1/1`、EditMode `94/94`、PlayMode `93/93`でした。3セクションを1,091 tickで完走し、最大Replay Drift `0 m`、インタラクション`4/0`、Stable ID、Collider、Missing Component、既存シーンのHashを再確認しました。

### カメラ・HUD・画面内の読みやすさ

2026-07-22に確認しました。

- 1920x1080のキャプチャ8枚で、デバッグ表示とCursorがなく、必要なキャラクターが画面内に入ることを確認
- EditMode `99/99`、`25.5873039 s`
- PlayMode `105/105`、`23.096738 s`
- 警告／エラー`0/0`
- 3セクション完走、Replay Drift `0 m`、インタラクション`4/0`、テレメトリーJSONあり
- BuildReport成功、291 File、204,479,394 B
- 最大3体のEcho、600フレーム：平均／95パーセンタイル／最大`8.341 / 8.370 / 8.821 ms`
- Main Thread平均／最大`8.337 / 8.832 ms`
- Camera平均／最大`0.0077 / 0.0629 ms`
- UI平均／最大`0.0036 / 0.0532 ms`
- 定常時GC `0 B/frame`

### キャラクター・ドア・回路表示

2026-07-23に確認しました。

- EditMode `116/116`、`49.3721874 s`
- PlayMode `127/127`、`37.6364498 s`
- 失敗／Skip `0/0`、Unity終了コード`0`
- 全検証シーンと3セクションの表示経路が成功し、Replay Drift `0 m`、インタラクション`4/0`
- 最大3体のEcho、600フレーム：平均／95パーセンタイル／最大`8.342 / 8.388 / 8.883 ms`
- Main Thread平均／最大`8.331 / 8.894 ms`
- Camera平均／最大`0.0126 / 0.0432 ms`
- UI平均／最大`0.0070 / 0.0310 ms`
- Robot Pose平均／最大`0.0054 / 0.0389 ms`
- ドア表示平均／最大`0.0181 / 0.0423 ms`
- 定常時GC `0 B/frame`
- 遷移最大`16.887 ms`、再計測`18.038 ms`
- D3D11キャプチャ8枚と、30.025秒の通常描画確認で必要な9状態を記録
- Windows x86_64 Development Build：`PHASE4_3_BUILD_OK`、警告／エラー`0/0`、291 File

### VFXと動画

2026-07-31に確認しました。

- EditMode `120/120`、PlayMode `130/130`、失敗／Skip `0/0`
- Radial Alpha Textureを使う8 ParticleのPulse、最大サイズ`0.32 m`、最大Alpha`0.42`、同時実行最大4System
- Bloom強度`0.22`、しきい値`1.35`、Scatter `0.42`
- H.264動画は1920x1080、30 fps、39.966667秒。計測したゲームプレイ39.626秒、完了画面0秒、`Time.timeScale = 1`、必要な12場面を収録
- 画面全体で輝度`210`以上の占有率は最大`2.00%`、巨大な不透明白Particleは`0`
- 3セクションを1,070 tickで完走し、Replay Drift `0 m`、インタラクション`4/0`
- 最大3体のEchoで平均／95パーセンタイル／最大`8.342 / 8.405 / 8.639 ms`
- Main Thread平均／最大`8.337 / 8.637 ms`、定常時GC `0 B/frame`
- Build Marker `PHASE4_3_BUILD_OK`、警告／エラー`0/0`、Compiler／Missing／Null／Unhandled一致`0`
- 録画あり／なし、Pause、自動終了、Recorder初期化あり／なしの6条件すべてで、終了時の`UnityPlayer.dll`内`0xC0000005`を再現

## Windows版終了時の確認

画面表示を伴うWindows版は、終了時に`UnityPlayer.dll`のnative cleanup内で`0xC0000005`を再現します。ゲームプレイ、テレメトリー保存、テスト結果とは分けて未解決事項として管理しています。詳細は[Windows版終了時クラッシュの調査](Phase4WindowsExitCrashInvestigation.md)を参照してください。

テストXML、ログ、キャプチャ、ビルド、Unityの`Library`は、端末固有のパスや時刻を含む再生成可能な出力のためGit管理していません。
