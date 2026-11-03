using System;
using System.Collections.Generic;
using UnityEngine;
namespace ZombieAttack
{
    public enum RunState { Menu, Wave, Shop, GameOver }
    // Both lanes always simulate; visibility filters only aiming and presentation.
    public class RunManager
    {
        public readonly GameCatalog catalog;
        public readonly PlayerProfile profile;
        public readonly List<Zombie> zombies=new List<Zombie>();
        public readonly List<Coin> coins=new List<Coin>();
        public readonly List<Projectile> projectiles=new List<Projectile>();
        public readonly List<CombatEffect> effects=new List<CombatEffect>();
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
        public int AmmoPackPrice => 12;
        public int AmmoPackAmount => 36;
        public int TowerPrice(int index) => catalog.towers[index].price;
        public static int SideIndex(ViewSide side) => side==ViewSide.Back?1:0;
        public static string SideName(ViewSide side) => side==ViewSide.Back?"COSTAS":"FRENTE";
        public int SlotIndex(int localSlot,ViewSide side) => SideIndex(side)*3+localSlot;
        public int CountOnSide(ViewSide side) => zombies.FindAll(z=>z.side==side).Count;
        public int ThreatsOnSide(ViewSide side) => zombies.FindAll(z=>z.side==side && z.distance<.22f).Count;
        public void Start(bool demonstration=false)
        {
            demo=demonstration; paused=false; newRecord=false; viewSide=ViewSide.Front;
            character=catalog.characters[profile.selected]; weapon=character.weapon;
            DepthProjector.Configure(character.theme);
            health=character.health; ammo=weapon.capacity; reserve=weapon.startingReserve;
            wave=completed=kills=wallet=spawned=earnedGems=healCount=waveBonus=0;
            reloadTimer=reloadDuration=fireCooldown=abilityCooldown=damageFlash=recoil=hitMarker=focusTimer=temporaryTurret=smokeTimer=otherAttackTimer=switchFlash=0;
            zombies.Clear(); coins.Clear(); projectiles.Clear(); effects.Clear(); ownedWeapons.Clear(); ownedWeapons.Add(weapon);
            Array.Clear(upgrades,0,4); Array.Clear(towers,0,6); Array.Clear(barricades,0,6); Array.Clear(rootTimers,0,2); Array.Clear(decoyTimers,0,2);
            NextWave();
        }
        public void SwitchSide()
        {
            if(paused || state==RunState.Menu || state==RunState.GameOver) return;
            viewSide=OtherSide; switchFlash=.28f; OnSound?.Invoke("turn");
        }
        public void NextWave()
        {
            if(state!=RunState.Shop && wave!=0) return;
            wave++; total=catalog.waves.Count(wave); spawned=0; spawnTimer=.5f;
            state=RunState.Wave; OnWaveStarted?.Invoke(wave); OnSound?.Invoke("wave");
        }
        public void Tick(float dt)
        {
            if(paused || state==RunState.Menu || state==RunState.GameOver) return;
            dt=Mathf.Clamp(dt,0,.05f);
            fireCooldown=Mathf.Max(0,fireCooldown-dt); abilityCooldown=Mathf.Max(0,abilityCooldown-dt);
            damageFlash=Mathf.Max(0,damageFlash-dt*2); recoil=Mathf.Max(0,recoil-dt*6); hitMarker=Mathf.Max(0,hitMarker-dt*3);
            otherAttackTimer=Mathf.Max(0,otherAttackTimer-dt); switchFlash=Mathf.Max(0,switchFlash-dt);
            focusTimer=Mathf.Max(0,focusTimer-dt); smokeTimer=Mathf.Max(0,smokeTimer-dt);
            for(int i=0;i<2;i++) { rootTimers[i]=Mathf.Max(0,rootTimers[i]-dt); decoyTimers[i]=Mathf.Max(0,decoyTimers[i]-dt); }
            if(Reloading)
            {
                reloadTimer=Mathf.Max(0,reloadTimer-dt);
                if(reloadTimer==0) { int count=Mathf.Min(Capacity,reserve); ammo=count; reserve-=count; OnSound?.Invoke("loaded"); }
            }
            else if(ammo<1 && reserve>0) BeginReload();
            for(int i=effects.Count-1;i>=0;i--) { effects[i].life-=dt; if(effects[i].life<=0) effects.RemoveAt(i); }
            for(int i=coins.Count-1;i>=0;i--) { coins[i].life-=dt; coins[i].age+=dt; if(coins[i].life<=0) coins.RemoveAt(i); }
            if(state==RunState.Shop)
            {
                shopTimer-=dt;
                if(demo && shopTimer<catalog.waves.shopSeconds-6)
                {
                    if(reserve<40) BuyAmmo();
                    if(wallet>=catalog.shopWeapons[0].price && !ownedWeapons.Contains(catalog.shopWeapons[0])) BuyWeapon(0);
                    BuyUpgrade(0); NextWave();
                }
                else if(shopTimer<=0) NextWave();
                return;
            }
            spawnTimer-=dt;
            if(spawned<total && spawnTimer<=0) { Spawn(); spawnTimer=catalog.waves.Interval(wave); }
            foreach(var z in zombies.ToArray())
            {
                if(!zombies.Contains(z)) continue;
                z.hitFlash=Mathf.Max(0,z.hitFlash-dt*5); z.phase+=dt*(z.data.kind==ZombieKind.Runner?11:4.5f);
                int side=SideIndex(z.side); if(rootTimers[side]>0) continue;
                z.specialTimer-=dt;
                if(z.specialTimer<=0)
                {
                    z.specialTimer=z.data.kind==ZombieKind.Boss?9:5;
                    if(z.data.kind==ZombieKind.Medic)
                        foreach(var friend in zombies) if(friend!=z && friend.side==z.side && Mathf.Abs(friend.lateral-z.lateral)<.45f) friend.health=Mathf.Min(friend.maxHealth,friend.health+1.3f);
                    if(z.data.kind==ZombieKind.Boss && zombies.Count<50)
                    { AddZombie(catalog.zombies[0],z.side,Mathf.Clamp(z.lateral+.2f,-.85f,.85f),Mathf.Min(1,z.distance+.2f)); OnSpatialSound?.Invoke("groan",z.side); }
                }
                Tower blocking=null; int blockingIndex=-1; bool isBarricade=false; float slow=1;
                for(int slot=0;slot<3;slot++)
                {
                    int index=side*3+slot;
                    if(Mathf.Abs((slot-1)*.66f-z.lateral)>.34f) continue;
                    var tower=towers[index]; var barrier=barricades[index];
                    if(tower!=null && tower.data.kind==TowerKind.Ice && z.distance<.6f) slow=.4f;
                    if(barrier!=null && z.distance<=.25f) { blocking=barrier; blockingIndex=index; isBarricade=true; }
                    else if(tower!=null && z.distance<=.11f) { blocking=tower; blockingIndex=index; }
                }
                if(decoyTimers[side]>0) continue;
                bool ranged=z.data.kind==ZombieKind.Spitter && z.distance<=.46f;
                float speed=z.speed*slow*(z.data.kind==ZombieKind.Shade && z.side!=viewSide?1.5f:1);
                if(z.distance>0 && blocking==null && !ranged) z.distance=Mathf.Max(0,z.distance-speed*dt);
                else
                {
                    z.attackTimer-=dt;
                    if(z.attackTimer<=0)
                    {
                        z.attackTimer=ranged?3.1f:z.data.attackInterval;
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
                if(tower==null || tower.data.kind==TowerKind.Ice) continue;
                tower.cooldown-=dt; var target=Closest(tower.Lateral,.65f,tower.side);
                if(target==null || tower.cooldown>0) continue;
                tower.cooldown=tower.data.interval; Vector2 aim=target.Head.center;
                effects.Add(new CombatEffect{side=tower.side,kind=1,origin=new Vector2(280+tower.slot*520,690),position=aim,color=Color.cyan,life=.15f,duration=.15f});
                if(tower.data.kind==TowerKind.Explosive) Blast(aim,tower.data.radius,tower.data.damage*tower.level,tower.side);
                else Hit(target,tower.data.damage*tower.level,false);
                OnSpatialSound?.Invoke("tower",tower.side);
            }
            for(int i=projectiles.Count-1;i>=0;i--)
            {
                var p=projectiles[i]; p.distance-=dt*.21f;
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
            if(spawned==total && zombies.Count==0 && coins.Count==0 && projectiles.Count==0)
            {
                completed=wave; waveBonus=12+wave*3; wallet+=waveBonus;
                state=RunState.Shop; shopTimer=catalog.waves.shopSeconds; OnSound?.Invoke("wave");
            }
            if(demo) DemoTick(dt);
        }
        public void BeginReload()
        {
            if(ammo>=1 || reserve<=0 || Reloading) return;
            reloadDuration=ReloadSeconds; reloadTimer=reloadDuration; OnSound?.Invoke("reload_"+weapon.kind);
        }
        void Spawn()
        {
            ZombieData data;
            if(wave%10==0 && spawned==total-1) data=catalog.zombies[5];
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
            zombies.Add(new Zombie{data=data,side=side,lateral=lateral,distance=distance,health=hp,maxHealth=hp,speed=data.speed*Mathf.Min(catalog.waves.speedCap,1+catalog.waves.speedPerWave*wave),phase=(float)random.NextDouble()*6});
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
            foreach(var z in zombies) if(z.side==viewSide && (z.Head.Contains(point)||z.Body.Contains(point)) && (result==null || z.distance<result.distance)) result=z;
            return result;
        }
        public void Shoot(Vector2 point)
        {
            if(paused || state!=RunState.Wave) return;
            var coin=CoinAt(point);
            if(coin!=null) { wallet+=coin.value; coins.Remove(coin); Text(point,"+"+coin.value,Draw2D.Gold,viewSide); OnSound?.Invoke("coin"); return; }
            if(Reloading || fireCooldown>0) return;
            if(ammo<1) { BeginReload(); fireCooldown=.25f; if(reserve==0) OnSound?.Invoke("empty"); return; }
            ammo-=1; fireCooldown=1/FireRate; recoil=1; OnSound?.Invoke("shot_"+weapon.kind);
            effects.Add(new CombatEffect{side=viewSide,kind=1,origin=new Vector2(810,760),position=point,color=Draw2D.Gold,life=.09f,duration=.09f});
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
                    bool head=z.Head.Contains(point); Hit(z,Damage*(head?weapon.headMultiplier:1),head);
                    if(weapon.splashRadius>0)
                        foreach(var other in zombies.ToArray()) if(other!=z && other.side==viewSide && Vector2.Distance(other.Body.center,point)<weapon.splashRadius) Hit(other,Damage*.6f,false);
                }
                else effects.Add(new CombatEffect{side=viewSide,position=point,kind=2,color=new Color(.7f,.63f,.45f),life=.25f,duration=.25f});
            }
            if(ammo<1) BeginReload();
        }
        public void Hit(Zombie z,float damage,bool head)
        {
            if(!zombies.Contains(z)) return;
            damage*=head?1:z.data.bodyArmor; z.health-=damage; z.hitFlash=1;
            if(z.side==viewSide) hitMarker=1;
            Text(z.Head.center,head?"NA CABEÇA!":damage.ToString("0.#"),head?Draw2D.Gold:Color.white,z.side);
            OnSpatialSound?.Invoke(z.data.bodyArmor<1 && !head?"metal":head?"head":"hit",z.side);
            if(z.health>0) return;
            kills++; zombies.Remove(z);
            if(random.NextDouble()<z.data.coinChance) coins.Add(new Coin{side=z.side,position=new Vector2(z.Position.x,Mathf.Min(690,z.Position.y-35)),value=z.data.coins});
            effects.Add(new CombatEffect{side=z.side,kind=2,position=z.Body.center,color=z.data.skin,life=.35f,duration=.35f});
            OnZombieDied?.Invoke(z);
            if(z.data.kind==ZombieKind.Volatile) { OnSpatialSound?.Invoke("explosion",z.side); Blast(z.Body.center,155,4,z.side); }
        }
        public void UseAbility(Vector2 point)
        {
            if(state!=RunState.Wave || paused || abilityCooldown>0) return;
            abilityCooldown=character.cooldown; OnSound?.Invoke("ability_"+character.ability);
            switch(character.ability)
            {
                case AbilityKind.Grenade: Blast(point,230,12,viewSide); break;
                case AbilityKind.Roots: rootTimers[SideIndex(viewSide)]=5; Ring(point,350,Draw2D.Mint); break;
                case AbilityKind.Turret: temporaryTurret=10; temporaryShot=0; turretSide=viewSide; break;
                case AbilityKind.Deadeye: focusTimer=6; break;
                case AbilityKind.Stomp:
                    Ring(new Vector2(800,660),660,Draw2D.Gold);
                    foreach(var z in zombies.ToArray()) if(z.side==viewSide) { z.distance=Mathf.Min(1,z.distance+.38f); Hit(z,3,false); } break;
                case AbilityKind.Decoy:
                    decoyTimers[SideIndex(viewSide)]=6;
                    foreach(var z in zombies) if(z.side==viewSide) z.distance=Mathf.Max(.28f,z.distance);
                    break;
                case AbilityKind.Cleave:
                    Ring(point,300,new Color(.9f,.5f,.3f));
                    foreach(var z in zombies.ToArray()) if(z.side==viewSide && Vector2.Distance(z.Body.center,point)<300) Hit(z,18,true); break;
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
            if(state==RunState.GameOver || smokeTimer>0) return;
            health=Mathf.Max(0,health-damage); damageFlash=.6f; lastAttackSide=side;
            if(side!=viewSide) otherAttackTimer=2.5f;
            OnSpatialSound?.Invoke("hurt",side);
            if(health>0) return;
            state=RunState.GameOver; paused=false; newRecord=completed>profile.bestWave;
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
            wallet-=AmmoPackPrice; reserve+=AmmoPackAmount; BeginReload(); OnSound?.Invoke("buy"); return true;
        }
        public bool BuyWeapon(int index)
        {
            if(state!=RunState.Shop || index<0 || index>=catalog.shopWeapons.Length) return false;
            var next=catalog.shopWeapons[index]; if(next==weapon) return false;
            bool owned=ownedWeapons.Contains(next); if(!owned && wallet<next.price) return false;
            if(!owned) { wallet-=next.price; ownedWeapons.Add(next); reserve+=next.capacity*2; }
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
            if(barrier?(towers[index]==null || barricades[index]!=null):towers[index]!=null) return false;
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
            if(coins.Count>0) { viewSide=coins[0].side; Shoot(coins[0].ScreenPosition); return; }
            Zombie target=null; foreach(var z in zombies) if(target==null || z.distance<target.distance) target=z;
            if(target==null || target.distance>.9f) return;
            viewSide=target.side; Shoot(target.Head.center);
            if((ThreatsOnSide(viewSide)>2 || reserve==0) && abilityCooldown<=0) UseAbility(target.Body.center);
        }
    }
}
