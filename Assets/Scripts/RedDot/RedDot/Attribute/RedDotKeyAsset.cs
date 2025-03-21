using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    // [CreateAssetMenu(fileName = "RedDotKeyAsset", menuName = "RedDot/RedDotKeyAsset")]

    public class RedDotKeyAsset : ScriptableObject
    {
        [SerializeField] private List<RedDotKeyData> m_AllRedDotList = new List<RedDotKeyData>();
        private Dictionary<int, RedDotKeyData> m_AllRedDotDic;

        public IReadOnlyDictionary<int, RedDotKeyData> AllRedDotDic
        {
            get
            {
                if (m_AllRedDotDic == null)
                {
                    var dict = new Dictionary<int, RedDotKeyData>();
                    foreach (var data in m_AllRedDotList)
                    {
                        dict.Add(data.Id, data);
                    }

                    m_AllRedDotDic = dict;
                }

                return m_AllRedDotDic;
            }
        }

        private void OnValidate()
        {
            m_AllRedDotDic = null; // 强制重建字典
        }
    }

    [Serializable]
    public class RedDotKeyData
    {
        [SerializeField, ReadOnly] private int id = 1;
        [SerializeField, ReadOnly] private string des;

        public int Id
        {
            get => id;
            internal set => id = value;
        }

        public string Des
        {
            get => des;
            internal set => des = value;
        }

        // 改为公共的无参构造函数
        public RedDotKeyData()
        {
        }

        public RedDotKeyData(int id)
        {
            Id = id;
        }

        public RedDotKeyData(int id, string des)
        {
            Id = id;
            Des = des;
        }

        internal void ChangeDes(string des)
        {
            Des = des;
        }
    }
}
