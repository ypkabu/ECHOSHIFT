# Version and build matrix

## Unity versions and exit behavior

These are saved investigation results from separate, non-destructive project copies.

| Unity | Build / route | Crashes / runs | Notes |
| --- | --- | ---: | --- |
| 6000.4.6f1 | Product Development, automatic quit | 3/3 | Three full dumps; same stack |
| 6000.4.6f1 | Earlier simple minimal project | 2/3 | Graphical Player |
| 6000.4.8f1 | Product copy, automatic quit | 0/10 | Temporary non-reproduction did not hold for minimal Player |
| 6000.4.8f1 | Product copy, Pause Quit | 0/10 | Temporary non-reproduction |
| 6000.4.8f1 | Product copy, Window Close | 0/10 | Temporary non-reproduction |
| 6000.4.8f1 | Earlier simple minimal project | 9/10 | Dump stack matches normalized product stack |
| 6000.4.12f1 | Product Development, automatic quit | 7/10 | Normal gameplay completed before shutdown |
| 6000.4.12f1 | Product Development, Pause Quit | 8/10 | Same native shutdown class |
| 6000.4.12f1 | Product Development, Window Close | 2/10 | Same graphical Player family |
| 6000.4.12f1 | Product Non-Development, automatic quit | 9/10 | Two runs reached the external timeout before normal completion; crash still occurred during cleanup |
| 6000.4.12f1 | Earlier simple minimal project | 9/10 | Full dump and symbols available |
| 6000.4.12f1 | Staged Stage B, initial Window Close | 1/5 | No dump; stack identity unconfirmed |
| 6000.4.12f1 | Stage B under ProcDump | 0/20 | No dump |
| 6000.4.12f1 | Stage B with WER LocalDumps | 0/30 | No dump; all code 0 |

No tested 6000.4 patch eliminated the defect. This matrix does not establish the first regressed Unity version.

## Graphics and headless matrix

| Condition | Crashes / runs | Result |
| --- | ---: | --- |
| NVIDIA device 0, D3D11 | 4/5 | Reproduced |
| Intel device 1, D3D11 | 5/5 | Reproduced |
| WARP, D3D11 | 5/5 | Reproduced |
| `-batchmode -nographics` | 0/5 | Did not reproduce |

WARP reproducing while `-nographics` does not makes a vendor GPU driver explanation unlikely and isolates the difference to the graphical Player shutdown path. It does not identify which Unity subsystem owns the invalid lifetime.

## A-I build hashes

Whole-build hashes are SHA-256 over the UTF-8 sequence of sorted `relative/path file-sha256` lines. The EXE and `UnityPlayer.dll` are identical across A-I; serialized Data differs.

| Stage | Whole-build SHA-256 | Files | Bytes |
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

Common EXE SHA-256: `C2F1EC702A37D272F5AA869DB417D533BC59EB7ABB9CE16898C0471454DF8850` (`667648` bytes).

Common `UnityPlayer.dll` SHA-256: `4D693D0453F540155E0498F75C94687B250970568A1C9C1CE96B12067EA7F5F6` (`85499304` bytes).

## Backend and other-machine status

- Mono: tested; reproduces.
- IL2CPP: not tested because the Windows IL2CPP module is not installed. No large module was installed for this investigation.
- Second Windows PC: not available; untested.
