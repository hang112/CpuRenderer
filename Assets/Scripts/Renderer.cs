using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CpuRender
{
    public class Renderer
    {
        MeshFilter _mesh;
        Shader _shader;

        VAO _vao;
        Matrix4x4 _vMtx;
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
            _shader.camTrans = stage.cam.transform;
            _shader.lights = stage.lights;

            _vao = new VAO(mesh);
        }

        public void Render(Matrix4x4 vMtx, Matrix4x4 vpMtx, List<Vertex> verts, List<Triangle> triangles, byte[,] stecilBuffer, float[,] depthBuffer, FrameBuffer frameBuffer)
        {
            var mMtx = _mesh.transform.localToWorldMatrix; ;

            _vMtx = vMtx;
            _mvpMtx = vpMtx * mMtx;

            _shader.MATRIX_M = mMtx;
            _shader.MATRIX_MVP = _mvpMtx;

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
                Vertex v = new Vertex();

                v.o = _shader.vert(a2v);
                var svp = v.o.vertex;
                //齐次除法得到ndc坐标
                Vector4 ndc = new Vector4(svp.x / svp.w, svp.y / svp.w, svp.z / svp.w, 1f / svp.w);
                //屏幕映射得到屏幕坐标
                v.x = (ndc.x / 2 + 0.5f) * Stage.WIDTH;
                v.y = (ndc.y / 2 + 0.5f) * Stage.HEIGHT;
                //深度值
                v.z = ndc.z / 2 + 0.5f;

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
                    //cull
                    //需要转换到view空间
                    var va = _vMtx.MultiplyPoint3x4(a.o.worldPos);
                    var vb = _vMtx.MultiplyPoint3x4(b.o.worldPos);
                    var vc = _vMtx.MultiplyPoint3x4(c.o.worldPos);
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
            int fragLen = 0;
            for (int i = 0; i < triangles.Count; i++)
            {
                var fragList = Rast(triangles[i]);
                fragLen += fragList.Count;
                foreach (var frag in fragList)
                {
                    //1.fragment shader
                    float4 srcColor = _shader.frag(frag.i);

                    //2.alpha test
                    if (_shader.alphaTest)
                    {
                        if (!Helper.IfSatisfyComparison(_shader.alphaTestComp, srcColor.w, _shader.alphaTestValue))
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
                    float4 destColor = frameBuffer[frag.x, frag.y];
                    float r = destColor.x;
                    float g = destColor.y;
                    float b = destColor.z;
                    float a = destColor.w;
                    //5.alpha blend
                    if (_shader.blend)
                    {
                        //开启blend
                        float factor1 = srcColor.w;
                        float factor2 = 1 - srcColor.w;

                        if ((colorMask & ColorMask.A) > 0) a = 1;

                        switch (_shader.blendFactor)
                        {
                            //todo
                        }
                        switch (_shader.blendOp)
                        {
                            case EBlendOp.Add:
                                if ((colorMask & ColorMask.R) > 0) r = srcColor.x * factor1 + destColor.x * factor2;
                                if ((colorMask & ColorMask.G) > 0) g = srcColor.y * factor1 + destColor.y * factor2;
                                if ((colorMask & ColorMask.B) > 0) b = srcColor.z * factor1 + destColor.z * factor2;
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
                        if ((colorMask & ColorMask.R) > 0) r = srcColor.x;
                        if ((colorMask & ColorMask.G) > 0) g = srcColor.y;
                        if ((colorMask & ColorMask.B) > 0) b = srcColor.z;
                        if ((colorMask & ColorMask.A) > 0) a = srcColor.w;
                    }
                    frameBuffer[frag.x, frag.y] = new float4(r, g, b, a);
                }
            }
            //Debug.LogError($"frag count = {fragLen}");
        }

        const float threshold = -0.000001f;
        List<Fragment> Rast(Triangle t)
        {
            //clip
            int xMin = Mathf.Clamp((int)Mathf.Min(t[0].x, t[1].x, t[2].x), 0, Stage.WIDTH - 1);
            int xMax = Mathf.Clamp((int)Mathf.Max(t[0].x, t[1].x, t[2].x), 0, Stage.WIDTH - 1);
            int yMin = Mathf.Clamp((int)Mathf.Min(t[0].y, t[1].y, t[2].y), 0, Stage.HEIGHT - 1);
            int yMax = Mathf.Clamp((int)Mathf.Max(t[0].y, t[1].y, t[2].y), 0, Stage.HEIGHT - 1);

            var fragList = new List<Fragment>((xMax - xMin) * (yMax - yMin));
            for (int x = xMin; x <= xMax; x++)
            {
                for (int y = yMin; y <= yMax; y++)
                {
                    Vertex a = t[0];
                    Vertex b = t[1];
                    Vertex c = t[2];
                    var barycentricCoord = Helper.GetBarycentricCoord(a, b, c, new Vector2(x, y), out var outside);
                    if (outside)
                        continue;

                    if (barycentricCoord.x < threshold || barycentricCoord.y < threshold || barycentricCoord.z < threshold)
                        continue;

                    //w 透视矫正
                    //todo

                    var frag = new Fragment();
                    frag.x = x;
                    frag.y = y;
                    //重心差值
                    frag.z = Helper.BarycentricValue(a.z, b.z, c.z, barycentricCoord);
                    for (int i = 0; i < 8; i++)
                    {
                        frag.i[i] = Helper.BarycentricValue(a.o[i], b.o[i], c.o[i], barycentricCoord);
                    }

                    fragList.Add(frag);
                }
            }
            return fragList;
        }
        #endregion
    }
}
