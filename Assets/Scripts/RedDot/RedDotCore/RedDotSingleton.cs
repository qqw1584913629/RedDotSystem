using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public class RedDotSingleton : MonoBehaviour
    {
        private static RedDotSingleton _instance;
        public static RedDotSingleton Instance => _instance;
        public delegate void OnRdCountChange(RedDotNode node);
        public Dictionary<ERedDotKeyType, RedDotNode> AllRedDotNodes = new Dictionary<ERedDotKeyType, RedDotNode>();
        public RedDotConfigAsset _config;
        public void Awake()
        { 
            if (_instance == null)
                _instance = this;
            // todo 根据自己项目的资源管理进行加载就好 这里演示直接Inspector拖动了
            // _config = Resources.Load<RedDotConfigAsset>(nameof(RedDotConfigAsset));
            InitRedDotTreeNode();
        }
        public void InitRedDotTreeNode()
        {
            // 创建所有节点
            foreach (var config in _config.AllRedDotConfigList)
            {
                if (!AllRedDotNodes.ContainsKey(config.key))
                {
                    var node = new RedDotNode { rdType = config.key };
                    AllRedDotNodes.Add(config.key, node);
                }
            }

            // 建立父子关系
            foreach (var config in _config.AllRedDotConfigList)
            {
                var node = AllRedDotNodes[config.key];
                foreach (var parentKey in config.parentList)
                {
                    if (AllRedDotNodes.TryGetValue(parentKey, out var parentNode))
                    {
                        node.parents.Add(parentNode);
                        parentNode.rdChildrenDic[config.key] = node;
                    }
                }
            }
        }
        public void RemoveRedDotNodeCallBack(ERedDotKeyType key, OnRdCountChange callBack)
        {
            if (AllRedDotNodes.TryGetValue(key, out var node))
            {
                node.countChangeFunc -= callBack;
            }
            else
            {
                Debug.LogError($"未找到红点节点: {key}");
            }
        }
        public void SetRedDotNodeCallBack(ERedDotKeyType key, OnRdCountChange callBack)
        {
            if (AllRedDotNodes.TryGetValue(key, out var node))
            {
                node.countChangeFunc += callBack;
                callBack?.Invoke(node);
            }
            else
            {
                Debug.LogError($"未找到红点节点: {key}");
            }
        }

        public void Set(ERedDotKeyType key, int rdCount = 1, RedDotFlags redDotFlags = RedDotFlags.Default)
        {
            if (AllRedDotNodes.TryGetValue(key, out var node))
            {
                node.SetRedDotCount(Math.Max(0, rdCount), redDotFlags);
            }
            else
            {
                Debug.LogError($"未找到红点节点: {key}");
            }
        }
        public int GetRedDotInfoCount(ERedDotKeyType key, RedDotFlags redDotFlags)
        {
            if (AllRedDotNodes.TryGetValue(key, out var node))
            {
                return node.GetCount(redDotFlags);
            }
            return 0;
        }
        public (int count, int priority) GetRedDotInfo(ERedDotKeyType key)
        {
            if (AllRedDotNodes.TryGetValue(key, out var node))
            {
                return (node.rdCount, node.rdPriority);
            }
            return (0, 0);
        }
    }
}