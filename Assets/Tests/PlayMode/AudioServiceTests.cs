using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace ARMuseum.Tests.PlayMode
{
    public class AudioServiceTests
    {
        private readonly List<Object> createdObjects = new List<Object>();
        private bool originalAudioState;
        private bool originalListenerPause;

        [SetUp]
        public void SaveAudioState()
        {
            originalAudioState = AppState.IsAudioOn;
            originalListenerPause = AudioListener.pause;
        }

        [UnityTearDown]
        public IEnumerator RestoreAudioState()
        {
            foreach (Object createdObject in createdObjects)
            {
                if (createdObject != null)
                    Object.Destroy(createdObject);
            }
            createdObjects.Clear();
            yield return null;
            AppState.IsAudioOn = originalAudioState;
            AudioListener.pause = originalListenerPause;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EnablingService_AppliesSavedStateToSoundAndButton(bool audioOn)
        {
            AppState.IsAudioOn = audioOn;
            AudioListener.pause = audioOn;
            Button button;
            Sprite muteSprite;
            Sprite voiceSprite;
            AudioService service = CreateService(out button, out muteSprite, out voiceSprite);

            service.gameObject.SetActive(true);

            Assert.AreEqual(!audioOn, AudioListener.pause);
            Assert.AreSame(audioOn ? voiceSprite : muteSprite, button.image.sprite);
            Assert.AreSame(audioOn ? voiceSprite : muteSprite, button.spriteState.pressedSprite);
        }

        [Test]
        public void ButtonToggle_ControlsActualAudioAndSurvivesServiceReplacement()
        {
            AppState.IsAudioOn = false;
            Button button;
            Sprite muteSprite;
            Sprite voiceSprite;
            AudioService service = CreateService(out button, out muteSprite, out voiceSprite);
            service.gameObject.SetActive(true);

            button.onClick.Invoke();

            Assert.IsTrue(AppState.IsAudioOn);
            Assert.IsFalse(AudioListener.pause);
            Assert.AreSame(voiceSprite, button.image.sprite);
            Assert.IsTrue(service.AudioOn.activeSelf);
            Assert.IsFalse(service.AudioOff.activeSelf);

            service.gameObject.SetActive(false);
            AudioService nextService = CreateService(out button, out muteSprite, out voiceSprite);
            nextService.gameObject.SetActive(true);

            Assert.IsFalse(AudioListener.pause);
            Assert.AreSame(voiceSprite, button.image.sprite);
            button.onClick.Invoke();
            Assert.IsFalse(AppState.IsAudioOn);
            Assert.IsTrue(AudioListener.pause);
            Assert.AreSame(muteSprite, button.image.sprite);
            Assert.IsFalse(nextService.AudioOn.activeSelf);
            Assert.IsTrue(nextService.AudioOff.activeSelf);
        }

        [Test]
        public void DisableAndReenable_DetachesAndReattachesExactlyOneButtonHandler()
        {
            AppState.IsAudioOn = false;
            Button button;
            Sprite muteSprite;
            Sprite voiceSprite;
            AudioService service = CreateService(out button, out muteSprite, out voiceSprite);
            service.gameObject.SetActive(true);
            service.enabled = false;

            button.onClick.Invoke();
            Assert.IsFalse(AppState.IsAudioOn, "禁用组件后按钮不应再修改音频状态。");

            service.enabled = true;
            button.onClick.Invoke();
            Assert.IsTrue(AppState.IsAudioOn, "重新启用后每次点击应只切换一次。");
            service.enabled = false;
            Assert.IsFalse(service.AudioOn.activeSelf);
            Assert.IsFalse(service.AudioOff.activeSelf);
        }

        [Test]
        public void MissingOptionalBindings_StillAllowsAudioControl()
        {
            AppState.IsAudioOn = false;
            GameObject root = Track(new GameObject("AudioServiceWithoutUI"));
            AudioService service = root.AddComponent<AudioService>();

            Assert.IsTrue(AudioListener.pause);
            Assert.DoesNotThrow(() => service.AudioPlay(null));
            Assert.DoesNotThrow(() => service.Audio());
            Assert.IsFalse(AudioListener.pause);
            Assert.DoesNotThrow(() => service.Audio());
            Assert.IsTrue(AudioListener.pause);
        }

        [UnityTest]
        public IEnumerator RapidToggles_KeepLatestHintVisibleForItsFullDuration()
        {
            AppState.IsAudioOn = false;
            Button button;
            Sprite muteSprite;
            Sprite voiceSprite;
            AudioService service = CreateService(out button, out muteSprite, out voiceSprite);
            service.gameObject.SetActive(true);
            button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.6f);

            button.onClick.Invoke();
            yield return new WaitForSecondsRealtime(0.6f);

            Assert.IsTrue(service.AudioOff.activeSelf, "前一次提示计时不应提前关闭最新提示。");
            yield return new WaitForSecondsRealtime(0.5f);
            Assert.IsFalse(service.AudioOn.activeSelf);
            Assert.IsFalse(service.AudioOff.activeSelf);
        }

        private AudioService CreateService(out Button button, out Sprite muteSprite, out Sprite voiceSprite)
        {
            GameObject root = Track(new GameObject("AudioServiceTest"));
            root.SetActive(false);
            root.AddComponent<AudioSource>();
            AudioService service = root.AddComponent<AudioService>();

            GameObject buttonObject = new GameObject("AudioButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(root.transform);
            button = buttonObject.GetComponent<Button>();
            GameObject audioOn = new GameObject("AudioOn");
            audioOn.transform.SetParent(root.transform);
            GameObject audioOff = new GameObject("AudioOff");
            audioOff.transform.SetParent(root.transform);
            service.AudioOn = audioOn;
            service.AudioOff = audioOff;

            Texture2D texture = Track(new Texture2D(2, 2));
            muteSprite = Track(Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero));
            voiceSprite = Track(Sprite.Create(texture, new Rect(1, 1, 1, 1), Vector2.zero));
            SetField(service, "audioButton", button);
            SetField(service, "muteNormalSprite", muteSprite);
            SetField(service, "mutePressedSprite", muteSprite);
            SetField(service, "voiceNormalSprite", voiceSprite);
            SetField(service, "voicePressedSprite", voiceSprite);
            return service;
        }

        private T Track<T>(T createdObject) where T : Object
        {
            createdObjects.Add(createdObject);
            return createdObject;
        }

        private static void SetField(AudioService service, string fieldName, object value)
        {
            typeof(AudioService).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(service, value);
        }
    }
}
