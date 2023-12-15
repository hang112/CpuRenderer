using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// 边缘光
    /// </summary>
    public class rim_shader : Shader
    {
        CRTexture _mainTex;
        Color _mainColor;

        Color _rimColor;
        float _rimPower;

        public rim_shader()
        {
            _mainTex = new CRTexture(Resources.Load<Texture2D>("Explorer/Explorer"));
            _mainColor = new Color(0.2f, 0.2f, 0.2f);

            _rimColor = new Color(1, 0.77f, 0.77f);
            _rimPower = 0.55f;

            blend = true;
        }

        public override Color frag(v2f v)
        {
            var color = _mainTex.tex2D(v.uv);
            color *= _mainColor;

            var viewDir = WorldSpaceCameraPos - v.worldPos;
            var rim = 1 - Mathf.Max(0, Vector3.Dot(Vector3.Normalize(v.normal), Vector3.Normalize(viewDir)));
            var rimPower = Mathf.Pow(rim, 1 / _rimPower);

            color += _rimColor * rimPower;

            return color;
        }
    }

}
