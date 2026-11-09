using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZHold.Editor
{
    public class CombatTextureImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if((!assetPath.StartsWith("Assets/_Project/Art/Combat-v1/")&&!assetPath.StartsWith("Assets/_Project/Resources/Art/Expansion/"))||!assetPath.EndsWith(".png")) return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.mipmapEnabled=false; importer.filterMode=FilterMode.Point; importer.wrapMode=TextureWrapMode.Clamp;
            importer.textureCompression=TextureImporterCompression.Uncompressed; importer.maxTextureSize=2048;
            importer.npotScale=TextureImporterNPOTScale.None; importer.alphaIsTransparency=true; importer.isReadable=false;
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            var sprites=new SpriteMetaData[6];
            for(int i=0;i<6;i++) sprites[i]=new SpriteMetaData{name=Path.GetFileNameWithoutExtension(assetPath)+"_"+i.ToString("00"),rect=new Rect(i%3*(width/3), (1-i/3)*(height/2),width/3,height/2),alignment=9,pivot=new Vector2(.5f,.08f)};
#pragma warning disable 618
            importer.spritesheet=sprites;
#pragma warning restore 618
        }
    }
    public static class CombatAssetImporter
    {
        const string Root="Assets/_Project/Art/Combat-v1";
        [MenuItem("Z-Hold/Atualizar arte de combate")]
        public static void Import()
        {
            AssetDatabase.Refresh();
            const string path="Assets/_Project/Resources/CombatSpriteCatalog.asset";
            var catalog=AssetDatabase.LoadAssetAtPath<CombatSpriteCatalog>(path);
            if(catalog==null) { catalog=ScriptableObject.CreateInstance<CombatSpriteCatalog>(); AssetDatabase.CreateAsset(catalog,path); }
            catalog.weapons=Enum.GetNames(typeof(WeaponKind)).Select(id=>Sheet("Weapons",id)).ToArray();
            catalog.attacks=Enum.GetNames(typeof(ZombieKind)).Select(id=>Sheet("Attacks",id)).ToArray();
            catalog.defenses=Enum.GetNames(typeof(TowerKind)).Select(id=>Sheet("Defenses",id)).ToArray();
            EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets();
            ExportSoundSamples();
            PrototypeBuilder.Validate();
            Debug.Log("COMBAT_ASSETS_IMPORTED: 27 sheets / 162 sprite frames");
        }
        static CombatSpriteCatalog.Sheet Sheet(string folder,string id)
        {
            string path=Root+"/"+folder+"/"+id+".png";
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if(texture==null) throw new Exception("Missing combat sheet: "+path);
            // Analyze source pixels only in the editor; retain no extra CPU texture copy in WebGL.
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false);
            ImageConversion.LoadImage(source,File.ReadAllBytes(path));
            var pixels=source.GetPixels32(); int cw=source.width/3,ch=source.height/2;
            UnityEngine.Object.DestroyImmediate(source);
            var result=new CombatSpriteCatalog.Sheet{id=id,texture=texture,bounds=new Rect[6]};
            float maximumHeight=0;
            for(int frame=0;frame<6;frame++)
            {
                int x0=frame%3*cw,y0=(1-frame/3)*ch,minX=cw,minY=ch,maxX=0,maxY=0,opaque=0,transparent=0;
                for(int y=0;y<ch;y++) for(int x=0;x<cw;x++)
                {
                    byte alpha=pixels[(y0+y)*texture.width+x0+x].a;
                    if(alpha<8) transparent++;
                    if(alpha<180) continue;
                    opaque++; minX=Mathf.Min(minX,x); maxX=Mathf.Max(maxX,x); minY=Mathf.Min(minY,ch-1-y); maxY=Mathf.Max(maxY,ch-1-y);
                }
                if(opaque<cw*ch*.015f||transparent<cw*ch*.15f) throw new Exception("Missing figure or transparency: "+id+" frame "+frame);
                result.bounds[frame]=new Rect(minX,minY,maxX-minX+1,maxY-minY+1);
                maximumHeight=Mathf.Max(maximumHeight,maxY-minY+1);
            }
            result.referenceHeight=maximumHeight;
            return result;
        }
        static void ExportSoundSamples()
        {
            string folder=Root+"/Sounds"; Directory.CreateDirectory(folder);
            foreach(WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
                foreach(string action in new[]{"shot_","reload_"}) WriteWav(folder+"/"+action+kind+".wav",PrototypeAudio.CreateSamples(action+kind));
            WriteWav(folder+"/draw_Bow.wav",PrototypeAudio.CreateSamples("draw_Bow"));
        }
        static void WriteWav(string path,float[] samples)
        {
            using(var writer=new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+samples.Length*2);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1);
                writer.Write(44100); writer.Write(88200); writer.Write((short)2); writer.Write((short)16);
                writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(samples.Length*2);
                foreach(float sample in samples) writer.Write((short)(Mathf.Clamp(sample,-1,1)*32767));
            }
        }
    }
}
