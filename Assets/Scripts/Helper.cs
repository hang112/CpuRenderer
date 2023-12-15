namespace CpuRender
{
    public static class Helper
    {
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
