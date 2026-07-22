using System;
using System.Collections.Generic;
using System.Text;
using EchoShift.Interaction.Recorded;
using UnityEngine;

namespace EchoShift.Presentation
{
    [CreateAssetMenu(fileName = "Phase3TextCatalog", menuName = "ECHO SHIFT/Phase 3 Text Catalog")]
    public sealed class Phase3TextCatalog : ScriptableObject
    {
        public const string JapaneseLanguageCode = "ja-JP";

        private static readonly string[] DisplayKeys =
        {
            "section.1.name", "section.2.name", "section.3.name",
            "section.1.objective", "section.2.objective", "section.3.objective",
            "tutorial.1.1", "tutorial.1.2", "tutorial.1.3", "tutorial.1.4",
            "tutorial.2.1", "tutorial.2.2", "tutorial.2.3", "tutorial.2.4",
            "tutorial.3.1", "tutorial.3.2", "tutorial.3.3",
            "prompt.move.keyboard", "prompt.move.gamepad", "prompt.interact.keyboard",
            "prompt.interact.gamepad", "prompt.pickup.keyboard", "prompt.pickup.gamepad",
            "prompt.insert.keyboard", "prompt.insert.gamepad", "prompt.drop.keyboard",
            "prompt.drop.gamepad", "prompt.end-loop", "prompt.pause", "prompt.start",
            "hud.loop", "hud.time", "hud.echoes", "hud.battery-carried",
            "marker.player", "marker.switch", "marker.battery", "marker.power",
            "marker.gate", "marker.exit",
            "transition.loop-recorded", "transition.echo-created", "transition.next-loop",
            "completion.section", "completion.game", "completion.synchronization",
            "pause.title", "pause.resume", "pause.restart-section", "pause.restart-game",
            "pause.quit", "confirm.restart-section", "confirm.restart-game", "confirm.quit",
            "state.section-restarted",
            "failure.none", "failure.no-candidate", "failure.target-not-found",
            "failure.target-inactive", "failure.out-of-range", "failure.target-unavailable",
            "failure.held-by-another", "failure.actor-not-carrying", "failure.socket-occupied",
            "failure.target-busy", "failure.unsupported", "failure.missing-interactor"
        };

        [SerializeField] private string languageCode = JapaneseLanguageCode;
        [SerializeField] private string section1Name = "セクション1：過去の自分";
        [SerializeField] private string section2Name = "セクション2：記録された操作";
        [SerializeField] private string section3Name = "セクション3：過去との協力";
        [SerializeField] private string section1Objective = "スイッチを使って扉を開ける";
        [SerializeField] private string section2Objective = "電池で扉に電力を送る";
        [SerializeField] private string section3Objective = "2体のエコーと協力して出口へ進む";

        [SerializeField] private string section1Tutorial1 =
            "黄色の「自分」を青いスイッチまで移動する（WASD / 左スティック）";
        [SerializeField] private string section1Tutorial2 =
            "扉が緑になったら、その場で R / START：ループを終了してエコーを作る";
        [SerializeField] private string section1Tutorial3 =
            "E1・E2…のエコーは、それぞれ記録した過去の動きを繰り返す";
        [SerializeField] private string section1Tutorial4 =
            "エコーを青いスイッチに残し、黄色の「自分」で緑の扉へ進む";
        [SerializeField] private string section2Tutorial1 =
            "オレンジの電池に近づき E / A・× で持つ";
        [SerializeField] private string section2Tutorial2 =
            "電池を持ったまま紫の電源へ運ぶ";
        [SerializeField] private string section2Tutorial3 =
            "電池を入れたら R / START で操作を記録する";
        [SerializeField] private string section2Tutorial4 =
            "エコーが電池を運ぶ間に、黄色の「自分」で緑の扉へ進む";
        [SerializeField] private string section3Tutorial1 = "1体目にスイッチを任せる";
        [SerializeField] private string section3Tutorial2 = "2体目に電池を運ばせる";
        [SerializeField] private string section3Tutorial3 = "過去の自分たちと協力する";

        [SerializeField] private string moveKeyboard = "WASD：移動";
        [SerializeField] private string moveGamepad = "左スティック：移動";
        [SerializeField] private string interactKeyboard = "[E] 調べる";
        [SerializeField] private string interactGamepad = "[A / ×] 調べる";
        [SerializeField] private string pickupBatteryKeyboard = "[E] 電池を持つ";
        [SerializeField] private string pickupBatteryGamepad = "[A / ×] 電池を持つ";
        [SerializeField] private string insertBatteryKeyboard = "[E] 電池を入れる";
        [SerializeField] private string insertBatteryGamepad = "[A / ×] 電池を入れる";
        [SerializeField] private string dropBatteryKeyboard = "[E] 電池を置く";
        [SerializeField] private string dropBatteryGamepad = "[A / ×] 電池を置く";
        [SerializeField] private string endLoop = "R / START：ループを終了";
        [SerializeField] private string pausePrompt = "ESC / SELECT：一時停止";
        [SerializeField] private string startPrompt =
            "画面をクリック、またはキー／ボタンを押して開始";
        [SerializeField] private string loopLabel = "ループ";
        [SerializeField] private string timeLabel = "残り時間";
        [SerializeField] private string echoesLabel = "エコー";
        [SerializeField] private string batteryCarried = "電池を保持中：紫の電源へ運ぶ";
        [SerializeField] private string playerMarker = "自分";
        [SerializeField] private string switchMarker = "スイッチ";
        [SerializeField] private string batteryMarker = "電池";
        [SerializeField] private string powerMarker = "電源";
        [SerializeField] private string gateMarker = "扉";
        [SerializeField] private string exitMarker = "出口";

        [SerializeField] private string loopRecorded = "行動を記録しました";
        [SerializeField] private string echoCreated = "エコー{0}を生成";
        [SerializeField] private string nextLoop = "次のループを開始";
        [SerializeField] private string sectionCompleted = "セクション完了";
        [SerializeField] private string gameCompleted = "すべての実験を完了しました";
        [SerializeField] private string synchronizationCompleted = "過去の自分たちとの同期に成功";
        [SerializeField] private string paused = "一時停止";
        [SerializeField] private string sectionRestarted = "このセクションをやり直しました";

        [SerializeField] private string resume = "ゲームに戻る";
        [SerializeField] private string restartSection = "このセクションをやり直す";
        [SerializeField] private string restartGame = "最初からやり直す";
        [SerializeField] private string quit = "ゲームを終了する";
        [SerializeField] private string confirmRestartSection = "このセクションをやり直しますか？\nもう一度押すと実行します";
        [SerializeField] private string confirmRestartGame = "最初からやり直しますか？\nもう一度押すと実行します";
        [SerializeField] private string confirmQuit = "ゲームを終了しますか？\nもう一度押すと実行します";

        [SerializeField] private string failureNone = "問題ありません";
        [SerializeField] private string failureNoCandidate = "操作する対象がありません";
        [SerializeField] private string failureTargetNotFound = "操作する対象がありません";
        [SerializeField] private string failureTargetInactive = "操作する対象がありません";
        [SerializeField] private string failureOutOfRange = "もう少し近づいてください";
        [SerializeField] private string failureTargetUnavailable = "今は操作できません";
        [SerializeField] private string failureHeldByAnother = "すでに誰かが持っています";
        [SerializeField] private string failureActorNotCarrying = "電池を持っていません";
        [SerializeField] private string failureSocketOccupied = "すでに電池が入っています";
        [SerializeField] private string failureTargetBusy = "ほかのエコーが使用中です";
        [SerializeField] private string failureUnsupported = "今は操作できません";
        [SerializeField] private string failureMissingInteractor = "今は操作できません";

        public string LanguageCode => languageCode;
        public string MoveKeyboard => moveKeyboard;
        public string MoveGamepad => moveGamepad;
        public string InteractKeyboard => interactKeyboard;
        public string InteractGamepad => interactGamepad;
        public string PickupBatteryKeyboard => pickupBatteryKeyboard;
        public string PickupBatteryGamepad => pickupBatteryGamepad;
        public string InsertBatteryKeyboard => insertBatteryKeyboard;
        public string InsertBatteryGamepad => insertBatteryGamepad;
        public string DropBatteryKeyboard => dropBatteryKeyboard;
        public string DropBatteryGamepad => dropBatteryGamepad;
        public string EndLoop => endLoop;
        public string PausePrompt => pausePrompt;
        public string StartPrompt => startPrompt;
        public string LoopRecorded => loopRecorded;
        public string NextLoop => nextLoop;
        public string SectionCompleted => sectionCompleted;
        public string GameCompleted => gameCompleted;
        public string SynchronizationCompleted => synchronizationCompleted;
        public string Paused => paused;
        public string SectionRestarted => sectionRestarted;
        public string Resume => resume;
        public string RestartSection => restartSection;
        public string RestartGame => restartGame;
        public string Quit => quit;
        public string ConfirmRestartSection => confirmRestartSection;
        public string ConfirmRestartGame => confirmRestartGame;
        public string ConfirmQuit => confirmQuit;
        public string BatteryCarried => batteryCarried;
        public string PlayerMarker => playerMarker;
        public string SwitchMarker => switchMarker;
        public string BatteryMarker => batteryMarker;
        public string PowerMarker => powerMarker;
        public string GateMarker => gateMarker;
        public string ExitMarker => exitMarker;
        public static IReadOnlyList<string> PlayerFacingTextKeys => DisplayKeys;

        public string GetSectionName(int index) => index switch
        {
            0 => section1Name,
            1 => section2Name,
            _ => section3Name
        };

        public string GetSectionObjective(int index) => index switch
        {
            0 => section1Objective,
            1 => section2Objective,
            _ => section3Objective
        };

        public string GetTutorialText(int section, int step) => (section, step) switch
        {
            (0, 0) => section1Tutorial1,
            (0, 1) => section1Tutorial2,
            (0, 2) => section1Tutorial3,
            (0, _) => section1Tutorial4,
            (1, 0) => section2Tutorial1,
            (1, 1) => section2Tutorial2,
            (1, 2) => section2Tutorial3,
            (1, _) => section2Tutorial4,
            (2, 0) => section3Tutorial1,
            (2, 1) => section3Tutorial2,
            _ => section3Tutorial3
        };

        public string GetInitialTutorial(int section) => section switch
        {
            0 => section1Tutorial1,
            1 => section2Tutorial1,
            _ => string.Empty
        };

        public string FormatLoop(int loop) => $"{loopLabel}  {loop}";
        public string FormatTime(float seconds) => $"{timeLabel}  {seconds:00.0}";
        public string FormatEchoCount(int count, int maximum) =>
            $"{echoesLabel}  {count}/{maximum}";
        public string FormatEchoCreated(int generation) =>
            string.Format(echoCreated, generation);
        public string FormatLoopTransition(int generation) =>
            $"{loopRecorded}\n{FormatEchoCreated(generation)}\n{nextLoop}";
        public string FormatGameCompleted() =>
            $"{gameCompleted}\n{synchronizationCompleted}";

        public string GetInteractionPrompt(InteractionKind kind, bool gamepad) => kind switch
        {
            InteractionKind.PickupBattery => gamepad
                ? pickupBatteryGamepad : pickupBatteryKeyboard,
            InteractionKind.InsertBattery => gamepad
                ? insertBatteryGamepad : insertBatteryKeyboard,
            InteractionKind.DropBattery => gamepad
                ? dropBatteryGamepad : dropBatteryKeyboard,
            _ => gamepad ? interactGamepad : interactKeyboard
        };

        public string GetFailureText(InteractionFailureReason reason) => reason switch
        {
            InteractionFailureReason.None => failureNone,
            InteractionFailureReason.NoCandidate => failureNoCandidate,
            InteractionFailureReason.TargetNotFound => failureTargetNotFound,
            InteractionFailureReason.TargetInactive => failureTargetInactive,
            InteractionFailureReason.OutOfRange => failureOutOfRange,
            InteractionFailureReason.TargetUnavailable => failureTargetUnavailable,
            InteractionFailureReason.HeldByAnotherActor => failureHeldByAnother,
            InteractionFailureReason.ActorNotCarrying => failureActorNotCarrying,
            InteractionFailureReason.SocketOccupied => failureSocketOccupied,
            InteractionFailureReason.TargetBusy => failureTargetBusy,
            InteractionFailureReason.UnsupportedInteraction => failureUnsupported,
            InteractionFailureReason.MissingInteractor => failureMissingInteractor,
            _ => failureTargetUnavailable
        };

        public bool HasNoEmptyValues()
        {
            if (!string.Equals(languageCode, JapaneseLanguageCode, StringComparison.Ordinal))
            {
                return false;
            }
            foreach (string value in EnumeratePlayerFacingTexts())
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
            }
            return true;
        }

        public static bool HasUniqueKeys()
        {
            for (int i = 0; i < DisplayKeys.Length; i++)
            for (int j = i + 1; j < DisplayKeys.Length; j++)
            {
                if (string.Equals(DisplayKeys[i], DisplayKeys[j], StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        public bool ContainsLegacyEnglishPlayerText()
        {
            string[] legacy =
            {
                "SECTION 1", "SECTION 2", "SECTION 3", "INTERACT", "END LOOP",
                "LOOP RECORDED", "SECTION COMPLETE", "PAUSED", "RESUME",
                "RESTART SECTION", "RESTART GAME", "QUIT TO DESKTOP", "TARGET BUSY",
                "TARGET MISSING", "OUT OF RANGE", "INVALID STATE", "ALREADY HELD",
                "SOCKET OCCUPIED", "CELL CARRIED", "ECHOES  ", "TIME  "
            };
            foreach (string value in EnumeratePlayerFacingTexts())
            foreach (string token in legacy)
            {
                if (value.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return true;
            }
            return false;
        }

        public string GetRequiredGlyphCharacters()
        {
            StringBuilder builder = new StringBuilder(1024);
            foreach (string value in EnumeratePlayerFacingTexts()) builder.Append(value);
            return builder.ToString();
        }

        public IEnumerable<string> EnumeratePlayerFacingTexts()
        {
            for (int i = 0; i < 3; i++)
            {
                yield return GetSectionName(i);
                yield return GetSectionObjective(i);
            }
            for (int section = 0; section < 3; section++)
            {
                int steps = section == 2 ? 3 : 4;
                for (int step = 0; step < steps; step++) yield return GetTutorialText(section, step);
            }
            yield return moveKeyboard;
            yield return moveGamepad;
            yield return interactKeyboard;
            yield return interactGamepad;
            yield return pickupBatteryKeyboard;
            yield return pickupBatteryGamepad;
            yield return insertBatteryKeyboard;
            yield return insertBatteryGamepad;
            yield return dropBatteryKeyboard;
            yield return dropBatteryGamepad;
            yield return endLoop;
            yield return pausePrompt;
            yield return startPrompt;
            yield return loopLabel;
            yield return timeLabel;
            yield return echoesLabel;
            yield return batteryCarried;
            yield return playerMarker;
            yield return switchMarker;
            yield return batteryMarker;
            yield return powerMarker;
            yield return gateMarker;
            yield return exitMarker;
            yield return loopRecorded;
            yield return echoCreated;
            yield return nextLoop;
            yield return sectionCompleted;
            yield return gameCompleted;
            yield return synchronizationCompleted;
            yield return paused;
            yield return sectionRestarted;
            yield return resume;
            yield return restartSection;
            yield return restartGame;
            yield return quit;
            yield return confirmRestartSection;
            yield return confirmRestartGame;
            yield return confirmQuit;
            foreach (InteractionFailureReason reason in Enum.GetValues(typeof(InteractionFailureReason)))
                yield return GetFailureText(reason);
        }

        public void ApplyJapaneseDefaults()
        {
            languageCode = JapaneseLanguageCode;
            section1Name = "セクション1：過去の自分";
            section2Name = "セクション2：記録された操作";
            section3Name = "セクション3：過去との協力";
            section1Objective = "スイッチを使って扉を開ける";
            section2Objective = "電池で扉に電力を送る";
            section3Objective = "2体のエコーと協力して出口へ進む";
            section1Tutorial1 = "黄色の「自分」を青いスイッチまで移動する（WASD / 左スティック）";
            section1Tutorial2 = "扉が緑になったら、その場で R / START：ループを終了してエコーを作る";
            section1Tutorial3 = "E1・E2…のエコーは、それぞれ記録した過去の動きを繰り返す";
            section1Tutorial4 = "エコーを青いスイッチに残し、黄色の「自分」で緑の扉へ進む";
            section2Tutorial1 = "オレンジの電池に近づき E / A・× で持つ";
            section2Tutorial2 = "電池を持ったまま紫の電源へ運ぶ";
            section2Tutorial3 = "電池を入れたら R / START で操作を記録する";
            section2Tutorial4 = "エコーが電池を運ぶ間に、黄色の「自分」で緑の扉へ進む";
            section3Tutorial1 = "1体目にスイッチを任せる";
            section3Tutorial2 = "2体目に電池を運ばせる";
            section3Tutorial3 = "過去の自分たちと協力する";
            moveKeyboard = "WASD：移動";
            moveGamepad = "左スティック：移動";
            interactKeyboard = "[E] 調べる";
            interactGamepad = "[A / ×] 調べる";
            pickupBatteryKeyboard = "[E] 電池を持つ";
            pickupBatteryGamepad = "[A / ×] 電池を持つ";
            insertBatteryKeyboard = "[E] 電池を入れる";
            insertBatteryGamepad = "[A / ×] 電池を入れる";
            dropBatteryKeyboard = "[E] 電池を置く";
            dropBatteryGamepad = "[A / ×] 電池を置く";
            endLoop = "R / START：ループを終了";
            pausePrompt = "ESC / SELECT：一時停止";
            startPrompt = "画面をクリック、またはキー／ボタンを押して開始";
            loopLabel = "ループ";
            timeLabel = "残り時間";
            echoesLabel = "エコー";
            batteryCarried = "電池を保持中：紫の電源へ運ぶ";
            playerMarker = "自分";
            switchMarker = "スイッチ";
            batteryMarker = "電池";
            powerMarker = "電源";
            gateMarker = "扉";
            exitMarker = "出口";
            loopRecorded = "行動を記録しました";
            echoCreated = "エコー{0}を生成";
            nextLoop = "次のループを開始";
            sectionCompleted = "セクション完了";
            gameCompleted = "すべての実験を完了しました";
            synchronizationCompleted = "過去の自分たちとの同期に成功";
            paused = "一時停止";
            sectionRestarted = "このセクションをやり直しました";
            resume = "ゲームに戻る";
            restartSection = "このセクションをやり直す";
            restartGame = "最初からやり直す";
            quit = "ゲームを終了する";
            confirmRestartSection = "このセクションをやり直しますか？\nもう一度押すと実行します";
            confirmRestartGame = "最初からやり直しますか？\nもう一度押すと実行します";
            confirmQuit = "ゲームを終了しますか？\nもう一度押すと実行します";
            failureNone = "問題ありません";
            failureNoCandidate = "操作する対象がありません";
            failureTargetNotFound = "操作する対象がありません";
            failureTargetInactive = "操作する対象がありません";
            failureOutOfRange = "もう少し近づいてください";
            failureTargetUnavailable = "今は操作できません";
            failureHeldByAnother = "すでに誰かが持っています";
            failureActorNotCarrying = "電池を持っていません";
            failureSocketOccupied = "すでに電池が入っています";
            failureTargetBusy = "ほかのエコーが使用中です";
            failureUnsupported = "今は操作できません";
            failureMissingInteractor = "今は操作できません";
        }
    }

    public sealed class TutorialProgress
    {
        private readonly byte[] _steps = new byte[3];
        public int ActiveSection { get; private set; }
        public byte CurrentStep => _steps[ActiveSection];

        public void BeginSection(int zeroBasedSection)
        {
            ActiveSection = Mathf.Clamp(zeroBasedSection, 0, 2);
        }

        public void ResetSection(int zeroBasedSection)
        {
            ActiveSection = Mathf.Clamp(zeroBasedSection, 0, 2);
            _steps[ActiveSection] = 0;
        }

        public bool Advance(byte completedStep)
        {
            if (completedStep < _steps[ActiveSection]) return false;
            _steps[ActiveSection] = (byte)(completedStep + 1);
            return true;
        }

        public byte GetSectionStep(int zeroBasedSection) => _steps[zeroBasedSection];
    }
}
