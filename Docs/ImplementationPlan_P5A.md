# Phase 5A Implementation Plan — Authorship & Identity Pass

## 目的

Phase 4.3で合格したCamera、HUD可読性、Character pose、Battery carry、Door、床配線を固定したまま、既存のCC0素材とProject-owned primitiveを再構成し、ECHO SHIFT固有の視覚・文言・音の候補を三案提示する。Phase 5Aは選定前の比較工程であり、A／B／Cの勝者を自動決定しない。

## 固定境界

- 製品Scene `P0_ReplayLab`～`P3_PlayableGreybox`、Prefab、Stable ID、Collider、Puzzle座標、Section解法、Goal、Telemetryを変更しない。
- fixed tick、Replay、Echo、Interaction、CharacterMotor、Battery ownership、Socket、Door collider timingを変更しない。
- Phase 4.3のRobot animation構造、Camera framing、HUD情報量を変更しない。
- Windows graphical Player shutdown crashの調査・修正・証跡変更を行わない。
- Quaternius／Kenney／Noto Sans JPの原本を変更しない。新しい外部素材と画像生成AIは使わない。
- Previewは独立したEditor生成Scene／Assetと`Captures/Phase5A/`だけへ出力する。

## 基準状態

- Branch開始点: `feature/phase4-windows-exit-crash`
- 基準commit: `4d0269ef50b9e03d2712fc95596c7e0bfb227529`
- 作業branch: `feature/phase5a-authorship-identity`
- `phase4-validated`: 存在しない。
- P0 SHA-256: `1411EB0EC0574F1E24BEDF09AE78DEE33ADA3B69B70FC1A3DDBC0BA5E27124C5`
- P1 SHA-256: `4AADD3D34BE36B0288DDF4B46E167017758C007E8FC65E4D7095FD0B80747EBB`
- P2 SHA-256: `73EC41AA4B72E30BDFCD874E0DFA3BBC59C7F407519B3160A157C9380F08822C`
- P3 SHA-256: `4ACA5F734DB1C754E7C237CA6EE819B30F8F50B53BB5C37298C31C92F36961E8`

## 候補生成

各Optionは次の同一フォーマットを持つ。

- 固有motif、palette、logo lockup、monochrome mark、emissive variant。
- Echo Chamber hero object。現段階ではPreview状態を明示し、採用後もLoop eventを読み取るだけのVisual hookとする。
- 12種の1024px以下SVG decal atlas。環境運用表示は短い英語／section code、HUDは日本語を原則とする。
- Section 3のGameplay座標を模した非Gameplay preview layout。asymmetry、密度階層、Goal焦点、保守／観察領域を比較する。
- stable text keyを維持した短い日本語copy set。
- Kenney CC0原音から生成した15秒以内のpreview WAV。原本を編集せず、pitch、EQ、reverse tail、layer、delayだけをOption別に適用する。

## Preview出力

各Optionに次の5枚をD3D11相当のURP Editor render、1920×1080で出力する。

1. LogoとEcho Chamber
2. Section 3 overview
3. Player、Echo、固有motif
4. Door、Battery、decal
5. HUD copyとGoal

Captureは`Captures/Phase5A/Options/{A|B|C}/`へ保存し、Git管理しない。Option source、Builder、SVG atlasはProject-owned assetとして追跡する。

## 検証

- Preview Builderをbatchmodeで実行し、15 PNGが1920×1080、3 WAVが15秒以内であることを確認する。
- Optionごとに12 decal、5 capture、1 audio preview、logo／chamber／UI copyを検証する。
- Production SceneのGit diff 0とP0～P3 SHA-256一致を確認する。
- Stable ID、Collider、Replay、P3自動完走、Drift 0.05m以内、Interaction失敗0の既存回帰を実行する。
- Text Catalog、Kenney／Quaternius原本、Phase 4 tag、shutdown evidenceのdiff 0を確認する。
- Capture、audio preview、Build、Log、Library、TestResultsが追跡対象外であることを確認する。

## Human選定ゲート

自動工程はOptionの成立性と非侵襲性だけを確認する。Steam代表画像としての強さ、ES作品としての適切さ、借り物感の低減、過剰演出リスク、最終的なA／B／C選択はHuman Review対象である。選定まで製品Sceneへ適用せず、Phase 5 tagは作成しない。
