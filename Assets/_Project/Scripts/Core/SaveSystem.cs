using System;
using System.IO;
using UnityEngine;
namespace ZombieAttack
{
    [Serializable]
    public class PlayerProfile
    {
        public int gems, bestWave, selected;
        public int version=3;
        public bool[] unlocked = { true, false, false, false, false, false, false, false };
    }
    public static class SaveSystem
    {
        const string Key = "DefesaZumbi.Profile.v1";
        public static PlayerProfile Load()
        {
            try
            {
                string json;
#if UNITY_WEBGL && !UNITY_EDITOR
                json = PlayerPrefs.GetString(Key, "");
#else
                string path = Path.Combine(Application.persistentDataPath, "profile.json");
                json = File.Exists(path) ? File.ReadAllText(path) : "";
#endif
                var profile = string.IsNullOrEmpty(json) ? new PlayerProfile() : JsonUtility.FromJson<PlayerProfile>(json);
                return Migrate(profile);
            }
            catch (Exception e) { Debug.LogWarning("Não foi possível ler o progresso: " + e.Message); return new PlayerProfile(); }
        }
        public static PlayerProfile Migrate(PlayerProfile profile)
        {
            if(profile == null) profile = new PlayerProfile();
            var owned=new bool[8]; owned[0]=true;
            if(profile.version<2)
            {
                // The original four-character roster placed the engineer in slot 3.
                int[] legacySlots={0,1,3,2};
                if(profile.unlocked!=null)
                    for(int i=0;i<Math.Min(4,profile.unlocked.Length);i++)owned[legacySlots[i]]=profile.unlocked[i];
                profile.selected=profile.selected>=0&&profile.selected<4?legacySlots[profile.selected]:0;
            }
            else if(profile.version>=3 && profile.unlocked!=null)
                Array.Copy(profile.unlocked,owned,Math.Min(8,profile.unlocked.Length));
            // Version 2 granted the whole roster for preview, without purchases.
            owned[0]=true; profile.unlocked=owned; profile.version=3;
            profile.selected = Mathf.Clamp(profile.selected,0,7);
            if(!profile.unlocked[profile.selected])profile.selected=0;
            profile.gems = Mathf.Max(0,profile.gems); profile.bestWave = Mathf.Max(0,profile.bestWave);
            return profile;
        }
        public static void Save(PlayerProfile profile)
        {
            string json = JsonUtility.ToJson(profile, true);
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                PlayerPrefs.SetString(Key, json); PlayerPrefs.Save();
#else
                string path = Path.Combine(Application.persistentDataPath, "profile.json");
                File.WriteAllText(path + ".tmp", json);
                File.Copy(path + ".tmp", path, true);
                File.Delete(path + ".tmp");
#endif
            }
            catch (Exception e) { Debug.LogWarning("Não foi possível salvar: " + e.Message); }
        }
    }
}
