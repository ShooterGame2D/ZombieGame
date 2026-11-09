using System.Collections.Generic;
using UnityEngine;
using static ZHold.Draw2D;
namespace ZHold
{
    public static class ExpansionArt
    {
        static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        public static Texture2D Texture(string id)
        {
            if(!textures.TryGetValue(id,out var texture)) textures[id]=texture=Resources.Load<Texture2D>("Art/Expansion/"+id);
            return texture;
        }
        public static void Sprite(string id,int frame,UnityEngine.Rect rect)
        {
            var texture=Texture(id);if(texture==null)return;
            frame=Mathf.Clamp(frame,0,5);
            GUI.DrawTextureWithTexCoords(rect,texture,new UnityEngine.Rect(frame%3/3f,1-(frame/3+1)/2f,1f/3,.5f));
        }
        public static bool ZombieState(Zombie z)
        {
            if(z.emergeTimer<=0&&!z.distracted) return false;
            string id=z.emergeTimer>0?"Emerge":"Distracted";
            if(Texture(id)==null)return false;
            int frame=z.emergeTimer>0?Mathf.Clamp((int)((1-z.emergeTimer/1.1f)*6),0,5):(int)(z.distractPhase*5)%6;
            float size=255*z.Size;
            Sprite(id,frame,new UnityEngine.Rect(z.Position.x-size/2,z.Position.y-size*(z.emergeTimer>0?(frame<3?434f:466f)/512:476f/512),size,size));
            if(z.distracted) Text("?",z.Position.x-18,z.Position.y-size-25,36,35,30,Gold,true,TextAnchor.MiddleCenter);
            return true;
        }
        public static bool Weapon(Vector2 aim,RunManager run,float time)
        {
            var kind=run.weapon.kind;
            if(kind!=WeaponKind.Bow&&kind!=WeaponKind.Slingshot&&kind!=WeaponKind.Axe) return false;
            string id=kind.ToString();if(Texture(id)==null)return false;
            if(kind==WeaponKind.Axe && run.thrown.Exists(a=>!a.grenade&&a.kind==kind&&a.age>=0)) return true;
            int frame=CombatSprites.WeaponFrame(run);
            if(kind==WeaponKind.Axe)frame=run.weaponAction>0?Mathf.Min(2,frame):0;
            float width=kind==WeaponKind.Bow?300:280,x=kind==WeaponKind.Bow?730:890;
            x+=(aim.x-800)*.025f;float y=915+(run.Reloading?35*Mathf.Sin(run.ReloadProgress*Mathf.PI):0);
            GUI.color=Color.white;Sprite(id,frame,new UnityEngine.Rect(x-width/2,y-width,width,width));return true;
        }
        public static void Effects(RunManager run,float time)
        {
            GUI.color=Color.white;
            if(run.decoyTimers[RunManager.SideIndex(run.viewSide)]>0)
            {
                Sprite("Teddy",(int)(time*6)%6,new UnityEngine.Rect(744,510,112,112));
                Text("ISCA ATIVA",714,625,172,26,18,Gold,true,TextAnchor.MiddleCenter);
            }
            if(run.treeTimer>0&&run.treeSide==run.viewSide)
            {
                var pos=DepthProjector.Project((run.treeLane-1)*.66f,.05f);
                int frame=Mathf.Clamp((int)((1-run.treeTimer/1.65f)*6),0,5);
                Sprite("Tree",frame,new UnityEngine.Rect(pos.x-245,pos.y-471,490,490));
            }
            foreach(var a in run.thrown)
            {
                if(a.side!=run.viewSide||a.age<0)continue;
                float t=Mathf.Clamp01(a.age/a.duration);
                if(a.grenade)
                {
                    var pos=Vector2.Lerp(a.origin,a.target,t)+Vector2.up*(-Mathf.Sin(t*Mathf.PI)*140);
                    bool boom=a.impacted;float size=boom?240:Mathf.Lerp(85,42,t);
                    Sprite("Grenade",boom?(a.age-a.duration<.24f?4:5):1+(int)(t*2),new UnityEngine.Rect(pos.x-size/2,pos.y-size/2,size,size));
                }
                else
                {
                    bool back=a.kind==WeaponKind.Boomerang&&a.impacted;
                    var pos=back?Vector2.Lerp(a.target,a.origin,Mathf.Clamp01((a.age-a.duration)/a.duration)):Vector2.Lerp(a.origin,a.target,t);
                    pos.x+=Mathf.Sin(t*Mathf.PI)*(back?-90:90);
                    float size=Mathf.Lerp(110,44,back?1-Mathf.Clamp01((a.age-a.duration)/a.duration):t);
                    if(a.kind==WeaponKind.Axe) Sprite("Axe",3+(int)(a.age*14)%3,new UnityEngine.Rect(pos.x-size/2,pos.y-size/2,size,size));
                    else
                    {
                        var old=GUI.matrix;GUIUtility.RotateAroundPivot(a.age*720,pos);
                        Line(pos+new Vector2(-size*.4f,-size*.2f),pos+new Vector2(0,size*.15f),9,Hex("B78046"));
                        Line(pos+new Vector2(0,size*.15f),pos+new Vector2(size*.4f,-size*.2f),9,Gold);GUI.matrix=old;
                    }
                }
            }
        }
    }
}
