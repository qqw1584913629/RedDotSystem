using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    [Flags]
    public enum RedDotFlags
    {
        None = 0,
        Default = 1 << 0,  // 普通红点
        Number = 1 << 1,   // 数值红点
        New = 1 << 2,      // 新红点
        Tips = 1 << 3,     // 提示红点
        Important = 1 << 4  // 重要红点
    }
    public class RedDotNode
    {
        public ERedDotKeyType rdType { get; set; }
        public Dictionary<RedDotFlags, int> rdCounts = new Dictionary<RedDotFlags, int>();
        public int rdCount { get; private set; }  // 保留原有的总计数
        public int rdPriority { get; private set; }
        public List<RedDotNode> parents = new List<RedDotNode>();
        public RedDotSingleton.OnRdCountChange countChangeFunc;
        public Dictionary<ERedDotKeyType, RedDotNode> rdChildrenDic = new Dictionary<ERedDotKeyType, RedDotNode>();

        private void CheckRedDotCount()
        {
            var newCounts = new Dictionary<RedDotFlags, int>();
            int maxPriority = 0;
            int totalCount = 0;
            
            foreach (RedDotNode node in rdChildrenDic.Values)
            {
                foreach (var pair in node.rdCounts)
                {
                    if (pair.Value > 0)
                    {
                        if (!newCounts.ContainsKey(pair.Key))
                            newCounts[pair.Key] = 0;
                        newCounts[pair.Key] += pair.Value;
                        totalCount += pair.Value;
                        
                        int priorityValue = (int)pair.Key;
                        if (priorityValue > maxPriority)
                        {
                            maxPriority = priorityValue;
                        }
                    }
                }
            }

            bool needUpdate = !DictionaryEquals(rdCounts, newCounts) || rdCount != totalCount || rdPriority != maxPriority;
            if (needUpdate)
            {
                rdCounts = newCounts;
                rdCount = totalCount;
                rdPriority = maxPriority;
                NotifyRedDotCountChange();
            }

            foreach (var parent in parents)
            {
                parent.CheckRedDotCount();
            }
        }

        private void NotifyRedDotCountChange()
        {
            countChangeFunc?.Invoke(this);
        }

        private bool DictionaryEquals(Dictionary<RedDotFlags, int> dict1, Dictionary<RedDotFlags, int> dict2)
        {
            if (dict1.Count != dict2.Count) return false;
            foreach (var pair in dict1)
            {
                if (!dict2.TryGetValue(pair.Key, out int value) || value != pair.Value)
                    return false;
            }
            return true;
        }

        public void SetRedDotCount(int count, RedDotFlags priority)
        {
            if (rdChildrenDic.Count > 0)
            {
                Debug.LogWarning("不可直接设定根节点的红点数");
                return;
            }

            if (count <= 0)
            {
                rdCounts.Remove(priority);
            }
            else
            {
                rdCounts[priority] = count;
            }

            int maxPriority = 0;
            foreach (var flag in rdCounts.Keys)
            {
                int priorityValue = (int)flag;
                if (priorityValue > maxPriority)
                {
                    maxPriority = priorityValue;
                }
            }

            rdPriority = maxPriority;
            NotifyRedDotCountChange();
            foreach (var parent in parents)
            {
                parent.CheckRedDotCount();
            }
        }

        public int GetCount(RedDotFlags priority)
        {
            return rdCounts.TryGetValue(priority, out int count) ? count : 0;
        }

        public int GetTotalCount()
        {
            int total = 0;
            foreach (var count in rdCounts.Values)
            {
                total += count;
            }
            return total;
        }
    }
}