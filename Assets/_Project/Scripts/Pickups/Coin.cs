using UnityEngine;
namespace ZHold
{
    public class Coin
    {
        public ViewSide side;
        public Vector2 position;
        public float life = 4, age, bounceX;
        public int value;
        public Vector2 ScreenPosition
        {
            get
            {
                float first=Mathf.Clamp01(age/.45f),second=Mathf.Clamp01((age-.45f)/.32f);
                float lift=age<.45f?-28*Mathf.Sin(first*Mathf.PI):age<.77f?-9*Mathf.Sin(second*Mathf.PI):0;
                float side=bounceX*(1-Mathf.Cos(Mathf.Clamp01(age/.77f)*Mathf.PI))*.5f;
                return position+new Vector2(side,lift);
            }
        }
    }
}
