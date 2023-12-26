using Unity.Mathematics;

namespace CpuRender
{
    /// <summary>
    /// 光照(方向光和点光源)
    /// 没有反射
    /// </summary>
    public class light_shader : Shader
    {
        public override float4 frag(v2f v)
        {
            GetAllLightsColorOnVert(v, out var lightColor);
            return new float4(lightColor, 1);
        }
    }
}
