using UnityEngine;
using static ZHold.Draw2D;

namespace ZHold
{
    public static class CombatSprites
    {
        static CombatSpriteCatalog catalog;
        public static CombatSpriteCatalog Catalog => catalog!=null?catalog:(catalog=Resources.Load<CombatSpriteCatalog>("CombatSpriteCatalog"));
        static CombatSpriteCatalog.Sheet Get(CombatSpriteCatalog.Sheet[] sheets,int index) => sheets!=null&&index>=0&&index<sheets.Length?sheets[index]:null;
        public static CombatSpriteCatalog.Sheet WeaponSheet(WeaponKind kind) => Catalog==null?null:Get(Catalog.weapons,(int)kind);
        public static CombatSpriteCatalog.Sheet AttackSheet(ZombieKind kind) => Catalog==null?null:Get(Catalog.attacks,(int)kind);
        public static CombatSpriteCatalog.Sheet DefenseSheet(TowerKind kind) => Catalog==null?null:Get(Catalog.defenses,(int)kind);
        public static Rect AnchoredRect(CombatSpriteCatalog.Sheet sheet,int frame,Vector2 feet,float height)
        {
            float cellWidth=sheet.texture.width/(float)sheet.columns,cellHeight=sheet.texture.height/(float)sheet.rows;
            Rect b=sheet.bounds[frame]; float scale=height/Mathf.Max(1,sheet.referenceHeight);
            return new Rect(feet.x-b.center.x*scale,feet.y-b.yMax*scale,cellWidth*scale,cellHeight*scale);
        }
        public static bool Attack(Zombie zombie)
        {
            var sheet=AttackSheet(zombie.data.kind); if(sheet==null||sheet.texture==null||!zombie.attacking) return false;
            int frame=zombie.AttackFrame;
            GUI.DrawTextureWithTexCoords(AnchoredRect(sheet,frame,zombie.Position,215*zombie.Size),sheet.texture,sheet.UV(frame)); return true;
        }
        public static int WeaponFrame(RunManager run)
        {
            if(run.BowDrawing) return Mathf.Clamp(1+Mathf.FloorToInt(run.BowDrawProgress*3),1,3);
            if(run.weaponAction>0)
            {
                if(run.weapon.kind==WeaponKind.Bow||run.weapon.kind==WeaponKind.Slingshot) return run.weaponAction>.1f?4:5;
                if(run.weapon.kind==WeaponKind.Boomerang||run.weapon.kind==WeaponKind.Shuriken||run.weapon.kind==WeaponKind.Axe)
                    return Mathf.Clamp(1+Mathf.FloorToInt((1-run.weaponAction/(run.weapon.kind==WeaponKind.Boomerang?1.25f:run.weapon.kind==WeaponKind.Axe?.7f:.28f))*5),1,5);
                return run.weaponAction>.18f?1:run.weaponAction>.08f?2:3;
            }
            if(run.Reloading) return run.ReloadProgress<.55f?4:5;
            return 0;
        }
        public static bool Weapon(Vector2 aim,RunManager run,float time)
        {
            if(run.weapon.kind==WeaponKind.Boomerang&&run.thrown.Exists(a=>a.kind==WeaponKind.Boomerang&&!a.grenade))return true;
            if(ExpansionArt.Weapon(aim,run,time))return true;
            var sheet=WeaponSheet(run.weapon.kind); if(sheet==null||sheet.texture==null) return false;
            int frame=WeaponFrame(run);
            float width=run.weapon.kind==WeaponKind.Bow?440:run.weapon.kind==WeaponKind.Axe?370:340;
            float x=810+(aim.x-800)*.04f+Mathf.Sin(time*1.4f)*2;
            float y=926+(run.Reloading?35*Mathf.Sin(run.ReloadProgress*Mathf.PI):0);
            float height=width*(sheet.texture.height/(float)sheet.rows)/(sheet.texture.width/(float)sheet.columns);
            GUI.color=Color.white; GUI.DrawTextureWithTexCoords(new Rect(x-width/2,y-height,width,height),sheet.texture,sheet.UV(frame)); return true;
        }
        public static Vector2 DefensePosition(int slot,bool barrier) => DepthProjector.Project((slot-1)*.66f,barrier?.25f:.11f);
        public static Vector2 Muzzle(Tower tower) => DefensePosition(tower.slot,false)+new Vector2(0,-130*DepthProjector.Scale(.11f));
        public static bool Defense(int slot,Tower tower,bool barrier,float time=0)
        {
            if(tower==null) return true;
            var sheet=DefenseSheet(tower.data.kind); if(sheet==null||sheet.texture==null) return false;
            int row=barrier?(tower.health<tower.maxHealth*.5f?1:0):(tower.shotRecoil>0||tower.data.kind==TowerKind.Ice&&Mathf.Sin(time*4)>0?1:0);
            int frame=row*3+slot; var pos=DefensePosition(slot,barrier);
            float height=barrier?115:155;
            GUI.color=Color.white;
            var rect=AnchoredRect(sheet,frame,pos,height*DepthProjector.Scale(barrier?.25f:.11f));
            if(barrier) { rect.x=pos.x+(rect.x-pos.x)*1.8f;rect.width*=1.8f; }
            GUI.DrawTextureWithTexCoords(rect,sheet.texture,sheet.UV(frame));
            Rect(pos.x-40,pos.y+7,80,5,Ink); Rect(pos.x-40,pos.y+7,80*Mathf.Clamp01(tower.health/tower.maxHealth),5,barrier?Gold:Mint);
            return true;
        }
        public static void TemporaryTower(Vector2 pos,float time)
        {
            var sheet=DefenseSheet(TowerKind.Shooter); if(sheet==null||sheet.texture==null) return;
            int frame=Mathf.Repeat(time,.45f)<.18f?5:2;
            GUI.color=Color.white; GUI.DrawTextureWithTexCoords(AnchoredRect(sheet,frame,pos,140),sheet.texture,sheet.UV(frame));
        }
    }
}
