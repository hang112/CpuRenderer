using UnityEditor;
using UnityEngine;

namespace CpuRender
{
    [CustomEditor(typeof(Main))]
    [ExecuteInEditMode]
    public class MainEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Draw"))
            {
                var main = (Main)target;
                main.CrtRenderer();
                main.DrawFrame();

                AssetDatabase.Refresh();
            }
        }
    }

}
