using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// 纹理采样
    /// </summary>
    public class map_shader : Shader
    {
        CRTexture _mainTex;

        public map_shader()
        {
            _mainTex = new CRTexture(Resources.Load<Texture2D>("Explorer/Explorer"));
        }

        public override Color frag(v2f v)
        {
            //采样
            var color = _mainTex.tex2D(v.uv);

            //获取光照信息
            GetLightRgbColorOnVert(v, out var lr, out var lg, out var lb);

            //最终颜色=纹理颜色*光照颜色
            var r = color.r * lr;
            var g = color.g * lg;
            var b = color.b * lb;

            return new Color(r, g, b);
        }
    }

}
