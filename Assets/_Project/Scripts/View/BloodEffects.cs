using System.Collections.Generic;
using UnityEngine;

namespace ZombieAttack
{
    public class BloodEffects
    {
        public const int MaxParticles=256,MaxStains=384;
        public class Drop
        {
            public ViewSide side;
            public Vector2 origin,velocity,position;
            public float age,gravity,groundY,size;
            public int shade;
            public bool leavesStain;
        }
        public class Stain
        {
            public ViewSide side;
            public Vector2 position;
            public float size;
            public int shape;
        }
        public readonly List<Drop> particles=new List<Drop>();
        public readonly List<Stain> stains=new List<Stain>();
        // Effects cannot change the simulation's spawn/coin-drop random sequence.
        readonly System.Random random=new System.Random(84519);
        float Range(float min,float max)=>Mathf.Lerp(min,max,(float)random.NextDouble());
        public void Burst(Zombie zombie,Vector2 impact,bool head,bool lethal)
        {
            float scale=zombie.Size;
            AddStain(zombie.side,zombie.Position+new Vector2(Range(-10,10)*scale,2*scale),Range(6,10)*scale);
            int count=lethal?18:head?13:9;
            for(int i=0;i<count;i++)
            {
                if(particles.Count>=MaxParticles) particles.RemoveAt(0);
                particles.Add(new Drop { side=zombie.side,origin=impact,position=impact,
                    velocity=new Vector2(Range(-145,145),Range(-185,-55))*scale,
                    gravity=750*scale,groundY=zombie.Position.y+Range(-3,8)*scale,
                    size=Range(2.5f,5)*scale,shade=random.Next(3),leavesStain=i%3==0 });
            }
        }
        public void Tick(float dt)
        {
            if(dt<=0) return;
            for(int i=particles.Count-1;i>=0;i--)
            {
                var drop=particles[i]; drop.age+=dt;
                drop.position=drop.origin+drop.velocity*drop.age+Vector2.up*(.5f*drop.gravity*drop.age*drop.age);
                if(drop.position.y>=drop.groundY||drop.age>2)
                {
                    if(drop.leavesStain) AddStain(drop.side,new Vector2(drop.position.x,drop.groundY),drop.size*1.8f);
                    particles.RemoveAt(i);
                }
            }
        }
        void AddStain(ViewSide side,Vector2 position,float size)
        {
            Stain nearest=null; float nearestDistance=float.MaxValue; int sameSideCount=0;
            foreach(var stain in stains)
            {
                if(stain.side!=side) continue;
                sameSideCount++;
                float distance=(stain.position-position).sqrMagnitude;
                if(distance<nearestDistance) { nearest=stain; nearestDistance=distance; }
            }
            if(nearest!=null&&(nearestDistance<144||sameSideCount>=MaxStains/2))
            {
                nearest.size=Mathf.Min(32,nearest.size+size*.18f); return;
            }
            if(stains.Count<MaxStains) stains.Add(new Stain { side=side,position=position,size=size,shape=random.Next(4) });
        }
        public void Clear() { particles.Clear(); stains.Clear(); }
    }
}
