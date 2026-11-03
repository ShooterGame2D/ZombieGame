using System;
using UnityEngine;

namespace ZombieAttack.Editor
{
    public static class ZombieWalkValidation
    {
        static int checks;
        static void Check(bool condition,string message)
        {
            if(!condition) throw new Exception("WALK ANIMATION FAILED: "+message);
            checks++;
        }
        static bool Near(float a,float b) => Mathf.Abs(a-b)<.001f;
        static RunManager Fresh(GameCatalog catalog)
        {
            var run=new RunManager(catalog,new PlayerProfile(),123);
            run.Start(); run.spawnTimer=999;
            return run;
        }
        static Zombie Enemy(RunManager run,int kind=0,ViewSide side=ViewSide.Front,float distance=.8f)
        {
            var data=run.catalog.zombies[kind];
            var zombie=new Zombie{data=data,side=side,distance=distance,health=1000,maxHealth=1000,speed=data.speed};
            run.zombies.Add(zombie); return zombie;
        }
        public static void Run(GameCatalog catalog)
        {
            checks=0;
            foreach(var data in catalog.zombies)
            {
                var entry=ZombieWalkAnimation.Get(data.kind);
                Check(entry!=null && entry.texture.width==1536 && entry.texture.height==1024,"runtime texture "+data.kind);
                Check(entry.footBaselines.Length==8 && entry.texture.filterMode==FilterMode.Point,"pixel filter and eight baselines "+data.kind);
                var run=Fresh(catalog); var zombie=Enemy(run,(int)data.kind);
                run.Tick(.05f);
                Check(Near(zombie.phase,entry.framesPerSecond*.05f),"movement rate "+data.kind);
                for(int frame=0;frame<8;frame++)
                {
                    zombie.phase=frame;
                    var rect=ZombieWalkAnimation.DrawRect(zombie,entry);
                    Check(Near(rect.y+entry.footBaselines[frame]*rect.height/512,zombie.Position.y),"ground anchor "+data.kind+" frame "+frame);
                }
            }
            Check(ZombieWalkAnimation.UV(0)==new Rect(0,.5f,.25f,.5f) && ZombieWalkAnimation.UV(7)==new Rect(.75f,0,.25f,.5f),"atlas rows map top to bottom");
            Check(ZombieWalkAnimation.Frame(8)==0 && ZombieWalkAnimation.Frame(15.9f)==7,"loop wraps without an extra frame");
            var r=Fresh(catalog); var front=Enemy(r); var back=Enemy(r,0,ViewSide.Back);
            r.Tick(.05f); Check(front.phase>0 && Near(front.phase,back.phase),"both cameras animate");
            r.SwitchSide(); float phase=front.phase; r.Tick(.05f);
            Check(front.phase>phase && Near(front.phase,back.phase),"camera switch does not reset hidden animation");
            r.paused=true; phase=front.phase; r.Tick(.05f); Check(front.phase==phase,"pause freezes animation");
            r.paused=false; r.rootTimers[0]=1; phase=front.phase; float backPhase=back.phase; r.Tick(.05f);
            Check(front.phase==phase && back.phase>backPhase,"roots freeze only their lane");
            r.rootTimers[0]=0; r.decoyTimers[0]=1; r.Tick(.05f); Check(front.phase==phase,"decoy stops walking");
            r.decoyTimers[0]=0; r.Tick(.05f); Check(front.phase>phase,"walk resumes after crowd control");
            r=Fresh(catalog); front=Enemy(r,0,ViewSide.Front,.5f); back=Enemy(r,0,ViewSide.Back,.5f);
            r.state=RunState.Shop; r.wallet=1000; r.BuyTower(1,1); r.state=RunState.Wave; r.Tick(.05f);
            Check(Near(front.phase,back.phase*.4f),"ice slows the animation with movement");
            r=Fresh(catalog); r.state=RunState.Shop; r.wallet=1000; r.BuyTower(0,1); r.BuyTower(3,1); r.state=RunState.Wave;
            front=Enemy(r,0,ViewSide.Front,.24f); r.Tick(.05f); Check(front.phase==0,"barricade melee does not walk in place");
            r=Fresh(catalog); front=Enemy(r,0,ViewSide.Front,0); r.Tick(.05f); Check(front.phase==0,"player melee stops the walk");
            r=Fresh(catalog); front=Enemy(r,3,ViewSide.Front,.4f); r.Tick(.05f); Check(front.phase==0,"stationary spitter does not walk");
            r=Fresh(catalog); front=Enemy(r,9); back=Enemy(r,9,ViewSide.Back); r.Tick(.05f);
            Check(Near(back.phase,front.phase*1.5f),"hidden shade animates at its actual accelerated speed");
            r=Fresh(catalog); front=Enemy(r); front.speed*=2; r.Tick(.05f);
            Check(Near(front.phase,ZombieWalkAnimation.Get(front.data.kind).framesPerSecond*.1f),"wave speed changes cadence");
            front.phase=7.99f; r.Tick(.05f); Check(front.phase>=0 && front.phase<1,"phase wraps during real simulation");
            r.state=RunState.Shop; phase=front.phase; r.Tick(.05f); Check(front.phase==phase,"shop does not advance animation");
            r.state=RunState.GameOver; r.Tick(.05f); Check(front.phase==phase,"game over does not advance animation");
            Debug.Log("ZOMBIE_WALK_TESTS_PASSED: "+checks+" checks");
        }
    }
}
