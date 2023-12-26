using System.IO;
using Unity.Mathematics;
using UnityEngine;

namespace CpuRender
{
    public class FrameBuffer
    {
        public readonly int width;
        public readonly int height;

        float4[] cols;

        public FrameBuffer(int width, int height)
        {
            cols = new float4[width * height];
            this.width = width;
            this.height = height;
        }

        public float4 this[int x, int y]
        {
            get
            {
                if (x > width - 1) x = width - 1;
                if (y > height - 1) y = height - 1;
                return cols[y * width + x];
            }
            set
            {
                cols[y * width + x] = value;
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

        public void Clear(float4 c)
        {
            for (int i = 0, imax = cols.Length; i < imax; i++)
            {
                cols[i] = c;
            }
        }

        public void Save(string path)
        {
            Texture2D outT = new Texture2D(width, height, TextureFormat.RGBA32, false);

            var colors = new Color[cols.Length];
            for (int i = 0, imax = cols.Length; i < imax; i++)
            {
                var c = cols[i];
                colors[i] = new Color(c.x, c.y, c.z, c.w);
            }

            outT.SetPixels(colors);
            byte[] bytes = outT.EncodeToPNG();

            File.WriteAllBytes(path, bytes);
            Debug.Log($"save texture 2 {path}");

            Texture2D.DestroyImmediate(outT);
        }
    }
}
