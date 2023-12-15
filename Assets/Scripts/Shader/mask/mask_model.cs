using UnityEngine;

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

        public override Color frag(v2f v)
        {
            GetLightRgbColorOnVert(v, out var r, out var g, out var b);
            return new Color(r, g, b);
        }
    }

}
