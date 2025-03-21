using UnityEngine;
using UnityEditor;
using System;

namespace MH
{
    [CustomPropertyDrawer(typeof(ERedDotKeyType))]
    public class EnumDescriptionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            var enumValue = (ERedDotKeyType)property.enumValueIndex;
            var options = Enum.GetValues(typeof(ERedDotKeyType));
            var descriptions = new string[options.Length];
            
            for (int i = 0; i < options.Length; i++)
            {
                descriptions[i] = ((ERedDotKeyType)options.GetValue(i)).GetDescription();
            }
            
            property.enumValueIndex = EditorGUI.Popup(position, label.text, property.enumValueIndex, descriptions);
            
            EditorGUI.EndProperty();
        }
    }
}