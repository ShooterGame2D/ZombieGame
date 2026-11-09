using UnityEngine;
using static ZHold.Draw2D;

namespace ZHold
{
    public static class BloodRenderer
    {
        static readonly Color Dark=Hex("611421"),Red=Hex("A21C2C"),Fresh=Hex("DF3446");
        public static void Ground(BloodEffects blood,ViewSide side)
        {
            foreach(var stain in blood.stains)
            {
                if(stain.side!=side) continue;
                float x=stain.position.x,y=stain.position.y,r=stain.size;
                Ellipse(x-r,y-r*.18f,r*2,r*.58f,Dark);
                Rect(x-r*.65f,y-r*.18f,r*1.2f,r*.28f,Red);
                float direction=stain.shape%2==0?1:-1;
                Rect(x+direction*r*.95f,y+r*.2f,r*.4f,r*.22f,Dark);
                Rect(x-direction*r*.65f,y-r*.35f,r*.3f,r*.2f,Red);
            }
        }
        public static void Air(BloodEffects blood,ViewSide side)
        {
            foreach(var drop in blood.particles)
            {
                if(drop.side!=side) continue;
                var color=drop.shade==0?Fresh:drop.shade==1?Red:Dark;
                float size=Mathf.Max(2,drop.size);
                var velocity=drop.velocity+Vector2.up*drop.gravity*drop.age;
                var tail=drop.position-velocity.normalized*Mathf.Min(12,size*2);
                Line(tail,drop.position,size*.65f,color);
                Rect(drop.position.x-size*.5f,drop.position.y-size*.5f,size,size,color);
            }
        }
    }
}
