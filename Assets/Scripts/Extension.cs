using Unity.Mathematics;
using UnityEngine;

namespace CpuRender
{
    public static class Extension
    {
        public static float3 rgb(this Light light)
        {
            var c = light.color;
            return new float3(c.r, c.g, c.b);
        }
    }
}
