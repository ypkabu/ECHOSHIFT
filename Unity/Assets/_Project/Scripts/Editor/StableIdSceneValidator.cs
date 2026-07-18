using System;
using System.Collections.Generic;
using EchoShift.Interaction.Recorded;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoShift.Editor
{
    public readonly struct StableIdValidationResult
    {
        public StableIdValidationResult(int emptyCount, int duplicateCount)
        {
            EmptyCount = emptyCount;
            DuplicateCount = duplicateCount;
        }

        public int EmptyCount { get; }
        public int DuplicateCount { get; }
        public bool IsValid => EmptyCount == 0 && DuplicateCount == 0;
    }

    public static class StableIdSceneValidator
    {
        [MenuItem("ECHO SHIFT/Validate Stable Interaction IDs")]
        public static void ValidateActiveSceneFromMenu()
        {
            StableIdValidationResult result = ValidateScene(
                SceneManager.GetActiveScene(),
                out string error);
            if (!result.IsValid)
            {
                throw new InvalidOperationException(error);
            }

            Debug.Log("Stable interaction IDs are valid.");
        }

        [MenuItem("ECHO SHIFT/Repair Stable Interaction IDs")]
        public static void RepairActiveSceneFromMenu()
        {
            StableId[] identities = CollectSceneIds(SceneManager.GetActiveScene());
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int repairCount = 0;
            for (int i = 0; i < identities.Length; i++)
            {
                StableId identity = identities[i];
                string value = identity.Value;
                if (!string.IsNullOrWhiteSpace(value) && seen.Add(value))
                {
                    continue;
                }

                string replacement;
                do
                {
                    replacement = Guid.NewGuid().ToString("N");
                }
                while (!seen.Add(replacement));

                SerializedObject serializedIdentity = new SerializedObject(identity);
                serializedIdentity.FindProperty("value").stringValue = replacement;
                serializedIdentity.ApplyModifiedProperties();
                EditorUtility.SetDirty(identity);
                repairCount++;
            }

            Debug.Log($"Repaired {repairCount} Stable interaction ID(s).");
        }

        public static StableIdValidationResult Validate(
            IReadOnlyList<StableId> identities,
            out string error)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            int emptyCount = 0;
            int duplicateCount = 0;
            for (int i = 0; i < identities.Count; i++)
            {
                StableId identity = identities[i];
                string value = identity != null ? identity.Value : string.Empty;
                if (string.IsNullOrWhiteSpace(value))
                {
                    emptyCount++;
                    continue;
                }

                if (!seen.Add(value))
                {
                    duplicateCount++;
                }
            }

            StableIdValidationResult result = new StableIdValidationResult(
                emptyCount,
                duplicateCount);
            error = result.IsValid
                ? string.Empty
                : $"Stable ID validation failed: empty={emptyCount}, duplicates={duplicateCount}.";
            return result;
        }

        public static StableIdValidationResult ValidateScene(
            Scene scene,
            out string error)
        {
            return Validate(CollectSceneIds(scene), out error);
        }

        private static StableId[] CollectSceneIds(Scene scene)
        {
            List<StableId> identities = new List<StableId>(8);
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                roots[i].GetComponentsInChildren(true, identities);
            }

            return identities.ToArray();
        }
    }
}
