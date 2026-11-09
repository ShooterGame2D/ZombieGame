using UnityEngine;
namespace ZHold
{
    public static class PixelArtCatalog
    {
        // Exact atlas bounds, measured from alpha, keep adjacent sprites out of each frame.
        static readonly Vector4[] zombies={new Vector4(33,22,388,434),new Vector4(388,15,710,434),new Vector4(727,26,1032,434),new Vector4(1052,22,1381,435),new Vector4(1387,8,1758,434),new Vector4(40,445,368,868),new Vector4(388,446,710,867),new Vector4(734,446,1034,867),new Vector4(1058,446,1383,868),new Vector4(1396,445,1742,868)};
        static readonly Vector4[] heroes={new Vector4(22,41,454,434),new Vector4(467,58,886,438),new Vector4(901,36,1322,447),new Vector4(1345,41,1751,440),new Vector4(21,438,436,862),new Vector4(467,511,866,856),new Vector4(884,447,1339,866),new Vector4(1354,464,1751,859)};
        static Rect UV(Vector4 b)=>new Rect(b.x/1774f,1-b.w/887f,(b.z-b.x)/1774f,(b.w-b.y)/887f);
        public static Rect ZombieUV(int index)=>UV(zombies[Mathf.Clamp(index,0,9)]);
        public static float ZombieAspect(int index) {var b=zombies[Mathf.Clamp(index,0,9)];return (b.z-b.x)/(b.w-b.y);}
        public static Rect HeroUV(int index)=>UV(heroes[Mathf.Clamp(index,0,7)]);
    }
}
