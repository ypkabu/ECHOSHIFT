# バージョン・ビルド別の結果

## Unityバージョン別の終了動作

元Projectを変更しない別Copyで確認した結果です。

| Unity | Build／終了方法 | Crash／回数 | 備考 |
| --- | --- | ---: | --- |
| 6000.4.6f1 | 製品版Development、自動終了 | 3/3 | Full Dump 3件、同じCall Stack |
| 6000.4.6f1 | 以前の小さいProject | 2/3 | 画面付きPlayer |
| 6000.4.8f1 | 製品版Copy、自動終了 | 0/10 | 一時的に再現せず。小さいPlayerでは再現 |
| 6000.4.8f1 | 製品版Copy、Pause Quit | 0/10 | 一時的に再現せず |
| 6000.4.8f1 | 製品版Copy、Window Close | 0/10 | 一時的に再現せず |
| 6000.4.8f1 | 以前の小さいProject | 9/10 | DumpのCall Stackが製品版と一致 |
| 6000.4.12f1 | 製品版Development、自動終了 | 7/10 | 終了前にゲームは正常完了 |
| 6000.4.12f1 | 製品版Development、Pause Quit | 8/10 | 同じNative Shutdown内 |
| 6000.4.12f1 | 製品版Development、Window Close | 2/10 | 同じ画面付きPlayer |
| 6000.4.12f1 | 製品版Non-Development、自動終了 | 9/10 | 2回は通常完了前に外部Timeoutへ到達したがCleanup中にCrash |
| 6000.4.12f1 | 以前の小さいProject | 9/10 | Full DumpとSymbolあり |
| 6000.4.12f1 | 構成B、最初のWindow Close | 1/5 | Dumpなし、Call Stack未確認 |
| 6000.4.12f1 | 構成B、ProcDump使用 | 0/20 | Dumpなし |
| 6000.4.12f1 | 構成B、WER LocalDumps使用 | 0/30 | Dumpなし、全て終了コード0 |

確認した6000.4系のPatchでは問題を解消できませんでした。この表から最初に問題が発生したUnity Versionは特定できません。

## Graphics・headless別の結果

| 条件 | Crash／回数 | 結果 |
| --- | ---: | --- |
| NVIDIA Device 0、D3D11 | 4/5 | 再現 |
| Intel Device 1、D3D11 | 5/5 | 再現 |
| WARP、D3D11 | 5/5 | 再現 |
| `-batchmode -nographics` | 0/5 | 再現せず |

WARPで再現し、`-nographics`では再現しないため、特定GPU VendorのDriverだけが原因である可能性は低く、画面付きPlayerの終了処理に差があります。ただし、Unity内部のどのSubsystemが不正なLifetimeを持つかは特定できません。

## A〜Iビルドのハッシュ

Build全体のHashは、`relative/path file-sha256`を並べ替えたUTF-8文字列に対するSHA-256です。A〜IでEXEと`UnityPlayer.dll`は同一で、Serialized Dataだけが異なります。

| 構成 | Build全体のSHA-256 | File数 | Bytes |
| --- | --- | ---: | ---: |
| A | `3EBFAFD22336A552891D36A7837E7D3EAC27731998891612B4E0F839E319D749` | 229 | 147579021 |
| B | `229597DCD872E3F09E46FFB35AF0814745F06B4311E1F385861B10DFFC3D8B69` | 280 | 160444896 |
| C | `A6558BA0441A28D39D5F27C645B705DC2E849D7C19EC32EB56457BD6A13F6E61` | 284 | 162433670 |
| D | `F44178168CE6484E135FBABF47A7BC2CA71A1534C297B5C5DDCFB093B599F0B1` | 284 | 162435518 |
| E | `8E59519B6DB89FFAC647E8D3A21A1A54ACEA06038D9DE0F8C91B45D962D9962C` | 284 | 162435866 |
| F | `D140C9E3209663DC08FD7F4D7CD1F197D11B01335D1AE21CA05A18A40AC95FA5` | 284 | 162437674 |
| G | `840E7E700901B272EA1CA8265020EC8FAD4F8C1A1C54F94746EE7161F65B5C16` | 284 | 162437736 |
| H | `A42659D75F87DE1D82E3CD92C45D4494FDEF424B72D7068A8A773E3A1DFCDC47` | 284 | 162437740 |
| I | `5AE75C16F6BCE2454DAD8643AF57066A168567995B98EAB13BFE2AC4BDBF0FD4` | 284 | 162470571 |

共通EXEのSHA-256：`C2F1EC702A37D272F5AA869DB417D533BC59EB7ABB9CE16898C0471454DF8850`（`667648` bytes）

共通`UnityPlayer.dll`のSHA-256：`4D693D0453F540155E0498F75C94687B250970568A1C9C1CE96B12067EA7F5F6`（`85499304` bytes）

## backend・別PCでの確認状況

- Mono：確認済み、再現
- IL2CPP：Windows IL2CPP Module未導入のため未検証。確認のための追加導入は実施していません。
- 2台目のWindows PC：利用できず未検証
