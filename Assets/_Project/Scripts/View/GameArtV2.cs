using UnityEngine;
using static ZHold.Draw2D;
namespace ZHold
{
    public static class GameArt
    {
        static Texture2D zombies,portraits,environments,rearEnvironments;
        static void Load()
        {
            if(zombies!=null) return;
            zombies=Resources.Load<Texture2D>("Art/Pixel/Zombies"); portraits=Resources.Load<Texture2D>("Art/Pixel/Heroes");
            environments=Resources.Load<Texture2D>("Art/Maps-v5/Front") ?? Resources.Load<Texture2D>("Art/Pixel/Front");
            rearEnvironments=Resources.Load<Texture2D>("Art/Maps-v5/Back") ?? Resources.Load<Texture2D>("Art/Pixel/Back");
        }
        public static void Background(MapTheme theme,ViewSide side,float time)
        {
            Load(); int index=(int)theme; bool rear=side==ViewSide.Back;
            var atlas=rear && rearEnvironments!=null?rearEnvironments:environments;
            var uv=BackgroundUV(index);
            if(rear && rearEnvironments==null) { uv.x+=uv.width; uv.width=-uv.width; }
            GUI.color=Color.white; GUI.DrawTextureWithTexCoords(new Rect(0,0,1600,900),atlas,uv);
            Rect(0,0,1600,120,new Color(.02f,.05f,.07f,.25f)); Rect(0,790,1600,110,new Color(.02f,.05f,.07f,.35f));
            for(int i=0;i<18;i++)
            {
                float x=(i*117+time*(3+i%3))%1600, y=210+(i*71%430)+Mathf.Sin(time*.6f+i)*10;
                Ellipse(x,y,2+i%3,2+i%3,new Color(1,.88f,.58f,.18f));
            }
            // Low foreground supply crates mark the player's defensive position.
            for(int k=0;k<2;k++)
            {
                float x=k==0?-25:1510;
                Round(x,745,115,95,6,Hex("283935")); Rect(x+9,756,96,10,Hex("667359")); Rect(x+9,804,96,9,Hex("667359"));
                Line(new Vector2(x+17,758),new Vector2(x+93,811),6,Hex("59644D"));
            }
        }
        // Frame the new open clearing so the fixed spawn row (y=400) is on ground.
        // Only the painting is framed; DepthProjector and every spawn position stay unchanged.
        public static Rect BackgroundUV(int index) => new Rect(index%2*.5f+.001f,1-(index/2+1)*.25f+.001f,.498f,.25f*.86f-.002f);
        public static void ZombieArt(Zombie z,float time,bool bar=true)
        {
            Load(); Vector2 p=z.Position; float s=z.Size, height=215*s, width=z.ArtWidth;
            Ellipse(p.x-width*.4f,p.y-8*s,width*.8f,18*s,new Color(.02f,.03f,.02f,.43f));
            GUI.color=z.hitFlash>0?new Color(1,1-z.hitFlash*.28f,1-z.hitFlash*.38f):Color.white;
            if(z.data.kind==ZombieKind.Shade) GUI.color=new Color(.8f,.86f,1,.72f+.28f*Mathf.Sin(time*2)*Mathf.Sin(time*2));
            var walk=ZombieWalkAnimation.Get(z.data.kind);
            if(ExpansionArt.ZombieState(z)) { }
            else if(CombatSprites.Attack(z)) { }
            else if(walk!=null)
                GUI.DrawTextureWithTexCoords(ZombieWalkAnimation.DrawRect(z,walk),walk.texture,ZombieWalkAnimation.UV(z.phase));
            else
                GUI.DrawTextureWithTexCoords(new Rect(p.x-width/2,p.y-height,width,height),zombies,z.data.artUV);
            GUI.color=Color.white;
            if(bar && z.health<z.maxHealth)
            {
                Round(p.x-40*s,p.y-233*s,80*s,7*s,3*s,Ink); Round(p.x-39*s,p.y-232*s,78*s*Mathf.Clamp01(z.health/z.maxHealth),5*s,2*s,z.data.kind==ZombieKind.Armored?Gold:Mint);
            }
            if(z.data.kind==ZombieKind.Medic && z.specialTimer<.8f)
            { Rect(p.x-3*s,p.y-255*s,6*s,22*s,Mint); Rect(p.x-11*s,p.y-247*s,22*s,6*s,Mint); }
        }
        public static void Portrait(int index,float x,float y,float width,float height)
        {
            Load(); GUI.color=Color.white;
            var uv=PixelArtCatalog.HeroUV(index); float aspect=uv.width*2/uv.height;
            float dw=Mathf.Min(width,height*aspect),dh=dw/aspect;
            GUI.DrawTextureWithTexCoords(new Rect(Snap(x+(width-dw)/2),Snap(y+height-dh),Snap(dw),Snap(dh)),portraits,uv);
        }
        public static void CoinArt(Coin coin,float time) => LegacyArt.CoinArt(coin,time);
        public static void TowerArt(int slot,Tower tower,bool highlight=false)
        { if(!CombatSprites.Defense(slot,tower,false)) LegacyArt.TowerArt(slot,tower,highlight); }
        public static void BarricadeArt(int slot,Tower tower)
        {
            if(tower==null) return;
            if(CombatSprites.Defense(slot,tower,true)) return;
            float x=280+slot*520; var old=GUI.matrix;
            GUI.matrix=old*Matrix4x4.TRS(new Vector3(x*.08f,-64,0),Quaternion.identity,new Vector3(.92f,.92f,1));
            LegacyArt.TowerArt(slot,tower); GUI.matrix=old;
        }
        public static void Weapon(Vector2 aim,RunManager run,float time)
        {
            if(CombatSprites.Weapon(aim,run,time)) return;
            float x=820+(aim.x-800)*.045f,y=916+run.recoil*17+(run.Reloading?60*Mathf.Sin(run.ReloadProgress*Mathf.PI):0);
            var old=GUI.matrix; GUI.matrix=old*Matrix4x4.Translate(new Vector3(x,y,0));
            Color metal=Hex("6E8285"),dark=Hex("15262D"),wood=Hex("95603C"),skin=run.character.theme==MapTheme.Amazon?Hex("B47B51"):Hex("D5A477");
            if(run.character.theme==MapTheme.Outback) skin=Hex("B58A54");
            Quad(new Vector2(-82,12),new Vector2(-57,-56),new Vector2(-6,-40),new Vector2(-13,12),skin);
            Quad(new Vector2(86,12),new Vector2(51,-59),new Vector2(8,-35),new Vector2(17,12),skin);
            WeaponKind kind=run.weapon.kind;
            if(kind==WeaponKind.Bow || kind==WeaponKind.Crossbow)
            {
                Vector2 last=new Vector2(-96,-240);
                for(int i=1;i<=20;i++)
                { float t=i/20f; Vector2 next=new Vector2(-96+Mathf.Sin(t*Mathf.PI)*65,-240+t*215); Line(last,next,12,dark); Line(last,next,7,wood); last=next; }
                Line(new Vector2(-96,-240),new Vector2(-96,-25),2,Cream);
                Line(new Vector2(-115,-118),new Vector2(34,-231),5,wood); Triangle(new Vector2(23,-238),new Vector2(42,-246),new Vector2(36,-224),metal);
                if(kind==WeaponKind.Crossbow) { Round(-54,-154,90,42,8,dark); Line(new Vector2(-90,-178),new Vector2(90,-178),12,metal); Rect(-11,-158,20,126,wood); }
            }
            else if(kind==WeaponKind.Slingshot)
            {
                Line(new Vector2(0,-40),new Vector2(0,-146),19,dark); Line(new Vector2(0,-40),new Vector2(0,-146),12,wood);
                Line(new Vector2(0,-140),new Vector2(-48,-222),17,dark); Line(new Vector2(0,-140),new Vector2(48,-222),17,dark);
                Line(new Vector2(0,-140),new Vector2(-48,-222),10,wood); Line(new Vector2(0,-140),new Vector2(48,-222),10,wood);
                Line(new Vector2(-48,-218),new Vector2(2,-181),5,Gold); Line(new Vector2(48,-218),new Vector2(2,-181),5,Gold); Ellipse(-9,-190,18,18,metal);
            }
            else if(kind==WeaponKind.Boomerang)
            {
                Line(new Vector2(-94,-192),new Vector2(-10,-105),33,dark); Line(new Vector2(-10,-105),new Vector2(87,-215),33,dark);
                Line(new Vector2(-94,-192),new Vector2(-10,-105),24,wood); Line(new Vector2(-10,-105),new Vector2(87,-215),24,wood);
                for(int i=0;i<3;i++) Line(new Vector2(36+i*13,-155-i*15),new Vector2(50+i*13,-146-i*15),6,Gold);
            }
            else if(kind==WeaponKind.Axe)
            {
                Line(new Vector2(3,-18),new Vector2(65,-246),22,dark); Line(new Vector2(3,-18),new Vector2(65,-246),13,wood);
                Quad(new Vector2(25,-236),new Vector2(118,-269),new Vector2(142,-192),new Vector2(37,-193),dark);
                Quad(new Vector2(35,-229),new Vector2(111,-259),new Vector2(132,-201),new Vector2(42,-201),metal);
                Line(new Vector2(111,-259),new Vector2(132,-201),7,Hex("D5DDCF"));
            }
            else if(kind==WeaponKind.Shuriken)
            {
                for(int i=0;i<4;i++) { float a=time*.4f+i*Mathf.PI/2; Vector2 p=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),q=new Vector2(-p.y,p.x); Triangle(new Vector2(0,-150)+q*21,new Vector2(0,-150)+p*81,new Vector2(0,-150)-q*21,metal); }
                Ellipse(-18,-168,36,36,dark); Ellipse(-7,-157,14,14,Cream);
            }
            else if(kind==WeaponKind.Nailgun)
            {
                Round(-27,-127,50,99,10,dark); Round(-67,-207,133,96,12,dark); Round(-60,-199,119,75,10,Gold);
                Rect(-42,-224,85,27,metal); Rect(-20,-237,43,20,dark); Rect(-34,-143,70,46,metal);
                for(int i=0;i<5;i++) Rect(-25+i*12,-141,5,35,Ink);
                Text("N",-25,-189,50,45,29,Ink,true,TextAnchor.MiddleCenter);
            }
            else
            {
                bool longGun=kind==WeaponKind.Rifle || kind==WeaponKind.Smg || kind==WeaponKind.Shotgun || kind==WeaponKind.Launcher;
                float barrel=longGun?235:180, width=kind==WeaponKind.Launcher?87:kind==WeaponKind.Shotgun?73:58;
                Round(-28,-111,60,84,9,dark); Round(-width/2-5,-barrel,width+10,barrel-75,9,dark); Rect(-width/2+3,-barrel+8,width-6,barrel-96,metal);
                Rect(-width/2+7,-barrel+11,width-14,12,run.character.accent); Rect(-8,-barrel-9,16,13,dark); Rect(-3,-barrel-11,6,7,Mint);
                for(int i=0;i<4;i++) Rect(-width/2+5,-128+i*9,width-10,3,dark);
                if(kind==WeaponKind.Revolver) { Round(-42,-159,84,43,10,dark); for(int i=0;i<5;i++) Rect(-31+i*14,-151,8,27,metal); Rect(-17,-96,35,52,wood); }
                if(kind==WeaponKind.Rifle) { Round(-15,-207,30,58,8,dark); Ellipse(-9,-203,18,18,Hex("91C9C7")); }
                if(kind==WeaponKind.Launcher) { Ellipse(-44,-barrel-12,88,38,dark); Ellipse(-31,-barrel-6,62,22,Hex("3B4D47")); }
                if(run.recoil>.6f) { Triangle(new Vector2(-30,-barrel-15),new Vector2(0,-barrel-90),new Vector2(30,-barrel-15),Gold); Triangle(new Vector2(-14,-barrel-15),new Vector2(0,-barrel-65),new Vector2(14,-barrel-15),Cream); }
            }
            Round(-42,-79,82,48,17,skin); Rect(-17,-55,38,22,Color.Lerp(skin,dark,.2f));
            GUI.matrix=old;
        }
    }
}
