using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    // [CreateAssetMenu(fileName = "RedDotConfigAsset", menuName = "RedDot/RedDotConfigAsset")]

    public class RedDotConfigAsset : ScriptableObject
    {
        [SerializeField] private List<RedDotConfig> m_AllRedDotConfigList = new List<RedDotConfig>();

        public IReadOnlyList<RedDotConfig> AllRedDotConfigList => m_AllRedDotConfigList;
    }

    [Serializable]
    public class RedDotConfig
    {
        public ERedDotKeyType key;
        public List<ERedDotKeyType> parentList = new List<ERedDotKeyType>();
        public int flags; // 0: 数字红点, 1: 新红点, 2: 提示红点
    }

}