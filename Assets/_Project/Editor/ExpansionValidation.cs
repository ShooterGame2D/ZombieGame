using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
namespace ZHold.Editor
{
    public static class ExpansionValidation
    {
        static int checks;
        static void Check(bool value,string label){if(!value)throw new Exception("EXPANSION FAILED: "+label);checks++;}
        static RunManager Fresh(GameCatalog c,int hero=0){var r=new RunManager(c,new PlayerProfile{selected=hero,unlocked=new[]{true,true,true,true,true,true,true,true}},821,persistProgress:false);r.Start(true);r.demo=false;r.spawnTimer=999;return r;}
        static void Step(RunManager r,float time){for(int i=0;i<Mathf.CeilToInt(time/.02f);i++)r.Tick(.02f);}
        static Zombie Enemy(RunManager r,ViewSide side=ViewSide.Front,float lateral=0,float hp=100)
        {var z=new Zombie{data=r.catalog.zombies[1],side=side,lateral=lateral,distance=.5f,health=hp,maxHealth=hp,speed=0};r.zombies.Add(z);return z;}
        public static void Run(GameCatalog c)
        {
            checks=0;
            float sensitivity=GameSettings.Sensitivity;
            GameSettings.Sensitivity=1;Check(Vector2.Distance(GameSettings.BrowserAim(new Vector2(400,600)),new Vector2(400,600))<.01f,"web aim neutral calibration");
            GameSettings.Sensitivity=2;Check(GameSettings.BrowserAim(new Vector2(850,500)).x>850,"web sensitivity increases movement");
            Check(GameSettings.BrowserAim(new Vector2(800,482.5f))==new Vector2(800,482.5f),"web aim keeps center stable");
            Check(GameSettings.BrowserAim(new Vector2(1500,40))==new Vector2(1500,40),"web sensitivity preserves interface clicks");
            GameSettings.Sensitivity=sensitivity;
            var soundHashes=new System.Collections.Generic.HashSet<long>();
            foreach(WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
            {
                foreach(string action in new[]{"shot_","reload_"})
                {
                    var clip=Resources.Load<AudioClip>("Audio/Recorded/"+action+kind);
                    Check(clip!=null&&clip.length>.1f&&clip.length<1.5f,"recorded clip "+action+kind);
                    var audio=new float[clip.samples*clip.channels];clip.GetData(audio,0);float peak=0;double energy=0;long hash=17;
                    foreach(float v in audio){peak=Mathf.Max(peak,Mathf.Abs(v));energy+=v*v;unchecked{hash=hash*31+(int)(v*10000);}}
                    Check(peak<.92f&&energy>1&&soundHashes.Add(hash),"audible unique normalized recording "+action+kind+" peak="+peak+" energy="+energy);
                }
            }
            foreach(string id in new[]{"Bow","Slingshot","Axe","Grenade","Teddy","Tree","Emerge","Distracted"})
            {
                var texture=ExpansionArt.Texture(id);Check(texture!=null&&texture.filterMode==FilterMode.Point,"pixel sheet "+id);
                Check(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(texture)).OfType<Sprite>().Count()==6,"six reusable sprites "+id);
            }
            var r=Fresh(c);r.ammo=3;r.reserve=2;r.BeginReload();Step(r,3);
            Check(r.ammo==5&&r.reserve==0,"partial manual reload conserves every round");
            r=Fresh(c);r.ammo=r.Capacity;r.BeginReload();Check(!r.Reloading,"full magazine cannot reload");
            r.ammo=3;r.BeginReload();float timer=r.reloadTimer;r.BeginReload();Check(timer==r.reloadTimer,"repeated R does not reset timer");
            r.paused=true;Step(r,.3f);Check(r.reloadTimer==timer,"paused reload is stable");
            r=Fresh(c);var point=new Vector2(800,550);r.coins.Add(new Coin{side=ViewSide.Front,position=point,value=5});
            float ammo=r.ammo;r.Shoot(point);Check(r.wallet==0&&r.ammo==ammo-1,"left click shoots through coin");
            r.CollectCoin(r.coins[0].ScreenPosition);Check(r.wallet==5&&r.ammo==ammo-1,"right click collects without ammo cost");
            r=Fresh(c);var front=Enemy(r);var back=Enemy(r,ViewSide.Back);r.UseAbility(front.Body.center);
            Check(front.health==100&&r.thrown.Count==1,"grenade must fly before explosion");r.SwitchSide();Step(r,1.05f);
            Check(front.health==84&&back.health==100&&r.shake>0,"grenade explodes on original side with shake");
            r=Fresh(c,6);front=Enemy(r,ViewSide.Front,-.66f);var middle=Enemy(r);back=Enemy(r,ViewSide.Back,-.66f);
            r.UseAbility(front.Position);r.SelectTreeLane(0);r.UseAbility(front.Position);Check(r.treeLane==0&&front.health==100,"tree selects projected lane and winds up");Step(r,1.1f);
            Check(!r.zombies.Contains(front)&&middle.health==100&&back.health==100,"tree kills only selected column and side");
            r=Fresh(c,6);front=Enemy(r);r.Shoot(front.Head.center);Check(front.health==100&&r.thrown.Count==1,"axe is a projectile");
            Step(r,.4f);Check(front.health==100,"axe flight not instantaneous");Step(r,.4f);Check(front.health<100,"axe impacts after travel");
            r=Fresh(c,4);front=Enemy(r);r.Shoot(front.Head.center);Check(r.fireCooldown>=1.49f&&front.health==100,"boomerang deliberate slower throw");Step(r,.9f);Check(front.health<100&&r.thrown.Count==1,"boomerang hits then returns");Step(r,.9f);Check(r.thrown.Count==0,"boomerang returns to hand");
            r=Fresh(c,5);front=Enemy(r);ammo=r.ammo;r.Shoot(front.Head.center);Check(r.BowDrawing&&r.ammo==ammo,"slingshot pulls before release");Step(r,.3f);Check(r.ammo==ammo-1,"slingshot releases one projectile");
            r=Fresh(c,3);front=Enemy(r);back=Enemy(r,ViewSide.Back);front.speed=back.speed=.1f;r.UseAbility(Vector2.zero);
            Step(r,1);Check(Mathf.Abs(front.distance-.468f)<.005f&&Mathf.Abs(back.distance-front.distance)<.001f,"deadeye slows both cameras");
            Check(r.focusTimer>3.9f&&r.focusTimer<4.1f,"deadeye uses real seconds");Step(r,4.05f);Check(r.focusTimer==0&&r.WorldSpeed==1,"deadeye ends after five seconds");
            r=Fresh(c);r.spawnTimer=0;Step(r,.05f);front=r.zombies[0];float distance=front.distance;Step(r,.5f);
            Check(front.emergeTimer>0&&front.distance==distance,"emergence prevents invisible walking");Step(r,.8f);Check(front.emergeTimer==0&&front.distance<distance,"walk follows emergence");
            r=Fresh(c,5);front=Enemy(r);r.UseAbility(front.Body.center);Step(r,.2f);Check(front.distracted&&front.distractPhase>0,"teddy triggers animated distraction");
            r=Fresh(c);r.spawned=r.total;Step(r,.02f);Check(r.state==RunState.WaveEnd&&r.waveEndTimer==3,"wave enters three-second countdown");
            r.paused=true;Step(r,1);Check(r.waveEndTimer==3,"countdown pauses");r.paused=false;Step(r,2.8f);Check(r.state==RunState.WaveEnd,"shop does not open early");Step(r,.25f);Check(r.state==RunState.Shop,"countdown opens shop");
            for(int wave=5;wave<=20;wave+=5)
            {
                r=Fresh(c);r.wave=wave;r.total=c.waves.Count(wave);r.spawned=3;r.spawnTimer=0;Step(r,.02f);
                Check(r.MainBoss!=null&&r.MainBoss.primaryBoss&&r.MainBoss.bossTier==wave/5,"principal boss wave "+wave);
                Check(r.MainBoss.BossName==RunManager.BossNames[wave/5-1],"distinct boss identity "+wave);
            }
            r=Fresh(c);r.wave=8;r.total=c.waves.Count(8);r.spawned=r.total-1;r.spawnTimer=0;Step(r,.02f);
            Check(r.MainBoss!=null&&!r.MainBoss.primaryBoss&&r.MainBoss.bossTier==1,"defeated boss returns as elite");
            r=Fresh(c);r.state=RunState.Shop;r.wallet=2000;r.BuyTower(0,1);r.BuyTower(3,1);var barrier=r.barricades[1];int wallet=r.wallet;
            Check(r.BuyTower(2,1)&&r.wallet==wallet-c.towers[2].price&&r.barricades[1]==barrier&&r.towers[1].level==1,"replace tower, preserve barrier, pay full price");
            for(int i=0;i<3;i++)r.ServiceTower(1,true);wallet=r.wallet;Check(!r.ServiceTower(1,true)&&r.wallet==wallet,"max tower level cannot spend more");
            Check(c.towers[0].price>=90&&c.towers[0].damage/c.towers[0].interval<1,"towers cost more and have lower DPS");
            Check(r.AmmoPackPrice>=24&&c.waves.Count(3)>c.waves.Count(1)&&c.waves.Interval(3)<c.waves.Interval(1),"more expensive ammo, faster escalation");
            var hashes=new System.Collections.Generic.HashSet<long>();
            foreach(MapTheme theme in Enum.GetValues(typeof(MapTheme)))
            {var samples=MapMusic.Samples(theme,out int rate);long h=17;float peak=0;double energy=0;foreach(float v in samples){peak=Mathf.Max(peak,Mathf.Abs(v));energy+=v*v;unchecked{h=h*31+(int)(v*10000);}}Check(rate==22050&&peak<=.65f&&energy>10&&hashes.Add(h),"unique unclipped music "+theme);}
            r=Fresh(c);r.StartTutorial();Check(r.Tutorial&&r.tutorialWaiting&&r.zombies.Count==0,"tutorial starts paused without hostiles");
            r.ContinueTutorial();front=r.zombies[0];r.Shoot(front.Body.center);Step(r,.02f);Check(r.tutorialStep==2&&r.tutorialWaiting,"tutorial confirms firing");
            r.ContinueTutorial();r.fireCooldown=0;r.Shoot(r.zombies[0].Head.center);Step(r,.02f);Check(r.tutorialStep==3,"tutorial requires headshot");
            r.ContinueTutorial();r.BeginReload();Step(r,3);Check(r.tutorialStep==4,"tutorial requires completed reload");
            r.ContinueTutorial();r.CollectCoin(r.coins[0].ScreenPosition);Step(r,.02f);Check(r.tutorialStep==5,"tutorial requires collection");
            r.ContinueTutorial();r.SwitchSide();Step(r,.02f);Check(r.tutorialStep==6,"tutorial requires camera swap");
            r.ContinueTutorial();r.UseAbility(new Vector2(800,500));Step(r,2.2f);Check(r.tutorialStep==7,"tutorial waits for special animation");
            r.ContinueTutorial();foreach(var z in r.zombies.ToArray())r.Hit(z,999,true);Step(r,3.2f);Check(r.tutorialStep==8&&r.state==RunState.Shop,"tutorial short wave reaches shop");
            r.ContinueTutorial();r.BuyAmmo();Check(r.tutorialStep==9&&r.tutorialWaiting,"tutorial requires actual shop purchase");
            Debug.Log("EXPANSION_TESTS_PASSED: "+checks+" checks");
        }
    }
}
