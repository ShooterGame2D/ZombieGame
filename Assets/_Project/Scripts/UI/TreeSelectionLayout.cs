using UnityEngine;
namespace ZHold
{
    public static class TreeSelectionLayout
    {
        public static Rect Button(int lane) => new Rect(210+lane*400,708,380,64);
        public static int HitTest(Vector2 pointer)
        {
            for(int lane=0;lane<3;lane++)if(Button(lane).Contains(pointer))return lane;
            return -1;
        }
    }
}
