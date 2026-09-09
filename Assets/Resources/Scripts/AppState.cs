/// <summary>
/// 应用级全局状态。集中管理跨场景共享的设置（当前为语言与音频开关），
/// 取代原先语义模糊的 Variables 静态字段。
/// </summary>
public static class AppState
{
    public static bool IsChinese { get; set; } = true;
    public static bool IsAudioOn { get; set; } = false;
}
