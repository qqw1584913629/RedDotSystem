using UnityEngine;

namespace MH
{
    public class UI_Test : MonoBehaviour
    {
        /// <summary>
        /// 添加一个背包物品数量
        /// </summary>
        public void OnAddBagItemCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key7, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key7, count + 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 减少一个背包物品
        /// </summary>
        public void OnReduceBagItemCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key7, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key7, count - 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 获得一点属性点
        /// </summary>
        public void OnAddAttributeCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key6, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key6, count + 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 减少一点属性点
        /// </summary>
        public void OnReduceAttributeCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key6, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key6, count - 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 当存在免费抽奖的时候
        /// </summary>
        public void OnExistFreeLotteryCountHandler()
        {
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key11, 1, RedDotFlags.Tips);
        }
        /// <summary>
        /// 移除免费抽奖
        /// </summary>
        public void OnNotExistFreeLotteryCountHandler()
        {
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key11, 0, RedDotFlags.Tips);
        }
        /// <summary>
        /// 当存在免费抽奖的时候
        /// </summary>
        public void OnExistAdShopItemCountHandler()
        {
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key12, 1, RedDotFlags.Default);
        }
        /// <summary>
        /// 移除免费抽奖
        /// </summary>
        public void OnNotExistAdShopItemCountHandler()
        {
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key12, 0, RedDotFlags.Default);
        }
        /// <summary>
        /// 添加一个系统邮件
        /// </summary>
        public void OnAddSystemMailCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key8, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key8, count + 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 减少一个系统邮件
        /// </summary>
        public void OnReduceSystemMailCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key8, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key8, count - 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 添加一个个人邮件
        /// </summary>
        public void OnAddPersonMailCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key9, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key9, count + 1, RedDotFlags.Number);
        }
        /// <summary>
        /// 减少一个个人邮件
        /// </summary>
        public void OnReducePersonMailCountHandler()
        {
            int count = RedDotSingleton.Instance.GetRedDotInfoCount(ERedDotKeyType.Key9, RedDotFlags.Number);
            RedDotSingleton.Instance.Set(ERedDotKeyType.Key9, count - 1, RedDotFlags.Number);
        }
    }
}