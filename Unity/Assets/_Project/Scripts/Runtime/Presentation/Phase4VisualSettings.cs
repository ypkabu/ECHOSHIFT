using System;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace EchoShift.Presentation
{
    [CreateAssetMenu(menuName = "ECHO SHIFT/Phase 4 Visual Settings")]
    public sealed class Phase4VisualSettings : ScriptableObject
    {
        public const int RequiredEchoGenerations = 3;
        public const int RequiredHudIcons = 6;

        [Header("Visual language")]
        [SerializeField] private Color environmentLight = new Color(0.82f, 0.86f, 0.9f, 1f);
        [SerializeField] private Color environmentDark = new Color(0.035f, 0.05f, 0.075f, 1f);
        [SerializeField] private Color playerColor = new Color(1f, 0.68f, 0.12f, 1f);
        [SerializeField] private Color plateColor = new Color(0.05f, 0.82f, 1f, 1f);
        [SerializeField] private Color batteryColor = new Color(1f, 0.36f, 0.055f, 1f);
        [SerializeField] private Color goalColor = new Color(0.72f, 1f, 0.82f, 1f);
        [SerializeField] private Color dangerColor = new Color(1f, 0.12f, 0.16f, 1f);
        [SerializeField] private Color[] echoColors =
        {
            new Color(0.06f, 0.78f, 1f, 1f),
            new Color(0.66f, 0.32f, 1f, 1f),
            new Color(0.16f, 1f, 0.68f, 1f)
        };

        [Header("Shared materials")]
        [SerializeField] private Material facilityPanelMaterial;
        [SerializeField] private Material facilityDarkMaterial;
        [SerializeField] private Material facilityTrimMaterial;
        [SerializeField] private Material playerMaterial;
        [SerializeField] private Material plateMaterial;
        [SerializeField] private Material batteryMaterial;
        [SerializeField] private Material goalMaterial;
        [SerializeField] private Material dangerMaterial;
        [SerializeField] private Material glassMaterial;
        [SerializeField] private Material[] echoMaterials = Array.Empty<Material>();

        [Header("Packaged font")]
        [SerializeField] private Font packagedJapaneseFont;
        [SerializeField] private TMP_FontAsset packagedJapaneseTmpFont;

        [Header("Feedback")]
        [SerializeField] private GameObject pulseVfxPrefab;
        [SerializeField] private Phase4AudioCueSet audioCues;
        [SerializeField] private Sprite[] hudIcons = Array.Empty<Sprite>();
        [SerializeField] private VolumeProfile volumeProfile;

        [Header("Safe presentation limits")]
        [SerializeField, Range(0f, 1.5f)] private float bloomIntensity = 0.3f;
        [SerializeField, Range(0f, 0.4f)] private float vignetteIntensity = 0.07f;
        [SerializeField, Range(-25f, 25f)] private float colorContrast = 2f;
        [SerializeField, Range(-25f, 25f)] private float colorSaturation = -2f;
        [SerializeField, Range(1, 8)] private int maximumRealtimeLightsPerSection = 2;
        [SerializeField, Min(4)] private int feedbackPoolSize = 12;

        [Header("Capture")]
        [SerializeField] private int captureWidth = 1920;
        [SerializeField] private int captureHeight = 1080;
        [SerializeField] private string captureDirectory = "Captures/Phase4_2";

        public Color EnvironmentLight => environmentLight;
        public Color EnvironmentDark => environmentDark;
        public Color PlayerColor => playerColor;
        public Color PlateColor => plateColor;
        public Color BatteryColor => batteryColor;
        public Color GoalColor => goalColor;
        public Color DangerColor => dangerColor;
        public Material FacilityPanelMaterial => facilityPanelMaterial;
        public Material FacilityDarkMaterial => facilityDarkMaterial;
        public Material FacilityTrimMaterial => facilityTrimMaterial;
        public Material PlayerMaterial => playerMaterial;
        public Material PlateMaterial => plateMaterial;
        public Material BatteryMaterial => batteryMaterial;
        public Material GoalMaterial => goalMaterial;
        public Material DangerMaterial => dangerMaterial;
        public Material GlassMaterial => glassMaterial;
        public Font PackagedJapaneseFont => packagedJapaneseFont;
        public TMP_FontAsset PackagedJapaneseTmpFont => packagedJapaneseTmpFont;
        public GameObject PulseVfxPrefab => pulseVfxPrefab;
        public Phase4AudioCueSet AudioCues => audioCues;
        public Sprite[] HudIcons => hudIcons;
        public VolumeProfile VolumeProfile => volumeProfile;
        public float BloomIntensity => bloomIntensity;
        public float VignetteIntensity => vignetteIntensity;
        public float ColorContrast => colorContrast;
        public float ColorSaturation => colorSaturation;
        public int MaximumRealtimeLightsPerSection => maximumRealtimeLightsPerSection;
        public int FeedbackPoolSize => feedbackPoolSize;
        public int CaptureWidth => captureWidth;
        public int CaptureHeight => captureHeight;
        public string CaptureDirectory => captureDirectory;

        public bool HasRequiredReferences =>
            facilityPanelMaterial != null && facilityDarkMaterial != null &&
            facilityTrimMaterial != null && playerMaterial != null &&
            plateMaterial != null && batteryMaterial != null &&
            goalMaterial != null && dangerMaterial != null && glassMaterial != null &&
            echoMaterials != null && echoMaterials.Length == RequiredEchoGenerations &&
            echoMaterials[0] != null && echoMaterials[1] != null && echoMaterials[2] != null &&
            packagedJapaneseFont != null && packagedJapaneseTmpFont != null &&
            pulseVfxPrefab != null && audioCues != null && audioCues.HasAllCues &&
            hudIcons != null && hudIcons.Length >= RequiredHudIcons &&
            volumeProfile != null;

        public Color GetEchoColor(int generation)
        {
            int index = generation > 0
                ? (generation - 1) % RequiredEchoGenerations
                : 0;
            return echoColors != null && echoColors.Length == RequiredEchoGenerations
                ? echoColors[index]
                : Color.cyan;
        }

        public Material GetEchoMaterial(int generation)
        {
            int index = generation > 0
                ? (generation - 1) % RequiredEchoGenerations
                : 0;
            return echoMaterials != null && echoMaterials.Length == RequiredEchoGenerations
                ? echoMaterials[index]
                : null;
        }

        public bool EchoColorsAreDistinct()
        {
            if (echoColors == null || echoColors.Length != RequiredEchoGenerations) return false;
            for (int i = 0; i < echoColors.Length; i++)
            {
                for (int j = i + 1; j < echoColors.Length; j++)
                {
                    Vector3 difference = new Vector3(
                        echoColors[i].r - echoColors[j].r,
                        echoColors[i].g - echoColors[j].g,
                        echoColors[i].b - echoColors[j].b);
                    if (difference.sqrMagnitude < 0.08f) return false;
                }
            }
            return true;
        }

        public bool HasSafePostProcessingValues =>
            bloomIntensity >= 0f && bloomIntensity <= 0.75f &&
            vignetteIntensity >= 0f && vignetteIntensity <= 0.22f &&
            Mathf.Abs(colorContrast) <= 15f && Mathf.Abs(colorSaturation) <= 15f;

        public void Configure(
            Material panels, Material dark, Material trim,
            Material player, Material plate, Material battery,
            Material goal, Material danger, Material glass,
            Material[] generations, Font legacyFont, TMP_FontAsset tmpFont,
            GameObject vfxPrefab, Phase4AudioCueSet cues, Sprite[] icons,
            VolumeProfile profile)
        {
            facilityPanelMaterial = panels;
            facilityDarkMaterial = dark;
            facilityTrimMaterial = trim;
            playerMaterial = player;
            plateMaterial = plate;
            batteryMaterial = battery;
            goalMaterial = goal;
            dangerMaterial = danger;
            glassMaterial = glass;
            echoMaterials = generations ?? Array.Empty<Material>();
            packagedJapaneseFont = legacyFont;
            packagedJapaneseTmpFont = tmpFont;
            pulseVfxPrefab = vfxPrefab;
            audioCues = cues;
            hudIcons = icons ?? Array.Empty<Sprite>();
            volumeProfile = profile;
            bloomIntensity = 0.3f;
            vignetteIntensity = 0.07f;
            colorContrast = 2f;
            colorSaturation = -2f;
            captureDirectory = "Captures/Phase4_2";
        }
    }
}
