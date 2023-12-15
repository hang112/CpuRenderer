using System.Collections.Generic;
using UnityEngine;

namespace CpuRender
{
    public class Stage
    {
        public const int WIDTH = 1280;
        public const int HEIGHT = 720;

        Color _stageColor;

        Camera _cam;
        public Camera cam => _cam;
        Light[] _lights;
        public Light[] lights => _lights;
        List<Renderer> _renderers;

        /// <summary>
        /// 帧缓冲区
        /// </summary>
        FrameBuffer _frameBuffer;
        /// <summary>
        /// 模板缓冲区
        /// </summary>
        byte[,] _stecilBuffer = new byte[WIDTH, HEIGHT];
        /// <summary>
        /// 深度缓冲区
        /// </summary>
        float[,] _depthBuffer = new float[WIDTH, HEIGHT];

        List<Vertex> _verts;
        List<Triangle> _triangles;

        public Stage(Camera cam, Light[] ligths)
        {
            _stageColor = cam.backgroundColor;
            _stageColor.a = 1f;

            _cam = cam;
            _lights = ligths;
            _renderers = new List<Renderer>();

            _verts = new List<Vertex>(1024 * 4);
            _triangles = new List<Triangle>(1024 * 4);

            _frameBuffer = new FrameBuffer(WIDTH, HEIGHT);
        }

        public void AddRenderer(Renderer r)
        {
            _renderers.Add(r);
        }

        public void DrawFrame(string path)
        {
            //清空缓冲区
            _frameBuffer.Clear(_stageColor);
            for (int x = 0; x < WIDTH; x++)
            {
                for (int y = 0; y < HEIGHT; y++)
                {
                    _stecilBuffer[x, y] = 0;
                    _depthBuffer[x, y] = float.MaxValue;
                }
            }

            var vMtx = _cam.worldToCameraMatrix;
            var vpMtx = _cam.projectionMatrix * _cam.worldToCameraMatrix;
            foreach (var renderer in _renderers) 
            {
                renderer.Render(vMtx, vpMtx, _verts, _triangles, _stecilBuffer, _depthBuffer, _frameBuffer);
                _verts.Clear();
                _triangles.Clear();
            }

            if (!string.IsNullOrEmpty(path)) 
            {
                _frameBuffer.Save(path);
            }
        }
    }
}
