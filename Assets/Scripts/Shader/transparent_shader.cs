using Unity.Mathematics;

namespace CpuRender
{
    /// <summary>
    /// 透明
    /// </summary>
    public class transparent_shader : Shader
    {
        float4 _mainColor;

        public transparent_shader()
        {
            _mainColor = new float4(1, 1, 1, 0.5f);
            blend = true;
            zWrite = false;
        }

        public override float4 frag(v2f v)
        {
            return _mainColor;
        }
    }

}
