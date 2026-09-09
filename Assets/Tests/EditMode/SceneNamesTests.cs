using NUnit.Framework;
using UnityEditor;
using ARMuseum;

namespace ARMuseum.Tests.EditMode
{
    /// <summary>
    /// 场景名常量的纯逻辑校验：非空、唯一、且与 Assets/Scenes 下实际场景一一对应。
    /// </summary>
    public class SceneNamesTests
    {
        private static readonly string[] AllSceneNames =
        {
            SceneNames.Beginning,
            SceneNames.NanYueWenDi,
            SceneNames.MeiYuDaGuan,
            SceneNames.ShengHuoQiJu,
            SceneNames.HaiLuYangFan,
            SceneNames.Navigation,
            SceneNames.Scan,
        };

        [Test]
        public void AllSceneNames_AreNonEmpty()
        {
            foreach (var name in AllSceneNames)
            {
                Assert.IsFalse(string.IsNullOrEmpty(name), "场景名常量不能为空");
            }
        }

        [Test]
        public void AllSceneNames_AreUnique()
        {
            CollectionAssert.AllItemsAreUnique(AllSceneNames, "场景名常量必须唯一");
        }

        [Test]
        public void AllSceneNames_HaveCorrespondingSceneAsset()
        {
            foreach (var name in AllSceneNames)
            {
                var asset = AssetDatabase.LoadAssetAtPath<SceneAsset>($"Assets/Scenes/{name}.unity");
                Assert.IsNotNull(asset, $"场景 {name} 缺少对应场景资源 Assets/Scenes/{name}.unity");
            }
        }
    }
}
