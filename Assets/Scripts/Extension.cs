using UnityEngine;

namespace CpuRender
{
    public static class Extension
    {
        public static Color AddExceptAlpha(this Color c, Color other)
        {
            return new Color(c.r + other.r, c.g + other.g, c.b + other.b, c.a);
        }
        public static Color MulExceptAlpha(this Color c, float factor)
        {
            return new Color(c.r * factor, c.g * factor, c.b * factor, c.a);
        }
    }
}
