using UnityEngine;
namespace ZHold
{
    public static class GameSettings
    {
        public static float Brightness=1, Sensitivity=1, Master=.8f, Effects=.85f, Music=.35f, Shake=.65f;
        public static bool ShowCornerCrates=true;
        public static Vector2 BrowserAim(Vector2 position)
        {
            // Keep native coordinates over interface panels; shape movement only in the playfield.
            if(position.y<=175||position.y>=790||position.x>1300&&position.y<405)return position;
            float nx=Mathf.Clamp((position.x-800)/800,-1,1),ny=Mathf.Clamp((position.y-482.5f)/307.5f,-1,1);
            float sensitivity=Mathf.Clamp(Sensitivity,.25f,3);
            return new Vector2(800+nx*sensitivity/(1+(sensitivity-1)*Mathf.Abs(nx))*800,
                482.5f+ny*sensitivity/(1+(sensitivity-1)*Mathf.Abs(ny))*307.5f);
        }
        public static void Load()
        {
            Brightness=Mathf.Clamp(LegacyProgress.Setting("Brightness",1),.65f,1.35f);
            Sensitivity=Mathf.Clamp(LegacyProgress.Setting("Sensitivity",1),.25f,3);
            Master=Mathf.Clamp01(LegacyProgress.Setting("Master",.8f)); Effects=Mathf.Clamp01(LegacyProgress.Setting("Effects",.85f));
            Music=Mathf.Clamp01(LegacyProgress.Setting("Music",.35f)); Shake=Mathf.Clamp01(LegacyProgress.Setting("Shake",.65f));
            ShowCornerCrates=PlayerPrefs.GetInt("ZHold.ShowCornerCrates",1)!=0;
        }
        public static void Save()
        {
            PlayerPrefs.SetFloat("ZHold.Brightness",Brightness); PlayerPrefs.SetFloat("ZHold.Sensitivity",Sensitivity);
            PlayerPrefs.SetFloat("ZHold.Master",Master); PlayerPrefs.SetFloat("ZHold.Effects",Effects);
            PlayerPrefs.SetFloat("ZHold.Music",Music); PlayerPrefs.SetFloat("ZHold.Shake",Shake);
            PlayerPrefs.SetInt("ZHold.ShowCornerCrates",ShowCornerCrates?1:0); PlayerPrefs.Save();
        }
    }
}
