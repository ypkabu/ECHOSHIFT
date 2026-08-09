# Recruiter Review — CA Game Gym

基準日: 2026-08-09 JST

## Overall assessment

**Application material readiness: Public-ready after repository, PR, and video link verification.**

公式募集要項のUnity経験とGit経験はrepositoryで具体的に示せる。課題解決と検証は強く、公開repositoryと約51秒のgameplay videoを応募導線にする。応募者の参加条件は本人確認が必要で、配布BuildにはWindows graphical shutdown blockerがあるため主提出物にしない。

## 強い点

1. Replayを「入力保存」だけで終わらせず、fixed tick、Physics、Interaction、Carry、Door commitまで一貫したclient simulationとして説明できる。
2. 実Sceneの自動完走、drift、interaction count、Missing/exception auditがあり、数値を根拠に品質を話せる。
3. Human ReviewのFailを受けてCamera/HUD/character/visual generatorを修正し、automationとhuman acceptanceを分離している。
4. Git履歴にfailure、fix、safety fix、revalidationが残り、都合の悪い結果を削除していない。
5. Third-party assetのlicense、source、hash、加工境界まで追跡している。

## 弱い点

- 個人制作のため、共同PR、review、conflict resolution、他人のcodebaseへ変更した証拠はない。
- 53 commitsと多数のdocs/testsが短期間に集中し、Codex支援の比率や本人の理解を面接で確認されやすい。
- graphical Windows Playerが終了時にUnity native crashを起こすため、downloadable buildを無条件に勧められない。
- Playable buildは公開しないため、採用担当による操作確認は動画・source・validation記録に限定される。
- Steam向けを想定したprototypeだが、Steamworks、installer/signing、save、release operationは未経験。

## 書類で伝わりにくい点

- 151/151・131/131だけでは、何を守るtestかが伝わらない。Replay、Stable ID、Carry cleanup、real Scene completionの4例を先に示すべき。
- documentation量が多く、採用担当が最初に読む順序を迷う。最初はREADME、次に30〜40秒動画、最後にEvidenceの順がよい。
- Phase名が多く、作品の遊びより工程管理が前に見えやすい。応募文ではSection 3の「Echo 1がPlate、Echo 2がBattery、PlayerがDoor通過」を最初に置く。
- native shutdown調査は技術的に濃いが、作品説明冒頭へ置くと未完成感が勝つ。Known Limitationsと問題解決補足へ分離する。

## 「AIに作らせただけでは？」と疑われる点

- 開発期間約3週間で121 C# files、25,796 lines、282 passing test cases、長いvalidation docsがある。
- 文書の粒度と英日混在表現が均一で、AI-assisted repositoryに見える。
- commit authorが1名で、外部reviewerのGit evidenceはない。

### Response strategy

AI利用は隠さず、次の3点を本人の説明で証明する。

1. `LoopDirector`の1 tickが、なぜその順序なのかを図なしで説明する。
2. Stable ID no-fallbackとBatteryをActor hierarchy外へ置く理由を、失敗caseから説明する。
3. Builder determinismの最初のFAIL、local fileID差分、canonical guardの保証範囲と非保証範囲を説明する。

「Codexに作らせた」か「全部手書きした」かの二択にせず、本人が仕様・受入基準・優先順位を決め、AI outputをtest/diff/human reviewで検証した開発プロセスを示す。

## ゲームクライアント経験として弱い点

- commercial release、platform certification、asset memory optimization、複数実機performance、save migrationの経験はない。
- advanced animation、shader authoring、production UI pipelineは限定的。
- graphical build shutdown blockerが未解決で、release lifecycleは完了していない。
- Input Systemは使っているが、物理gamepad実機と複数controller環境の記録は限定的。

## Git経験として弱い点

- 公開remoteと応募準備Pull Requestはあるが、個人repositoryでありreview conversationや共同開発の証拠ではない。CI serviceも追加していない。
- merge commitは0で、fast-forward中心。複雑なbranch conflictを解いた証拠はない。
- Phase 4/5Aは`main`未mergeで、release branch/tag policyは個人運用。

これらを「team Git経験」と表現せず、「teamへ持ち込める基礎」として述べる。

## チーム開発適性として弱い点

- 実際に他者の要求とcode ownershipが衝突した経験はrepositoryから確認できない。
- task estimation、daily communication、pair work、review turnaroundの実績はない。
- Human Reviewは存在するが、source reviewや共同実装ではない。

補完材料は、ADR、small commits、test evidence、failure preservation、scope freezeへの対応である。Game Gymで学びたい差分として正直に使う。

## 今から応募締切までに改善する価値があるもの

### P0 — 応募前に必須

1. READMEを閲覧できるpublic repositoryまたはread-only archive URLを用意し、リンクを実際に開いて確認する。
2. 30〜40秒の通常速度gameplay videoを公開し、Echo生成、Plate協力、Battery carry/insert、2 Echo協力を字幕なしでも確認できるようにする。
3. buildを提出するか決める。提出するならshutdown crashを明示し、正常終了を装わない。締切優先ならvideo + sourceを主資料にし、crashy graphical buildを主導線にしない。
4. 応募資格、全日程参加、持参PC、併願制限、学校/卒業予定を本人が確認する。
5. public URL上でREADME画像、全relative link、license notice、個人情報、email、Git author情報の公開可否を最終確認する。

### P1 — 時間があればやる

1. README冒頭の2画像から30〜40秒videoへ直接遷移できるlinkを追加する。
2. 面接用にReplay tick、Stable ID、Builder determinismの3件を各60秒で説明できるよう練習する。
3. GitHub等へ公開する場合、repository description、topic、release非提供理由を短く設定する。
4. shutdown issueを別PCまたはfixed Unity versionで再確認できるなら行う。ただし応募資料作成を止めない。

### P2 — Game Gym応募後でよい

- Logo spacing、Chamber violet density、Observation text依存、Decal surface blending。
- final art、BGM、Steamworks、save、installer/signing、新stage、新mechanic。
- 長文技術記事、長大PDF、完全なinterview question集。

## Recommended recruiter path

1. Public README: game premise、hero image、technical highlights、validation。
2. 30〜40秒video: Section 3のtwo-Echo coordination。
3. `Evidence.md`: Unity/Git/problem-solvingの根拠。
4. 必要時だけArchitecture、Builder determinism、shutdown investigation。

公開後はREADME、Gameplay動画、Evidenceの順に実在URLを確認する。
