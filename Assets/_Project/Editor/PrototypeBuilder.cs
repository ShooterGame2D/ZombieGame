using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace ZombieAttack.Editor
{
    public static class PrototypeBuilder
    {
        const string Root="Assets/_Project";
        [MenuItem("Defesa Zumbi/Restaurar dados iniciais")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Root+"/Resources"); Directory.CreateDirectory(Root+"/Scenes");
            var catalog=Asset<GameCatalog>(Root+"/Resources/GameCatalog.asset");
            catalog.waves=Asset<WaveConfig>(Root+"/Data/Waves.asset");
            catalog.waves.healthPerWave=.08f; catalog.waves.speedPerWave=.025f; catalog.waves.spawnInterval=1.9f; catalog.waves.shopSeconds=45;
            string[] zn={"Errante","Andador","Corredor","Cuspidor","Brutamontes","Comandante","Blindado","Instável","Médico","Sombra"};
            int[] starts={1,1,2,3,5,10,3,4,5,6}, drops={3,4,5,7,12,50,8,7,9,10};
            float[] hp={2,3,2.8f,4,11,38,6,4,5,4}, speeds={.027f,.032f,.057f,.03f,.024f,.027f,.026f,.035f,.029f,.038f}, damage={6,8,7,10,16,23,10,10,8,11};
            catalog.zombies=new ZombieData[10];
            for(int i=0;i<10;i++)
            {
                var d=Asset<ZombieData>(Root+"/Data/Zombies/"+((ZombieKind)i)+".asset");
                d.kind=(ZombieKind)i; d.title=zn[i]; d.firstWave=starts[i]; d.coins=drops[i]; d.health=hp[i]; d.speed=speeds[i]; d.damage=damage[i];
                d.coinChance=i==5?1:.52f+(i>3?.1f:0); d.bodyArmor=i==6?.35f:1;
                d.artUV=PixelArtCatalog.ZombieUV(i); d.artAspect=PixelArtCatalog.ZombieAspect(i);
                d.skin=Draw2D.Hex(i==3?"A8D76A":i==9?"A2BFE0":"83B398"); d.shirt=Draw2D.Hex("587A75");
                EditorUtility.SetDirty(d); catalog.zombies[i]=d;
            }
            string[] names={"Recruta","Indígena","Engenheiro","Cowboy","Canguru","Criança","Lenhador","Ninja"};
            string[] weapons={"Pistola gasta","Arco simples","Parafusadeira de pregos","Revólver antigo","Bumerangue","Estilingue em Y","Machado de arremesso","Shuriken de treino"};
            string[] abilities={"Granada","Raízes da floresta","Torre de emergência","Olho de águia","Salto sísmico","Isca de brinquedo","Corte devastador","Nuvem de fumaça"};
            string[] descriptions={"Explosão na mira: 12 de dano em área.","Prende os zumbis do lado atual por 5 s.","Monta uma torre automática por 10 s no lado atual.","Disparos causam 2,5× dano durante 6 s.","Empurra a horda do lado atual e causa dano.","Um brinquedo distrai o lado atual durante 6 s.","Arremesso poderoso: 18 de dano em área na mira.","Invulnerabilidade por 5 s e rajada no lado atual."};
            string[] maps={"Posto militar","Amazônia","Canteiro de obras","Faroeste","Outback","Pátio da escola","Serraria na floresta","Templo ao luar"};
            string[] accents={"AAC18D","81CC9F","F1BE67","E2A97C","EBA870","8ACEDC","DD8C72","B5ACDE"};
            float[] health={110,95,105,100,125,80,135,90}, cooldowns={22,24,30,26,23,22,26,30};
            float[] damages={1,1.05f,.98f,1.08f,1, .95f,1.1f,.98f}, rates={2.7f,2.5f,2.8f,2.6f,2.6f,2.9f,2.4f,2.8f}, reload={2.4f,2.3f,2.5f,2.6f,2.35f,2.3f,2.55f,2.4f};
            int[] caps={8,7,8,6,7,8,6,8};
            catalog.characters=new CharacterData[8];
            for(int i=0;i<8;i++)
            {
                var w=Asset<WeaponData>(Root+"/Data/Weapons/Starter"+i+".asset");
                w.title=weapons[i]; w.kind=(WeaponKind)i; w.damage=damages[i]; w.capacity=caps[i]; w.fireRate=rates[i]; w.reloadSeconds=reload[i]; w.headMultiplier=2; w.splashRadius=0; w.startingReserve=72; w.price=0;
                EditorUtility.SetDirty(w);
                var c=Asset<CharacterData>(Root+"/Data/Characters/Character"+i+".asset");
                c.title=names[i]; c.weapon=w; c.price=0; c.health=health[i]; c.ability=(AbilityKind)i; c.cooldown=cooldowns[i]; c.description=descriptions[i]; c.abilityDescription=descriptions[i]; c.abilityTitle=abilities[i]; c.accent=Draw2D.Hex(accents[i]); c.theme=(MapTheme)i; c.mapTitle=maps[i];
                EditorUtility.SetDirty(c); catalog.characters[i]=c;
            }
            string[] shopNames={"Submetralhadora","Escopeta","Carabina","Besta de precisão","Lança-granadas"};
            string[] shopDesc={"Rajadas rápidas e pente de 24","Impacto em área curta","Precisão, alcance e dano","Cabeça 3× e impacto forte","Explosões na posição da mira"};
            float[] shopDamage={1.5f,4,2.5f,4.5f,7}, shopRates={7,1.3f,3.8f,1.6f,1}, shopReloads={2.1f,3,2.2f,2.4f,3.4f}, radii={0,100,0,0,180};
            int[] shopCaps={24,6,16,5,4}, shopPrices={55,70,90,100,145};
            catalog.shopWeapons=new WeaponData[5];
            for(int i=0;i<5;i++)
            {
                var w=Asset<WeaponData>(Root+"/Data/Weapons/Shop"+i+".asset");
                w.title=shopNames[i]; w.description=shopDesc[i]; w.kind=(WeaponKind)(8+i); w.damage=shopDamage[i]; w.fireRate=shopRates[i]; w.reloadSeconds=shopReloads[i]; w.capacity=shopCaps[i]; w.price=shopPrices[i]; w.splashRadius=radii[i]; w.headMultiplier=i==3?3:2;
                EditorUtility.SetDirty(w); catalog.shopWeapons[i]=w;
            }
            string[] towerNames={"Atiradora","Gelo","Explosiva","Barricada"}, towerDesc={"Tiro automático neste lado","Desacelera zumbis próximos","Disparo com dano em área","Somente à frente de uma torre"};
            int[] prices={45,60,80,25}; catalog.towers=new TowerData[4];
            for(int i=0;i<4;i++)
            {
                var t=Asset<TowerData>(Root+"/Data/Towers/"+((TowerKind)i)+".asset"); t.kind=(TowerKind)i; t.title=towerNames[i]; t.description=towerDesc[i]; t.price=prices[i]; t.health=i==3?160:85; t.damage=i==2?4:1.5f; t.interval=i==2?3:1.2f;
                EditorUtility.SetDirty(t); catalog.towers[i]=t;
            }
            foreach(string path in Directory.GetFiles(Root+"/Resources/Art/Pixel","*.png"))
            {
                AssetDatabase.ImportAsset(path); var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default; importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.filterMode=FilterMode.Point; importer.npotScale=TextureImporterNPOTScale.None; importer.wrapMode=TextureWrapMode.Clamp; importer.maxTextureSize=4096; importer.textureCompression=TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            }
            EditorUtility.SetDirty(catalog); EditorUtility.SetDirty(catalog.waves);
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh(); Debug.Log("PROTOTYPE_DATA_RESTORED");
        }
        static T Asset<T>(string path) where T:ScriptableObject
        {
            var asset=AssetDatabase.LoadAssetAtPath<T>(path); if(asset!=null) return asset;
            Directory.CreateDirectory(Path.GetDirectoryName(path)); asset=ScriptableObject.CreateInstance<T>(); AssetDatabase.CreateAsset(asset,path); return asset;
        }
        [MenuItem("Defesa Zumbi/Validar mecânicas")]
        public static void Validate() => PrototypeValidation.Run(AssetDatabase.LoadAssetAtPath<GameCatalog>(Root+"/Resources/GameCatalog.asset"));
        [MenuItem("Defesa Zumbi/Abrir protótipo")]
        public static void OpenPrototype()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene(Root+"/Scenes/Boot.unity");
        }
        [MenuItem("Defesa Zumbi/Gerar Windows")]
        public static void BuildWindows()
        {
            Validate(); Directory.CreateDirectory("Builds/Windows");
            Build("Builds/Windows/Defesa Zumbi.exe",BuildTarget.StandaloneWindows64);
        }
        [MenuItem("Defesa Zumbi/Gerar Web")]
        public static void BuildWeb()
        {
            Validate();
            var compression=PlayerSettings.WebGL.compressionFormat;
            var hashes=PlayerSettings.WebGL.nameFilesAsHashes;
            var template=PlayerSettings.WebGL.template;
            try
            {
                PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
                PlayerSettings.WebGL.nameFilesAsHashes=true;
                PlayerSettings.WebGL.template="PROJECT:Quintal";
                Directory.CreateDirectory("Builds/Web"); Build("Builds/Web",BuildTarget.WebGL);
            }
            finally
            {
                PlayerSettings.WebGL.compressionFormat=compression;
                PlayerSettings.WebGL.nameFilesAsHashes=hashes;
                PlayerSettings.WebGL.template=template;
            }
        }
        static void Build(string path,BuildTarget target)
        {
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{Root+"/Scenes/Boot.unity"},locationPathName=path,target=target,options=BuildOptions.None});
            if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded) throw new Exception("Build falhou: "+report.summary.result);
            Debug.Log("BUILD_SUCCESS "+path);
        }
    }
}
