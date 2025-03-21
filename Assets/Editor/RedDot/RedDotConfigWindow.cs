using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MH
{
    public class RedDotConfigWindow : EditorWindow
    {
        private RedDotConfigAsset m_Target;
        private RedDotKeyAsset m_KeyAsset;
        private Vector2 m_ScrollPosition;
        private ERedDotKeyType m_NewKey = ERedDotKeyType.None;
        private Dictionary<ERedDotKeyType, bool> m_ShowParentDropdown = new Dictionary<ERedDotKeyType, bool>();

        private Dictionary<ERedDotKeyType, ERedDotKeyType> m_SelectedParent =
            new Dictionary<ERedDotKeyType, ERedDotKeyType>();

        private Dictionary<ERedDotKeyType, bool> m_ParentListFoldout = new Dictionary<ERedDotKeyType, bool>();
        private Dictionary<ERedDotKeyType, bool> m_ShowFlagsDropdown = new Dictionary<ERedDotKeyType, bool>();

        [MenuItem("Tool/RedDot/RedDotConfig Editor")]
        private static void ShowWindow()
        {
            var window = GetWindow<RedDotConfigWindow>();
            window.titleContent = new GUIContent("红点配置关系");
            window.Show();
        }

        private bool m_ParentsFoldout = true;
        private Vector2 m_ParentsScrollPos;

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 如果目标为空，尝试加载默认配置
            if (m_Target == null)
            {
                m_Target = AssetDatabase.LoadAssetAtPath<RedDotConfigAsset>(RedDotEditorSettings.Instance
                    .redDotConfigAssetPath);
            }

            if (m_KeyAsset == null)
            {
                m_KeyAsset =
                    AssetDatabase.LoadAssetAtPath<RedDotKeyAsset>(RedDotEditorSettings.Instance.redDotKeyAssetPath);
            }

            if (m_Target == null || m_KeyAsset == null)
            {
                EditorGUILayout.HelpBox("请先选择配置文件", MessageType.Warning);
                return;
            }

            GUI.enabled = false;
            m_Target =
                EditorGUILayout.ObjectField("目标配置", m_Target, typeof(RedDotConfigAsset), false) as RedDotConfigAsset;
            GUI.enabled = true;
            GUI.enabled = false;
            m_KeyAsset =
                EditorGUILayout.ObjectField("Key配置", m_KeyAsset, typeof(RedDotKeyAsset), false) as RedDotKeyAsset;
            GUI.enabled = true;
            EditorGUILayout.Space(10);
            // 删除多余的 BeginHorizontal 和 Space
            EditorGUILayout.BeginHorizontal();
            // 替换原来的 EnumPopup
            var enumValues = Enum.GetValues(typeof(ERedDotKeyType));
            int selectedIndex = 0;
            string[] options = new string[enumValues.Length];
            for (int i = 0; i < enumValues.Length; i++)
            {
                var enumValue = (ERedDotKeyType)enumValues.GetValue(i);
                options[i] = GetDescription(enumValue);
                if (enumValue == m_NewKey)
                    selectedIndex = i;
            }

            int newSelectedIndex = EditorGUILayout.Popup("新Key", selectedIndex, options);
            m_NewKey = (ERedDotKeyType)enumValues.GetValue(newSelectedIndex);

            if (GUILayout.Button("添加", GUILayout.Width(60)))
            {
                AddNewItem();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

            var serializedObject = new SerializedObject(m_Target);
            var listProperty = serializedObject.FindProperty("m_AllRedDotConfigList");

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                EditorGUILayout.BeginVertical("box");
                var element = listProperty.GetArrayElementAtIndex(i);
                var key = (ERedDotKeyType)element.FindPropertyRelative("key").enumValueIndex;
                var desc = GetDescription(key);
                var parentListProp = element.FindPropertyRelative("parentList");

                EditorGUILayout.BeginHorizontal();
                if (!m_ParentListFoldout.ContainsKey(key))
                    m_ParentListFoldout[key] = false;
                m_ParentListFoldout[key] =
                    EditorGUILayout.Foldout(m_ParentListFoldout[key], $"Key: {desc} ({key})", true);

                if (GUILayout.Button("删除配置", GUILayout.Width(100)))
                {
                    listProperty.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                    // 重新获取 SerializedObject 和 listProperty
                    serializedObject = new SerializedObject(m_Target);
                    listProperty = serializedObject.FindProperty("m_AllRedDotConfigList");
                    i--;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    continue;
                }

                EditorGUILayout.EndHorizontal();

                // 在 OnGUI 方法中，找到显示父节点列表的部分，在其后添加 Flags 设置
                if (m_ParentListFoldout[key])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("添加父节点", GUILayout.Width(100)))
                    {
                        m_ShowParentDropdown[key] = true;
                        m_SelectedParent[key] = ERedDotKeyType.None;
                    }
                    if (GUILayout.Button("设置Flags", GUILayout.Width(100)))
                    {
                        m_ShowFlagsDropdown[key] = true;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (m_ShowParentDropdown.TryGetValue(key, out bool show) && show)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var parentEnumValues = Enum.GetValues(typeof(ERedDotKeyType));
                        var parentOptions = new List<string>();
                        var validValues = new List<ERedDotKeyType>();

                        parentOptions.Add("请选择父节点");
                        validValues.Add(ERedDotKeyType.None);

                        foreach (ERedDotKeyType enumValue in parentEnumValues)
                        {
                            if (enumValue != key && enumValue != ERedDotKeyType.None)
                            {
                                bool exists = false;
                                for (int j = 0; j < parentListProp.arraySize; j++)
                                {
                                    if ((ERedDotKeyType)parentListProp.GetArrayElementAtIndex(j).enumValueIndex ==
                                        enumValue)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }

                                if (!exists)
                                {
                                    parentOptions.Add(GetDescription(enumValue));
                                    validValues.Add(enumValue);
                                }
                            }
                        }

                        int newDropdownSelectedIndex = EditorGUILayout.Popup(0, parentOptions.ToArray());
                        if (newDropdownSelectedIndex > 0)
                        {
                            var selectedParent = validValues[newDropdownSelectedIndex];
                            if (CheckCircularReference(key, selectedParent))
                            {
                                EditorUtility.DisplayDialog("错误", "检测到循环引用，无法添加该父节点", "确定");
                            }
                            else
                            {
                                parentListProp.arraySize++;
                                var newElement = parentListProp.GetArrayElementAtIndex(parentListProp.arraySize - 1);
                                newElement.enumValueIndex = (int)selectedParent;
                            }

                            m_ShowParentDropdown[key] = false;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUILayout.Space(5);
                    EditorGUILayout.LabelField("父节点列表:");
                    EditorGUI.indentLevel++;
                    for (int j = 0; j < parentListProp.arraySize; j++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        var currentParentKey = (ERedDotKeyType)parentListProp.GetArrayElementAtIndex(j).enumValueIndex;

                        // 创建可选的父节点列表
                        var availableParents = new List<ERedDotKeyType>();
                        var parentNames = new List<string>();
                        foreach (ERedDotKeyType enumValue in Enum.GetValues(typeof(ERedDotKeyType)))
                        {
                            if (enumValue != key && enumValue != ERedDotKeyType.None)
                            {
                                bool exists = false;
                                for (int k = 0; k < parentListProp.arraySize; k++)
                                {
                                    if (k != j && (ERedDotKeyType)parentListProp.GetArrayElementAtIndex(k)
                                            .enumValueIndex == enumValue)
                                    {
                                        exists = true;
                                        break;
                                    }
                                }

                                if (!exists || enumValue == currentParentKey)
                                {
                                    availableParents.Add(enumValue);
                                    parentNames.Add(GetDescription(enumValue));
                                }
                            }
                        }

                        // 找到当前选中项的索引
                        int currentIndex = availableParents.IndexOf(currentParentKey);

                        // 显示下拉列表
                        int newIndex = EditorGUILayout.Popup(currentIndex, parentNames.ToArray());
                        if (newIndex != currentIndex)
                        {
                            var newParentKey = availableParents[newIndex];
                            if (CheckCircularReference(key, newParentKey))
                            {
                                EditorUtility.DisplayDialog("错误", "检测到循环引用，无法修改为该父节点", "确定");
                            }
                            else
                            {
                                parentListProp.GetArrayElementAtIndex(j).enumValueIndex = (int)newParentKey;
                            }
                        }

                        if (GUILayout.Button("删除", GUILayout.Width(60)))
                        {
                            parentListProp.DeleteArrayElementAtIndex(j);
                            j--;
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    EditorGUI.indentLevel--;
                    
                    // 修改 Flags 下拉框部分
                    if (m_ShowFlagsDropdown.TryGetValue(key, out bool showFlags) && showFlags)
                    {
                        EditorGUILayout.BeginVertical("box");
                        var flagsProperty = element.FindPropertyRelative("flags");
                        if (flagsProperty == null)
                        {
                            EditorGUILayout.HelpBox("未找到flags属性", MessageType.Error);
                        }
                        else
                        {
                            EditorGUILayout.LabelField("红点类型设置：");
                            int currentFlags = flagsProperty.intValue;
                            bool changed = false;

                            changed |= DrawFlagToggle(ref currentFlags, (int)RedDotFlags.Default, "普通红点");
                            changed |= DrawFlagToggle(ref currentFlags, (int)RedDotFlags.Number, "数值红点");
                            changed |= DrawFlagToggle(ref currentFlags, (int)RedDotFlags.New, "新红点");
                            changed |= DrawFlagToggle(ref currentFlags, (int)RedDotFlags.Tips, "提示红点");
                            changed |= DrawFlagToggle(ref currentFlags, (int)RedDotFlags.Important, "重要红点");

                            if (changed)
                            {
                                flagsProperty.intValue = currentFlags;
                                serializedObject.ApplyModifiedProperties();
                                EditorUtility.SetDirty(m_Target);
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.Space();
                        if (GUILayout.Button("确定", GUILayout.Width(60)))
                        {
                            m_ShowFlagsDropdown[key] = false;
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    // 显示当前的 Flags 值
                    var currentFlagsProperty = element.FindPropertyRelative("flags");
                    if (currentFlagsProperty != null)
                    {
                        var flags = (RedDotFlags)currentFlagsProperty.intValue;
                        var flagsList = new List<string>();
                        
                        if (flags.HasFlag(RedDotFlags.Default)) flagsList.Add("普通红点");
                        if (flags.HasFlag(RedDotFlags.Number)) flagsList.Add("数值红点");
                        if (flags.HasFlag(RedDotFlags.New)) flagsList.Add("新红点");
                        if (flags.HasFlag(RedDotFlags.Tips)) flagsList.Add("提示红点");
                        if (flags.HasFlag(RedDotFlags.Important)) flagsList.Add("重要红点");

                        string flagsText = flagsList.Count > 0 ? string.Join(", ", flagsList) : "无";
                        EditorGUILayout.LabelField($"当前红点类型: {flagsText}");
                    }
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();

            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(m_Target);
            }
        }

        private void AddNewItem()
        {
            if (m_Target == null || m_NewKey == ERedDotKeyType.None) return;

            var serializedObject = new SerializedObject(m_Target);
            var listProperty = serializedObject.FindProperty("m_AllRedDotConfigList");

            // 检查Key是否已存在
            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var element = listProperty.GetArrayElementAtIndex(i);
                if ((ERedDotKeyType)element.FindPropertyRelative("key").enumValueIndex == m_NewKey)
                {
                    EditorUtility.DisplayDialog("错误", "Key已存在", "确定");
                    return;
                }
            }

            listProperty.arraySize++;
            var newElement = listProperty.GetArrayElementAtIndex(listProperty.arraySize - 1);
            newElement.FindPropertyRelative("key").enumValueIndex = (int)m_NewKey;

            // 删除父节点添加部分，因为现在是在列表项中单独添加
            var parentListProp = newElement.FindPropertyRelative("parentList");
            parentListProp.ClearArray();

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(m_Target);
            AssetDatabase.SaveAssets();

            m_NewKey = ERedDotKeyType.None;
        }

        private string GetDescription(ERedDotKeyType key)
        {
            if (m_KeyAsset == null) return key.ToString();
            var id = (int)key;
            var data = m_KeyAsset.AllRedDotDic.TryGetValue(id, out var value) ? value : null;
            return data?.Des ?? key.ToString();
        }

        private bool CheckCircularReference(ERedDotKeyType childKey, ERedDotKeyType parentKey)
        {
            var listProperty = new SerializedObject(m_Target).FindProperty("m_AllRedDotConfigList");
            HashSet<ERedDotKeyType> visited = new HashSet<ERedDotKeyType>();
            Queue<ERedDotKeyType> queue = new Queue<ERedDotKeyType>();
            queue.Enqueue(parentKey);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == childKey) return true;
                if (!visited.Add(current)) continue;

                // 查找当前节点的所有父节点
                for (int i = 0; i < listProperty.arraySize; i++)
                {
                    var element = listProperty.GetArrayElementAtIndex(i);
                    var key = (ERedDotKeyType)element.FindPropertyRelative("key").enumValueIndex;
                    if (key == current)
                    {
                        var parentListProp = element.FindPropertyRelative("parentList");
                        for (int j = 0; j < parentListProp.arraySize; j++)
                        {
                            var nextParentKey = (ERedDotKeyType)parentListProp.GetArrayElementAtIndex(j).enumValueIndex;
                            queue.Enqueue(nextParentKey);
                        }

                        break;
                    }
                }
            }

            return false;
        }
        // 添加辅助方法用于绘制单个标记复选框
        private bool DrawFlagToggle(ref int flags, int flag, string label)
        {
            bool hasFlag = (flags & flag) != 0;
            bool newValue = EditorGUILayout.Toggle(label, hasFlag);
            
            if (newValue != hasFlag)
            {
                if (newValue)
                    flags |= flag;
                else
                    flags &= ~flag;
                return true;
            }
            return false;
        }
    }
}


