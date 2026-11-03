using System;
using System.Collections.Generic;
using UnityEngine;
namespace ZombieAttack.Editor
{
    public static class PrototypeValidation
    {
        static int checks;
        static void Check(bool ok,string label) { if(!ok) throw new Exception("VALIDATION FAILED: "+label); checks++; }
        static bool Near(float a,float b)=>Mathf.Abs(a-b)<.01f;
        static RunManager Fresh(GameCatalog c,int character=0)
        {
            var r=new RunManager(c,new PlayerProfile{selected=character,version=2},1234); r.Start(); r.spawnTimer=999; return r;
        }
        static void Step(RunManager r,float seconds) { for(int i=0;i<Mathf.CeilToInt(seconds/.05f);i++) r.Tick(.05f); }
        static Zombie Enemy(RunManager r,ViewSide side,float distance=.5f,int kind=0,float hp=30,float lateral=0)
        {
            var z=new Zombie{data=r.catalog.zombies[kind],side=side,distance=distance,lateral=lateral,health=hp,maxHealth=hp,speed=.03f}; r.zombies.Add(z); return z;
        }
        public static void Run(GameCatalog c)
        {
            checks=0;
            Check(c.characters.Length==8 && c.zombies.Length==10 && c.shopWeapons.Length==5,"complete roster, bestiary, shared arsenal");
            var skills=new HashSet<AbilityKind>();
            foreach(var hero in c.characters)
            {
                skills.Add(hero.ability);
                Check(hero.weapon.damage>=.9f && hero.weapon.damage<=1.15f && hero.weapon.fireRate>=2.3f && hero.weapon.fireRate<=3 && hero.weapon.reloadSeconds>=2.2f && hero.weapon.reloadSeconds<=2.7f,"starter balance "+hero.title);
            }
            Check(skills.Count==8,"eight distinct abilities");
            var r=Fresh(c); Check(r.wave==1 && r.total==7 && r.reserve==72,"initial run");
            var far=Enemy(r,ViewSide.Front,.5f); var near=Enemy(r,ViewSide.Front,.4f); var back=Enemy(r,ViewSide.Back,.4f);
            var point=near.Head.center; Check(r.ZombieAt(point)==near,"nearest target on visible side");
            r.Shoot(point); Check(Near(near.health,28) && Near(back.health,30) && Near(far.health,30),"headshot cannot hit opposite camera");
            r.SwitchSide(); Check(r.ZombieAt(point)==back,"camera switches aiming only");
            r.fireCooldown=0; r.Shoot(point); Check(Near(back.health,28),"shooting back camera");
            float d=near.distance; r.Tick(.05f); Check(near.distance<d,"hidden side continues moving");
            r.coins.Add(new Coin{side=ViewSide.Front,position=point,value=7}); Check(r.CoinAt(point)==null,"hidden coin is not collected");
            r.SwitchSide(); float ammo=r.ammo; r.Shoot(point); Check(r.wallet==7 && r.ammo==ammo,"coin priority costs no ammo");
            r=Fresh(c); r.ammo=4; Step(r,1); Check(r.ammo==4 && !r.Reloading,"partial magazine never regenerates");
            r.BeginReload(); Check(!r.Reloading,"cannot reload a partial magazine");
            r.ammo=1; r.Shoot(new Vector2(100,700)); Check(r.ammo==0 && r.Reloading && r.reserve==72,"last shot starts timed reload");
            float timer=r.reloadTimer; r.Shoot(new Vector2(100,700)); Check(r.ammo==0 && r.reloadTimer==timer,"no shot during reload");
            r.coins.Add(new Coin{side=ViewSide.Front,position=new Vector2(100,600),value=3}); r.Shoot(new Vector2(100,600)); Check(r.wallet==3 && r.Reloading,"coins collectible while reloading");
            r.SwitchSide(); Step(r,3); Check(r.ammo==r.Capacity && r.reserve==72-r.Capacity,"reload finishes across camera change and consumes reserve");
            r=Fresh(c); r.ammo=0; r.reserve=2; r.BeginReload(); Step(r,3); Check(r.ammo==2 && r.reserve==0,"partial last reserve magazine");
            r.ammo=0; r.BeginReload(); Step(r,3); Check(r.ammo==0 && !r.Reloading,"zero reserve cannot create ammo");
            r.state=RunState.Shop; r.wallet=100; Check(r.BuyAmmo() && r.reserve==36 && r.Reloading && r.wallet==88,"shop ammunition purchase starts empty-mag reload");
            float duration=r.ReloadSeconds; Check(r.BuyUpgrade(3) && r.ReloadSeconds<duration,"faster reload upgrade");
            int capacity=r.Capacity; Check(r.BuyUpgrade(2) && r.Capacity==capacity+2,"magazine upgrade");
            r.wallet=1000; Check(r.BuyWeapon(0) && r.weapon==c.shopWeapons[0],"shared weapon purchase"); int balance=r.wallet;
            Check(!r.BuyWeapon(0) && r.wallet==balance,"cannot rebuy equipped weapon");
            r.BuyWeapon(1); balance=r.wallet; r.BuyWeapon(0); Check(r.wallet==balance,"owned weapon re-equip is free");
            r=Fresh(c); r.state=RunState.Shop; r.wallet=1000;
            Check(!r.BuyTower(3,1),"barricade requires a tower behind it");
            Check(r.BuyTower(0,1) && r.BuyTower(3,1),"separate tower and barricade slots");
            Check(!r.BuyTower(3,1) && !r.BuyTower(0,1),"occupied slots protected");
            var tower=r.towers[1]; var barrier=r.barricades[1]; r.state=RunState.Wave;
            var attacker=Enemy(r,ViewSide.Front,.24f); attacker.attackTimer=0; Step(r,.1f);
            Check(barrier.health<barrier.maxHealth && tower.health==tower.maxHealth,"barricade takes damage before tower");
            r.SwitchSide(); r.state=RunState.Shop; Check(!r.BuyTower(3,1),"opposite camera needs its own tower");
            Check(r.BuyTower(1,1) && r.towers[4]!=null && r.towers[1]==tower,"independent defenses on each side");
            r=Fresh(c); var hidden=Enemy(r,ViewSide.Back,0); hidden.attackTimer=0; Step(r,.05f);
            Check(r.health<r.character.health && r.otherAttackTimer>0 && r.lastAttackSide==ViewSide.Back,"offscreen attack raises warning");
            r=Fresh(c); r.projectiles.Add(new Projectile{side=ViewSide.Back,lateral=0,distance=.001f,damage=10}); Step(r,.05f);
            Check(r.otherAttackTimer>0 && Near(r.health,r.character.health-10),"offscreen spit also warns and damages");
            r=Fresh(c); var front=Enemy(r,ViewSide.Front,.5f,0,30); back=Enemy(r,ViewSide.Back,.5f,0,30); r.UseAbility(front.Body.center);
            Check(front.health<30 && back.health==30,"area ability only affects its camera");
            float cooldown=r.abilityCooldown; r.UseAbility(front.Body.center); Check(r.abilityCooldown==cooldown,"ability cooldown enforced");
            r=Fresh(c,1); front=Enemy(r,ViewSide.Front); back=Enemy(r,ViewSide.Back); r.UseAbility(front.Body.center); d=front.distance; float bd=back.distance; Step(r,1);
            Check(front.distance==d && back.distance<bd,"roots freeze only chosen side");
            r=Fresh(c,2); front=Enemy(r,ViewSide.Front); r.UseAbility(front.Body.center); r.SwitchSide(); Step(r,1);
            Check(front.health<30 && r.turretSide==ViewSide.Front,"engineer turret persists on original side");
            r=Fresh(c,3); float damage=r.Damage; r.UseAbility(Vector2.zero); Check(Near(r.Damage,damage*2.5f),"cowboy deadeye");
            r=Fresh(c,4); front=Enemy(r,ViewSide.Front,.3f); back=Enemy(r,ViewSide.Back,.3f); r.UseAbility(Vector2.zero);
            Check(front.distance>.3f && back.distance==.3f,"kangaroo knockback stays local");
            r=Fresh(c,5); front=Enemy(r,ViewSide.Front,0); r.UseAbility(Vector2.zero); Step(r,1);
            Check(r.health==r.character.health && r.decoyTimers[0]>0,"child decoy prevents local melee attacks");
            r=Fresh(c,6); front=Enemy(r,ViewSide.Front); r.UseAbility(front.Body.center); Check(front.health<15,"lumberjack cleave");
            r=Fresh(c,7); r.UseAbility(Vector2.zero); r.HurtPlayer(30,ViewSide.Back); Check(r.health==r.character.health,"ninja smoke protects both sides temporarily");
            r=Fresh(c); var armor=Enemy(r,ViewSide.Front,.5f,6,30); r.Hit(armor,10,false); Check(Near(armor.health,26.5f),"armor reduces body hits"); r.Hit(armor,10,true); Check(Near(armor.health,16.5f),"headshot bypasses armor");
            r=Fresh(c); var medic=Enemy(r,ViewSide.Front,.5f,8); front=Enemy(r,ViewSide.Front); front.health=10; medic.specialTimer=0; Step(r,.05f); Check(front.health>10,"medic heals nearby allies");
            r=Fresh(c); var shade=Enemy(r,ViewSide.Back,.6f,9); var visible=Enemy(r,ViewSide.Front,.6f,9); Step(r,1); Check(shade.distance<visible.distance,"shade accelerates outside camera");
            r=Fresh(c); var volatileZombie=Enemy(r,ViewSide.Front,.5f,7,1); front=Enemy(r,ViewSide.Front,.5f); back=Enemy(r,ViewSide.Back,.5f); r.Hit(volatileZombie,5,true);
            Check(front.health<30 && back.health==30,"volatile death blast respects sides");
            r=Fresh(c); var boss=Enemy(r,ViewSide.Back,.5f,5); boss.specialTimer=0; Step(r,.05f); Check(r.zombies.Count==2 && r.zombies[1].side==ViewSide.Back,"commander summons in own lane");
            r=Fresh(c); for(int i=0;i<100;i++) { var z=Enemy(r,ViewSide.Front,.5f,0,1); r.Hit(z,10,true); }
            Check(r.coins.Count>30 && r.coins.Count<75,"coin drops are probabilistic");
            Check(r.coins.TrueForAll(coin=>coin.value==c.zombies[0].coins),"drop value still depends on zombie");
            r=Fresh(c); r.coins.Add(new Coin{side=ViewSide.Back,life=.01f}); Step(r,.05f); Check(r.coins.Count==0,"hidden coins still expire");
            r=Fresh(c); r.paused=true; r.ammo=0; r.BeginReload(); timer=r.reloadTimer; Step(r,1); Check(r.reloadTimer==timer,"pause freezes reload");
            r=Fresh(c); r.spawnTimer=.01f; Step(r,2.2f); Check(r.CountOnSide(ViewSide.Front)>0 && r.CountOnSide(ViewSide.Back)>0,"first wave spawns both directions");
            r=Fresh(c); r.spawned=r.total; r.Tick(.05f); Check(r.state==RunState.Shop && r.wallet==15,"wave bonus funds ammunition independently of random drops");
            r.NextWave(); Check(r.wave==2 && r.state==RunState.Wave,"next wave transition");
            r.demo=true; r.completed=3; r.kills=15; r.HurtPlayer(999,ViewSide.Back); Check(r.state==RunState.GameOver && r.earnedGems==83 && r.profile.gems==0,"demo reward never writes real progress");
            r.Start(true); Check(r.weapon==r.character.weapon && r.wallet==0 && r.reserve==72 && r.barricades[1]==null && r.viewSide==ViewSide.Front,"restart resets all run-only equipment");
            var migrated=SaveSystem.Migrate(new PlayerProfile{version=0,gems=345,bestWave=9,selected=3,unlocked=new[]{true,false,false,true}});
            Check(migrated.gems==345 && migrated.bestWave==9 && migrated.selected==2 && migrated.unlocked.Length==8,"old save migration preserves progress");
            r=new RunManager(c,new PlayerProfile(),77); r.Start(true);
            for(int i=0;i<18000 && r.wave<3 && r.state!=RunState.GameOver;i++) r.Tick(.05f);
            Check(r.wave>=3,"demo completes two bidirectional waves and shops");
            float previous=0;
            foreach(WeaponKind kind in Enum.GetValues(typeof(WeaponKind)))
            {
                var samples=PrototypeAudio.CreateSamples("shot_"+kind); float energy=0,peak=0;
                foreach(float v in samples) { energy+=v*v; peak=Mathf.Max(peak,Mathf.Abs(v)); }
                Check(energy>1 && peak<=.92f && !float.IsNaN(energy),"audible unclipped weapon sound "+kind);
                if(kind!=WeaponKind.Pistol) Check(Mathf.Abs(energy-previous)>.001f,"distinct sound signature "+kind);
                previous=energy;
            }
            Debug.Log("PROTOTYPE_V2_TESTS_PASSED: "+checks+" checks");
        }
    }
}
