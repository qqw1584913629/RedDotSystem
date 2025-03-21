using UnityEditor;
using UnityEngine;

namespace MH
{
    // [CreateAssetMenu(fileName = "RedDotEditorSettings", menuName = "RedDot/RedDotEditorSettings")]

    public class RedDotEditorSettings : ScriptableObject
    {
        public string redDotKeyAssetPath = "Assets/Config/RedDot/RedDotKeyAsset.asset";
        public string redDotConfigAssetPath = "Assets/Config/RedDot/RedDotConfigAsset.asset";

        private static RedDotEditorSettings instance;

        public static RedDotEditorSettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<RedDotEditorSettings>("RedDotEditorSettings");
                    if (instance == null)
                    {
                        instance = CreateInstance<RedDotEditorSettings>();
                        if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                        {
                            AssetDatabase.CreateFolder("Assets", "Resources");
                        }

                        AssetDatabase.CreateAsset(instance, "Assets/Resources/RedDotEditorSettings.asset");
                        AssetDatabase.SaveAssets();
                    }
                }

                return instance;
            }
        }
    }
}