using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MH
{
    /*
     * 表现层：根据红点是否显示或显示数，自定义红点的表现方式
     */

    /// <summary>
    /// UGUI红点物体脚本
    /// </summary>
    public class RedDotItem : MonoBehaviour
    {
        [Header("红点父节点")] [SerializeField] public GameObject m_DotObj;

        [Header("红点数文本")] [SerializeField] private TextMeshProUGUI m_DotCountText;
        [Header("红点所属键")] [SerializeField] private ERedDotKeyType m_RedDotType;
        [Header("红点类型")] [SerializeField] private RedDotFlags m_Priority;
        
        #if UNITY_EDITOR
                private void OnValidate()
                {
                    if (!Application.isPlaying)
                    {
                        UnityEditor.EditorApplication.delayCall += () =>
                        {
                            if (this != null)
                            {
                                UnityEditor.EditorUtility.SetDirty(this);
                            }
                        };
                    }
                }
        #endif

        private void Start()
        {
            RedDotSingleton.Instance.SetRedDotNodeCallBack(m_RedDotType, SetDotState);
        }

        public void SetDotState(RedDotNode node)
        {
            // 获取当前最高优先级的红点类型
            RedDotFlags highestPriority = (RedDotFlags)node.rdPriority;
            bool shouldShow = node.GetCount(highestPriority) > 0 && highestPriority == m_Priority;
            m_DotObj.gameObject.SetActive(shouldShow);
            if (shouldShow && m_DotCountText)
            {
                m_DotCountText.SetText($"{(node.GetCount(highestPriority) >= 100 ? "99+" : node.GetCount(highestPriority))}");
                m_DotCountText.gameObject.SetActive(m_Priority == RedDotFlags.Number);
            }
        }

        private void OnDestroy()
        {
            m_DotObj = null;
            m_DotCountText = null;
            RedDotSingleton.Instance.RemoveRedDotNodeCallBack(m_RedDotType, SetDotState);
        }
    }
}