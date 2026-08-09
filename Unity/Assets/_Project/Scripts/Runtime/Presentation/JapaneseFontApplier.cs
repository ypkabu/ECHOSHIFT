using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EchoShift.Presentation
{
    public sealed class JapaneseFontApplier : MonoBehaviour
    {
        private static readonly string[] FontCandidates =
        {
            "Noto Sans JP",
            "BIZ UDPGothic",
            "Yu Gothic UI",
            "Yu Gothic",
            "Meiryo"
        };

        [SerializeField] private Phase3TextCatalog catalog;
        [SerializeField] private Transform[] roots = Array.Empty<Transform>();
        [SerializeField] private Font packagedFont;
        [SerializeField] private TMP_FontAsset packagedTmpFont;

        public string ResolvedFontName { get; private set; } = string.Empty;
        public bool HasRequiredGlyphs { get; private set; }
        public bool IsReady => packagedFont != null && packagedTmpFont != null && HasRequiredGlyphs;
        public bool UsesPackagedFont => packagedFont != null && packagedTmpFont != null;
        public bool HasCatalog => catalog != null;
        public int RootCount => roots != null ? roots.Length : -1;
        public static IReadOnlyList<string> OrderedFontCandidates => FontCandidates;

        public void Configure(Phase3TextCatalog textCatalog, Transform[] textRoots)
        {
            catalog = textCatalog;
            roots = textRoots ?? Array.Empty<Transform>();
        }

        public void Configure(
            Phase3TextCatalog textCatalog,
            Transform[] textRoots,
            Font legacyFont,
            TMP_FontAsset tmpFont)
        {
            Configure(textCatalog, textRoots);
            packagedFont = legacyFont;
            packagedTmpFont = tmpFont;
        }

        private void Awake()
        {
            if (!ApplyNow())
            {
                Debug.LogError("Japanese UI font initialization failed or required glyphs are missing.", this);
            }
        }

        public bool ApplyNow()
        {
            if (catalog == null || roots == null || roots.Length == 0 ||
                packagedFont == null || packagedTmpFont == null)
            {
                ResolvedFontName = string.Empty;
                HasRequiredGlyphs = false;
                return false;
            }

            string required = catalog.GetRequiredGlyphCharacters();
            packagedFont.RequestCharactersInTexture(required, 32, FontStyle.Normal);
            HasRequiredGlyphs = SupportsAllRequiredGlyphs(packagedTmpFont, required);
            if (!HasRequiredGlyphs)
            {
                ResolvedFontName = string.Empty;
                return false;
            }

            ResolvedFontName = "Noto Sans JP (Packaged)";
            ApplyToRoots(packagedFont, packagedTmpFont);
            return true;
        }

        public static bool SupportsAllRequiredGlyphs(Font font, string required)
        {
            if (font == null || string.IsNullOrEmpty(required)) return false;
            for (int i = 0; i < required.Length; i++)
            {
                char character = required[i];
                if (char.IsWhiteSpace(character) || char.IsControl(character)) continue;
                if (!font.HasCharacter(character)) return false;
            }
            return true;
        }

        public static bool SupportsAllRequiredGlyphs(TMP_FontAsset font, string required)
        {
            return string.IsNullOrEmpty(GetMissingRequiredGlyphs(font, required));
        }

        public static string GetMissingRequiredGlyphs(TMP_FontAsset font, string required)
        {
            if (font == null || string.IsNullOrEmpty(required) || font.characterTable == null)
                return required ?? string.Empty;
            StringBuilder missing = new StringBuilder();
            for (int i = 0; i < required.Length; i++)
            {
                char character = required[i];
                if (char.IsWhiteSpace(character) || char.IsControl(character)) continue;
                bool found = false;
                for (int j = 0; j < font.characterTable.Count; j++)
                {
                    if (font.characterTable[j].unicode != character) continue;
                    found = true;
                    break;
                }
                if (!found && missing.ToString().IndexOf(character) < 0) missing.Append(character);
            }
            return missing.ToString();
        }

        private void ApplyToRoots(Font font, TMP_FontAsset tmpFont)
        {
            for (int i = 0; i < roots.Length; i++)
            {
                Transform root = roots[i];
                if (root == null) continue;
                Text[] labels = root.GetComponentsInChildren<Text>(true);
                for (int labelIndex = 0; labelIndex < labels.Length; labelIndex++)
                {
                    labels[labelIndex].font = font;
                }
                TextMesh[] worldLabels = root.GetComponentsInChildren<TextMesh>(true);
                for (int labelIndex = 0; labelIndex < worldLabels.Length; labelIndex++)
                {
                    TextMesh label = worldLabels[labelIndex];
                    label.font = font;
                    MeshRenderer renderer = label.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.sharedMaterial = font.material;
                }
                TMP_Text[] tmpLabels = root.GetComponentsInChildren<TMP_Text>(true);
                for (int labelIndex = 0; labelIndex < tmpLabels.Length; labelIndex++)
                {
                    tmpLabels[labelIndex].font = tmpFont;
                }
            }
        }
    }
}
