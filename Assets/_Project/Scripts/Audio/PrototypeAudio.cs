using System;
using System.Collections.Generic;
using UnityEngine;
namespace ZombieAttack
{
    // Layered transient/noise sound design, with separate signatures for each weapon family.
    public class PrototypeAudio : MonoBehaviour
    {
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        readonly AudioSource[] voices=new AudioSource[16];
        int voice;
        bool muted;
        float lastGroan;
        public ViewSide ListenerSide { get; set; }
        public bool Muted { get=>muted; set { muted=value; foreach(var source in voices) if(source!=null) source.mute=value; } }
        void Awake()
        {
            for(int i=0;i<voices.Length;i++) { voices[i]=gameObject.AddComponent<AudioSource>(); voices[i].playOnAwake=false; voices[i].spatialBlend=0; }
            foreach(WeaponKind kind in Enum.GetValues(typeof(WeaponKind))) { Add("shot_"+kind); Add("reload_"+kind); }
            foreach(AbilityKind kind in Enum.GetValues(typeof(AbilityKind))) Add("ability_"+kind);
            foreach(string key in new[]{"hit","head","coin","empty","wave","hurt","buy","loaded","turn","metal","wood","spit","groan","explosion","tower"}) Add(key);
        }
        void Add(string key)
        {
            var samples=CreateSamples(key); var clip=AudioClip.Create(key,samples.Length,1,44100,false); clip.SetData(samples,0); clips[key]=clip;
        }
        public void Play(string key) => PlayClip(key,false);
        public void PlaySpatial(string key,ViewSide side) => PlayClip(key,side!=ListenerSide);
        void PlayClip(string key,bool behind)
        {
            if(muted || !clips.TryGetValue(key,out var clip)) return;
            if(key=="groan") { if(Time.unscaledTime-lastGroan<2.5f) return; lastGroan=Time.unscaledTime; }
            var source=voices[voice++%voices.Length]; source.Stop(); source.clip=clip;
            source.pitch=key.StartsWith("shot_")?UnityEngine.Random.Range(.97f,1.03f):1;
            source.volume=(behind?.25f:.48f)*(key=="groan"?.55f:1); source.panStereo=behind?-.22f:0; source.Play();
        }
        static float Pulse(float t,float start,float decay) => t<start?0:Mathf.Exp(-(t-start)*decay);
        static float Sin(float frequency,float t) => Mathf.Sin(2*Mathf.PI*frequency*t);
        public static float[] CreateSamples(string key)
        {
            const int rate=44100; bool reload=key.StartsWith("reload_"),shot=key.StartsWith("shot_");
            WeaponKind kind=WeaponKind.Pistol;
            if(shot || reload) Enum.TryParse(key.Substring(reload?7:5),out kind);
            float duration=reload?.9f:key=="groan"?1.1f:key=="wave"?.75f:key.StartsWith("ability_")?.75f:.45f;
            var data=new float[(int)(rate*duration)]; var random=new System.Random(773); float low=0,mid=0;
            for(int i=0;i<data.Length;i++)
            {
                float t=(float)i/rate,n=(float)random.NextDouble()*2-1;
                low=low*.965f+n*.035f; mid=mid*.68f+n*.32f; float high=n-mid, value=0;
                if(reload)
                {
                    bool organic=kind==WeaponKind.Bow || kind==WeaponKind.Boomerang || kind==WeaponKind.Slingshot || kind==WeaponKind.Axe;
                    float pulse=Pulse(t,.02f,85)+Pulse(t,.24f,65)*.8f+Pulse(t,.55f,80)*.9f;
                    value=(organic?mid*.6f+Sin(170,t)*.13f:high*.45f+Sin(1750,t)*.18f+Sin(830,t)*.15f)*pulse;
                    value+=mid*.14f*Mathf.Sin(Mathf.PI*t/duration);
                }
                else if(shot)
                {
                    switch(kind)
                    {
                        case WeaponKind.Bow:
                        case WeaponKind.Crossbow:
                            value=(Sin(205,t)+.4f*Sin(410,t)+.2f*Sin(620,t))*.32f*Mathf.Exp(-t*24)+high*.22f*Mathf.Exp(-t*38);
                            if(kind==WeaponKind.Crossbow) value+=mid*.4f*Mathf.Exp(-t*55); break;
                        case WeaponKind.Slingshot:
                            value=Sin(310-360*t,t)*.55f*Mathf.Exp(-t*31)+high*.12f*Mathf.Exp(-t*60); break;
                        case WeaponKind.Nailgun:
                            value=mid*.75f*Mathf.Exp(-t*65)+Sin(170,t)*.3f*Mathf.Exp(-t*42)+high*.2f*Pulse(t,.035f,80); break;
                        case WeaponKind.Boomerang:
                        case WeaponKind.Shuriken:
                        case WeaponKind.Axe:
                            float sweep=Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.24f)); float pitch=kind==WeaponKind.Shuriken?1450:kind==WeaponKind.Axe?180:520;
                            value=(mid*.85f+Sin(pitch+1000*t,t)*.12f)*sweep*Mathf.Exp(-t*7);
                            if(kind==WeaponKind.Shuriken) value+=Sin(2300,t)*.12f*Mathf.Exp(-t*30); break;
                        default:
                            float f=kind==WeaponKind.Shotgun?61:kind==WeaponKind.Revolver?92:kind==WeaponKind.Rifle?115:kind==WeaponKind.Launcher?46:145;
                            float decay=kind==WeaponKind.Shotgun?16:kind==WeaponKind.Launcher?11:28;
                            value=high*.64f*Mathf.Exp(-t*85)+mid*.63f*Mathf.Exp(-t*decay)+Sin(f-50*t,t)*.55f*Mathf.Exp(-t*decay);
                            value+=high*.12f*Pulse(t,.065f,70); break;
                    }
                }
                else if(key=="explosion" || key=="ability_Grenade" || key=="ability_Stomp") value=low*3.4f*Mathf.Exp(-t*5)+mid*.6f*Mathf.Exp(-t*17)+Sin(42,t)*.4f*Mathf.Exp(-t*11);
                else if(key=="groan") value=(Sin(71+5*Mathf.Sin(t*7),t)*.3f+Sin(139,t)*.1f+low*.5f)*Mathf.Sin(Mathf.PI*t/duration)*(.7f+.3f*Mathf.Sin(t*27));
                else if(key=="spit") value=(mid+Sin(140-70*t,t)*.25f)*Mathf.Exp(-t*13)*(.5f+.5f*Mathf.Sin(t*100));
                else if(key=="hurt") value=(low*2+Sin(78,t)*.4f)*Mathf.Exp(-t*16)+mid*.2f*Pulse(t,.12f,30);
                else if(key=="hit" || key=="wood") value=(mid*.75f+Sin(key=="wood"?160:110,t)*.3f)*Mathf.Exp(-t*42);
                else if(key=="head") value=mid*.6f*Mathf.Exp(-t*65)+Sin(1100,t)*.14f*Mathf.Exp(-t*38);
                else if(key=="metal" || key=="loaded" || key=="empty") value=(high*.35f+Sin(1870,t)*.3f+Sin(2903,t)*.12f)*Mathf.Exp(-t*(key=="metal"?23:65));
                else if(key=="tower") value=(high*.4f+Sin(130,t)*.3f)*Mathf.Exp(-t*50);
                else if(key=="coin" || key=="buy") value=Sin(1320,t)*.3f*Mathf.Exp(-t*19)+Sin(1980,t)*.22f*Pulse(t,.06f,22);
                else if(key=="wave") value=(Sin(t<.2f?330:t<.4f?440:660,t)*.2f+Sin(220,t)*.08f)*Mathf.Sin(Mathf.PI*t/duration);
                else if(key=="turn" || key=="ability_Smoke") value=mid*.65f*Mathf.Sin(Mathf.PI*t/duration)*Mathf.Exp(-t*8);
                else if(key=="ability_Roots") value=low*2*Mathf.Exp(-t*6)+mid*.3f*(Pulse(t,.05f,40)+Pulse(t,.2f,30)+Pulse(t,.35f,30));
                else if(key=="ability_Turret") value=(mid*.5f+Sin(650,t)*.2f)*(Pulse(t,.02f,55)+Pulse(t,.2f,50)+Pulse(t,.4f,50));
                else if(key=="ability_Decoy") value=Sin(620+180*Mathf.Sin(t*18),t)*.22f*Mathf.Exp(-t*4);
                else if(key=="ability_Cleave") value=mid*.85f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.4f))*Mathf.Exp(-t*4);
                else if(key=="ability_Deadeye") value=(Sin(330,t)+Sin(495,t))*.17f*Mathf.Sin(Mathf.PI*t/duration);
                float fade=Mathf.Min(1,i/70f)*Mathf.Min(1,(data.Length-1-i)/200f);
                data[i]=Mathf.Clamp(value*.75f*fade,-.92f,.92f);
            }
            return data;
        }
    }
}
