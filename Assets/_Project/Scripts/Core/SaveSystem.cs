using System;
using System.IO;
using UnityEngine;
namespace ZombieAttack
{
    [Serializable]
    public class PlayerProfile
    {
        public int gems, bestWave, selected, version;
        public bool[] unlocked = { true, true, true, true, true, true, true, true };
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
            if(profile.version < 2) profile.selected = profile.selected == 3 ? 2 : 0;
            profile.version = 2;
            profile.unlocked = new[]{true,true,true,true,true,true,true,true};
            profile.selected = Mathf.Clamp(profile.selected,0,7);
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
