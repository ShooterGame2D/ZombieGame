using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ZombieAttack.Editor
{
    public static class CombatAnimationValidation
    {
        static int checks;
        static void Check(bool ok,string label) { if(!ok) throw new Exception("COMBAT ANIMATION FAILED: "+label); checks++; }
        static RunManager Fresh(GameCatalog c,int hero=0)
        { var r=new RunManager(c,new PlayerProfile{selected=hero,unlocked=new[]{true,true,true,true,true,true,true,true}},418,persistProgress:false); r.Start(true); r.demo=false; r.spawnTimer=999; return r; }
        static Zombie Enemy(RunManager r,ViewSide side=ViewSide.Front,int kind=0,float depth=0)
        {
            var z=new Zombie{data=r.catalog.zombies[kind],side=side,distance=depth,health=100,maxHealth=100,speed=.03f,attackTimer=.32f}; r.zombies.Add(z);return z;
        }
        static void Step(RunManager run,float seconds) { for(int i=0;i<Mathf.CeilToInt(seconds/.02f);i++) run.Tick(.02f); }
        public static void Run(GameCatalog c)
        {
            checks=0; var art=CombatSprites.Catalog;
            Check(art!=null&&art.weapons.Length==13&&art.attacks.Length==10&&art.defenses.Length==4,"all requested equipment and zombie types have sheets");
            foreach(var sheet in art.weapons.Concat(art.attacks).Concat(art.defenses))
            {
                Check(sheet.texture!=null&&sheet.bounds.Length==6&&sheet.referenceHeight>0,"six frames "+sheet.id);
                Check(sheet.texture.filterMode==FilterMode.Point,"pixel filter "+sheet.id);
                var sprites=AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(sheet.texture)).OfType<Sprite>().ToArray();
                Check(sprites.Length==6,"six usable Unity sprite subassets "+sheet.id);
                for(int frame=0;frame<6;frame++)
                {
                    var pos=new Vector2(500,700); var rect=CombatSprites.AnchoredRect(sheet,frame,pos,215);
                    float scale=215/sheet.referenceHeight;
                    Check(Mathf.Abs(rect.y+sheet.bounds[frame].yMax*scale-pos.y)<.01f,"ground anchor "+sheet.id+frame);
                }
            }
            var r=Fresh(c); var z=Enemy(r); float hp=r.health;
            r.Tick(.02f); Check(z.attacking&&z.AttackFrame==0&&r.health==hp,"melee windup starts before damage");
            Step(r,.16f); Check(z.AttackFrame>0&&r.health==hp,"preparation visibly progresses");
            Step(r,.16f); Check(r.health<hp&&z.AttackFrame>=3&&z.attackRecovery>0,"damage occurs on strike frame");
            r.paused=true; var recovery=z.attackRecovery; Step(r,.2f); Check(z.attackRecovery==recovery,"pause freezes strike");
            r.paused=false; r.rootTimers[0]=1; Step(r,.1f); Check(z.attackRecovery==recovery,"roots freeze strike");
            r.rootTimers[0]=0; Step(r,.4f); Check(z.AttackFrame==0&&z.attacking,"recovery returns to ready");
            r=Fresh(c); z=Enemy(r,ViewSide.Back); Step(r,.34f); Check(z.attackRecovery>0&&r.otherAttackTimer>0,"hidden attacks animate and warn");
            r=Fresh(c); z=Enemy(r,ViewSide.Front,3,.4f); Step(r,.34f); Check(z.attacking&&z.AttackFrame>=3&&r.projectiles.Count==1,"spit animation accompanies projectile");
            r=Fresh(c); r.state=RunState.Shop;r.wallet=1000;r.BuyTower(1,1);r.BuyTower(3,1);r.state=RunState.Wave;z=Enemy(r,ViewSide.Front,0,.25f);
            var barrier=r.barricades[1];Step(r,.34f);Check(z.attacking&&barrier.health<barrier.maxHealth,"barricade strikes animate");
            r.decoyTimers[0]=2;r.Tick(.02f);Check(!z.attacking,"decoy cancels attack pose");
            r=Fresh(c,1);z=Enemy(r,ViewSide.Front,0,.5f);z.speed=0;hp=z.health;float ammo=r.ammo;
            r.Shoot(z.Head.center);Check(r.BowDrawing&&r.ammo==ammo&&z.health==hp,"bow begins drawing without spending arrow");
            Step(r,.1f);Check(r.BowDrawProgress>0&&CombatSprites.WeaponFrame(r)>=1,"bow draw advances visually");
            r.paused=true;float draw=r.bowDrawTimer;Step(r,.2f);Check(r.bowDrawTimer==draw,"pause freezes bow");
            r.paused=false;Step(r,.2f);Check(!r.BowDrawing&&r.ammo==ammo-1&&z.health<hp&&r.weaponAction>0,"release spends one arrow and deals damage");
            Step(r,.5f);Check(r.ammo==ammo-1,"one click emits one arrow");
            r.fireCooldown=0;r.Shoot(z.Body.center);r.SwitchSide();Step(r,.4f);Check(!r.BowDrawing&&r.ammo==ammo-1,"switch cancels drawing without firing into other side");
            r=Fresh(c,1);r.ammo=1;r.Shoot(new Vector2(200,600));Step(r,.3f);Check(r.ammo==0&&r.Reloading,"last arrow reload starts after release");
            r=Fresh(c,1);r.Shoot(new Vector2(200,600));r.Start(true);Check(!r.BowDrawing&&r.weaponAction==0,"restart cancels pending arrow");
            for(int slot=0;slot<3;slot++) Check(CombatSprites.DefensePosition(slot,true).y<CombatSprites.DefensePosition(slot,false).y,"barricade is ahead of tower lane "+slot);
            var signatures=new HashSet<string>();
            foreach(WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
                foreach(string action in new[]{"shot_","reload_"})
                {
                    var samples=PrototypeAudio.CreateSamples(action+kind); double energy=0;uint hash=2166136261;bool finite=true;
                    foreach(float sample in samples) { finite&=!float.IsNaN(sample)&&Mathf.Abs(sample)<=.92f; energy+=sample*sample;unchecked{hash=(hash^(uint)(int)(sample*32767))*16777619;} }
                    Check(finite,"finite unclipped sound "+action+kind);
                    Check(energy>1&&signatures.Add(hash.ToString()),"distinct audible sound "+action+kind);
                }
            Debug.Log("COMBAT_ANIMATION_TESTS_PASSED: "+checks+" checks");
        }
    }
}
