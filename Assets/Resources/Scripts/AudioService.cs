using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ARMuseum
{
    /// <summary>
    /// 统一音频服务：整合 SFX 播放（原 AudioManager）与静音开关（原 GameController.Audio）。
    /// 纯 SFX 实例（原 AudioManager 对象）取消勾选 handleAudioToggle，避免重复接线静音按钮。
    /// 背景音乐由 BGM.prefab 的 AudioSource 直接播放，无需控制器脚本。
    /// </summary>
    public class AudioService : MonoBehaviour
    {
        // 静音开关角色：Start 里接线 Audio 按钮；纯 SFX 实例设为 false
        [SerializeField] private bool handleAudioToggle = true;

        public GameObject AudioOn;
        public GameObject AudioOff;

        [SerializeField] private Button audioButton;

        // 静音/语音两态的按钮图标（普通态 + 按压态）
        [SerializeField] private Sprite muteNormalSprite;   // UI/Button/BClick/静音
        [SerializeField] private Sprite mutePressedSprite;  // UI/Button/AClick/静音
        [SerializeField] private Sprite voiceNormalSprite;  // UI/Button/BClick/语音
        [SerializeField] private Sprite voicePressedSprite; // UI/Button/AClick/语音

        private AudioSource _audioSource;

        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();

            if (!handleAudioToggle)
                return;
            if (SceneManager.GetActiveScene().name == SceneNames.Navigation)
                return;

            if (audioButton != null)
                audioButton.onClick.AddListener(Audio);
        }

        // SFX 播放（原 AudioManager.AudioPlay）
        public void AudioPlay(AudioClip clip)
        {
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
                _audioSource.PlayOneShot(clip);
        }

        public void Audio()
        {
            Sprite sprite1;
            Sprite sprite2;

            if (AppState.IsAudioOn) // 当前声音功能开启
            {
                sprite1 = muteNormalSprite;
                sprite2 = mutePressedSprite;
                AudioOff.SetActive(true);
            }
            else
            {
                sprite1 = voiceNormalSprite;
                sprite2 = voicePressedSprite;
                AudioOn.SetActive(true);
            }
            audioButton.image.sprite = sprite1;
            SpriteState spriteState = new SpriteState();
            spriteState.pressedSprite = sprite2;
            audioButton.spriteState = spriteState;

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
