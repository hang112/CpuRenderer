namespace CpuRender
{
    /// <summary>
    /// 剔除
    /// </summary>
    public enum ECull
    {
        /// <summary>
        /// 背面剔除
        /// </summary>
        Back = 0,
        /// <summary>
        /// 正面剔除
        /// </summary>
        Front,
        /// <summary>
        /// 不剔除
        /// </summary>
        Off,
    }

    /// <summary>
    /// 混合因子
    /// </summary>
    public enum EBlendFactor
    {
        SrcAlpha_OneMinusSrcAlpha = 0,
        Zero_One,
        SrcAlpha_One,
        One_OneMinusSrcAlpha,
        SrcAlpha_OneMinusDstAlpha,
        DstColor_Zero,
    }
    /// <summary>
    /// 混合运算
    /// </summary>
    public enum EBlendOp
    {
        Add = 0,
        Sub,
        RevSub,
        Min,
        Max,
    }

    public enum ECompareFunc
    {
        Never = 0,
        Less,
        Equal,
        LEqual,
        Greater,
        NotEqual,
        GEqual,
        Always,
    }
    public enum EStencilOp
    {
        /// <summary>
        /// 保持模板缓冲区的当前内容
        /// </summary>
        Keep = 0,
        /// <summary>
        /// 将 0 写入模板缓冲区
        /// </summary>
        Zero,
        /// <summary>
        /// 将参考值写入缓冲区
        /// </summary>
        Replace,
        /// <summary>
        /// 增缓冲区中的当前值。如果该值已经是 255，则保持为 255
        /// </summary>
        IncrSat,
        /// <summary>
        /// 递减缓冲区中的当前值。如果该值已经是 0，则保持为 0
        /// </summary>
        DecrSat,
        /// <summary>
        /// 将缓冲区中当前值的所有位求反
        /// </summary>
        Invert,
        /// <summary>
        /// 递增缓冲区中的当前值。如果该值已经是 255，则变为 0
        /// </summary>
        IncrWrap,
        /// <summary>
        /// 递减缓冲区中的当前值。如果该值已经是 0，则变为 255
        /// </summary>
        DecrWrap,
    }
    /// <summary>
    /// 渲染队列
    /// </summary>
    public class RenderQueue
    {
        public const int Background = 1000;
        /// <summary>
        /// 默认(非透明)
        /// </summary>
        public const int Geometry = 2000;
        public const int AlphaTest = 2450;
        /// <summary>
        /// 透明
        /// </summary>
        public const int Transparent = 3000;
        public const int Overlay = 4000;
    }

    public class ColorMask
    {
        public const byte R = 1;
        public const byte G = 2;
        public const byte B = 4;
        public const byte A = 8;
    }

}
