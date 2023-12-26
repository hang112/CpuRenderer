using Unity.Mathematics;
using UnityEngine;

namespace CpuRender
{
    public enum EWrapMode
    {
        Repeat,
        Clamp,
        Mirror,
    }

    public class CRTexture
    {
        float4[] _colors;
        public readonly int width;
        public readonly int height;

        public EWrapMode wrapMode = EWrapMode.Repeat;

        public CRTexture(Texture2D t)
        {
            var tmp = t.GetPixels();
            _colors = new float4[tmp.Length];
            for (int i = 0, imax = tmp.Length; i < imax; i++) 
            {
                var c = tmp[i];
                _colors[i] = new float4(c.r, c.g, c.b, c.a);
            }
            width = t.width;
            height = t.height;
        }

        public float4 this[int x, int y]
        {
            get
            {
                //这里要取wrapMode
                //todo

                if (x > width - 1)
                    x = width - 1;
                else if (x < 0)
                    x = 0;

                if (y > height - 1)
                    y = height - 1;
                else if (y < 0)
                    y = 0;

                return _colors[y * width + x];
            }
            set
            {
                _colors[y * width + x] = value;
            }
        }

        public float4 this[float x, float y]
        {
            get
            {
                int m = (int)(x * width + 0.49f);
                int n = (int)(y * height + 0.49f);
                return this[m, n];
            }
        }

        public float4 tex2D(Vector2 uv)
        {
            return this[uv.x, uv.y];
        }
    }
}
