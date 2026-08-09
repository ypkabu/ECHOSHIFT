using UnityEditor;
using UnityEngine;

namespace EchoShift.Editor
{
    public sealed class Phase4ThirdPartyImportProcessor : AssetPostprocessor
    {
        private const string ThirdPartyRoot = "Assets/_Project/ThirdParty/";
        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot, System.StringComparison.Ordinal)) return;
            ModelImporter importer = (ModelImporter)assetImporter;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.importCameras = false;
            importer.importLights = false;
            importer.isReadable = false;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.importBlendShapes = false;
            importer.addCollider = false;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.CalculateMikk;

            // Phase 4.1 adopts the robot as a static presentation mesh. Importing its
            // clips would add an unused Animator and make the wrapper hierarchy unstable.
            importer.importAnimation = false;
            importer.animationType = ModelImporterAnimationType.None;
        }

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot, System.StringComparison.Ordinal)) return;
            TextureImporter importer = (TextureImporter)assetImporter;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.compressionQuality = 60;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.alphaSource = TextureImporterAlphaSource.None;
            if (assetPath.EndsWith("_Normal.png", System.StringComparison.Ordinal))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.sRGBTexture = false;
            }
            else
            {
                importer.textureType = TextureImporterType.Default;
                importer.sRGBTexture = true;
            }
        }

        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith(ThirdPartyRoot, System.StringComparison.Ordinal)) return;
            AudioImporter importer = (AudioImporter)assetImporter;
            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.6f;
            settings.preloadAudioData = true;
            importer.defaultSampleSettings = settings;
            importer.forceToMono = false;
            importer.loadInBackground = false;
        }
    }
}
