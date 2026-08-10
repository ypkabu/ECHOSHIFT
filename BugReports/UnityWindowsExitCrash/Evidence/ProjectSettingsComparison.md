# PackageとProject Settingsの比較

## Package構成別の結果

Git管理している初期状態は構成Bです。`Packages/manifest.json`はURP `17.4.0`を含み、`packages-lock.json`はUnity 6000.4.12f1で新しく解決しました。A〜IのManifest Snapshotは`StageDefinitions`に保存しています。

| 構成 | 明示的に追加するPackage／設定 |
| --- | --- |
| A | なし |
| B | `com.unity.render-pipelines.universal` 17.4.0 |
| C | `com.unity.inputsystem` 1.19.0 |
| D | `com.unity.ugui` 2.0.0。Import前に最小のTMP Settingsを追加 |
| E | Package差分なし。Volume Componentを追加 |
| F | `com.unity.modules.audio` 1.0.0 |
| G | Package差分なし。Player Settingsを変更 |
| H | Package差分なし。Projectと同じ終了Componentを追加 |
| I | `com.unity.modules.physics` 1.0.0。表示用Object／Colliderを1つ追加 |

## 製品Packageの基準

- URP 17.4.0
- Input System 1.19.0
- uGUI/TMP 2.0.0
- 元の6000.4.6f1 ProjectはVisual Studio Editor 2.0.22。確認用Copyでは実行用Packageを変えず2.0.27へ更新

## Player Settingsの差分

製品版はCompany `Echo Shift Prototype`、Product `ECHO SHIFT`、1920x1080、Window表示、Run In Background OFF、Resizable Window OFF、Flip Model Swapchain ON、Mono、Input Systemを使用します。構成Gは、Builderから画面に関係するWindows設定を反映します。Serialized Active Input Handlerは構成Gの生成後に個別確認していないため、完全一致とはしていません。

AとBの比較で変わるのは、Builderが追加するURP PackageとPipeline Assetだけです。ただしBの再現は1回で、C〜Iへ進んでも発生率が増えていないため、原因を確定する差分ではなく候補の境界として扱います。
