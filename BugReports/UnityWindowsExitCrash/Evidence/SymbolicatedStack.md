# symbol付きstack trace

## dumpから確認できた事実

- 例外：`0xC0000005` Access Violation
- 操作：読み取り（`Parameter[0] = 0`）
- 不正なAddress：`0x0000000000000138`（`Parameter[1]`）
- Register：`RAX = 0`。障害命令は`[RAX+0x138]`を読み取り
- 障害関数：`UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9`
- Thread：Unity Main Thread
- 終了処理：`RegisterRuntimeInitializeAndCleanup::ExecuteCleanup`／`RuntimeCleanup`内の`PlatformAccessibilityManager`を破棄する処理
- 障害時のCall StackにUser DLL、Native Plugin、GPU Driver DLL、managed Callbackなし

## 取得できたstack trace

Native Call Stackは8つの呼び出しを遡った後に終了しており、30フレーム分は存在しません。

```text
UnityPlayer!ExternalGPUProfiler::GetGameViewWindowHandle+0x9
UnityPlayer!RuntimeStatic<PlatformAccessibilityManager,0>::StaticDestroy+0x3b
UnityPlayer!RegisterRuntimeInitializeAndCleanup::ExecuteCleanup+0xed
UnityPlayer!RuntimeCleanup+0x27
UnityPlayer!UnityMainImpl+0x1300
UnityPlayer!UnityMain+0xb
<player-exe>!__scrt_common_main_seh+0x106
kernel32!BaseThreadInitThunk+0x17
ntdll!RtlUserThreadStart+0x2c
```

最初のUnity以外のModuleは、`UnityMain`より下にある生成済みPlayer EXEのCRT Entry（`__scrt_common_main_seh`）です。間に第三者Moduleはありません。

Version固有のOffsetを除外し、Unity内の6関数を正規化したSHA-256：

`2D128D9A9C6B4956572BA97A01E6C2E5B03F8DBE6EEB93632AB6DC3B03E34C98`

## バージョンごとのoffset比較

| Unity | 取得元 | UnityPlayer Offset | 正規化したCall Stack |
| --- | --- | --- | --- |
| 6000.4.6f1 | 製品版、Dump 3件 | `+0x1d2f39` | 同じHash |
| 6000.4.8f1 | 以前の小さいPlayer | `+0x1d3e49` | 同じHash |
| 6000.4.12f1 | 以前の小さいPlayer | `+0x1d4a89` | 同じHash |
| 6000.4.12f1 | 構成B | **未確認** | **Dumpなし** |

確認した3つのUnity VersionはBinary Offsetが異なりますが、同じ関数順です。確認済みDumpではUnityのNative Cleanup内で同じ問題が繰り返されたことを示しますが、構成Bの1件が同じ障害であることは証明できません。
