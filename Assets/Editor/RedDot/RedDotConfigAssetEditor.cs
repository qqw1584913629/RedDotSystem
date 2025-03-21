using UnityEngine;
using UnityEditor;

namespace MH
{
    [CustomEditor(typeof(RedDotConfigAsset))]

    public class RedDotConfigAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AllRedDotConfigList"), true);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}