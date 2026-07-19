# Phase 3 JA Black-Box Playtest Record

## Test Environment

- Build: `Runtime/ECHOSHIFT_Phase3_JA.exe`（画面右下に `Development Build` 表示）
- Unity Version: ブラックボックス画面内に表示なし
- Date and time: 2026-07-19 18:14-18:29 JST
- Input device: キーボード、マウス
- Resolution: 1280x720 client、windowed（OS表示倍率 150%）
- GUI operation method: 可視ウィンドウを画面認識し、通常の Windows マウス入力とキーボードのスキャンコード入力を送信
- Source-blind constraint maintained: Yes。指定された `起動説明.md`、`結果記録テンプレート.md`、Runtime、Screenshots だけを使用。Unity Project、ソース、YAML、Docs、Git、既存 Logs、プレイ中 Logs、Debug Overlay/F3 は参照していない
- OS dialog before timing: Windows Security / Firewall ダイアログは表示されなかった。ゲーム画面を完全に確認した約18:14から計時
- Operation-environment note: 起動直後はウィンドウが foreground でなく、最初の通常送信入力は届かなかった。画面への通常クリックで foreground を確認後、物理キー相当のスキャンコード入力では移動と ESC が反応した。この初期フォーカス事象はゲーム内の観察と分離する

## Result

- Completion result: **PRETEST FAIL**
- Total time: 15:00 の実プレイ上限（18:14頃-18:29頃）。上限後は Pause 状態のまま Quit の最終確認だけを行い、攻略操作はしていない
- Section 1 time / loops: 15:00 / 最終画面表示は Loop 29、未完了
- Section 2 time / loops: 未到達 / N/A
- Section 3 time / loops: 未到達 / N/A
- Restart Section count: 0回実行。Pause Menu から実行を試したが反応せず、Loop/Section表示は変わらなかった
- Interaction failure count: 画面で確認できたゲーム内の明確な未達成試行 3回（閉じた扉で停止）。Pause Menuの入力失敗は別件として記録
- Pause Menu usable: 部分的。ESCで開く/閉じることはできたが、表示されたボタンは通常クリックおよびキーボード操作で反応しなかった
- Quit successful: No。Pause Menu の「ゲームを終了する」を通常クリックおよびキーボード操作で試したが終了しなかった。指定に従い代替の強制終了は行わず、ゲームは Pause 状態のまま
- Verdict basis: Section 1未完走、Section 2/3未到達、Pause Menu Quit不能のため FAIL。GUI画面認識とゲーム内移動入力自体は可能だったため NOT EXECUTABLE とは判定しない

## Immediate notes

- First understood control: 開始画面で最初に理解できたのは `R / START: ループを終了` と `ESC / SELECT: 一時停止`。移動キーは画面に表示されず、W移動を確認できたのは試行後
- First unclear display: `エコー 0/3` の意味、プレイヤーと色違いのActorの対応、色ごとの役割
- When the Echo behavior became understandable: 完全には理解できなかった。Loop 20以降、色違いActorが以前の移動らしき軌跡を繰り返すことは画面から推測できたが、どのLoop/色がどの記録か確信できなかった
- When the Battery/Socket relationship became understandable: Section 2未到達のため N/A
- Could distinguish the two Echo roles: Section 3未到達のため N/A。Section 1内でも色と記録Loopの対応は判別困難
- Camera-lost object: プレイヤーまたはEchoが手前壁の後ろ、画面下端、扉付近で隠れた（例: `Screenshots/15_OnSwitchAttempt.png`、`Screenshots/19_Section1_EndAttempt.png`）
- HUD-obscured object: 画面下中央の指示/キー表示と、下端にいるプレイヤーまたはEchoが重なり、輪郭が読み取りにくい場面があった
- Unclear Japanese: `R / START`、`ESC / SELECT` はキーボードとゲームパッド表現が混在しているが、移動キーの説明がない。`残り時間 08,9` の小数カンマと単位なしも日本語UIとして不自然
- Clipped or unreadable Japanese: ゲーム内で明確なクリップは未観察。配布物 `起動説明.md` は Windows PowerShell 5.1 の既定読込では文字化けした。試験終了後の明示的UTF-8読込では正しい日本語を確認したため、内容破損ではなく文字コード互換性の事象
- Interaction failure reason understandable: No。扉が赤いまま停止することは見えたが、必要条件、Echoの担当、失敗理由を示すメッセージはなかった

## Observations

- Understood: Section 1の目標は青いスイッチで扉を開くこと。Loop 23で黄色Actorが青いスイッチ上に乗った瞬間、扉が赤から緑になったため、スイッチと扉の因果関係は画面だけで理解できた（`Screenshots/17_OnSwitchPossible.png`）
- Confusing: 移動操作が未表示、Loopが自動で進む、Echoが早期に3/3へ到達、色と記録Loopの対応が未説明、カメラ角度が大きく変わる、Pause Menuボタンが反応しない
- Missed display: WASDなどの移動キー、プレイヤー色、Echo色/番号、現在再生中の記録Loop、スイッチ保持中の担当を示す凡例。最初のEcho生成瞬間は操作入力の確認中に経過し、最寄りの保存は Loop 4 / Echo 3/3 の `Screenshots/02_After_W.png`
- Unnatural Japanese: `R / START` と `ESC / SELECT` の混在、`残り時間 08,9`。`起動説明.md` はUTF-8を明示すれば正しい日本語だが、Windows PowerShell 5.1の既定読込では文字化けした
- Camera / HUD / Echo identification: カメラ回転と手前壁による遮蔽で同じActorを追跡しづらい。黄色を現在プレイヤー、紫/水色/緑等をEchoと推測したが、画面内の明示はない。下中央HUDは下端Actorと競合する
- Difficulty: オンボーディング段階として高すぎる。標準的なWASDを推測しても、Echoの記録とスイッチ保持の組み立てを15分内に確信できなかった
- Tempo: 短いカウントダウンが説明を読む間も進行し、入力確認中にEcho 3/3へ到達した。理解の試行よりLoop消費が先行し、学習テンポが急

## Issues

### Issue ID: BBJA-001

- Severity: **High**
- Section: System / Pause Menu
- Reproduction steps: 可視ゲームウィンドウをforegroundにし、ESCでPause Menuを開く。画面上の「このセクションをやり直す」または「ゲームを終了する」を通常クリックする。さらに S/Down と Enter/Space/E による選択・決定も試す
- Expected: Restart SectionがSection 1を初期化する、またはQuitがStandaloneを正常終了する。選択中/hover状態が視覚的に分かる
- Actual: ESCでPause表示/解除はできるが、ボタン入力では画面・Loop・プロセス状態が変化しなかった。Restart 0回、Quit失敗
- Screenshot: `Screenshots/20_Menu_EConfirm.png`
- Proposed fix: StandaloneのEventSystem/Input Systemでマウスポインタ座標、button click、navigation、submit actionを検証する。キーボードフォーカスの初期選択、hover/selected表示、Enter/Space決定を明示的に用意する
- Confidence in proposed fix: Low-Medium。foreground確認後も複数方式で再現した事実はあるが、注入入力とUnity Input Systemの互換性を実機の物理マウス/キーボードで切り分けていないため、ゲーム不具合とは断定しない

### Issue ID: BBJA-002

- Severity: **High**
- Section: Section 1 / Onboarding
- Reproduction steps: 新規起動し、Section 1開始画面のHUDとチュートリアルだけを読む
- Expected: 最低限の移動キーと、必要な操作が日本語で画面内に表示される
- Actual: 表示されるキーはRとESCのみ。移動キーがなく、最初の入力確認中にLoopが自動進行してEcho 3/3へ到達した
- Screenshot: `Screenshots/01_Section1_Start.png`、`Screenshots/02_After_W.png`
- Proposed fix: 初回Loopに `WASD: 移動` を常時または段階表示し、最初の移動入力を確認するまでタイマーを開始しない。入力デバイスに合わせて表記を切り替える
- Confidence in proposed fix: High

### Issue ID: BBJA-003

- Severity: **Medium**
- Section: Section 1 / Echo identification
- Reproduction steps: 複数Loopを経過させ、`エコー 3/3` と複数色Actorを同時に見る
- Expected: 現在プレイヤー、各Echo、元になったLoop、再生中の役割を瞬時に区別できる
- Actual: 色は異なるが凡例・番号・軌跡がなく、カメラ回転と重なりもあってどの記録を再生しているか追跡できなかった
- Screenshot: `Screenshots/18_DoorRunWithEcho.png`
- Proposed fix: プレイヤーに固定色/リング、Echoに `E1`-`E3` ラベルと同色HUD、短い残像または記録Loop番号を表示する
- Confidence in proposed fix: Medium

### Issue ID: BBJA-004

- Severity: **Medium**
- Section: Section 1 / Camera and HUD
- Reproduction steps: 開始地点からスイッチ、扉付近へ移動し、カメラが回転した状態で手前側へ戻る
- Expected: プレイヤー、Echo、スイッチ、扉が壁に隠れず、同じ画面座標関係で追跡できる
- Actual: 手前壁と画面下端でActorが一部または大部分隠れ、カメラ角度変更で方向感覚も崩れた。下中央HUDとActorが競合した
- Screenshot: `Screenshots/15_OnSwitchAttempt.png`、`Screenshots/19_Section1_EndAttempt.png`
- Proposed fix: Section 1では固定または弱い追従角度にする。手前壁を透過/低くし、重要Actorを壁越しアウトライン表示する。下中央HUDに安全余白を設ける
- Confidence in proposed fix: Medium-High

### Issue ID: BBJA-005

- Severity: **Low**
- Section: Launch documentation
- Reproduction steps: 配布フォルダの `起動説明.md` を標準テキストとして開く
- Expected: 実行ファイル名とブラックボックス試験注意事項が読める日本語で表示される
- Actual: ファイルはBOMなしUTF-8で、Windows PowerShell 5.1の既定読込では見出しと本文が文字化けした。試験終了後にUTF-8を明示して読み直すと正しい日本語だった
- Screenshot: N/A（文書の観察。デスクトップ情報を含む画像は保存しない）
- Proposed fix: Windows PowerShell 5.1など旧来の既定デコーダも配布対象ならUTF-8 BOMを付けるか、起動説明の文字コードを明記する。現代的なUTF-8自動判定環境だけを対象とするなら変更不要
- Confidence in proposed fix: High

### Issue ID: BBJA-006

- Severity: **Low**
- Section: Runtime UI / Japanese localization
- Reproduction steps: Section 1の残り時間表示を見る
- Expected: 日本語環境で自然な `残り時間 8.9秒` などの表記
- Actual: `残り時間 08,9` のように小数カンマで、単位がない
- Screenshot: `Screenshots/01_Section1_Start.png`
- Proposed fix: 日本語ロケール向け表示を `0.0秒` に統一し、先頭ゼロの要否もUI仕様として決める
- Confidence in proposed fix: Medium

## Improvement candidates

- Critical: 確認済みCriticalなし
- High: Pause MenuのRestart/Quit入力経路とフォーカス表示を物理入力で再検証・修正。Section 1開始時に移動操作を明示し、初回入力確認までタイマーを止める
- Medium: Echo番号/色凡例/軌跡の追加。カメラ回転抑制と手前壁透過
- Low: 残り時間の日本語小数・単位表記、`R / START`・`ESC / SELECT` の入力デバイス別表記。旧来Windows読込環境も対象なら `起動説明.md` にUTF-8 BOMまたは文字コード注記を追加

## Screenshot coverage

- Section 1 start: `Screenshots/01_Section1_Start.png`
- First Echo generation: exact moment not captured; nearest screen-only evidence is `Screenshots/02_After_W.png` at Loop 4 / Echo 3/3
- Section 2 start: not available; Section 2未到達
- Battery held: not available; Section 2未到達
- Two Echoes in Section 3: not available; Section 3未到達
- Game completion: not available; 未完走
- Problem screen: `Screenshots/20_Menu_EConfirm.png`
- Privacy: 保存済みPNG 21枚はすべてゲームclient領域のみの1280x720へcrop済み。デスクトップ全体画像は残していない
