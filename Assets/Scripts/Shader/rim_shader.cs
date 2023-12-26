using Unity.Mathematics;
using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// 边缘光
    /// </summary>
    public class rim_shader : Shader
    {
        CRTexture _mainTex;
        float4 _mainColor;

        float4 _rimColor;
        float _rimPower;

        public rim_shader()
        {
            _mainTex = new CRTexture(Resources.Load<Texture2D>("Explorer/Explorer"));
            _mainColor = new float4(0.2f, 0.2f, 0.2f, 1);

            _rimColor = new float4(1, 0.77f, 0.77f, 1);
            _rimPower = 0.55f;

            blend = true;
        }

        public override float4 frag(v2f v)
        {
            var texColor = _mainTex.tex2D(v.uv);
            texColor *= _mainColor;

            var viewDir = WorldSpaceCameraPos - v.worldPos;
            var rim = 1 - Mathf.Max(0, Vector3.Dot(Vector3.Normalize(v.normal), Vector3.Normalize(viewDir)));
            var rimPower = Mathf.Pow(rim, 1 / _rimPower);

            texColor += _rimColor * rimPower;

            return texColor;
        }
    }

}
