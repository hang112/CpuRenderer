using Unity.Mathematics;

namespace CpuRender
{
    public class mask_model : Shader
    {
        public mask_model()
        {
            stencilTest = true;
            stencilRef = 1;
            stencilTestComp = ECompareFunc.Equal;
        }

        public override float4 frag(v2f v)
        {
            GetAllLightsColorOnVert(v, out var lightColor);
            return new float4(lightColor, 1);
        }
    }

}
