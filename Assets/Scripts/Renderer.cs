using System.Collections.Generic;
using UnityEngine;

namespace CpuRender
{
    public class Renderer
    {
        MeshFilter _mesh;
        Shader _shader;

        VAO _vao;
        Matrix4x4 _viewMtx;
        Matrix4x4 _mvpMtx;

        /// <summary>
        /// 是否透明物体
        /// </summary>
        /// <returns></returns>
        public bool IsTransparent()
        {
            return _shader.alphaTest || _shader.blend;
        }
        /// <summary>
        /// 是否不透明物体
        /// </summary>
        /// <returns></returns>
        public bool IsOpaque()
        {
            return !IsTransparent();
        }

        public Renderer(Stage stage, MeshFilter mesh, Shader shader)
        {
            _mesh = mesh;

            _shader = shader;
            _shader.cam = stage.cam;
            _shader.lights = stage.lights;

            _vao = new VAO(mesh);
        }

        public void Render(Matrix4x4 vMtx, Matrix4x4 vpMtx, List<Vertex> verts, List<Triangle> triangles, byte[,] stecilBuffer, float[,] depthBuffer, FrameBuffer frameBuffer)
        {
            var mMtx = _mesh.transform.localToWorldMatrix; ;

            _viewMtx = vMtx;
            _mvpMtx = vpMtx * mMtx;

            _shader.mMtx = mMtx;
            _shader.mvpMtx = _mvpMtx;

            RunVertexShader(verts);
            TriangleSetup(verts, triangles);
            //early-z
            //Z-prepass
            //equal zwriteoff
            //todo
            //Debug.LogError(triangles.Count);
            Rasterization(triangles, stecilBuffer, depthBuffer, frameBuffer);

        }

        void RunVertexShader(List<Vertex> verts)
        {
            foreach (var a2v in _vao.vbo)
            {
                Vector4 svp = a2v.vertex;
                svp.w = 1f;
                svp = _mvpMtx * svp;

                Vertex v = new Vertex();
                //转换成投影坐标
                v.x = (svp.x / svp.w / 2 + 0.5f) * Stage.WIDTH;
                v.y = (svp.y / svp.w / 2 + 0.5f) * Stage.HEIGHT;
                //深度值
                v.z = (svp.z / svp.w / 2 + 0.5f);

                v.o = _shader.vert(a2v);

                verts.Add(v);
            }
        }

        /// <summary>
        /// 三角形组装
        /// </summary>
        void TriangleSetup(List<Vertex> verts, List<Triangle> triangles)
        {
            for (int i = 0, imax = _vao.ebo.Length; i < imax; i += 3)
            {
                var a = verts[_vao.ebo[i]];
                var b = verts[_vao.ebo[i + 1]];
                var c = verts[_vao.ebo[i + 2]];

                bool pass = true;
                if (_shader.cull != ECull.Off)
                {
                    //cull,转换到view空间
                    var va = _viewMtx.MultiplyPoint3x4(a.o.pos);
                    var vb = _viewMtx.MultiplyPoint3x4(b.o.pos);
                    var vc = _viewMtx.MultiplyPoint3x4(c.o.pos);
                    var ab = vb - va;
                    var bc = vc - vb;
                    //abc面的法向量
                    var faceNormal = Vector3.Cross(ab, bc);
                    var dot = Vector3.Dot(faceNormal, vc);

                    switch (_shader.cull)
                    {
                        case ECull.Back:
                            pass = dot > 0;
                            break;
                        case ECull.Front:
                            pass = dot < 0;
                            break;
                    }
                }
                
                if (pass)
                {
                    Triangle t = new Triangle(a, b, c);
                    triangles.Add(t);
                }
            }
        }

        #region rasterization
        /// <summary>
        /// 光栅化
        /// </summary>
        void Rasterization(List<Triangle> triangles, byte[,] stecilBuffer, float[,] depthBuffer, FrameBuffer frameBuffer)
        {
            for (int i = 0; i < triangles.Count; i++)
            {
                var fragList = Rast(triangles[i]);
                foreach (var frag in fragList)
                {
                    //1.fragment shader
                    Color srcColor = _shader.frag(frag.data);

                    //2.alpha test
                    if (_shader.alphaTest)
                    {
                        if (!Helper.IfSatisfyComparison(_shader.alphaTestComp, srcColor.a, _shader.alphaTestValue))
                        {
                            //没有通过alpha测试,像素被抛弃
                            continue;
                        }
                    }

                    //3.stencil test
                    if (_shader.stencilTest)
                    {
                        bool stencilTestPass = Helper.IfSatisfyComparison(_shader.stencilTestComp
                            , (byte)(_shader.stencilRef & _shader.stencilReadMask)
                            , (byte)(stecilBuffer[frag.x, frag.y] & _shader.stencilReadMask));
                        if (!stencilTestPass)
                        {
                            //没有通过模板测试,像素被抛弃
                            //更新模板缓冲值
                            Helper.UpdateStencilBufferValue(stecilBuffer, frag.x, frag.y, _shader.stencilFailOp, (byte)(_shader.stencilRef & _shader.stencilWriteMask));
                            continue;
                        }
                    }

                    //4.z test
                    bool zTestPass = Helper.IfSatisfyComparison(_shader.zTestComp, frag.z, depthBuffer[frag.x, frag.y]);
                    if (!zTestPass)
                    {
                        //没有通过深度测试,像素被抛弃
                        if (_shader.stencilTest)
                        {
                            //更新模板缓冲值
                            Helper.UpdateStencilBufferValue(stecilBuffer, frag.x, frag.y, _shader.stencilZFailOp, (byte)(_shader.stencilRef & _shader.stencilWriteMask));
                        }
                        continue;
                    }

                    if (_shader.zWrite)
                    {
                        //更新深度缓冲值
                        depthBuffer[frag.x, frag.y] = frag.z;
                    }

                    if (_shader.stencilTest)
                    {
                        //更新模板缓冲值
                        Helper.UpdateStencilBufferValue(stecilBuffer, frag.x, frag.y, _shader.stencilPassOp, (byte)(_shader.stencilRef & _shader.stencilWriteMask));
                    }

                    byte colorMask = _shader.colorMask;
                    if (colorMask == 0)
                    {
                        //不写入任何颜色
                        continue;
                    }
                    Color destColor = frameBuffer[frag.x, frag.y];
                    float r = destColor.r;
                    float g = destColor.g;
                    float b = destColor.b;
                    float a = destColor.a;
                    //5.alpha blend
                    if (_shader.blend)
                    {
                        //开启blend
                        float factor1 = srcColor.a;
                        float factor2 = 1 - srcColor.a;

                        if ((colorMask & ColorMask.A) > 0) a = 1;

                        switch (_shader.blendFactor)
                        {
                            //todo
                        }
                        switch (_shader.blendOp)
                        {
                            case EBlendOp.Add:
                                if ((colorMask & ColorMask.R) > 0) r = srcColor.r * factor1 + destColor.r * factor2;
                                if ((colorMask & ColorMask.G) > 0) g = srcColor.g * factor1 + destColor.g * factor2;
                                if ((colorMask & ColorMask.B) > 0) b = srcColor.b * factor1 + destColor.b * factor2;
                                break;
                            case EBlendOp.Sub:
                                break;
                            case EBlendOp.RevSub:
                                break;
                            case EBlendOp.Min:
                                break;
                            case EBlendOp.Max:
                                break;
                        }
                    }
                    else
                    {
                        if ((colorMask & ColorMask.R) > 0) r = srcColor.r;
                        if ((colorMask & ColorMask.G) > 0) g = srcColor.g;
                        if ((colorMask & ColorMask.B) > 0) b = srcColor.b;
                        if ((colorMask & ColorMask.A) > 0) a = srcColor.a;
                    }
                    frameBuffer[frag.x, frag.y] = new Color(r, g, b, a);
                }
            }
        }

        List<Fragment> Rast(Triangle t)
        {
            int xMin = (int)Mathf.Min(t[0].x, t[1].x, t[2].x);
            int xMax = (int)Mathf.Max(t[0].x, t[1].x, t[2].x);
            int yMin = (int)Mathf.Min(t[0].y, t[1].y, t[2].y);
            int yMax = (int)Mathf.Max(t[0].y, t[1].y, t[2].y);

            var fragList = new List<Fragment>((xMax - xMin) * (yMax - yMin));
            for (int x = xMin; x < xMax + 1; x++)
            {
                for (int y = yMin; y < yMax + 1; y++)
                {
                    //过滤屏幕外的点
                    if (x < 0 || x > Stage.WIDTH - 1 || y < 0 || y > Stage.HEIGHT - 1) continue;

                    var px = x + 0.5f;
                    var py = y + 0.5f;

                    //过滤三角形外的点
                    //应该不和cull相关
                    //todo
                    if (_shader.cull == ECull.Back)
                    {
                        if (!IsLeftPoint(t[0], t[1], px, py)) continue;
                        if (!IsLeftPoint(t[1], t[2], px, py)) continue;
                        if (!IsLeftPoint(t[2], t[0], px, py)) continue;
                    }
                    else if (_shader.cull == ECull.Front)
                    {
                        if (IsLeftPoint(t[0], t[1], px, py)) continue;
                        if (IsLeftPoint(t[1], t[2], px, py)) continue;
                        if (IsLeftPoint(t[2], t[0], px, py)) continue;
                    }

                    var frag = new Fragment();
                    frag.x = x;
                    frag.y = y;
                    LerpFragment(t[0], t[1], t[2], frag);
                    fragList.Add(frag);
                }
            }
            return fragList;
        }
        /// <summary>
        /// 点x,y是否在线段ab左侧
        /// </summary>
        bool IsLeftPoint(Vertex a, Vertex b, float x, float y)
        {
            return (a.x - x) * (b.y - y) - (a.y - y) * (b.x - x) <= 0;
        }
        /// <summary>
        /// 重心坐标系插值
        /// </summary>
        void LerpFragment(Vertex a, Vertex b, Vertex c, Fragment frag)
        {
            for (int i = 0; i < 8; i++)
            {
                frag.data[i] = LerpValue(a.o[i], a.x, a.y, b.o[i], b.x, b.y, c.o[i], c.x, c.y, frag.x, frag.y);
            }
            frag.z = LerpValue(a.z, a.x, a.y, b.z, b.x, b.y, c.z, c.x, c.y, frag.x, frag.y);
        }

        float LerpValue(float f1, float x1, float y1, float f2, float x2, float y2, float f3, float x3, float y3, float fragx, float fragy)
        {
            float left = (f1 * x2 - f2 * x1) / (y1 * x2 - y2 * x1) - (f1 * x3 - f3 * x1) / (y1 * x3 - y3 * x1);
            float right = (x2 - x1) / (y1 * x2 - y2 * x1) - (x3 - x1) / (y1 * x3 - y3 * x1);
            float c = left / right;
            left = (f1 * x2 - f2 * x1) / (x2 - x1) - (f1 * x3 - f3 * x1) / (x3 - x1);
            right = (y1 * x2 - y2 * x1) / (x2 - x1) - (y1 * x3 - y3 * x1) / (x3 - x1);
            float b = left / right;
            float a = (f1 - f3 - b * (y1 - y3)) / (x1 - x3);
            return fragx * a + fragy * b + c;
        }
        #endregion
    }
}
