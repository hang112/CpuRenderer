using UnityEngine;

namespace CpuRender
{
    /// <summary>
    /// application to vertex
    /// </summary>
    public struct a2v
    {
        public Vector3 vertex;
        public Vector3 normal;
        public Vector2 uv;
    }

    /// <summary>
    /// vertex to fragment
    /// </summary>
    public struct v2f
    {
        /// <summary>
        /// clip pos
        /// </summary>
        public Vector4 vertex;

        public Vector3 worldPos;
        public Vector3 normal;
        public Vector2 uv;
        public float this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return worldPos.x;
                    case 1:
                        return worldPos.y;
                    case 2:
                        return worldPos.z;
                    case 3:
                        return normal.x;
                    case 4:
                        return normal.y;
                    case 5:
                        return normal.z;
                    case 6:
                        return uv.x;
                    case 7:
                        return uv.y;
                }
                return 0.0f;
            }
            set
            {
                switch (i)
                {
                    case 0:
                        worldPos.x = value;
                        break;
                    case 1:
                        worldPos.y = value;
                        break;
                    case 2:
                        worldPos.z = value;
                        break;
                    case 3:
                        normal.x = value;
                        break;
                    case 4:
                        normal.y = value;
                        break;
                    case 5:
                        normal.z = value;
                        break;
                    case 6:
                        uv.x = value;
                        break;
                    case 7:
                        uv.y = value;
                        break;
                }
            }
        }
    }

    /// <summary>
    /// vertex array object
    /// </summary>
    public class VAO
    {
        /// <summary>
        /// vertex buffer object
        /// </summary>
        public a2v[] vbo;
        /// <summary>
        /// element buffer object
        /// </summary>
        public int[] ebo;

        public VAO(MeshFilter mf)
        {
            Mesh m = mf.sharedMesh;
            vbo = new a2v[m.vertexCount];
            for (int i = 0; i < m.vertexCount; i++)
            {
                a2v a = new a2v();
                a.vertex = m.vertices[i];
                a.normal = m.normals[i];
                a.uv = m.uv[i];
                vbo[i] = a;
            }
            ebo = m.triangles;
        }
    }

    /// <summary>
    /// 顶点数据
    /// VertexShader的输出
    /// </summary>
    public class Vertex
    {
        /// <summary>
        /// 像素点坐标
        /// </summary>
        public float x;
        public float y;
        /// <summary>
        /// 深度缓冲
        /// </summary>
        public float z;

        public v2f o;
    }

    /// <summary>
    /// 三角形数据
    /// </summary>
    public class Triangle
    {
        readonly Vertex[] verts;
        public Triangle(Vertex a, Vertex b, Vertex c)
        {
            verts = new Vertex[3] { a, b, c };
        }

        public Vertex this[int index] { get { return verts[index]; } }
    }

    /// <summary>
    /// fragment数据
    /// </summary>
    public class Fragment
    {
        /// <summary>
        /// 所在行
        /// </summary>
        public int x;
        /// <summary>
        /// 所在列
        /// </summary>
        public int y;
        /// <summary>
        /// 深度缓冲
        /// </summary>
        public float z;

        public v2f data;

        public float fx { get { return x + 0.5f; } }
        public float fy { get { return y + 0.5f; } }
    }
}
