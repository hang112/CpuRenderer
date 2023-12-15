using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// 透明
    /// </summary>
    public class transparent_shader : Shader
    {
        Color _mainColor;

        public transparent_shader()
        {
            _mainColor = new Color(1, 1, 1, 0.5f);
            blend = true;
            zWrite = false;
        }

        public override Color frag(v2f v)
        {
            return _mainColor;
        }
    }

}
