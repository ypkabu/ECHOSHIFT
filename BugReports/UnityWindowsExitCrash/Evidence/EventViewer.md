# Windows Event Viewerの記録

保存した製品版のApplication Errorには、次の内容が記録されています。

- 例外コード：`0xc0000005`
- 障害Module：`UnityPlayer.dll`
- Unity 6000.4.6f1のOffset：`0x1d2f39`
- 対象Process：`ECHOSHIFT_Phase4_3_CrashDebug.exe`

WinDbgでは、このOffsetを`ExternalGPUProfiler::GetGameViewWindowHandle+0x9`として解決できます。詳しくは`SymbolicatedStack.md`を参照してください。

構成Bの4回目は、`0xC0000005`に相当する符号付き終了コード`-1073741819`でした。ただし構成B専用のEvent Viewer RecordとDumpを保存できていません。製品版のEventを流用せず、Module、Offset、Call Stackは未確認として扱います。
