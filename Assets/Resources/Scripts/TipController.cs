using System.Collections;
using UnityEngine;

namespace ARMuseum
{
    /// <summary>
    /// 提示（Tip）控制。承接原 GameController 中的 Tip / TouchTipOpen 逻辑：
    /// 点击开始后展示提示图，延时跳转；触摸提示延时关闭。
    /// </summary>
    public class TipController : MonoBehaviour
    {
        public GameObject TipImg;
        public GameObject TouchTip;

        public void Tip()
        {
            TipImg.SetActive(true);
            StartCoroutine(DelayThenChangeToNYWD());
        }

        private IEnumerator DelayThenChangeToNYWD()
        {
            yield return new WaitForSeconds(5.0f);
            SceneLoader.Load(SceneNames.NanYueWenDi);
        }

        public void TouchTipOpen()
        {
            TouchTip.SetActive(true);
            StartCoroutine(DelayThenTouchTipOff());
        }

        private void TouchTipOff()
        {
            TouchTip.SetActive(false);
        }

        private IEnumerator DelayThenTouchTipOff()
        {
            yield return new WaitForSeconds(2.0f);
            TouchTipOff();
        }
    }
}
