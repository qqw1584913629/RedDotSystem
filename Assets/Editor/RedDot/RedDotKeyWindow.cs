using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MH
{
    public class RedDotKeyWindow : EditorWindow
    {
        private RedDotKeyAsset m_Target;
        private Vector2 m_ScrollPosition;
        private int m_NewId = 1;
        private string m_NewDes = "";
        private string m_ExportPath = "Assets/Scripts/RedDot/RedDot/"; // 添加导出路径字段

        [MenuItem("Tool/RedDot/RedDotKey Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<RedDotKeyWindow>();
            window.titleContent = new GUIContent("红点配置");
            window.InitializeNewId();
            window.Show();
        }

        private void InitializeNewId()
        {
            if (m_Target == null)
            {
                m_Target = AssetDatabase.LoadAssetAtPath<RedDotKeyAsset>(RedDotEditorSettings.Instance.redDotKeyAssetPath);
            }

            if (m_Target != null)
            {
                var serializedObject = new SerializedObject(m_Target);
                var listProperty = serializedObject.FindProperty("m_AllRedDotList");
                
                int maxId = 0;
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    int currentId = element.FindPropertyRelative("id").intValue;
                    maxId = Mathf.Max(maxId, currentId);
                }
                
                m_NewId = maxId + 1;
            }
            else
            {
                m_NewId = 1;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 如果目标为空，尝试加载默认配置
            if (m_Target == null)
            {
                m_Target = AssetDatabase.LoadAssetAtPath<RedDotKeyAsset>(RedDotEditorSettings.Instance
                    .redDotKeyAssetPath);
            }

            if (m_Target == null)
            {
                EditorGUILayout.HelpBox("请先选择一个RedDotKeyAsset配置文件", MessageType.Warning);
                return;
            }

            GUI.enabled = false;
            m_Target = EditorGUILayout.ObjectField("目标配置", m_Target, typeof(RedDotKeyAsset), false) as RedDotKeyAsset;
            GUI.enabled = true;


            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            m_ExportPath = EditorGUILayout.TextField("导出路径", m_ExportPath);
            GUI.enabled = true;
            if (GUILayout.Button("选择路径", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel(
                    "选择枚举文件导出文件夹",
                    Application.dataPath,
                    "");
                
                if (!string.IsNullOrEmpty(path))
                {
                    // 转换为相对于Assets的路径
                    if (path.StartsWith(Application.dataPath))
                    {
                        string relativePath = "Assets" + path.Substring(Application.dataPath.Length);
                        m_ExportPath = relativePath + "/";
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            m_NewId = EditorGUILayout.IntField("红点Id", m_NewId);
            m_NewDes = EditorGUILayout.TextField("红点描述", m_NewDes);
            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                AddNewItem();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            var scrollViewStyle = new GUIStyle(GUI.skin.box);
            scrollViewStyle.padding = new RectOffset(10, 10, 10, 10);
            EditorGUILayout.BeginVertical(scrollViewStyle);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            var serializedObject = new SerializedObject(m_Target);
            var listProperty = serializedObject.FindProperty("m_AllRedDotList");

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var boxStyle = new GUIStyle("box");
                boxStyle.normal.background = EditorGUIUtility.whiteTexture;
                var itemColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.4f, 0.4f, 0.4f); // 设置为浅灰色
                EditorGUILayout.BeginVertical(boxStyle);
                GUI.backgroundColor = itemColor;

                EditorGUILayout.BeginHorizontal();
                var element = listProperty.GetArrayElementAtIndex(i);

                EditorGUILayout.LabelField($"ID: {element.FindPropertyRelative("id").intValue}");
                EditorGUILayout.LabelField($"描述: {element.FindPropertyRelative("des").stringValue}");

                var deleteButtonColor = GUI.backgroundColor;
                GUI.backgroundColor = new Color(0.8f, 0.2f, 0.2f);
                if (GUILayout.Button("删除", GUILayout.Width(60)))
                {
                    listProperty.DeleteArrayElementAtIndex(i);
                    i--;
                }

                GUI.backgroundColor = deleteButtonColor;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_Target);
            }

            EditorGUILayout.Space(10);
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
            if (GUILayout.Button("生成枚举", GUILayout.Height(200)))
            {
                GenerateEnum();
            }

            GUI.backgroundColor = originalColor;
        }

        private void AddNewItem()
        {
            if (m_Target == null) return;

            var serializedObject = new SerializedObject(m_Target);
            var listProperty = serializedObject.FindProperty("m_AllRedDotList");

            // 检查ID是否已存在
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                if (element.FindPropertyRelative("id").intValue == m_NewId)
                {
                    EditorUtility.DisplayDialog("错误", "ID已存在", "确定");
                    return;
                }
            }

            var newData = new RedDotKeyData(m_NewId, m_NewDes);

            listProperty.arraySize++;
            var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            newElement.FindPropertyRelative("id").intValue = m_NewId;
            newElement.FindPropertyRelative("des").stringValue = m_NewDes;

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Target);

            // 清空输入并重新初始化ID
            m_NewDes = "";
            InitializeNewId();
        }

        private void GenerateEnum()
        {
            if (m_Target == null) return;
            if (string.IsNullOrEmpty(m_ExportPath))
            {
                EditorUtility.DisplayDialog("错误", "请先设置导出路径", "确定");
                return;
            }

            // 构建完整的文件路径
            string enumFilePath = Path.Combine(m_ExportPath, "ERedDotKeyType.cs");
            
            var sb = new StringBuilder();

            sb.AppendLine("");
            sb.AppendLine("using System.ComponentModel;");
            sb.AppendLine("namespace MH");
            sb.AppendLine("{");
            sb.AppendLine("    public enum ERedDotKeyType");
            sb.AppendLine("    {");
            sb.AppendLine("        [Description(\"None\")]");
            sb.AppendLine("        None = 0,");
            sb.AppendLine("");

            var serializedObject = new SerializedObject(m_Target);
            var listProperty = serializedObject.FindProperty("m_AllRedDotList");

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                var id = element.FindPropertyRelative("id").intValue;
                var des = element.FindPropertyRelative("des").stringValue;

                sb.AppendLine($"        [Description(\"{des}\")]");
                sb.AppendLine($"        Key{id} = {id},");
                sb.AppendLine("");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            // 确保目录存在
            var directoryPath = Path.GetDirectoryName(enumFilePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // 写入到文件而不是文件夹
            File.WriteAllText(enumFilePath, sb.ToString());
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("提示", "枚举生成成功", "确定");
        }
    }
}