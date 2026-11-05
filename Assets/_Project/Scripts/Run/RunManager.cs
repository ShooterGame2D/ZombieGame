using System;
using System.Collections.Generic;
using UnityEngine;
namespace ZombieAttack
{
    public enum RunState { Menu, Wave, Shop, GameOver, WaveEnd }
    // Both lanes always simulate; visibility filters only aiming and presentation.
    public partial class RunManager
    {
        public readonly GameCatalog catalog;
        public readonly PlayerProfile profile;
        public readonly List<Zombie> zombies=new List<Zombie>();
        public readonly List<Coin> coins=new List<Coin>();
        public readonly List<Projectile> projectiles=new List<Projectile>();
        public readonly List<CombatEffect> effects=new List<CombatEffect>();
        public readonly BloodEffects blood=new BloodEffects();
        public readonly Tower[] towers=new Tower[6], barricades=new Tower[6];
        public readonly int[] upgrades=new int[4];
        public readonly HashSet<WeaponData> ownedWeapons=new HashSet<WeaponData>();
        public readonly float[] rootTimers=new float[2], decoyTimers=new float[2];
        public RunState state=RunState.Menu;
        public ViewSide viewSide=ViewSide.Front,lastAttackSide,turretSide;
        public CharacterData character;
        public WeaponData weapon;
        public bool paused,demo,newRecord;
        public int wave,completed,kills,wallet,spawned,total,earnedGems,healCount,reserve,waveBonus;
        public float health,ammo,reloadTimer,reloadDuration,fireCooldown,abilityCooldown,shopTimer,spawnTimer;
        public float damageFlash,recoil,hitMarker,focusTimer,temporaryTurret,smokeTimer,otherAttackTimer,switchFlash;
        public float weaponAction,bowDrawTimer;
        Vector2 bowAim;
        ViewSide bowSide;
        float bowDuration;
        public bool BowDrawing => bowDrawTimer>0;
        public float BowDrawProgress => BowDrawing?1-bowDrawTimer/Mathf.Max(.01f,bowDuration):0;
        public void CancelDraw() { bowDrawTimer=0; }
        public event Action<string> OnSound;
        public event Action<string,ViewSide> OnSpatialSound;
        public event Action<int> OnWaveStarted;
        public event Action<Zombie> OnZombieDied;
        readonly System.Random random;
        float demoTimer,temporaryShot;
        public RunManager(GameCatalog catalog,PlayerProfile profile,int? seed=null)
        {
            this.catalog=catalog; this.profile=profile; random=seed.HasValue?new System.Random(seed.Value):new System.Random();
            character=catalog.characters[Mathf.Clamp(profile.selected,0,catalog.characters.Length-1)]; weapon=character.weapon;
        }
        public float Damage => (weapon.damage+upgrades[0]*.45f)*(focusTimer>0?2.5f:1);
        public float FireRate => weapon.fireRate*(1+upgrades[1]*.15f);
        public int Capacity => weapon.capacity+upgrades[2]*2;
        public float ReloadSeconds => Mathf.Max(.65f,weapon.reloadSeconds*Mathf.Pow(.84f,upgrades[3]));
        public bool Reloading => reloadTimer>0;
        public float ReloadProgress => Reloading?1-reloadTimer/reloadDuration:0;
        public ViewSide OtherSide => viewSide==ViewSide.Front?ViewSide.Back:ViewSide.Front;
        public int Remaining => total-spawned+zombies.Count;
        public int UpgradePrice(int index) => (index<2?25:20)*(1<<upgrades[index]);
        public int HealPrice => 25+healCount*10;
        public int AmmoPackPrice => 24 + (wave/5)*4;
        public int AmmoPackAmount => 36;
        public int TowerPrice(int index) => catalog.towers[index].price;
        public static int SideIndex(ViewSide side) => side==ViewSide.Back?1:0;
        public static string SideName(ViewSide side) => side==ViewSide.Back?"COSTAS":"FRENTE";
        public int SlotIndex(int localSlot,ViewSide side) => SideIndex(side)*3+localSlot;
        public int CountOnSide(ViewSide side) => zombies.FindAll(z=>z.side==side).Count;
        public int ThreatsOnSide(ViewSide side) => zombies.FindAll(z=>z.side==side && z.distance<.22f).Count;
        public void Start(bool demonstration=false)
        {
            ResetExpansion(); demo=demonstration; paused=false; newRecord=false; viewSide=ViewSide.Front;
            character=catalog.characters[profile.selected]; weapon=character.weapon;
            DepthProjector.Configure(character.theme);
            health=character.health; ammo=weapon.capacity; reserve=weapon.startingReserve;
            wave=completed=kills=wallet=spawned=earnedGems=healCount=waveBonus=0;
            reloadTimer=reloadDuration=fireCooldown=abilityCooldown=damageFlash=recoil=hitMarker=focusTimer=temporaryTurret=smokeTimer=otherAttackTimer=switchFlash=0;
            zombies.Clear(); coins.Clear(); projectiles.Clear(); effects.Clear(); ownedWeapons.Clear(); ownedWeapons.Add(weapon);
            blood.Clear();
            bowDrawTimer=weaponAction=0;
            Array.Clear(upgrades,0,4); Array.Clear(towers,0,6); Array.Clear(barricades,0,6); Array.Clear(rootTimers,0,2); Array.Clear(decoyTimers,0,2);
            NextWave();
        }
        public void SwitchSide()
        {
            if(paused || state==RunState.Menu || state==RunState.GameOver) return;
            CancelDraw();
            viewSide=OtherSide; sidesSwitched++; switchFlash=.28f; OnSound?.Invoke("turn");
        }
        public void NextWave()
        {
            if(state!=RunState.Shop && wave!=0) return;
            blood.Clear();
            wave++; total=catalog.waves.Count(wave); spawned=0; spawnTimer=.5f; waveEndTimer=0;
            state=RunState.Wave; OnWaveStarted?.Invoke(wave); OnSound?.Invoke("wave");
        }
        public void Tick(float dt)
        {
            if(paused || tutorialWaiting || state==RunState.Menu || state==RunState.GameOver) return;
            dt=Mathf.Clamp(dt,0,.05f);
            float worldDt=dt*WorldSpeed;
            TickExpansion(dt,worldDt); TickTutorial(dt);
            if(tutorialWaiting) return;
            if(state==RunState.WaveEnd) { waveEndTimer=Mathf.Max(0,waveEndTimer-dt); if(waveEndTimer==0) { completed=wave;waveBonus=24+wave*4;wallet+=waveBonus;blood.Clear();state=RunState.Shop;shopTimer=catalog.waves.shopSeconds;OnSound?.Invoke("wave"); } return; }
            blood.Tick(worldDt);
            weaponAction=Mathf.Max(0,weaponAction-dt);
            if(BowDrawing)
            {
                if(state!=RunState.Wave||viewSide!=bowSide||(weapon.kind!=WeaponKind.Bow&&weapon.kind!=WeaponKind.Slingshot)) CancelDraw();
                else
                {
                    bowDrawTimer=Mathf.Max(0,bowDrawTimer-dt);
                    if(bowDrawTimer==0 && ammo>=1 && !Reloading) { FireShot(bowAim); fireCooldown=Mathf.Max(.05f,fireCooldown-bowDuration); }
                }
            }
            fireCooldown=Mathf.Max(0,fireCooldown-dt); abilityCooldown=Mathf.Max(0,abilityCooldown-dt);
            damageFlash=Mathf.Max(0,damageFlash-dt*2); recoil=Mathf.Max(0,recoil-dt*6); hitMarker=Mathf.Max(0,hitMarker-dt*3);
            otherAttackTimer=Mathf.Max(0,otherAttackTimer-dt); switchFlash=Mathf.Max(0,switchFlash-dt);
            focusTimer=Mathf.Max(0,focusTimer-dt); smokeTimer=Mathf.Max(0,smokeTimer-dt);
            for(int i=0;i<2;i++) { rootTimers[i]=Mathf.Max(0,rootTimers[i]-dt); decoyTimers[i]=Mathf.Max(0,decoyTimers[i]-dt); }
            if(Reloading)
            {
                reloadTimer=Mathf.Max(0,reloadTimer-dt);
                if(reloadTimer==0) { int count=Mathf.Min(Capacity-Mathf.FloorToInt(ammo),reserve); ammo+=count; reserve-=count; reloadsCompleted++; OnSound?.Invoke("loaded"); }
            }
            else if(ammo<1 && reserve>0) BeginReload();
            for(int i=effects.Count-1;i>=0;i--) { effects[i].life-=worldDt; if(effects[i].life<=0) effects.RemoveAt(i); }
            for(int i=coins.Count-1;i>=0;i--) { coins[i].life-=worldDt; coins[i].age+=worldDt; if(coins[i].life<=0) coins.RemoveAt(i); }
            if(state==RunState.Shop)
            {
                if(!Tutorial) shopTimer-=dt;
                if(demo && shopTimer<catalog.waves.shopSeconds-6)
                {
                    if(reserve<40) BuyAmmo();
                    if(wallet>=catalog.shopWeapons[0].price && !ownedWeapons.Contains(catalog.shopWeapons[0])) BuyWeapon(0);
                    BuyUpgrade(0); NextWave();
                }
                else if(shopTimer<=0) NextWave();
                return;
            }
            spawnTimer-=worldDt;
            if(spawned<total && spawnTimer<=0) { Spawn(); spawnTimer=catalog.waves.Interval(wave); }
            foreach(var z in zombies.ToArray())
            {
                if(!zombies.Contains(z)) continue;
                z.hitFlash=Mathf.Max(0,z.hitFlash-worldDt*5);
                if(z.emergeTimer>0) { z.emergeTimer=Mathf.Max(0,z.emergeTimer-worldDt);continue; }
                z.distracted=decoyTimers[SideIndex(z.side)]>0;
                if(z.distracted) z.distractPhase+=worldDt;
                int side=SideIndex(z.side); if(rootTimers[side]>0) continue;
                z.attackRecovery=Mathf.Max(0,z.attackRecovery-worldDt);
                z.specialTimer-=worldDt;
                if(z.specialTimer<=0)
                {
                    z.specialTimer=z.bossTier>0?6:z.data.kind==ZombieKind.Boss?9:5;
                    if(z.bossTier>0&&(z.bossTier-1)%4==3) {z.distance=Mathf.Max(0,z.distance-.12f);OnSpatialSound?.Invoke("groan",z.side);}
                    if(z.bossTier>0&&(z.bossTier-1)%4==2) for(int spread=-1;spread<=1;spread++)projectiles.Add(new Projectile{side=z.side,lateral=Mathf.Clamp(z.lateral+spread*.35f,-.9f,.9f),distance=z.distance,damage=8});
                    if(z.data.kind==ZombieKind.Medic)
                        foreach(var friend in zombies) if(friend!=z && friend.side==z.side && Mathf.Abs(friend.lateral-z.lateral)<.45f) friend.health=Mathf.Min(friend.maxHealth,friend.health+1.3f);
                    if((z.data.kind==ZombieKind.Boss || z.bossTier>0&&(z.bossTier-1)%4==0) && zombies.Count<50)
                    { AddZombie(catalog.zombies[0],z.side,Mathf.Clamp(z.lateral+.2f,-.85f,.85f),Mathf.Min(1,z.distance+.2f)); OnSpatialSound?.Invoke("groan",z.side); }
                }
                Tower blocking=null; int blockingIndex=-1; bool isBarricade=false; float slow=1,stopDepth=0;
                for(int slot=0;slot<3;slot++)
                {
                    int index=side*3+slot;
                    if(Mathf.Abs((slot-1)*.66f-z.lateral)>.34f) continue;
                    var tower=towers[index]; var barrier=barricades[index];
                    if(barrier!=null && z.distance>=.25f) stopDepth=Mathf.Max(stopDepth,.25f);
                    else if(tower!=null && z.distance>=.11f) stopDepth=Mathf.Max(stopDepth,.11f);
                    if(tower!=null && tower.data.kind==TowerKind.Ice && z.distance<.6f) slow=.65f;
                    if(barrier!=null && z.distance<=.25f) { blocking=barrier; blockingIndex=index; isBarricade=true; }
                    else if(tower!=null && z.distance<=.11f) { blocking=tower; blockingIndex=index; }
                }
                if(decoyTimers[side]>0) { z.attacking=false; z.attackRecovery=0; continue; }
                bool ranged=z.data.kind==ZombieKind.Spitter && z.distance<=.46f;
                float speed=z.speed*slow*(z.data.kind==ZombieKind.Shade && z.side!=viewSide?1.5f:1);
                if(z.distance>0 && blocking==null && !ranged)
                {
                    z.attacking=false; z.attackRecovery=0;
                    float previousDistance=z.distance;
                    z.distance=Mathf.Max(stopDepth,z.distance-speed*worldDt);
                    ZombieWalkAnimation.Advance(z,previousDistance-z.distance);
                }
                else
                {
                    z.attacking=true;
                    z.attackTimer-=worldDt;
                    if(z.attackTimer<=0)
                    {
                        z.attackTimer=ranged?3.1f:z.data.attackInterval;
                        z.attackRecovery=Zombie.AttackRecover;
                        if(ranged) { projectiles.Add(new Projectile{side=z.side,lateral=z.lateral,distance=z.distance,damage=z.data.damage}); OnSpatialSound?.Invoke("spit",z.side); }
                        else if(blocking!=null)
                        {
                            blocking.health-=z.data.damage; OnSpatialSound?.Invoke("wood",z.side);
                            if(blocking.health<=0) { if(isBarricade) barricades[blockingIndex]=null; else towers[blockingIndex]=null; }
                        }
                        else HurtPlayer(z.data.damage,z.side);
                        if(state==RunState.GameOver) return;
                    }
                }
            }
            foreach(var tower in towers)
            {
                if(tower!=null) tower.shotRecoil=Mathf.Max(0,tower.shotRecoil-worldDt);
                if(tower==null || tower.data.kind==TowerKind.Ice) continue;
                tower.cooldown-=worldDt; var target=Closest(tower.Lateral,.65f,tower.side);
                if(target==null || tower.cooldown>0) continue;
                tower.cooldown=tower.data.interval; tower.shotRecoil=.18f; Vector2 aim=target.Head.center;
                effects.Add(new CombatEffect{side=tower.side,kind=1,origin=CombatSprites.Muzzle(tower),position=aim,color=Color.cyan,life=.15f,duration=.15f});
                if(tower.data.kind==TowerKind.Explosive) Blast(aim,tower.data.radius,tower.data.damage*(1+(tower.level-1)*.3f),tower.side);
                else Hit(target,tower.data.damage*(1+(tower.level-1)*.3f),false);
                OnSpatialSound?.Invoke("tower",tower.side);
            }
            for(int i=projectiles.Count-1;i>=0;i--)
            {
                var p=projectiles[i]; p.distance-=worldDt*.21f;
                if(p.distance<=0) { projectiles.RemoveAt(i); HurtPlayer(p.damage,p.side); if(state==RunState.GameOver) return; }
            }
            if(temporaryTurret>0)
            {
                temporaryTurret-=dt; temporaryShot-=dt; var target=Closest(0,2,turretSide);
                if(temporaryShot<=0 && target!=null)
                {
                    effects.Add(new CombatEffect{side=turretSide,kind=1,origin=new Vector2(1080,730),position=target.Head.center,color=Draw2D.Gold,life=.15f,duration=.15f});
                    Hit(target,2.5f,false); temporaryShot=.45f; OnSpatialSound?.Invoke("tower",turretSide);
                }
            }
            if(spawned==total && zombies.Count==0 && projectiles.Count==0 && thrown.Count==0 && treeTimer<=0 && (!Tutorial||tutorialStep==7))
            {
                foreach(var coin in coins) wallet+=coin.value;
                coins.Clear(); state=RunState.WaveEnd;waveEndTimer=3;CancelDraw();OnSound?.Invoke("wave");
            }
            if(demo) DemoTick(dt);
        }
        public void BeginReload()
        {
            if(paused||tutorialWaiting||state==RunState.Menu||state==RunState.GameOver||ammo>=Capacity || reserve<=0 || Reloading) return;
            CancelDraw();
            reloadDuration=ReloadSeconds; reloadTimer=reloadDuration; OnSound?.Invoke("reload_"+weapon.kind);
        }
        void Spawn()
        {
            ZombieData data;
            if(wave%5==0 && spawned==Mathf.Min(3,total-1)) { SpawnBoss(wave/5,ViewSide.Front,true);spawned++;return; }
            else if(wave>=8 && spawned==total-1) { int unlocked=Mathf.Max(1,(wave-3)/5);SpawnBoss(random.Next(1,unlocked+1),ViewSide.Back,false);spawned++;return; }
            else
            {
                var eligible=new List<ZombieData>();
                foreach(var d in catalog.zombies) if(d.kind!=ZombieKind.Boss && d.firstWave<=wave) eligible.Add(d);
                data=eligible[random.Next(eligible.Count)];
            }
            ViewSide side=spawned<2?(spawned==0?ViewSide.Front:ViewSide.Back):(random.NextDouble()<.5?ViewSide.Front:ViewSide.Back);
            AddZombie(data,side,(float)random.NextDouble()*1.8f-.9f,1);
            spawned++; OnSpatialSound?.Invoke("groan",side);
        }
        void AddZombie(ZombieData data,ViewSide side,float lateral,float distance)
        {
            float hp=data.health*(1+catalog.waves.healthPerWave*wave);
            zombies.Add(new Zombie{data=data,side=side,lateral=lateral,distance=distance,health=hp,maxHealth=hp,speed=data.speed*Mathf.Min(catalog.waves.speedCap,1+catalog.waves.speedPerWave*wave),emergeTimer=1.1f,phase=(float)random.NextDouble()*ZombieWalkAnimation.FrameCount});
        }
        Zombie Closest(float lateral,float range,ViewSide side)
        {
            Zombie result=null;
            foreach(var z in zombies) if(z.side==side && Mathf.Abs(z.lateral-lateral)<range && (result==null || z.distance<result.distance)) result=z;
            return result;
        }
        public Coin CoinAt(Vector2 point)
        {
            for(int i=coins.Count-1;i>=0;i--) if(coins[i].side==viewSide && Vector2.Distance(coins[i].ScreenPosition,point)<29) return coins[i];
            return null;
        }
        public Zombie ZombieAt(Vector2 point)
        {
            Zombie result=null;
            foreach(var z in zombies) if(z.side==viewSide && z.emergeTimer<=.35f && (z.Head.Contains(point)||z.Body.Contains(point)) && (result==null || z.distance<result.distance)) result=z;
            return result;
        }
        public void Shoot(Vector2 point)
        {
            if(paused || tutorialWaiting || state!=RunState.Wave) return;
            if(Reloading || fireCooldown>0) return;
            if(ammo<1) { BeginReload(); fireCooldown=.25f; if(reserve==0) OnSound?.Invoke("empty"); return; }
            if(weapon.kind==WeaponKind.Bow||weapon.kind==WeaponKind.Slingshot)
            {
                bowAim=point;
                if(!BowDrawing) { bowDuration=.65f/FireRate; bowDrawTimer=bowDuration; bowSide=viewSide; OnSound?.Invoke(weapon.kind==WeaponKind.Bow?"draw_Bow":"reload_Slingshot"); }
                return;
            }
            FireShot(point);
        }
        void FireShot(Vector2 point)
        {
            weaponAction=weapon.kind==WeaponKind.Boomerang?1.25f:weapon.kind==WeaponKind.Axe?.7f:.28f;
            shotsFired++; shake=Mathf.Max(shake,weapon.kind==WeaponKind.Shotgun?.32f:weapon.kind==WeaponKind.Launcher?.5f:.07f);
            ammo-=1; fireCooldown=1/FireRate; recoil=1; OnSound?.Invoke("shot_"+weapon.kind);
            if(weapon.kind==WeaponKind.Axe||weapon.kind==WeaponKind.Boomerang)
            {
                bool axe=weapon.kind==WeaponKind.Axe;float flight=axe?.58f:.85f;fireCooldown=Mathf.Max(fireCooldown,(axe?.8f:1.5f)/(1+upgrades[1]*.15f));
                thrown.Add(new ThrownAttack{kind=weapon.kind,side=viewSide,origin=new Vector2(900,760),target=point,duration=flight,damage=Damage,age=axe?-.16f:0});
                if(ammo<1) BeginReload();return;
            }
            bool arrow=weapon.kind==WeaponKind.Bow||weapon.kind==WeaponKind.Crossbow;
            effects.Add(new CombatEffect{side=viewSide,kind=arrow?4:1,origin=new Vector2(810,760),position=point,color=Draw2D.Gold,life=arrow?.18f:.09f,duration=arrow?.18f:.09f});
            bool intercepted=false;
            for(int i=projectiles.Count-1;i>=0;i--)
                if(projectiles[i].side==viewSide && Vector2.Distance(point,projectiles[i].Position)<projectiles[i].Radius+9)
                { projectiles.RemoveAt(i); Text(point,"INTERCEPTADO",Color.cyan,viewSide); hitMarker=1; intercepted=true; break; }
            if(!intercepted)
            {
                var z=ZombieAt(point);
                if(weapon.kind==WeaponKind.Launcher) Blast(point,weapon.splashRadius,Damage,viewSide);
                else if(z!=null)
                {
                    bool head=z.Head.Contains(point);if(head)headshots++; Hit(z,Damage*(head?weapon.headMultiplier:1),head,point);
                    if(weapon.splashRadius>0)
                        foreach(var other in zombies.ToArray()) if(other!=z && other.side==viewSide && Vector2.Distance(other.Body.center,point)<weapon.splashRadius) Hit(other,Damage*.6f,false);
                }
                else effects.Add(new CombatEffect{side=viewSide,position=point,kind=2,color=new Color(.7f,.63f,.45f),life=.25f,duration=.25f});
            }
            if(ammo<1) BeginReload();
        }
        public void Hit(Zombie z,float damage,bool head,Vector2? impact=null)
        {
            if(!zombies.Contains(z)||damage<=0) return;
            damage*=head?1:z.data.bodyArmor; z.health-=damage; z.hitFlash=1;
            blood.Burst(z,impact ?? (head?z.Head.center:z.Body.center),head,z.health<=0);
            if(z.side==viewSide) hitMarker=1;
            Text(z.Head.center,head?"NA CABEÇA!":damage.ToString("0.#"),head?Draw2D.Gold:Color.white,z.side);
            OnSpatialSound?.Invoke(z.data.bodyArmor<1 && !head?"metal":head?"head":"hit",z.side);
            if(z.health>0) return;
            kills++; zombies.Remove(z);
            if(z.bossTier>0 || random.NextDouble()<z.data.coinChance) coins.Add(new Coin{side=z.side,position=new Vector2(z.Position.x,Mathf.Min(690,z.Position.y-35)),value=z.bossTier>0?40+z.bossTier*10:z.data.coins});
            OnZombieDied?.Invoke(z);
            if(z.data.kind==ZombieKind.Volatile) { OnSpatialSound?.Invoke("explosion",z.side); Blast(z.Body.center,155,4,z.side); }
        }
        public void UseAbility(Vector2 point)
        {
            if(state!=RunState.Wave || paused || tutorialWaiting || abilityCooldown>0) return;
            abilityUses++;
            abilityCooldown=character.cooldown; OnSound?.Invoke("ability_"+character.ability);
            switch(character.ability)
            {
                case AbilityKind.Grenade: thrown.Add(new ThrownAttack{grenade=true,side=viewSide,origin=new Vector2(1000,780),target=point,duration=.95f}); break;
                case AbilityKind.Roots: rootTimers[SideIndex(viewSide)]=5; Ring(point,350,Draw2D.Mint); break;
                case AbilityKind.Turret: temporaryTurret=10; temporaryShot=0; turretSide=viewSide; break;
                case AbilityKind.Deadeye: focusTimer=5; break;
                case AbilityKind.Stomp:
                    Ring(new Vector2(800,660),660,Draw2D.Gold);
                    foreach(var z in zombies.ToArray()) if(z.side==viewSide) { z.distance=Mathf.Min(1,z.distance+.38f); Hit(z,3,false); } break;
                case AbilityKind.Decoy:
                    decoyTimers[SideIndex(viewSide)]=6;
                    foreach(var z in zombies) if(z.side==viewSide) z.distance=Mathf.Max(.28f,z.distance);
                    break;
                case AbilityKind.Cleave:
                    treeLane=LaneAt(point);treeSide=viewSide;treeTimer=1.65f;treeHit=false;break;
                case AbilityKind.Smoke:
                    smokeTimer=5; Ring(new Vector2(800,620),600,new Color(.6f,.6f,.85f));
                    foreach(var z in zombies.ToArray()) if(z.side==viewSide) Hit(z,4,true); break;
            }
        }
        void Ring(Vector2 point,float radius,Color color) => effects.Add(new CombatEffect{side=viewSide,kind=3,position=point,origin=new Vector2(radius,0),color=color,life=.55f,duration=.55f});
        void Blast(Vector2 point,float radius,float power,ViewSide side)
        {
            effects.Add(new CombatEffect{side=side,kind=3,position=point,origin=new Vector2(radius,0),color=new Color(1,.6f,.25f),life=.45f,duration=.45f});
            foreach(var z in zombies.ToArray()) if(z.side==side && Vector2.Distance(z.Body.center,point)<radius) Hit(z,power,false);
            projectiles.RemoveAll(p=>p.side==side && Vector2.Distance(p.Position,point)<radius);
        }
        public void HurtPlayer(float damage,ViewSide side=ViewSide.Front)
        {
            if(state==RunState.GameOver || smokeTimer>0 || Tutorial) return;
            shake=Mathf.Max(shake,.65f);
            health=Mathf.Max(0,health-damage); damageFlash=.6f; lastAttackSide=side;
            if(side!=viewSide) otherAttackTimer=2.5f;
            OnSpatialSound?.Invoke("hurt",side);
            if(health>0) return;
            state=RunState.GameOver; paused=false; newRecord=completed>profile.bestWave;
            CancelDraw();
            earnedGems=Mathf.Max(1,10*completed+kills/5+(newRecord?50:0));
            if(!demo) { profile.gems+=earnedGems; profile.bestWave=Mathf.Max(profile.bestWave,completed); SaveSystem.Save(profile); }
        }
        public bool BuyUpgrade(int index)
        {
            if(state!=RunState.Shop || index<0 || index>3 || upgrades[index]>=5 || wallet<UpgradePrice(index)) return false;
            wallet-=UpgradePrice(index); upgrades[index]++; OnSound?.Invoke("buy"); return true;
        }
        public bool BuyAmmo()
        {
            if(state!=RunState.Shop || wallet<AmmoPackPrice) return false;
            wallet-=AmmoPackPrice; reserve+=AmmoPackAmount; BeginReload(); OnSound?.Invoke("buy"); if(Tutorial&&tutorialStep==8)AdvanceTutorial(9);return true;
        }
        public bool BuyWeapon(int index)
        {
            if(state!=RunState.Shop || index<0 || index>=catalog.shopWeapons.Length) return false;
            var next=catalog.shopWeapons[index]; if(next==weapon) return false;
            bool owned=ownedWeapons.Contains(next); if(!owned && wallet<next.price) return false;
            if(!owned) { wallet-=next.price; ownedWeapons.Add(next); reserve+=next.capacity*2; }
            CancelDraw(); weaponAction=0;
            reserve+=Mathf.FloorToInt(ammo); weapon=next; ammo=0; reloadTimer=0; BeginReload(); OnSound?.Invoke("buy"); return true;
        }
        public bool Heal()
        {
            if(state!=RunState.Shop || wallet<HealPrice || health>=character.health) return false;
            wallet-=HealPrice; healCount++; health=Mathf.Min(character.health,health+character.health*.3f); OnSound?.Invoke("buy"); return true;
        }
        public bool BuyTower(int type,int slot)
        {
            if(state!=RunState.Shop || slot<0 || slot>2 || type<0 || type>=catalog.towers.Length || wallet<TowerPrice(type)) return false;
            int index=SlotIndex(slot,viewSide); var data=catalog.towers[type]; bool barrier=data.kind==TowerKind.Barricade;
            if(barrier?(towers[index]==null || barricades[index]!=null):(towers[index]!=null&&towers[index].data==data)) return false;
            wallet-=TowerPrice(type); var tower=new Tower{data=data,side=viewSide,slot=slot,health=data.health,maxHealth=data.health};
            if(barrier) barricades[index]=tower; else towers[index]=tower;
            OnSound?.Invoke("buy"); return true;
        }
        public bool ServiceTower(int slot,bool upgrade,bool barrier=false)
        {
            if(state!=RunState.Shop || slot<0 || slot>2) return false;
            var tower=(barrier?barricades:towers)[SlotIndex(slot,viewSide)]; if(tower==null) return false;
            int price=upgrade?25*tower.level:12;
            if(wallet<price || (!upgrade && tower.health>=tower.maxHealth) || (upgrade && tower.level>=4)) return false;
            wallet-=price; if(upgrade) { tower.level++; tower.maxHealth+=35; } tower.health=tower.maxHealth; OnSound?.Invoke("buy"); return true;
        }
        public bool SelectCharacter(int index)
        {
            if(state!=RunState.Menu || index<0 || index>=catalog.characters.Length) return false;
            profile.selected=index; character=catalog.characters[index]; weapon=character.weapon; SaveSystem.Save(profile); return true;
        }
        void Text(Vector2 pos,string text,Color color,ViewSide side) => effects.Add(new CombatEffect{side=side,position=pos,text=text,color=color});
        void DemoTick(float dt)
        {
            demoTimer-=dt; if(demoTimer>0) return; demoTimer=.22f;
            if(coins.Count>0) { viewSide=coins[0].side; CollectCoin(coins[0].ScreenPosition); return; }
            Zombie target=null; foreach(var z in zombies) if(target==null || z.distance<target.distance) target=z;
            if(target==null || target.distance>.9f) return;
            viewSide=target.side; Shoot(target.Head.center);
            if((ThreatsOnSide(viewSide)>2 || reserve==0) && abilityCooldown<=0) UseAbility(target.Body.center);
        }
    }
}
