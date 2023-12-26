using Unity.Mathematics;
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

        public override float4 frag(v2f v)
        {
            //采样
            var texColor = _mainTex.tex2D(v.uv);

            //获取光照信息
            GetAllLightsColorOnVert(v, out var lightColor);

            //最终颜色=纹理颜色*光照颜色
            return new float4(lightColor * texColor.xyz, 1);
        }
    }

}
