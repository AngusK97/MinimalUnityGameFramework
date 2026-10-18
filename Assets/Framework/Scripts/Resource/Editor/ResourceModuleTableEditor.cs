using UnityEditor;
using UnityEngine;

namespace Resource.Editor
{
    [CustomEditor(typeof(ResourceModuleTable))]
    public sealed class ResourceModuleTableEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Add a name and drag an asset, then generate. Renaming a member also requires updating its C# callers.", MessageType.Info);
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            var table = (ResourceModuleTable)target;
            bool current = ResourceModuleTableGenerator.ValidateGenerated(table, out string error);
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(current ? "Generated code is up to date." : error,
                current ? MessageType.Info : MessageType.Warning);

            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling))
            {
                if (GUILayout.Button("Validate and Generate"))
                {
                    AssetDatabase.SaveAssets();
                    ResourceModuleTableGenerator.Generate(table);
                }
            }
        }
    }
}
