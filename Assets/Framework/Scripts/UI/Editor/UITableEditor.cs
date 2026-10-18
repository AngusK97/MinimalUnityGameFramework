using UnityEditor;
using UnityEngine;

namespace UI.Editor
{
    [CustomEditor(typeof(UITable))]
    public sealed class UITableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var table = (UITable)target;
            bool current = UITableGenerator.ValidateGenerated(table, out string error);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(current ? "Generated code is up to date." : error,
                current ? MessageType.Info : MessageType.Warning);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("Validate and Generate"))
                {
                    AssetDatabase.SaveAssets();
                    UITableGenerator.Generate(table);
                }
            }
        }
    }
}
