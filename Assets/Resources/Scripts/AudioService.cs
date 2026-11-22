using System.Collections;
using UnityEngine;
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
        // 静音开关角色：启用时接线 Audio 按钮；纯 SFX 实例设为 false
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
        private Button _boundButton;
        private Coroutine _audioHintCoroutine;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnEnable()
        {
            SynchronizeAudioState();
            AudioTurnOff();

            if (!handleAudioToggle || audioButton == null)
                return;

            _boundButton = audioButton;
            _boundButton.onClick.AddListener(Audio);
        }

        private void OnDisable()
        {
            if (_boundButton != null)
                _boundButton.onClick.RemoveListener(Audio);
            _boundButton = null;

            StopAudioHint();
            AudioTurnOff();
        }

        // SFX 播放（原 AudioManager.AudioPlay）
        public void AudioPlay(AudioClip clip)
        {
            // 静音时不排队播放提示音，恢复声音后也不会补播。
            if (!AppState.IsAudioOn || clip == null)
                return;
            if (_audioSource == null)
                _audioSource = GetComponent<AudioSource>();
            if (_audioSource != null)
                _audioSource.PlayOneShot(clip);
        }

        public void Audio()
        {
            AppState.IsAudioOn = !AppState.IsAudioOn;
            SynchronizeAudioState();

            StopAudioHint();
            AudioTurnOff();
            if (!isActiveAndEnabled)
                return;

            GameObject hint = AppState.IsAudioOn ? AudioOn : AudioOff;
            if (hint != null)
            {
                hint.SetActive(true);
                _audioHintCoroutine = StartCoroutine(DelayThenAudioTurnOff());
            }
        }

        private void SynchronizeAudioState()
        {
            // BGM 与提示音共用全局听众状态，场景重建时保留用户开关设置。
            AudioListener.pause = !AppState.IsAudioOn;
            if (audioButton == null)
                return;

            if (audioButton.image != null)
                audioButton.image.sprite = AppState.IsAudioOn ? voiceNormalSprite : muteNormalSprite;
            SpriteState spriteState = audioButton.spriteState;
            spriteState.pressedSprite = AppState.IsAudioOn ? voicePressedSprite : mutePressedSprite;
            audioButton.spriteState = spriteState;
        }

        private void StopAudioHint()
        {
            if (_audioHintCoroutine == null)
                return;

            StopCoroutine(_audioHintCoroutine);
            _audioHintCoroutine = null;
        }

        private void AudioTurnOff()
        {
            if (AudioOn != null)
                AudioOn.SetActive(false);
            if (AudioOff != null)
                AudioOff.SetActive(false);
        }

        private IEnumerator DelayThenAudioTurnOff()
        {
            yield return new WaitForSecondsRealtime(1.0f);
            _audioHintCoroutine = null;
            AudioTurnOff();
        }
    }
}
