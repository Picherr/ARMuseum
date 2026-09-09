# ARMuseum

ARMuseum 是一个基于 Unity 与 EasyAR Sense 的移动端博物馆 AR 导览项目。应用通过图像目标识别展示文物模型、图文与音频内容，同时提供中英文界面、触控查看、玉佩拼图和稀疏空间地图导航功能。

## 主要功能

- 识别 `StreamingAssets` 中的 EasyAR 图像目标数据并叠加文物内容。
- 展示南越文帝、美玉大观、生活器具、海路扬帆等主题内容。
- 支持中英文 UI、展品详情、背景音乐和音效提示。
- 支持单指旋转、双指缩放、地图拖动及缩放。
- 在“美玉大观”场景中提供玉佩碎片拖拽拼图。
- 使用 EasyAR Sparse Spatial Map 构建地图并进行定位导航。

## 技术与环境

| 项目 | 当前配置 |
| --- | --- |
| Unity | `2021.3.9f1c1` |
| AR SDK | EasyAR Sense Unity Plugin `4.6.2+3022.b15d4240` |
| UI | Unity UI（uGUI）、TextMesh Pro `3.0.6` |
| 输入 | 旧版 Input Manager（`activeInputHandler: 0`） |
| 主要平台 | Android |
| Android 配置 | 最低 API 22、IL2CPP、ARMv7 + ARM64 |

项目还包含 Unity Visual Scripting、Timeline、Ingame Debug Console 和 Build Report Tool。`Assets/Common` 中保留了 Vuforia 示例公共代码，但相关程序集受 `VUFORIA_ENABLED` 条件约束；当前 `Packages/manifest.json` 未启用 Vuforia，项目实际 AR 场景使用 EasyAR。

## 目录结构

```text
ARMuseum/
├─ Assets/
│  ├─ Scenes/                 # 应用场景
│  ├─ Resources/
│  │  ├─ Scripts/             # 项目业务脚本
│  │  ├─ Prefabs/             # 菜单、导航、详情等预制体
│  │  ├─ UI/                  # 中英文界面资源
│  │  ├─ Models/              # 文物模型与贴图
│  │  ├─ Animations/          # 场景和 UI 动画
│  │  ├─ Audio/               # 背景音乐与音频
│  │  └─ WireFrame/           # AR 展示序列与相关素材
│  ├─ StreamingAssets/        # EasyAR `.etd` 目标数据及目标图片
│  ├─ XR/Settings/            # EasyAR 项目设置
│  ├─ Common/                 # 导入的 Vuforia 示例公共资源
│  ├─ Plugins/                # Ingame Debug Console 等第三方插件
│  └─ BuildReport/            # 第三方构建报告编辑器工具
├─ Packages/                  # Unity Package Manager 清单与锁文件
└─ ProjectSettings/           # Unity 工程及构建设置
```

## 场景说明

以下场景已按顺序加入 `ProjectSettings/EditorBuildSettings.asset`：

| 顺序 | 场景 | 用途 |
| ---: | --- | --- |
| 0 | `Beginning` | 启动页、背景音频和入口 |
| 1 | `NanYueWenDi` | 南越文帝主题 AR 展示 |
| 2 | `MeiYuDaGuan` | 玉佩等展品的 AR 展示、详情与拼图交互 |
| 3 | `ShengHuoQiJu` | 生活器具主题 AR 展示 |
| 4 | `HaiLuYangFan` | 海上丝绸之路主题 AR 展示 |
| 5 | `Navigation` | 稀疏空间地图加载与定位 |

`Assets/Scenes/Scan.unity` 用于构建并上传稀疏空间地图，目前没有加入构建列表。`GameController` 还保留了前往 `GongTingYanLe` 和 `BingQiCheMa` 的方法，但仓库中没有这两个场景；不要在 UI 中绑定这两个入口，除非补齐场景并同步构建设置。

## 开始使用

### 1. 准备环境

安装 Unity `2021.3.9f1c1`，并按需要添加 Android Build Support、Android SDK、NDK 和 OpenJDK。使用不同补丁版本可能触发资源重导入或场景序列化差异。

### 2. 恢复 EasyAR 依赖

当前 EasyAR 依赖是本地包，清单中的路径为：

```json
"com.easyar.sense": "file:D:/EasyARSenseUnityPlugin_4.6.2+3022.b15d4240/package"
```

该路径不会随仓库一起提供。首次打开项目之前，需要完成以下一种处理：

1. 将同版本 EasyAR Sense Unity Plugin 放到该路径；或
2. 修改 `Packages/manifest.json` 中的 `file:` 地址，使其指向本机的 EasyAR `package` 目录，然后让 Unity Package Manager 重新解析依赖并更新锁文件。

不要用不同 AR SDK 直接替换 EasyAR；业务脚本和场景组件依赖 `easyar` 命名空间及 EasyAR 的序列化类型。

### 3. 配置 EasyAR 服务

EasyAR 配置位于 `Assets/XR/Settings/EasyAR Settings.asset`，其中包含许可证及稀疏空间地图服务配置。请确认使用者拥有有效凭据和对应服务权限。

该文件可能包含敏感凭据。不要把凭据复制到日志、截图、Issue 或文档；若仓库将公开，应先轮换现有凭据并采用合适的密钥管理方式。

### 4. 打开与运行

1. 使用 Unity Hub 或 Unity Editor 打开仓库根目录。
2. 等待 Package Manager 解析依赖并完成首次资源导入。
3. 确认 Console 中没有编译错误或缺失脚本。
4. 打开 `Assets/Scenes/Beginning.unity`，从启动页进入应用。

普通 UI 可以在 Editor 中检查；摄像头、图像识别、多点触控和空间地图能力应在有摄像头的真机上验证。

## Android 构建

1. 打开 **File > Build Settings**，选择 Android 并切换平台。
2. 确认构建场景及顺序与“场景说明”一致。
3. 在 **Player Settings** 中把占位包名 `com.CompanyName.ARMuseum`、公司名、签名和版本号替换为实际发布配置。
4. 确认摄像头及网络权限、EasyAR 许可证和目标服务配置有效。
5. 构建 APK/AAB，并在至少一台支持的 Android 真机上完成冒烟测试。

项目的 APK、AAB 和常见 Unity 构建输出已在 `.gitignore` 中忽略。

## 稀疏空间地图工作流

1. 在 Editor 中打开 `Assets/Scenes/Scan.unity`，或在需要发布地图构建工具时将它加入构建列表。
2. 在支持运动跟踪的真机上扫描环境；进入 Tracking 状态后填写地图名称并保存。
3. 保存成功后，`BuildController` 会把 `MapID` 和 `MapName` 写入 `PlayerPrefs`。
4. 在 `Navigation` 场景中使用相同的地图 ID 和名称启动定位。

当前 `LocalizeController` 会在场景启动时立即调用定位，且读取 `PlayerPrefs` 的条件写成了 `inputID.text.Length < 0`，该条件不会成立。调试导航时应先确保场景输入框或 `MapManagerSource` 已配置正确；若要依赖 Scan 到 Navigation 的自动传递，需要先修正并验证这段逻辑。

## 开发注意事项

- Unity 资源和对应 `.meta` 文件必须一起提交，不能单独重建或丢弃 `.meta`。
- 多个脚本使用 `GameObject.Find`、`Resources.Load`、场景名及对象名硬编码；重命名场景、层级对象或资源前应先搜索所有引用。
- 中英文资源遵循 `Assets/Resources/UI/<类型>/<cn|en>/<名称>_<cn|en>` 的路径约定，修改命名会影响 `LanguageSwitch` 和 `NavInfoController`。
- `StreamingAssets` 中目标图片与同名 `.etd` 数据应成对维护。
- 部分业务脚本使用 GBK 兼容编码，批量格式化或统一转码可能制造大范围无关差异；修改时应检查中文注释和字符串是否保持正确。
- 仓库包含大量模型、PSD、BMP、PNG 和音频资源，当前未配置 Git LFS。替换大文件前应评估仓库体积和团队协作影响。
- 不要在没有明确需求时修改 `Assets/Common`、`Assets/Plugins` 或 `Assets/BuildReport` 内的第三方代码。

更详细的协作和自动化代理规则见 [AGENTS.md](AGENTS.md)。

## 验证建议

仓库当前没有自定义 Edit Mode 或 Play Mode 测试。提交功能改动前至少应检查：

- Unity 完成导入且 Console 无编译错误、缺失脚本或关键空引用。
- 六个构建场景可以按预期切换，返回路径有效。
- 中英文切换后菜单、简介、详情和导航图片正确。
- 音频开关、详情面板、触控旋转/缩放和玉佩拼图可用。
- 各 `.etd` 图像目标可在真机稳定识别，模型和 UI 显示正确。
- Android 构建成功，摄像头权限、横竖屏切换和 EasyAR 初始化正常。
- 若改动空间地图功能，分别验证 `Scan` 的地图保存与 `Navigation` 的加载、开始定位和停止定位。

## 许可

仓库根目录目前没有提供许可证文件。除非项目所有者另行授权，不应假定代码、模型、图片、字体、音频或第三方资源可以对外再分发。
