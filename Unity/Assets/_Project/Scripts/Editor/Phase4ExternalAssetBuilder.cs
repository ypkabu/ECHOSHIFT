using System;
using EchoShift.Presentation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EchoShift.Editor
{
    public static class Phase4ExternalAssetBuilder
    {
        public const string CatalogPath =
            "Assets/_Project/Settings/Phase4ExternalAssetCatalog.asset";
        private const string QuaterniusRoot =
            "Assets/_Project/ThirdParty/Quaternius/ModularSciFiMegaKit";
        private const string RobotPath =
            "Assets/_Project/ThirdParty/Quaternius/AnimatedRobot/Robot.fbx";
        private const string MaterialRoot = "Assets/_Project/Art/Materials";
        private const string EnvironmentRoot = "Assets/_Project/Art/Environment";
        private const string CharacterRoot = "Assets/_Project/Art/Characters";
        private const string PropRoot = "Assets/_Project/Art/Props";

        private static readonly string[] FloorSources =
        {
            "Models/Platforms/Platform_3Plates.fbx",
            "Models/Platforms/Platform_CenterPlate.fbx",
            "Models/Platforms/Platform_Metal.fbx",
            "Models/Platforms/Platform_Squares.fbx"
        };

        private static readonly string[] WallSources =
        {
            "Models/Walls/WallAstra_Straight.fbx",
            "Models/Walls/WallAstra_Straight_Flat.fbx",
            "Models/Walls/WallBand_Straight.fbx",
            "Models/Walls/ShortWall_WhitePlate2_Straight.fbx",
            "Models/Walls/ShortWall_AccentStrip_Straight.fbx",
            "Models/Walls/BottomMetal_Straight.fbx"
        };

        private static readonly string[] ColumnSources =
        {
            "Models/Columns/Column_Astra.fbx",
            "Models/Columns/Column_Simple.fbx",
            "Models/Columns/Column_MetalSupport.fbx"
        };

        private static readonly string[] PropSources =
        {
            "Models/Props/Prop_AccessPoint.fbx",
            "Models/Props/Prop_Computer.fbx",
            "Models/Props/Prop_Crate4.fbx",
            "Models/Props/Prop_ItemHolder.fbx",
            "Models/Props/Prop_Light_Floor.fbx",
            "Models/Props/Prop_Light_Wide.fbx",
            "Models/Props/Prop_Vent_Wide.fbx"
        };

        public static void BuildFromCommandLine()
        {
            Phase4ExternalAssetCatalog catalog = Build();
            Debug.Log($"PHASE4_EXTERNAL_ASSETS_OK complete={catalog.IsComplete}");
            EditorApplication.Exit(catalog.IsComplete ? 0 : 1);
        }

        public static Phase4ExternalAssetCatalog Build()
        {
            EnsureFolders();
            ReimportOriginals();

            Material floor = Material("P4X_Floor", "T_Trim_01_BaseColor.png",
                "T_Trim_01_Normal.png", new Color(1.1f, 1.1f, 1.08f), 0.35f, 0.58f);
            Material wall = Material("P4X_Wall", "T_Trim_02_BaseColor.png",
                "T_Trim_02_Normal.png", new Color(0.92f, 0.98f, 1.05f), 0.25f, 0.48f);
            Material dark = Material("P4X_Dark", "T_Trim_03_BaseColor.png",
                "T_Trim_03_Normal.png", new Color(0.64f, 0.7f, 0.8f), 0.65f, 0.56f);
            Material trim = Material("P4X_Trim", "T_Trim_01_BaseColor.png",
                "T_Trim_01_Normal.png", new Color(0.62f, 0.72f, 0.82f), 0.55f, 0.72f);

            GameObject[] floors = BuildGroup(FloorSources, "Floor", EnvironmentRoot,
                new Vector3(4.4f, 0.16f, 3f), floor, false);
            GameObject[] walls = BuildGroup(WallSources, "Wall", EnvironmentRoot,
                new Vector3(4f, 3f, 0.24f), wall, false);
            GameObject[] columns = BuildGroup(ColumnSources, "Column", EnvironmentRoot,
                new Vector3(0.8f, 3f, 0.8f), trim, false);
            GameObject[] props = BuildGroup(PropSources, "Prop", PropRoot,
                new Vector3(1.2f, 1.4f, 1.2f), dark, false);
            GameObject doorFrame = BuildWrapper(
                QuaterniusRoot + "/Models/Platforms/Door_Frame_A.fbx",
                PropRoot + "/P4X_DoorFrame.prefab", new Vector3(5f, 3.5f, 0.65f),
                trim, false);
            GameObject doorPanel = BuildWrapper(
                QuaterniusRoot + "/Models/Platforms/Door_Simple.fbx",
                PropRoot + "/P4X_DoorPanel.prefab", new Vector3(3.78f, 2.7f, 0.36f),
                dark, false);
            GameObject robot = BuildWrapper(RobotPath,
                CharacterRoot + "/P4X_RobotVisual.prefab", new Vector3(1.25f, 1.85f, 1.1f),
                wall, true);

            Phase4ExternalAssetCatalog catalog =
                AssetDatabase.LoadAssetAtPath<Phase4ExternalAssetCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<Phase4ExternalAssetCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }
            catalog.Configure(floors, walls, columns, props, doorFrame, doorPanel, robot,
                floor, wall, dark, trim);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            if (!catalog.IsComplete)
                throw new InvalidOperationException("Phase 4 external asset catalog is incomplete.");
            return catalog;
        }

        private static GameObject[] BuildGroup(string[] relativeSources, string prefix,
            string outputRoot, Vector3 targetSize, Material material, bool preserveAspect)
        {
            GameObject[] result = new GameObject[relativeSources.Length];
            for (int i = 0; i < result.Length; i++)
            {
                string source = QuaterniusRoot + "/" + relativeSources[i];
                string name = $"P4X_{prefix}_{i + 1:00}.prefab";
                result[i] = BuildWrapper(source, outputRoot + "/" + name,
                    targetSize, material, preserveAspect);
            }
            return result;
        }

        private static GameObject BuildWrapper(string sourcePath, string prefabPath,
            Vector3 targetSize, Material material, bool preserveAspect)
        {
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (existing != null) return existing;

            GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath);
            if (source == null) throw new InvalidOperationException(
                $"Third-party model did not import: {sourcePath}");

            GameObject root = new GameObject(System.IO.Path.GetFileNameWithoutExtension(prefabPath));
            GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(source);
            model.name = "Vendor Model";
            model.transform.SetParent(root.transform, false);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely,
                InteractionMode.AutomatedAction);
            StripNonVisualComponents(model);
            Renderer[] renderers = model.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
                throw new InvalidOperationException($"Third-party model has no renderer: {sourcePath}");
            for (int i = 0; i < renderers.Length; i++)
            {
                Material[] slots = renderers[i].sharedMaterials;
                if (slots.Length == 0) slots = new Material[1];
                for (int slot = 0; slot < slots.Length; slot++) slots[slot] = material;
                renderers[i].sharedMaterials = slots;
            }
            Normalize(model.transform, renderers, targetSize, preserveAspect);
            root.isStatic = !preserveAspect;
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void Normalize(Transform model, Renderer[] renderers,
            Vector3 targetSize, bool preserveAspect)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            Vector3 size = bounds.size;
            if (size.x < 0.0001f || size.y < 0.0001f || size.z < 0.0001f)
                throw new InvalidOperationException($"Model bounds are invalid: {model.name};{size}");
            Vector3 scale;
            if (preserveAspect)
            {
                float uniform = targetSize.y / size.y;
                scale = Vector3.one * uniform;
            }
            else
            {
                scale = new Vector3(targetSize.x / size.x, targetSize.y / size.y,
                    targetSize.z / size.z);
            }
            model.localScale = scale;
            bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            model.position -= bounds.center;
        }

        private static void StripNonVisualComponents(GameObject model)
        {
            Collider[] colliders = model.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < colliders.Length; i++) Object.DestroyImmediate(colliders[i]);
            Rigidbody[] bodies = model.GetComponentsInChildren<Rigidbody>(true);
            for (int i = 0; i < bodies.Length; i++) Object.DestroyImmediate(bodies[i]);
            Animator[] animators = model.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++) Object.DestroyImmediate(animators[i]);
            Animation[] animations = model.GetComponentsInChildren<Animation>(true);
            for (int i = 0; i < animations.Length; i++) Object.DestroyImmediate(animations[i]);
        }

        private static Material Material(string name, string colorTextureName,
            string normalTextureName, Color tint, float metallic, float smoothness)
        {
            string path = MaterialRoot + "/" + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) throw new InvalidOperationException("URP/Lit shader is missing.");
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            Texture2D color = AssetDatabase.LoadAssetAtPath<Texture2D>(
                QuaterniusRoot + "/Textures/" + colorTextureName);
            Texture2D normal = AssetDatabase.LoadAssetAtPath<Texture2D>(
                QuaterniusRoot + "/Textures/" + normalTextureName);
            if (color == null || normal == null) throw new InvalidOperationException(
                $"External texture import failed: {colorTextureName};{normalTextureName}");
            material.SetTexture("_BaseMap", color);
            material.SetColor("_BaseColor", tint);
            material.SetTexture("_BumpMap", normal);
            material.SetFloat("_BumpScale", 0.7f);
            material.EnableKeyword("_NORMALMAP");
            material.SetFloat("_Metallic", metallic);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void ReimportOriginals()
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty,
                new[] { "Assets/_Project/ThirdParty/Quaternius",
                    "Assets/_Project/ThirdParty/Kenney" });
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (AssetImporter.GetAtPath(path) != null)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        private static void EnsureFolders()
        {
            Folder("Assets/_Project", "Art");
            Folder("Assets/_Project/Art", "Environment");
            Folder("Assets/_Project/Art", "Characters");
            Folder("Assets/_Project/Art", "Props");
            Folder("Assets/_Project/Art", "Materials");
        }

        private static void Folder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, child);
        }
    }
}
