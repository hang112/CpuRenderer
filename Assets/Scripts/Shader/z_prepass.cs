using UnityEngine;

namespace CpuRender
{
    public class z_prepass : Shader
    {
        public z_prepass()
        {
            zWrite = true;
            colorMask = 0;
        }

        public override Color frag(v2f v)
        {
            return Color.black;
        }
    }
}
