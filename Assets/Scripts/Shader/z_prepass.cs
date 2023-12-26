using Unity.Mathematics;

namespace CpuRender
{
    public class z_prepass : Shader
    {
        public z_prepass()
        {
            zWrite = true;
            colorMask = 0;
        }

        public override float4 frag(v2f v)
        {
            return float4.zero;
        }
    }
}
