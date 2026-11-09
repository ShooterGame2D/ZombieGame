using UnityEngine;

namespace ZHold
{
    public enum ViewSide { Front, Right, Back, Left }

    // The only conversion from simulation depth to the 1600 x 900 virtual screen.
    public static class DepthProjector
    {
        static float horizon=490;
        public static void Configure(MapTheme theme)
        {
            horizon=400;
        }
        public static Vector2 Project(float lateral, float distance)
        {
            float near = 1f - Mathf.Clamp01(distance);
            return new Vector2(800 + lateral * Mathf.Lerp(270, 660, near), Mathf.Lerp(horizon, 760, near));
        }
        public static float Scale(float distance) => Mathf.Lerp(1.2f, .25f, Mathf.Clamp01(distance));
        public static int SortingOrder(float distance) => Mathf.RoundToInt((1 - distance) * 1000);
    }
}
