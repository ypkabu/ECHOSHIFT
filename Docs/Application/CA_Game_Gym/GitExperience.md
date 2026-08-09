# Git Experience — ECHO//SHIFT

## Confirmed repository facts

- Development history: 2026-07-19〜2026-08-09（current Git history）。
- Application-preparation baseline: 53 commits。公開準備commitとPull Request mergeはこの後に追加。
- Feature branches: Phase 1、2、3、Phase 4のvisual/presentation/character/crash調査、Phase 5Aを目的別branchへ分離。
- Commit prefixes: `feat` 20、`fix` 13、`test` 9、`docs` 8、`chore` 1、`perf` 1、`refactor` 1。
- Integration: Phase 1〜3は検証済みcommitを`main`へfast-forward。merge commitは0。
- Tags: `phase0-validated`〜`phase3-validated`、Phase 4 automation tags、`phase5a-validated`。
- Public remote: `https://github.com/ypkabu/ECHOSHIFT`。応募準備branchをPull Requestで`main`へ統合するが、個人repositoryであり共同code reviewの実績とは扱わない。
- Author: 1名。これは個人開発履歴であり、team repositoryだったとは説明しない。

## Operations actually used

| Operation | Purpose | Evidence |
| --- | --- | --- |
| `git init` / ignore rules | Unity Projectをsource、settings、license、docs中心で保存し、Library/Build/Logを除外 | `.gitignore`, initial Phase 0 commit |
| Feature branches | milestoneと調査scopeを分離し、validated baselineを保持 | `feature/phase1-*`〜`feature/phase5a-*` |
| Small-purpose commits | implementation、fix、test、docs、performanceを履歴上で分離 | commit prefix counts and log |
| Fast-forward integration | validated Phase 1〜3を履歴を増やさず`main`へ統合 | `Docs/Phase2Validation.md`, `Docs/Phase3Validation.md` |
| Tags | automation passとhuman/formal validationを区別 | `phase3-automation-passed`, `phase3-validated`, Phase 4 automation tags |
| `git status` / `git diff` / hashes | Builder/test side effect、unexpected Scene/Prefab changes、generated output追跡を監査 | Phase 4/5A validation docs |
| Isolated fix commits | failure evidence、root-cause fix、安全追加、revalidationを分離 | `3ed7907`, `35bc519`, `d471010`, `1245b8a` |

## Representative commits

| Commit | Type | What can be explained in an interview |
| --- | --- | --- |
| `5c87e8b` | `feat` | movement ReplayへStable-ID InteractionとCarry foundationを追加した境界 |
| `828670a` | `test` | exact target replay、target loss、short replay、resetを既存suiteへ追加 |
| `d0e0fe5` | `feat` | deterministic multi-Echo ordering、conflict resolution、Door tick commit |
| `866a464` | `fix` | source-blind black-boxで見つかったPhase 3 blockersを修正 |
| `38beb04` | `fix` | 浮遊VisualをScene instance非表示ではなくBuilder生成元から除去 |
| `910956a` | `fix` | gameplay座標を変えずCamera framingを改善 |
| `35bc519` | `fix` | approved Production assetのBuilder再serializeをcanonical guardで停止 |
| `d471010` | `fix` | canonical mismatchをlegacy rebuildせずwrite前にfail-closed |
| `1245b8a` | `test` | final HEADから151 EditMode / 131 PlayModeとProduction integrationを再検証 |

## What Git was used for

Gitは単なるbackupではなく、次の判断境界として使用した。

- Phaseごとの仕様scopeとvalidated baselineを固定する。
- Human Review前のautomation tagと正式validated tagを区別する。
- failure evidenceを消さず、fixと再検証を別commitに残す。
- Scene Builder再実行がProduction YAMLを意図せず変更していないか、hashとdiffで検出する。
- Captures、Builds、Logs、TestResults、Libraryをreproducible generated evidenceとして追跡対象外にする。
- Third-party source、project-owned wrapper、license noticeを別のownership boundaryとして確認する。

## Honest limitations

- 共同branch、Pull Request、review comment、conflict resolution、CI server、remote release運用は未経験として扱う。
- Phase 3以降は応募公開時にPull Requestで`main`へ統合する。既存commitとvalidated/automation tagは履歴rewriteせず維持する。
- commit authorは1名で、Codex-assisted changesを含む。commit数だけを手作業量の証明には使わない。

## Interview points

1. なぜautomation passとhuman/formal validationを別tagにしたか。
2. なぜ最初のFormal Validation FAILを削除せず、fixとrevalidationを分けたか。
3. Unity Scene YAMLのbyte差分とsemantic差分をどう切り分けたか。
4. generated XML/logをcommitせず、再現commandとstable result summaryを残した理由。
5. team開発では、同じ小さなcommit、test evidence、ownership boundaryをPull Requestへどう持ち込むか。
