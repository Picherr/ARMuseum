using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ARMuseum
{
    /// <summary>
    /// 音频服务。承接原 GameController 中的音频开关逻辑（Audio / AudioTurnOff）。
    /// 后续将合并 AudioManager / BGMController / DontDestroyBgm 以统一音频职责。
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        public GameObject AudioOn;
        public GameObject AudioOff;

        private Button AudioBtn;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name == SceneNames.Navigation)
                return;

            AudioBtn = GameObject.Find("/UI/MenuUI/Canvas/ButtonGroup/Audio").GetComponent<Button>();
            AudioBtn.onClick.AddListener(Audio);
        }

        public void Audio()
        {
            string Path1;
            string Path2;
            Sprite sprite1;
            Sprite sprite2;

            if (AppState.IsAudioOn) // 当前声音功能开启
            {
                Path1 = "UI/Button/BClick/静音";
                Path2 = "UI/Button/AClick/静音";
                AudioOff.SetActive(true);
            }
            else
            {
                Path1 = "UI/Button/BClick/语音";
                Path2 = "UI/Button/AClick/语音";
                AudioOn.SetActive(true);
            }
            sprite1 = Resources.Load<Sprite>(Path1) as Sprite;
            AudioBtn.image.sprite = sprite1;
            sprite2 = Resources.Load<Sprite>(Path2) as Sprite;
            SpriteState spriteState = new SpriteState();
            spriteState.pressedSprite = sprite2;
            AudioBtn.spriteState = spriteState;

            AppState.IsAudioOn = !AppState.IsAudioOn;
            StartCoroutine(DelayThenAudioTurnOff());
        }

        private void AudioTurnOff()
        {
            AudioOn.SetActive(false);
            AudioOff.SetActive(false);
        }

        private IEnumerator DelayThenAudioTurnOff()
        {
            yield return new WaitForSeconds(1.0f);
            AudioTurnOff();
        }
    }
}
