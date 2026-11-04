using System;
using UnityEngine;

namespace ZombieAttack.Editor
{
    public static class VisualFeedbackValidation
    {
        static int checks;
        static void Check(bool ok,string label) { if(!ok) throw new Exception("VISUAL FEEDBACK FAILED: "+label); checks++; }
        static RunManager Fresh(GameCatalog catalog)
        {
            var run=new RunManager(catalog,new PlayerProfile(),746); run.Start(true); run.demo=false; run.spawnTimer=999; return run;
        }
        static Zombie Enemy(RunManager run,ViewSide side=ViewSide.Front,float health=100)
        {
            var data=run.catalog.zombies[0];
            var zombie=new Zombie{data=data,side=side,distance=.6f,lateral=.3f,health=health,maxHealth=health,speed=data.speed};
            run.zombies.Add(zombie); return zombie;
        }
        public static void Run(GameCatalog catalog)
        {
            checks=0;
            var font=PixelFont.Face;
            Check(font!=null&&font.name=="PressStart2P-Regular","bundled Press Start 2P font");
            foreach(char c in "ÁÉÍÓÚÃÕÂÊÔÇáéíóúãõâêôç0123456789") Check(font.HasCharacter(c),"font glyph "+c);
            Check(PixelFont.FitSize("COMEÇAR DEFESA",188,22,16)>=8,"readable narrow-window button");
            Check(PixelFont.FitSize("DEFESA ZUMBI",385,45,32)>=24,"readable title");
            var run=Fresh(catalog); var zombie=Enemy(run);
            var impact=zombie.Body.center+new Vector2(8,10); run.Hit(zombie,1,false,impact);
            Check(run.blood.particles.Count==9&&run.blood.stains.Count>0,"nonlethal hit creates droplets and ground blood");
            Check(run.blood.particles.TrueForAll(p=>p.origin==impact&&p.side==ViewSide.Front),"blood starts at actual impact in its own camera");
            var particle=run.blood.particles[0]; run.Tick(.05f);
            Check(particle.position!=impact,"droplets move immediately");
            var position=particle.position; var age=particle.age; run.paused=true; run.Tick(.05f);
            Check(particle.position==position&&particle.age==age,"pause freezes blood physics");
            run.paused=false; run.SwitchSide(); run.Tick(.05f);
            Check(particle.age>age&&particle.side==ViewSide.Front,"hidden blood continues simulating without changing lanes");
            for(int i=0;i<50;i++) run.Tick(.05f);
            Check(run.blood.particles.Count==0&&run.blood.stains.Count>0,"droplets land and leave persistent stains");
            int stains=run.blood.stains.Count; var stainPosition=run.blood.stains[0].position;
            for(int i=0;i<100;i++) run.Tick(.05f);
            Check(run.blood.stains.Count==stains&&run.blood.stains[0].position==stainPosition,"stains stay on the ground throughout the wave");
            zombie=Enemy(run,ViewSide.Back); run.Hit(zombie,1,true);
            Check(run.blood.particles.Count==13&&run.blood.particles.TrueForAll(p=>p.side==ViewSide.Back),"head hit has stronger burst on opposite side");
            run=Fresh(catalog); zombie=Enemy(run,ViewSide.Front,1); run.Hit(zombie,2,true);
            Check(!run.zombies.Contains(zombie)&&run.blood.particles.Count==18,"lethal hit keeps its burst after zombie removal");
            int count=run.blood.particles.Count; run.Hit(zombie,2,true);
            Check(run.blood.particles.Count==count,"removed targets cannot emit twice");
            run.coins.Clear(); run.spawned=run.total; run.Tick(.05f);
            Check(run.state==RunState.Shop&&run.blood.stains.Count==0&&run.blood.particles.Count==0,"wave completion clears all blood");
            run=Fresh(catalog); zombie=Enemy(run); run.Hit(zombie,1,false); run.Start(true);
            Check(run.blood.stains.Count==0&&run.blood.particles.Count==0,"restart clears blood");
            run=Fresh(catalog); zombie=Enemy(run); run.Hit(zombie,1,false); run.state=RunState.Shop; run.NextWave();
            Check(run.blood.stains.Count==0&&run.blood.particles.Count==0,"new wave starts clean");
            var a=Fresh(catalog); var b=Fresh(catalog);
            for(int i=0;i<70;i++)
            {
                var za=Enemy(a,ViewSide.Front,1); var zb=Enemy(b,ViewSide.Front,1);
                a.blood.Burst(za,za.Body.center,true,false); a.Hit(za,2,true); b.Hit(zb,2,true);
            }
            Check(a.coins.Count==b.coins.Count,"visual randomness never changes coin drops");
            Check(a.blood.particles.Count<=BloodEffects.MaxParticles,"particle count bounded during rapid impacts");
            var blood=new BloodEffects(); zombie=Enemy(Fresh(catalog));
            for(int i=0;i<800;i++) { zombie.lateral=i*.25f; zombie.side=i%2==0?ViewSide.Front:ViewSide.Back; blood.Burst(zombie,zombie.Body.center,false,false); }
            Check(blood.stains.Count<=BloodEffects.MaxStains&&blood.stains.Exists(s=>s.side==ViewSide.Back)&&blood.stains.Exists(s=>s.side==ViewSide.Front),"bounded stains preserve both sides");
            Debug.Log("VISUAL_FEEDBACK_TESTS_PASSED: "+checks+" checks");
        }
    }
}
