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
        Color[] _colors;
        public readonly int width;
        public readonly int height;

        public EWrapMode wrapMode = EWrapMode.Repeat;

        public CRTexture(Texture2D t)
        {
            _colors = t.GetPixels();
            width = t.width;
            height = t.height;
        }

        public Color this[int x, int y]
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

        public Color this[float x, float y]
        {
            get
            {
                int m = (int)(x * width + 0.49f);
                int n = (int)(y * height + 0.49f);
                return this[m, n];
            }
        }

        public Color tex2D(Vector2 uv)
        {
            return this[uv.x, uv.y];
        }
    }
}
