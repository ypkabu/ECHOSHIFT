using EchoShift.Gameplay;
using UnityEngine;

namespace EchoShift.Presentation
{
    public sealed class TutorialGuide : MonoBehaviour
    {
        [SerializeField] private PuzzleSectionController section;
        [SerializeField] private Phase3TextCatalog catalog;
        [SerializeField] private GameplayHud hud;
        private readonly TutorialProgress _progress = new TutorialProgress();

        public TutorialProgress Progress => _progress;
        public string CurrentGuidance { get; private set; } = string.Empty;

        public void Configure(
            PuzzleSectionController puzzleSection,
            Phase3TextCatalog textCatalog,
            GameplayHud gameplayHud)
        {
            section = puzzleSection;
            catalog = textCatalog;
            hud = gameplayHud;
            _progress.BeginSection(Mathf.Max(0, section.SectionNumber - 1));
            CurrentGuidance = catalog.GetInitialTutorial(section.SectionNumber - 1);
        }

        public void ShowStep(byte step, string message)
        {
            if (_progress.Advance(step))
            {
                CurrentGuidance = message ?? string.Empty;
                hud?.SetTutorialMessage(CurrentGuidance);
            }
        }
    }

}
