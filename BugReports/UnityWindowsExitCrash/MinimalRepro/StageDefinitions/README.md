# 検証構成

ManifestはAからIへ順に要素を追加します。`Packages/manifest.json`と`packages-lock.json`は、Access Violationを観測した中で最小の構成Bに合わせています。別の構成を選ぶ場合は`Automation/BuildStage.ps1`を使います。ScriptがManifestをコピーし、Unityで新しいLock Fileを解決してからBuild Methodを呼びます。

| 構成 | 前の構成から追加する内容 | 5回確認時の終了方法 |
| --- | --- | --- |
| A | 空のScene、Camera 1台、D3D11 | 外部からの`WM_CLOSE` |
| B | URP 17.4.0とURP Asset | 外部からの`WM_CLOSE` |
| C | Input System 1.19.0と`PlayerInput` | 外部からの`WM_CLOSE` |
| D | uGUI 2.0.0、Canvas、CanvasScaler、TMP Label | 外部からの`WM_CLOSE` |
| E | Global Volume Component | 外部からの`WM_CLOSE` |
| F | Audio ModuleとAudioSource | 外部からの`WM_CLOSE` |
| G | ECHO//SHIFTと同じWindow／Player Settings | 外部からの`WM_CLOSE` |
| H | 重複防止付きの同期保存、Log、`Application.Quit(0)` | Projectと同じ自動終了 |
| I | URP/LitのCube 1個とCollider | Projectと同じ自動終了 |

A〜Gは同じWindow Close経路を使い、H〜Iは追加した自動終了処理を使います。最初のA〜I確認でAccess Violationが出たのは構成Bだけ（`1/5`）でしたが、追加確認では再現せず、構成BのDumpも取得できませんでした。安定した再現構成やURPが直接原因である根拠としては扱いません。
