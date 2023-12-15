using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// 光照(方向光和点光源)
    /// 没有反射
    /// </summary>
    public class light_shader : Shader
    {
        public override Color frag(v2f v)
        {
            GetLightRgbColorOnVert(v, out var r, out var g, out var b);
            return new Color(r, g, b);
        }
    }
}
