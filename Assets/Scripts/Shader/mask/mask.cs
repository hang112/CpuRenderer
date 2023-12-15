using UnityEngine;

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

        public override Color frag(v2f v)
        {
            return Color.white;
        }
    }

}
