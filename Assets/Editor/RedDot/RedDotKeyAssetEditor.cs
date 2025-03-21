using UnityEngine;
using UnityEditor;

namespace MH
{
    [CustomEditor(typeof(RedDotKeyAsset))]

    public class RedDotKeyAssetEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AllRedDotList"), true);
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }
    }
}