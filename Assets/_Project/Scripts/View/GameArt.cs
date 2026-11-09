using UnityEngine;
using static ZHold.Draw2D;
namespace ZHold
{
    public static class LegacyArt
    {
        public static void Background(float time)
        {
            Sky();
            Ellipse(1120,118,166,166,new Color(1,.86f,.59f,.14f)); Ellipse(1138,136,130,130,Hex("F4D49A"));
            Cloud(160,118,1.1f); Cloud(650,90,.8f); Cloud(1300,238,.8f);
            // Quiet neighborhood silhouettes.
            for(int i=0;i<11;i++)
            {
                float x=i*164-60, y=290+(i%3)*23;
                Rect(x,y,144,150,Hex(i%2==0?"607F7C":"718984"));
                Triangle(new Vector2(x-15,y),new Vector2(x+72,y-62),new Vector2(x+160,y),Hex("526E70"));
                Rect(x+24,y+35,25,33,Hex("BDAD82")); Rect(x+86,y+35,25,33,Hex("BDAD82"));
            }
            Rect(0,385,1600,515,Hex("637B48"));
            // Lawn, central path and receding horizontal cuts.
            Quad(new Vector2(755,390),new Vector2(845,390),new Vector2(1190,900),new Vector2(410,900),Hex("B8A475"));
            Quad(new Vector2(775,390),new Vector2(825,390),new Vector2(1110,900),new Vector2(490,900),Hex("C8B487"));
            for(int i=0;i<6;i++)
            {
                float y=425+i*i*13;
                Line(new Vector2(0,y),new Vector2(730-(y-390)*.64f,y),3,new Color(.2f,.32f,.22f,.18f));
                Line(new Vector2(870+(y-390)*.64f,y),new Vector2(1600,y),3,new Color(.2f,.32f,.22f,.18f));
            }
            for(int i=0;i<28;i++)
            {
                float y=443+(i*67%350), x=i%2==0? 90+(i*71%420):1110+(i*43%420);
                Line(new Vector2(x,y),new Vector2(x-7,y-12),3,Hex("8B9F5E")); Line(new Vector2(x,y),new Vector2(x+5,y-16),3,Hex("819859"));
            }
            // Low hedges, garden walls, and the open gate.
            for(int i=0;i<14;i++) { float x=i*125-45; Ellipse(x,348+(i%3)*9,170,85,Hex("3D6352")); Ellipse(x+18,345+(i%3)*9,116,50,Hex("4D7658")); }
            Rect(676,345,248,85,Hex("71866A"));
            for(int i=0;i<4;i++)
            {
                float x=620+i*118;
                Rect(x,350,18,95,Hex("CED0AC")); Round(x-6,341,30,17,4,Hex("E2D8B1"));
            }
            Rect(648,366,82,9,Hex("B6BC99")); Rect(648,404,82,9,Hex("B6BC99"));
            Rect(870,366,104,9,Hex("B6BC99")); Rect(870,404,104,9,Hex("B6BC99"));
            // Foreground framing.
            Fence(0,false); Fence(1600,true);
            Tree(-28,210,1.15f); Tree(1580,195,1.27f);
            Round(1222,479,139,57,8,Hex("253F3B")); Rect(1281,532,12,74,Hex("705D41"));
            Text("CUIDADO",1232,489,120,20,16,Gold,true,TextAnchor.MiddleCenter); Text("QUINTAL VIVO",1230,511,124,16,11,Cream,false,TextAnchor.MiddleCenter);
            // Foreground defense deck, visible beneath the gun.
            Quad(new Vector2(0,800),new Vector2(1600,800),new Vector2(1600,900),new Vector2(0,900),Hex("3D4E42"));
            Rect(0,800,1600,7,Hex("9A946A"));
            for(int i=0;i<12;i++) Line(new Vector2(i*145,807),new Vector2(i*145-65,900),2,Hex("2C413A"));
        }
        static void Cloud(float x,float y,float scale)
        {
            Color c=new Color(.88f,.87f,.71f,.22f); Ellipse(x,y,210*scale,25*scale,c); Ellipse(x+33*scale,y-17*scale,87*scale,45*scale,c);
        }
        static void Tree(float x,float y,float s)
        {
            Rect(x-12*s,y,34*s,390*s,Hex("3B5246"));
            Line(new Vector2(x,y+130*s),new Vector2(x-90*s,y+40*s),20*s,Hex("3B5246"));
            Ellipse(x-165*s,y-100*s,270*s,220*s,Hex("244F46")); Ellipse(x-95*s,y-140*s,240*s,210*s,Hex("2C5B4D")); Ellipse(x-140*s,y-110*s,190*s,150*s,Hex("366954"));
        }
        static void Fence(float x,bool flip)
        {
            float dir=flip?-1:1;
            for(int i=0;i<6;i++)
            {
                float px=x+dir*i*70, py=570-i*27;
                Quad(new Vector2(px,py),new Vector2(px+dir*49,py-16),new Vector2(px+dir*49,py+120),new Vector2(px,py+150),Hex(i%2==0?"9A9E75":"A9AB80"));
                Line(new Vector2(px+dir*10,py+4),new Vector2(px+dir*10,py+127),3,Hex("C3BE8A"));
            }
        }
        public static void ZombieArt(Zombie z,float time,bool bar=true)
        {
            Vector2 p=z.Position; float s=z.Size;
            // Keep artwork head and body exactly aligned with the hit boxes.
            float bob=Mathf.Sin(z.phase)*2*s, swing=Mathf.Sin(z.phase)*9;
            Ellipse(p.x-52*s,p.y-12*s,104*s,25*s,new Color(.07f,.17f,.12f,.27f));
            Matrix4x4 old=GUI.matrix; GUI.matrix=old*Matrix4x4.TRS(new Vector3(p.x,p.y,0),Quaternion.identity,new Vector3(s,s,1));
            Color skin=z.hitFlash>0?Color.Lerp(z.data.skin,Color.white,z.hitFlash):z.data.skin;
            Color shirt=z.data.shirt, edge=Hex("213E36"), pants=Hex("314B50");
            Line(new Vector2(-18,-65),new Vector2(-24+swing,-12),23,edge); Line(new Vector2(18,-65),new Vector2(24-swing,-12),23,edge);
            Line(new Vector2(-18,-64),new Vector2(-24+swing,-14),15,pants); Line(new Vector2(18,-64),new Vector2(24-swing,-14),15,pants);
            Round(-41+swing,-17,31,17,6,edge); Round(11-swing,-17,34,17,6,edge);
            Line(new Vector2(-30,-122),new Vector2(-49,-75+bob),20,edge); Line(new Vector2(-30,-122),new Vector2(-49,-77+bob),13,shirt);
            Line(new Vector2(30,-120),new Vector2(51,-83-bob),20,edge); Line(new Vector2(30,-120),new Vector2(51,-85-bob),13,shirt);
            Ellipse(-59,-89+bob,22,26,skin); Ellipse(39,-96-bob,22,26,skin);
            Round(-35,-135,70,78,11,edge); Round(-29,-131,58,68,8,shirt);
            Triangle(new Vector2(-19,-130),new Vector2(1,-106),new Vector2(20,-130),skin);
            Line(new Vector2(0,-107),new Vector2(0,-64),3,new Color(0,0,0,.2f));
            Rect(-22,-89,15,9,new Color(.8f,.8f,.58f,.45f));
            Ellipse(-36,-172,16,25,edge); Ellipse(20,-171,16,25,edge);
            Round(-32,-194,64,68,18,edge); Round(-27,-190,54,59,15,skin);
            Ellipse(-24,-183,38,19,new Color(1,1,1,.12f));
            // Lopsided eyes and single tooth keep the enemies playful.
            Ellipse(-22,-171,22,23,Cream); Ellipse(5,-168,19,20,Cream);
            Ellipse(-12,-164,7,10,edge); Ellipse(9,-161,6,9,edge);
            Line(new Vector2(-22,-175),new Vector2(-3,-171),5,edge);
            Round(-14,-143,33,10,4,edge); Rect(-6,-143,8,7,Cream);
            Line(new Vector2(-23,-184),new Vector2(-29,-202),6,edge); Line(new Vector2(-9,-189),new Vector2(-12,-207),6,edge);
            if(z.data.kind==ZombieKind.Runner) { Rect(-30,-181,60,8,Hex("E97857")); Rect(23,-180,19,6,Hex("E97857")); }
            if(z.data.kind==ZombieKind.Spitter) { Ellipse(-11,-148,26,22,Hex("C9E969")); Ellipse(-2,-142,10,9,Hex("527C44")); }
            if(z.data.kind==ZombieKind.Brute) { Round(-36,-200,72,29,9,Hex("D5A950")); Rect(-40,-177,80,9,Hex("E8BE61")); Rect(-26,-125,11,48,Hex("C7A552")); Rect(15,-125,11,48,Hex("C7A552")); }
            if(z.data.kind==ZombieKind.Boss)
            {
                Rect(-32,-207,64,23,Gold); Triangle(new Vector2(-32,-207),new Vector2(-32,-227),new Vector2(-10,-207),Gold); Triangle(new Vector2(-12,-207),new Vector2(0,-233),new Vector2(14,-207),Gold); Triangle(new Vector2(12,-207),new Vector2(32,-229),new Vector2(32,-207),Gold);
            }
            GUI.matrix=old;
            if(bar && z.health<z.maxHealth)
            {
                Round(p.x-32*s,p.y-216*s,64*s,7*s,3*s,edge);
                Round(p.x-31*s,p.y-215*s,62*s*Mathf.Max(0,z.health/z.maxHealth),5*s,2*s,Mint);
            }
        }
        public static void CoinArt(Coin coin,float time)
        {
            Vector2 p=coin.ScreenPosition;
            if(coin.life<1.2f && Mathf.Sin(time*22)<-.2f) return;
            Ellipse(p.x-27,p.y-27,54,54,new Color(1,.8f,.2f,.12f));
            Ellipse(p.x-20,p.y-17,40,40,Hex("95632E")); Ellipse(p.x-20,p.y-21,40,40,Gold); Ellipse(p.x-15,p.y-16,30,30,Hex("E7AC3E"));
            Text("$",p.x-16,p.y-20,32,34,25,Cream,true,TextAnchor.MiddleCenter);
            if(coin.value>1) Text("+"+coin.value,p.x+17,p.y-20,60,30,17,Gold,true);
        }
        public static void TowerArt(int slot,Tower tower,bool highlight=false)
        {
            float x=280+slot*520,y=747;
            Ellipse(x-70,y-12,140,40,new Color(.1f,.2f,.16f,.35f));
            Round(x-52,y-12,104,34,8,Hex("455C50")); Rect(x-43,y-12,86,5,highlight?Gold:Hex("84917A"));
            if(tower==null) { Text("0"+(slot+1),x-40,y-7,80,24,16,Hex("B7BD98"),true,TextAnchor.MiddleCenter); return; }
            Color accent=tower.data.kind==TowerKind.Ice?Hex("8DDEE7"):tower.data.kind==TowerKind.Explosive?Hex("F3925B"):Gold;
            if(tower.data.kind==TowerKind.Barricade)
            {
                for(int i=0;i<3;i++) Round(x-60+i*41,y-77,33,84,5,Hex("AB9466"));
                Rect(x-66,y-57,132,15,Hex("695D41")); Rect(x-66,y-23,132,15,Hex("695D41"));
            }
            else
            {
                Round(x-30,y-54,60,54,8,Hex("243F3D")); Round(x-21,y-55,42,38,6,accent);
                Round(x-39,y-86,78,35,10,Hex("213B3B")); Round(x-33,y-83,66,23,7,Hex("68827B"));
                Rect(x-10,y-113,20,53,Hex("213B3B")); Rect(x-6,y-113,12,14,accent);
                if(tower.data.kind==TowerKind.Ice) { Ellipse(x-16,y-105,32,32,accent); Line(new Vector2(x-10,y-89),new Vector2(x+10,y-89),3,Cream); }
            }
            Round(x-45,y+28,90,6,3,Ink); Round(x-45,y+28,90*Mathf.Clamp01(tower.health/tower.maxHealth),6,3,accent);
        }
        public static void Gun(Vector2 aim,float recoil,Color accent)
        {
            float x=820+(aim.x-800)*.045f,y=914+recoil*15;
            Matrix4x4 old=GUI.matrix; GUI.matrix=old*Matrix4x4.Translate(new Vector3(x,y,0));
            Quad(new Vector2(-110,15),new Vector2(-77,-70),new Vector2(-25,-43),new Vector2(-20,15),Hex("BA885E"));
            Quad(new Vector2(72,15),new Vector2(35,-80),new Vector2(-8,-50),new Vector2(8,15),Hex("D3A373"));
            Round(-45,-93,85,67,20,Hex("E0AE7D"));
            Quad(new Vector2(-28,-40),new Vector2(-27,-151),new Vector2(29,-151),new Vector2(38,-42),Hex("1B3438"));
            Round(-33,-170,66,90,10,Hex("243F43")); Rect(-25,-163,50,71,Hex("6A827C")); Rect(-18,-158,36,12,accent);
            Round(-16,-180,32,23,5,Ink); Rect(-4,-184,8,10,Mint);
            Rect(-29,-111,58,8,Ink); Line(new Vector2(-18,-57),new Vector2(-17,-84),5,Hex("738077"));
            if(recoil>.65f)
            {
                Triangle(new Vector2(-34,-189),new Vector2(0,-268),new Vector2(34,-189),Gold);
                Triangle(new Vector2(-17,-190),new Vector2(0,-238),new Vector2(17,-190),Cream);
            }
            GUI.matrix=old;
        }
        public static void Portrait(int index,float x,float y,float size,Color accent)
        {
            Round(x,y,size,size,16,Color.Lerp(Ink,accent,.18f));
            Ellipse(x+size*.21f,y+size*.6f,size*.58f,size*.5f,accent);
            Round(x+size*.3f,y+size*.18f,size*.4f,size*.5f,size*.14f,Hex(index==2?"BD8964":"E6B98C"));
            if(index==0) { Round(x+size*.23f,y+size*.12f,size*.54f,size*.22f,7,Hex("789876")); Rect(x+size*.24f,y+size*.28f,size*.6f,size*.07f,Hex("B1BD8F")); }
            if(index==1) { Ellipse(x+size*.23f,y+size*.08f,size*.53f,size*.25f,Hex("6C493D")); Rect(x+size*.22f,y+size*.2f,size*.1f,size*.4f,Hex("6C493D")); }
            if(index==2) { Round(x+size*.3f,y+size*.1f,size*.4f,size*.17f,6,Hex("473F38")); }
            if(index==3) { Round(x+size*.23f,y+size*.1f,size*.54f,size*.22f,7,Gold); Rect(x+size*.2f,y+size*.29f,size*.6f,size*.06f,Gold); }
            Ellipse(x+size*.39f,y+size*.38f,size*.045f,size*.055f,Ink); Ellipse(x+size*.57f,y+size*.38f,size*.045f,size*.055f,Ink);
            Line(new Vector2(x+size*.43f,y+size*.55f),new Vector2(x+size*.57f,y+size*.55f),2,Ink);
        }
    }
}
