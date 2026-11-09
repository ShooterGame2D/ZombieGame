using System.IO;
using UnityEngine;
namespace ZHold
{
    // Read-only compatibility names: never displayed as the current game title.
    public static class LegacyProgress
    {
        public const string ProfileKey="DefesaZumbi.Profile.v1";
        public static string FindProfile(string currentDirectory)
        {
            string current=Path.Combine(currentDirectory,"profile.json");
            if(File.Exists(current))return current;
            var company=Directory.GetParent(currentDirectory);
            if(company==null)return current;
            foreach(string folder in new[]{"Defesa Zumbi","ZombieGame"})
            {
                string candidate=Path.Combine(company.FullName,folder,"profile.json");
                if(File.Exists(candidate))return candidate;
            }
            return current;
        }
        public static float Setting(string key,float fallback)
        {
            string current="ZHold."+key;
            if(PlayerPrefs.HasKey(current))return PlayerPrefs.GetFloat(current,fallback);
            return PlayerPrefs.GetFloat("DZ."+key,fallback);
        }
    }
}
