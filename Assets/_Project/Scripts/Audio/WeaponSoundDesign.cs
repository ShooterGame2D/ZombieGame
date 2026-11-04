using UnityEngine;

namespace ZombieAttack
{
    // Each weapon has a distinct transient, resonant body and mechanical tail.
    public static class WeaponSoundDesign
    {
        static float S(float f,float t)=>Mathf.Sin(2*Mathf.PI*f*t);
        static float E(float t,float decay)=>Mathf.Exp(-t*decay);
        static float P(float t,float start,float decay)=>t<start?0:E(t-start,decay);
        public static float Shot(WeaponKind kind,float t,float low,float mid,float high)
        {
            switch(kind)
            {
                case WeaponKind.Pistol: return high*.66f*E(t,100)+mid*.5f*E(t,34)+S(160-65*t,t)*.44f*E(t,29)+high*.15f*P(t,.075f,80);
                case WeaponKind.Revolver: return high*.54f*E(t,75)+mid*.7f*E(t,19)+S(89-30*t,t)*.53f*E(t,17)+S(1650,t)*.12f*E(t,45);
                case WeaponKind.Smg: return high*.77f*E(t,135)+mid*.38f*E(t,62)+S(225,t)*.29f*E(t,50)+high*.22f*P(t,.038f,100);
                case WeaponKind.Rifle: return high*.81f*E(t,115)+mid*.68f*E(t,26)+S(116-45*t,t)*.48f*E(t,24)+(high*.19f+S(1200,t)*.1f)*P(t,.12f,65);
                case WeaponKind.Shotgun: return high*.7f*E(t,65)+mid*.82f*E(t,13)+low*1.1f*E(t,10)+S(62-18*t,t)*.58f*E(t,12)+mid*.23f*P(t,.26f,36)+high*.15f*P(t,.34f,60);
                case WeaponKind.Launcher: return low*2.6f*E(t,9)+S(48-20*t,t)*.65f*E(t,10)+mid*.36f*E(t,20)+high*.2f*P(t,.1f,40);
                case WeaponKind.Nailgun: return high*.52f*E(t,110)+mid*.82f*E(t,75)+S(185,t)*.28f*E(t,45)+mid*.28f*P(t,.045f,55)+S(890,t)*.08f*P(t,.035f,35);
                case WeaponKind.Bow: return (S(172,t)+.47f*S(346,t)+.18f*S(519,t))*.3f*E(t,25)+high*.08f*E(t,80)+mid*.23f*P(t,.025f,18);
                case WeaponKind.Crossbow: return (S(275,t)*.36f+S(551,t)*.17f)*E(t,34)+high*.38f*E(t,90)+mid*.42f*E(t,43)+S(1470,t)*.12f*P(t,.035f,45);
                case WeaponKind.Slingshot: return S(380-550*t,t)*.56f*E(t,23)+mid*.18f*E(t,36)+high*.13f*P(t,.018f,75);
                case WeaponKind.Boomerang: return (mid*.76f+S(420+900*t,t)*.14f)*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.34f))*E(t,4)*(.8f+.2f*S(24,t));
                case WeaponKind.Axe: return (low*2.1f+mid*.8f+S(135+220*t,t)*.12f)*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.37f))*E(t,3.5f);
                case WeaponKind.Shuriken: return (high*.47f+mid*.3f)*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.16f))*E(t,9)+S(2740,t)*.17f*E(t,28)+S(3470,t)*.08f*E(t,36);
                default: return 0;
            }
        }
        public static float Reload(WeaponKind kind,float t,float mid,float high)
        {
            int id=(int)kind;
            switch(kind)
            {
                case WeaponKind.Bow: return mid*.21f*(P(t,.04f,14)+P(t,.42f,24))+S(210,t)*.11f*P(t,.61f,40);
                case WeaponKind.Slingshot: return mid*.17f*P(t,.08f,18)+S(340-170*t,t)*.2f*P(t,.44f,30);
                case WeaponKind.Boomerang: return mid*.36f*P(t,.1f,13)+S(130,t)*.12f*P(t,.55f,40);
                case WeaponKind.Axe: return mid*.48f*P(t,.12f,11)+S(85,t)*.2f*P(t,.61f,35);
                case WeaponKind.Shuriken: return (high*.22f+S(2700,t)*.14f)*(P(t,.05f,45)+P(t,.32f,50)+P(t,.62f,55));
                case WeaponKind.Revolver: return (high*.26f+S(1900,t)*.14f)*(P(t,.04f,70)+P(t,.22f,80)+P(t,.32f,80)+P(t,.42f,80)+P(t,.67f,55));
                case WeaponKind.Shotgun: return mid*.42f*(P(t,.08f,24)+P(t,.38f,25))+high*.25f*P(t,.66f,55);
                case WeaponKind.Crossbow: return mid*.29f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(t/.6f))+(high*.3f+S(850,t)*.13f)*P(t,.65f,60);
                case WeaponKind.Nailgun: return (S(260+120*t,t)*.14f+mid*.3f)*P(t,.1f,6)+high*.35f*P(t,.64f,90);
                default: return (high*(.27f+id*.008f)+S(1050+id*95,t)*.15f+mid*.25f)*(P(t,.03f,70)+P(t,.25f+id*.008f,55)+P(t,.65f,75));
            }
        }
    }
}
