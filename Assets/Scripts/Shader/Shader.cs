using UnityEngine;

namespace CpuRender
{
    public abstract class Shader
    {
        public Transform camTrans;
        public Light[] lights;

        /// <summary>
        /// model(local -> world)
        /// </summary>
        public Matrix4x4 MATRIX_M;
        /// <summary>
        /// model view projection
        /// </summary>
        public Matrix4x4 MATRIX_MVP;

        /// <summary>
        /// 是否受光照影响
        /// </summary>
        public bool lighting = true;

        /// <summary>
        /// 面剔除
        /// </summary>
        public ECull cull = ECull.Back;

        /// <summary>
        /// 是否开启alpha测试
        /// </summary>
        public bool alphaTest = false;
        public ECompareFunc alphaTestComp = ECompareFunc.Always;
        public float alphaTestValue;

        /// <summary>
        /// 是否开启模板测试
        /// </summary>
        public bool stencilTest = false;
        /// <summary>
        /// 当前片元模板参考值
        /// </summary>
        public byte stencilRef = 0;
        /// <summary>
        /// 读取时与Ref值进行&操作
        /// </summary>
        public byte stencilReadMask = 255;
        /// <summary>
        /// 写入时与Ref值进行&操作
        /// </summary>
        public byte stencilWriteMask = 255;
        /// <summary>
        /// 模板测试的比较方法(默认始终通过)
        /// </summary>
        public ECompareFunc stencilTestComp = ECompareFunc.Always;
        /// <summary>
        /// 模板测试未通过时的操作
        /// </summary>
        public EStencilOp stencilFailOp = EStencilOp.Keep;
        /// <summary>
        /// 模板测试通过,深度测试未通过时的操作
        /// </summary>
        public EStencilOp stencilZFailOp = EStencilOp.Keep;
        /// <summary>
        /// 模板测试和深度测试均通过时的操作
        /// </summary>
        public EStencilOp stencilPassOp = EStencilOp.Keep;

        /// <summary>
        /// 深度测试比较方法(默认小于等于)
        /// </summary>
        public ECompareFunc zTestComp = ECompareFunc.LEqual;
        /// <summary>
        /// 是否开启深度写入
        /// 不透明物体一般开启,透明物体一般关闭
        /// </summary>
        public bool zWrite = true;

        /// <summary>
        /// 是否开启混合
        /// </summary>
        public bool blend = false;
        public EBlendFactor blendFactor = EBlendFactor.SrcAlpha_OneMinusSrcAlpha;
        public EBlendOp blendOp = EBlendOp.Add;

        /// <summary>
        /// 0,或者r,g,b,a的任意组合,默认为rgba
        /// </summary>
        public byte colorMask = 255;

        public virtual v2f vert(a2v v)
        {
            v2f o = new v2f();

            o.vertex = ObjectToClipPos(v.vertex);
            o.worldPos = ObjectToWorldPos(v.vertex);
            o.normal = MATRIX_M.MultiplyVector(v.normal);
            o.uv = v.uv;

            return o;
        }

        public abstract Color frag(v2f i);

        #region shader中的常用方法
        protected Vector4 ObjectToClipPos(Vector3 pos)
        {
            Vector4 clipPos = pos;
            clipPos.w = 1f;

            clipPos = MATRIX_MVP * clipPos;
            return clipPos;
        }
        protected Vector3 ObjectToWorldPos(Vector3 pos)
        {
            return MATRIX_M.MultiplyPoint3x4(pos);
        }

        protected Vector3 WorldSpaceCameraPos { get { return camTrans.position; } }



        protected void GetLightRgbColorOnVert(v2f v, out float r, out float g, out float b)
        {
            r = 0f;
            g = 0f;
            b = 0f;
            if (!lighting || lights == null)
                return;

            float atten = 0f;
            foreach (var light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    atten = Vector3.Dot(-light.transform.forward, Vector3.Normalize(v.normal));
                    atten = Mathf.Max(0, atten);
                }
                else if (light.type == LightType.Point)
                {
                    var disSqr = (light.transform.position - v.worldPos).sqrMagnitude;
                    if (disSqr > light.range * light.range)
                        continue;

                    atten = light.intensity / disSqr;
                    atten *= Vector3.Dot(Vector3.Normalize(light.transform.position - v.worldPos), Vector3.Normalize(v.normal));
                }
                r += light.color.r * atten;
                g += light.color.g * atten;
                b += light.color.b * atten;
            }
        }
        #endregion
    }
}
