using NUnit.Framework;
using ARMuseum;

namespace ARMuseum.Tests.EditMode
{
    /// <summary>
    /// 应用级全局状态 AppState 的纯逻辑校验：默认值与读写。
    /// </summary>
    public class AppStateTests
    {
        [TearDown]
        public void ResetState()
        {
            AppState.IsChinese = true;
            AppState.IsAudioOn = false;
        }

        [Test]
        public void Defaults_AreExpected()
        {
            Assert.IsTrue(AppState.IsChinese, "默认应为中文");
            Assert.IsFalse(AppState.IsAudioOn, "默认声音应关闭");
        }

        [Test]
        public void IsChinese_SetterWorks()
        {
            AppState.IsChinese = false;
            Assert.IsFalse(AppState.IsChinese, "IsChinese 应可设为 false");
        }

        [Test]
        public void IsAudioOn_SetterWorks()
        {
            AppState.IsAudioOn = true;
            Assert.IsTrue(AppState.IsAudioOn, "IsAudioOn 应可设为 true");
        }
    }
}
