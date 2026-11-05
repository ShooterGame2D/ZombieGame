using System.Collections.Generic;
using UnityEngine;
namespace ZombieAttack
{
    public class ThrownAttack
    {
        public WeaponKind kind;
        public ViewSide side;
        public Vector2 origin, target;
        public float age, duration, damage;
        public bool impacted, grenade;
    }
    public partial class RunManager
    {
        public readonly List<ThrownAttack> thrown=new List<ThrownAttack>();
        public float waveEndTimer, shake, bossNotice, treeTimer, tutorialActionTime;
        public int treeLane;
        public ViewSide treeSide;
        bool treeHit;
        public int shotsFired,headshots,coinsCollected,reloadsCompleted,sidesSwitched,abilityUses;
        public int tutorialStep=-1;
        public bool tutorialWaiting;
        public bool Tutorial => tutorialStep>=0;
        public float WorldSpeed => focusTimer>0?.32f:1;
        public Zombie MainBoss => zombies.Find(z=>z.primaryBoss) ?? zombies.Find(z=>z.bossTier>0);
        public static readonly string[] BossNames={"Comandante","Couraçado","Pestilento","Colosso"};
        public static int Lane(float lateral) => Mathf.Clamp(Mathf.RoundToInt(lateral/.66f)+1,0,2);
        public static int LaneAt(Vector2 point)
        {
            float depth=Mathf.Clamp01((760-point.y)/360);
            float spread=Mathf.Lerp(660,270,depth);
            return Lane((point.x-800)/spread);
        }
        public void CollectCoin(Vector2 point)
        {
            if(paused||state!=RunState.Wave||tutorialWaiting) return;
            var coin=CoinAt(point); if(coin==null) return;
            wallet+=coin.value; coinsCollected++; coins.Remove(coin);
            Text(point,"+"+coin.value,Draw2D.Gold,viewSide); OnSound?.Invoke("coin");
        }
        void ResetExpansion()
        {
            thrown.Clear(); waveEndTimer=shake=bossNotice=treeTimer=tutorialActionTime=0;
            shotsFired=headshots=coinsCollected=reloadsCompleted=sidesSwitched=abilityUses=0;
            tutorialStep=-1;tutorialWaiting=false;
        }
        void TickExpansion(float dt,float worldDt)
        {
            shake=Mathf.Max(0,shake-dt*2.8f); bossNotice=Mathf.Max(0,bossNotice-dt);
            for(int i=thrown.Count-1;i>=0;i--)
            {
                var a=thrown[i]; a.age+=worldDt;
                if(!a.impacted&&a.age>=a.duration)
                {
                    a.impacted=true;
                    if(a.grenade) { Blast(a.target,245,16,a.side); shake=Mathf.Max(shake,.8f);OnSpatialSound?.Invoke("explosion",a.side); }
                    else
                    {
                        Zombie target=null;
                        foreach(var z in zombies) if(z.side==a.side&&(z.Body.Contains(a.target)||z.Head.Contains(a.target))&&(target==null||z.distance<target.distance)) target=z;
                        if(target!=null) { bool head=target.Head.Contains(a.target);if(head)headshots++;Hit(target,a.damage*(head?2:1),head,a.target); }
                        OnSpatialSound?.Invoke(a.kind==WeaponKind.Axe?"wood":"hit",a.side);
                    }
                }
                float tail=a.grenade?.55f:a.kind==WeaponKind.Boomerang?a.duration:.12f;
                if(a.age>=a.duration+tail) thrown.RemoveAt(i);
            }
            if(treeTimer>0)
            {
                treeTimer=Mathf.Max(0,treeTimer-worldDt);
                if(!treeHit&&treeTimer<=.65f)
                {
                    treeHit=true; shake=Mathf.Max(shake,1);OnSpatialSound?.Invoke("explosion",treeSide);
                    foreach(var z in zombies.ToArray()) if(z.side==treeSide&&Lane(z.lateral)==treeLane) Hit(z,z.health+1,true);
                }
            }
        }
        void SpawnBoss(int tier,ViewSide side,bool principal)
        {
            int variant=(tier-1)%4;
            int[] kinds={5,6,3,4};
            var data=catalog.zombies[kinds[variant]];
            float hp=(38+wave*3.4f)*(principal?1:.6f);
            zombies.Add(new Zombie{data=data,side=side,lateral=0,distance=1,health=hp,maxHealth=hp,
                speed=(variant==3?.045f:.033f)*Mathf.Min(2,1+wave*.025f),bossTier=tier,primaryBoss=principal,
                emergeTimer=1.1f,specialTimer=variant==0?8:variant==2?4:6});
            if(principal) { bossNotice=4;OnSound?.Invoke("boss"); }
        }
        public void StartTutorial()
        {
            Start(); tutorialStep=0;tutorialWaiting=true;spawned=total=0;spawnTimer=999;
            zombies.Clear();health=character.health;reserve=96;
        }
        public void ContinueTutorial()
        {
            tutorialWaiting=false;
            if(tutorialStep==0) tutorialStep=1;
            if(tutorialStep==1||tutorialStep==2)
            {
                zombies.Clear(); AddTrainingZombie(ViewSide.Front,100);viewSide=ViewSide.Front;
            }
            if(tutorialStep==3) { ammo=Mathf.Max(1,Capacity-3);reloadTimer=0; }
            if(tutorialStep==4) { coins.Clear();coins.Add(new Coin{side=viewSide,position=new Vector2(800,540),value=8,life=999}); }
            if(tutorialStep==5) { zombies.Clear();AddTrainingZombie(OtherSide,100); }
            if(tutorialStep==6) { abilityCooldown=0;tutorialActionTime=0; }
            if(tutorialStep==7)
            {
                zombies.Clear();coins.Clear();total=spawned=4;
                for(int i=0;i<4;i++) AddTrainingZombie(i%2==0?ViewSide.Front:ViewSide.Back,2,(i-1.5f)*.38f);
            }
            if(tutorialStep==8) { wallet=Mathf.Max(wallet,100);state=RunState.Shop;shopTimer=999; }
        }
        void AddTrainingZombie(ViewSide side,float hp,float lateral=0)
        { zombies.Add(new Zombie{data=catalog.zombies[1],side=side,lateral=lateral,distance=.45f,health=hp,maxHealth=hp,speed=0}); }
        public void AdvanceTutorial(int step) { tutorialStep=step;tutorialWaiting=true;CancelDraw(); }
        void TickTutorial(float dt)
        {
            if(!Tutorial||tutorialWaiting)return;
            if(tutorialStep==1&&shotsFired>0) AdvanceTutorial(2);
            else if(tutorialStep==2&&headshots>0) AdvanceTutorial(3);
            else if(tutorialStep==3&&reloadsCompleted>0) AdvanceTutorial(4);
            else if(tutorialStep==4&&coinsCollected>0) AdvanceTutorial(5);
            else if(tutorialStep==5&&sidesSwitched>0) AdvanceTutorial(6);
            else if(tutorialStep==6&&abilityUses>0) { tutorialActionTime+=dt;if(tutorialActionTime>2) AdvanceTutorial(7); }
            else if(tutorialStep==7&&state==RunState.Shop) AdvanceTutorial(8);
        }
    }
}
