using UnityEngine;

namespace CpuRender 
{
    public enum EStage
    {
        light,
        cull,
        transparent,
        map_and_rim,
        mask,
    }

    public class Main : MonoBehaviour
    {
        public string outputDir;
        public EStage eStage;

        public Light[] ligths;
        public MeshFilter[] meshes;

        Stage _stage;

        private void Awake()
        {
            if (string.IsNullOrEmpty(outputDir)) 
                outputDir = $"{Application.dataPath}/output";

            CrtRenderer();
        }

        private void Start()
        {

        }

        private void Update()
        {
            _stage.DrawFrame(null);
        }
        public void CrtRenderer()
        {
            _stage = new Stage(Camera.main, ligths);

            switch (eStage)
            {
                case EStage.light:
                    var light_shader = new light_shader();
                    _stage.AddRenderer(new Renderer(_stage, meshes[0], light_shader));
                    _stage.AddRenderer(new Renderer(_stage, meshes[1], light_shader));
                    break;
                case EStage.cull:
                    _stage.AddRenderer(new Renderer(_stage, meshes[0], new light_shader() { cull = ECull.Front }));
                    _stage.AddRenderer(new Renderer(_stage, meshes[1], new light_shader() { cull = ECull.Back }));
                    break;
                case EStage.transparent:
                    var transparent_shader = new transparent_shader();
                    _stage.AddRenderer(new Renderer(_stage, meshes[0], transparent_shader));
                    _stage.AddRenderer(new Renderer(_stage, meshes[1], transparent_shader));
                    break;
                case EStage.map_and_rim:
                    _stage.AddRenderer(new Renderer(_stage, meshes[0], new map_shader()));
                    _stage.AddRenderer(new Renderer(_stage, meshes[1], new rim_shader()));
                    break;
                case EStage.mask:
                    _stage.AddRenderer(new Renderer(_stage, meshes[0], new mask()));
                    _stage.AddRenderer(new Renderer(_stage, meshes[1], new mask_model()));
                    break;
            }
        }
        public void DrawFrame()
        {
            _stage.DrawFrame($"{outputDir}/{UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name}.png");
        }
    }

}

