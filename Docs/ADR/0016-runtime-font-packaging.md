# ADR 0016：日本語フォントと固定TMP Atlas

## 状態

採用済み。

## 採用した方法

Google Fonts公式のOFL配布元からNoto Sans JPを取得し、原文のライセンスを`ThirdPartyNotices`へ保存します。現在の日本語文字一覧を収録した固定のTextMeshPro Atlasを1つ生成し、既存のUnity UI／TextMeshには同梱した`Font`を使用します。実行環境のOSフォント検索と、実行中のAtlas拡張は行いません。

## 理由

- 配布先にインストールされたフォントへ依存しません。
- SIL Open Font License 1.1は表示を同梱することでソフトウェアへの収録を認めています。
- 固定文字一覧により、欠落文字とビルド時の消去を再現可能な形で検証できます。
- TextMeshProと既存UIの境界を残し、画面全体を一度に書き換える危険を避けられます。

## 検討した別案

- `Font.CreateDynamicFontFromOSFont`：対象PCに同じフォントがある保証がありません。
- Dynamic TMP Atlas：実行中の確保、欠落文字の警告、ビルドの再現性に課題があります。
- 全UIの即時TMP化：検証済みの一時停止、Navigation、Layoutへ影響します。
- 出所の不明な日本語フォント：ライセンスを確認できません。

## 現在の制約

Atlasは現在の文字一覧だけを対象とし、任意の会話文やユーザー入力には対応しません。原本と固定Fontを同梱するためProject Sizeが増えます。TMP Essential Resourcesも明示的に収録します。

## 見直す条件

表示文字が変わった場合は固定Atlasを再生成します。全UIをTMPへ移行する場合は、日本語Layout、一時停止画面、キャプチャ、Windows版での文字表示を再検証します。
