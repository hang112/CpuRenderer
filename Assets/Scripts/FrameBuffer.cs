using System.IO;
using UnityEngine;

namespace CpuRender
{
    public class FrameBuffer
    {
        public readonly int width;
        public readonly int height;

        Color[] cols;

        public FrameBuffer(int width, int height)
        {
            cols = new Color[width * height];
            this.width = width;
            this.height = height;
        }

        public Color this[int x, int y]
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

        public Color this[float x, float y]
        {
            get
            {
                int m = (int)(x * width + 0.49f);
                int n = (int)(y * height + 0.49f);
                return this[m, n];
            }
        }

        public void Clear(Color c)
        {
            for (int i = 0, imax = cols.Length; i < imax; i++)
            {
                cols[i] = c;
            }
        }

        public void Save(string path)
        {
            Texture2D outT = new Texture2D(width, height, TextureFormat.RGBA32, false);
            outT.SetPixels(cols);
            byte[] bytes = outT.EncodeToPNG();

            File.WriteAllBytes(path, bytes);
            Debug.Log($"save texture 2 {path}");

            Texture2D.DestroyImmediate(outT);
        }
    }
}
