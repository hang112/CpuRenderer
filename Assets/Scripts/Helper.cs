using UnityEngine;

namespace CpuRender
{
    public static class Helper
    {
        /// <summary>
        /// 求点p在三角形abc的重心坐标
        /// </summary>
        public static Vector3 GetBarycentricCoord(Vertex a, Vertex b, Vertex c, Vector2 p, out bool outside)
        {
            var va = new Vector2(a.x, a.y);
            var vb = new Vector2(b.x, b.y);
            var vc = new Vector2(c.x, c.y);

            var v0 = vb - va;
            var v1 = vc - va;
            var v2 = p - va;

            float d00 = v0.x * v0.x + v0.y * v0.y;
            float d01 = v0.x * v1.x + v0.y * v1.y;
            float d11 = v1.x * v1.x + v1.y * v1.y;
            float d20 = v2.x * v0.x + v2.y * v0.y;
            float d21 = v2.x * v1.x + v2.y * v1.y;

            float denom = d00 * d11 - d01 * d01;
            if (Mathf.Abs(denom) < 0.000001f)
            {
                outside = false;
                return -Vector3.one;
            }

            float v = (d11 * d20 - d01 * d21) / denom;
            float w = (d00 * d21 - d01 * d20) / denom;
            float u = 1.0f - v - w;

            //u,v,w均>0,表示在三角内内部
            //1个为0,表示在某条边或其延长线上
            //2个为0,表示在某个顶点上
            outside = u <= 0 || v <= 0 || w <= 0;
            return new Vector3(u, v, w);
        }

        /// <summary>
        /// 重心插值
        /// </summary>
        public static float BarycentricValue(float va, float vb, float vc, Vector3 barycentricCoord)
        {
            return va * barycentricCoord.x + vb * barycentricCoord.y + vc * barycentricCoord.z;
        }

        public static bool IfSatisfyComparison(ECompareFunc comp, byte src, byte dest)
        {
            switch (comp)
            {
                case ECompareFunc.Never:
                    return false;
                case ECompareFunc.Less:
                    return src < dest;
                case ECompareFunc.Equal:
                    return src == dest;
                case ECompareFunc.LEqual:
                    return src <= dest;
                case ECompareFunc.Greater:
                    return src > dest;
                case ECompareFunc.NotEqual:
                    return src != dest;
                case ECompareFunc.GEqual:
                    return src >= dest;
                case ECompareFunc.Always:
                    return true;
            }
            return false;
        }
        public static bool IfSatisfyComparison(ECompareFunc comp, float src, float dest)
        {
            switch (comp)
            {
                case ECompareFunc.Never:
                    return false;
                case ECompareFunc.Less:
                    return src < dest;
                case ECompareFunc.Equal:
                    return src == dest;
                case ECompareFunc.LEqual:
                    return src <= dest;
                case ECompareFunc.Greater:
                    return src > dest;
                case ECompareFunc.NotEqual:
                    return src != dest;
                case ECompareFunc.GEqual:
                    return src >= dest;
                case ECompareFunc.Always:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 更新模板缓冲值
        /// </summary>
        /// <param name="stencilBuffer"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="op"></param>
        /// <param name="v"></param>
        public static void UpdateStencilBufferValue(byte[,] stencilBuffer, int x, int y, EStencilOp op, byte v)
        {
            if (op == EStencilOp.Keep)
                return;

            if (op == EStencilOp.Zero)
            {
                stencilBuffer[x, y] = 0;
            }
            else if (op == EStencilOp.Replace)
            {
                stencilBuffer[x, y] = v;
            }
            else
            {
                var buffer = stencilBuffer[x, y];
                switch (op)
                {
                    case EStencilOp.IncrSat:
                        if (buffer < 255)
                            stencilBuffer[x, y] = ++buffer;
                        break;
                    case EStencilOp.DecrSat:
                        if (buffer > 0)
                            stencilBuffer[x, y] = --buffer;
                        break;
                    case EStencilOp.Invert:
                        stencilBuffer[x, y] = (byte)~buffer;
                        break;
                    case EStencilOp.IncrWrap:
                        stencilBuffer[x, y] = ++buffer;
                        break;
                    case EStencilOp.DecrWrap:
                        stencilBuffer[x, y] = --buffer;
                        break;
                }
            }
        }

        /// <summary>
        /// 数字n第i位是否是1(最低位是0)
        /// </summary>
        public static bool HasMask(int n, int i)
        {
            return (n & (1 << i)) > 0;
        }
    }
}
