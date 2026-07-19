using System;
using System.Collections.Generic;
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
        [SerializeField, Min(12)] private int dynamicFontSize = 32;

        private Font _runtimeFont;

        public string ResolvedFontName { get; private set; } = string.Empty;
        public bool HasRequiredGlyphs { get; private set; }
        public bool IsReady => _runtimeFont != null && HasRequiredGlyphs;
        public static IReadOnlyList<string> OrderedFontCandidates => FontCandidates;

        public void Configure(Phase3TextCatalog textCatalog, Transform[] textRoots)
        {
            catalog = textCatalog;
            roots = textRoots ?? Array.Empty<Transform>();
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
            if (catalog == null || roots == null || roots.Length == 0) return false;
            ReleaseFont();
            string required = catalog.GetRequiredGlyphCharacters();
            for (int i = 0; i < FontCandidates.Length; i++)
            {
                Font candidate = Font.CreateDynamicFontFromOSFont(FontCandidates[i], dynamicFontSize);
                if (candidate == null) continue;
                candidate.RequestCharactersInTexture(required, dynamicFontSize, FontStyle.Normal);
                if (!SupportsAllRequiredGlyphs(candidate, required))
                {
                    DestroyFont(candidate);
                    continue;
                }
                _runtimeFont = candidate;
                ResolvedFontName = FontCandidates[i];
                HasRequiredGlyphs = true;
                ApplyToRoots(candidate);
                return true;
            }
            ResolvedFontName = string.Empty;
            HasRequiredGlyphs = false;
            return false;
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

        private void ApplyToRoots(Font font)
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
            }
        }

        private void OnDestroy()
        {
            ReleaseFont();
        }

        private void ReleaseFont()
        {
            if (_runtimeFont == null) return;
            DestroyFont(_runtimeFont);
            _runtimeFont = null;
            ResolvedFontName = string.Empty;
            HasRequiredGlyphs = false;
        }

        private static void DestroyFont(Font font)
        {
            if (font == null) return;
            if (Application.isPlaying) Destroy(font);
            else DestroyImmediate(font);
        }
    }
}
