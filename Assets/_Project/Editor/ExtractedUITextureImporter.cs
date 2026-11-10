using UnityEditor;

namespace ZHold.Editor
{
    public sealed class ExtractedUITextureImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/_Project/Resources/Art/UI/Extracted/")||!assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.alphaSource=TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;
            importer.filterMode=UnityEngine.FilterMode.Point;
            importer.wrapMode=UnityEngine.TextureWrapMode.Clamp;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.maxTextureSize=4096;
        }
    }
}
