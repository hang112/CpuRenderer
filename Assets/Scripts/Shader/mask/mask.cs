using Unity.Mathematics;

namespace CpuRender
{
    public class mask : Shader
    {
        public mask()
        {
            stencilTest = true;
            stencilRef = 1;
            stencilTestComp = ECompareFunc.Always;
            stencilPassOp = EStencilOp.Replace;

            zWrite = false;
            colorMask = 0;
        }

        public override float4 frag(v2f v)
        {
            return float4.zero;
        }
    }

}
