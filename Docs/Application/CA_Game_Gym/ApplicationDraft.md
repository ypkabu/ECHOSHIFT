# CA Game Gym Application Draft

基準日: 2026-08-09 JST

公式募集ページ: <https://www.cyberagent.co.jp/careers/students/event/detail/id=33425>

公式に記載された「Unityを用いた開発経験」「Gitを用いた開発経験」「ゲームクライアントエンジニア志望」「チーム開発への関心」「主体性」を、ECHO//SHIFTのcurrent repository evidenceへ対応付けた文案である。字数は句読点・英数字を1文字として数えた目安であり、応募フォームへ貼り付けた後にフォーム側のcountを確認する。

## CA Game Gym応募フォーム推奨版（300〜400字）

> ECHO//SHIFTは、過去の自分の移動と装置操作をEchoとして再生し、現在の自分と協力して仕掛けを解くUnity製3Dパズルです。記録、再生、現在の行動を重ねるほど、一人で時間を越えた協力を組み立てる面白さが生まれます。Replayは60Hzのfixed tickで入力とPhysicsの順序を固定し、各InteractionはSceneに保存したStable IDだけを解決します。Batteryの所有状態をActor階層から分離し、Echo破棄やLoop resetでもCarry状態が壊れないよう設計しました。実Sceneを含むEditMode 151件、PlayMode 131件を完走し、P3自動解法でReplay Drift 0m、Interaction成功4・失敗0を確認しています。

## 1. 作品一言説明（50字前後）

> 過去の自分の行動をEchoとして再生し、現在の自分と協力して仕掛けを解くUnity製3Dパズルゲーム。

## 2. 作品説明（100字前後）

> ECHO//SHIFTは、移動と装置操作を記録し、次のループでEchoとして再生するUnity製3Dパズルです。過去の自分にPlate保持やBattery運搬を任せ、最大3体のEchoと現在の自分でDoorを開きます。

## 3. 作品説明（200字前後）

> ECHO//SHIFTは、過去の移動と装置操作をEchoとして再生し、現在の自分と協力するUnity製3Dパズルです。PlateでDoorを開く基礎から、Battery取得とSocket挿入、2体のEchoへ異なる役割を与える協力までを3 Sectionで体験できます。Replayは60Hz固定tickで記録し、実Scene自動完走で最大Drift 0m、Interaction成功4・失敗0を確認しました。

## 4. 作品説明（300字前後）

> ECHO//SHIFTは、過去ループをEchoとして再生し、現在の自分と協力するUnity製3Dパズルです。Echo 1にPlate保持、Echo 2にBattery運搬を任せ、Playerが2つのDoorを通ります。60Hz固定tickとStable IDで記録外の装置へのfallbackを防ぎました。実Sceneを含むEditMode 151件、PlayMode 131件が通り、P3自動完走時のDrift 0m、Interaction成功4・失敗0です。Git差分と自動testを確認し、画面・音声のプレイテストを実施しました。

## 5. 作品説明（400字前後）

> ECHO//SHIFTは、過去の移動と装置操作をEchoとして再生し、現在の自分と協力するUnity製3Dパズルです。PlateとDoor、Battery取得とSocket挿入、2体のEchoへ別の役割を記録する協力を3 Sectionで学びます。Replayは60Hz fixed tickでPhysics同期、Interaction解決、Door反映、pose記録を順序化。InteractionはScene保存のStable IDだけを解決し、別装置へfallbackしません。BatteryはActor階層外とし、Echo破棄でも消失しません。実Sceneを含むEditMode 151件、PlayMode 131件がPassし、Drift 0m、Interaction成功4・失敗0でした。Git差分、自動Validation、画面・音声のプレイテストで検証しました。

## 6. Unity開発経験（200字前後）

> Unity 6/URPでPlayer移動、3 Echo Replay、Battery Carry/Socket挿入、Door/Goal、Camera/HUD、Pauseを実装しました。Input SystemとScriptableObjectを使い、Runtime/Editor/Testをasmdefで分離。fixed tickと実Scene自動完走でReplay/Physics/Interactionを検証しました。

## 7. Git利用経験（150〜250字）

> 個人開発でGitを使用し、Phaseごとのfeature branch、feat/fix/test/docs commit、検証済みtagを運用しました。53 commitsを残し、Phase 1〜3はfast-forwardでmainへ統合しています。失敗したFormal Validationも削除せず、原因修正、安全性追加、全Gate再実行を別commitにしました。Scene/Prefab差分と生成物追跡をstatus、diff、hashで監査しています。共同PR経験はありません。

## 8. 最も技術的に工夫したこと（300字前後）

> 最も工夫したのは、移動Replayと装置Interactionを同じtickで再現する仕組みです。60Hzで入力、Player/Echo移動、Physics同期、Interaction競合解決、Door反映、pose記録を順序化しました。Replayはcommandと期待pose、成功Interactionはtick・operation・Stable IDを保持。Echoは記録ID以外へfallbackしません。短い記録、target消失、同時要求、3 Echo evictionをtestし、P3実Scene完走でDrift 0m、Interaction成功4・失敗0を確認しました。

## 9. 問題解決経験（300字前後）

> Phase 5AのFormal Validationで、Builderはexit 0でも承認済みController、Volume、Prefab、Sceneへ毎回byte差分を出しました。3 runを比較し、dirty保存と再生成によるlocal fileID再採番へ原因を切り分けました。承認済み状態では再生成不要と判断し、4 assetのexact SHA-256 guardを追加。一致時はsafe no-op、不一致・一部欠落時はwrite前にfail-closedします。同一/別process各3回で差分0、意図的不一致test、全test再実行まで行い、最初のFAILも履歴へ残しました。

## 10. なぜゲームクライアントエンジニアなのか（200字前後）

> ECHO//SHIFTで、入力が移動や装置状態へ変わり、Camera/UI/Animation/Audioを通して遊びとして伝わるまでを調整する面白さを知りました。ReplayはPhysics順序が崩れれば成立せず、表示が弱ければ仕組みが伝わりません。内部状態をtestで守り、実機プレイテストを基に操作と可読性を改善できるゲームクライアントエンジニアを志望します。

## 11. チーム開発にどう活かせるか（200字前後）

> ECHO//SHIFTは個人制作で共同PRの実績ではありません。一方、Runtime/Editor/Testの責務分離、ADR、再現command、feature branch、小さなcommit、失敗を残す検証記録を整備しました。チームでは既存コードと制約を読み、問題を再現して変更scopeと確認方法を共有します。レビュー指摘は観察、原因、修正、回帰結果を分け、他の人が検証できる差分で対応します。

## 12. Game Gymで学びたいこと（200字前後）

> 個人制作では設計、実装、検証を一人で判断できましたが、他者が継続開発する実プロダクトでの優先順位、review、役割分担は未経験です。Game Gymでは実コードを短時間で読み、前提と調査結果を共有し、品質と期限を満たす変更へ落とす過程を学びたいです。fixed-tickや自動検証の経験が共同開発で何が過剰・不足か、社員のfeedbackから具体化します。

## Applicant-only confirmation before submission

- ゲームクライアントエンジニア志望か。
- チームゲーム開発へ興味があるか。
- 2028年4月以降に入社可能か。
- 開発用Windows/Mac PCを持参可能か。
- 2026年9月7日18:00〜20:00に参加可能か。
- 2026年9月11日に終日参加可能か。
- 2026年9月12日に終日参加可能か。
- 併願制限へ抵触しないか。
