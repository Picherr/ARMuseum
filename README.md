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

`Assets/Scenes/Scan.unity` 用于构建并上传稀疏空间地图，目前没有加入构建列表。

## 代码架构

业务脚本统一收在 `Assets/Resources/Scripts`（`ARMuseum` 命名空间，程序集 `ARMuseum.Runtime`，引用 `EasyAR.Sense`）。重构后按职责拆分为：

| 组件 | 职责 |
| --- | --- |
| `SceneNames` / `SceneLoader` | 场景名常量与场景跳转统一入口（静态） |
| `AppState` | 应用级全局状态（语言、音频开关） |
| `SceneNavigation` | 各 `changeToXXX` 场景跳转方法，供按钮 onClick 引用 |
| `AudioService` | SFX 播放和全局静音控制；启用时同步开关与图标，背景音乐仍由 `BGM.prefab` 的 AudioSource 播放 |
| `TipController` | 启动页提示与触摸提示 |
| `GamePanelController` | 美玉大观场景的游戏面板与拼图提示 |
| `WireFrameController` | 南越文帝 / 海路扬帆场景的线框图切换 |
| `NavigationController` / `NavInfoController` | 楼层导航与导航信息展示 |
| `LanguageSwitch` | 中英文 UI 切换 |
| `PuzzleDrag` / `YPController` | 玉佩拼图拖拽与匹配判定 |
| `BuildController` / `LocalizeController` | 稀疏空间地图的构建与定位（开发工具） |

优先使用 `[SerializeField]` 字段在 Inspector 里接线，避免运行时 `GameObject.Find` 硬编码路径；仅语言相关贴图按 `Resources.Load` 从 `cn`/`en` 目录动态加载。

## 开始使用

### 1. 准备环境

安装 Unity `2021.3.9f1c1`，并按需要添加 Android Build Support、Android SDK、NDK 和 OpenJDK。使用不同补丁版本可能触发资源重导入或场景序列化差异。

### 2. 恢复 EasyAR 依赖

当前 EasyAR 依赖是本地包，清单中的路径为：

```json
"com.easyar.sense": "file:D:/0Source/EasyARPlugin/EasyARSenseUnityPlugin_4.6.2+3022.b15d4240/package"
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
3. 填写非空地图名称并在跟踪稳定后保存。上传期间禁止重复保存，成功后 `BuildController` 会把 `MapID` 和 `MapName` 写入并保存到 `PlayerPrefs`。
4. 点击“前往定位”进入 `Navigation`；地图 ID 和名称为空时，会自动回填上次保存的信息。
5. 确认 ID 和名称后，先点击“加载地图”，加载成功后再点击“开始定位”；定位时可点击“停止定位”。切换地图前先停止定位，再修改输入并重新加载。

`Scan` 的界面和建图组件使用 Inspector 引用；`Navigation` 不会在启动时自动下载或定位。地图加载、卸载和上传失败后应按界面提示重试；实际服务权限、网络、运动跟踪和定位效果仍需在 Android 真机验证。

## 开发注意事项

- Unity 资源和对应 `.meta` 文件必须一起提交，不能单独重建或丢弃 `.meta`。
- 多个脚本此前用 `GameObject.Find`、`Resources.Load`、场景名及对象名硬编码；重构后大多数 `GameObject.Find` 已改为 `[SerializeField]` 字段，但重命名场景、层级对象或资源前仍应先搜索所有引用。
- 中英文资源遵循 `Assets/Resources/UI/<类型>/<cn|en>/<名称>_<cn|en>` 的路径约定，修改命名会影响 `LanguageSwitch` 和 `NavInfoController`。
- `StreamingAssets` 中目标图片与同名 `.etd` 数据应成对维护。
- `Assets/Resources/Scripts` 下的业务脚本已统一为 UTF-8（包括兼容 UTF-8 的 ASCII 文件）。修改时保留中文注释和字符串，不要批量转码或全文件格式化；第三方文件仍应逐个核对原有编码。
- 仓库已通过 `.gitattributes` 为模型、图像、音频和字体配置部分扩展名的 Git LFS 规则；当前 `git lfs ls-files` 为空，现有资源及历史尚未迁移到 LFS。规则未包含 `.etd`，且除 `.FBX` 外未单独列出大写扩展名；添加资源前应检查属性匹配，不能假定所有二进制格式均已覆盖。替换大文件前仍应评估仓库体积和团队协作影响。
- 不要在没有明确需求时修改 `Assets/Common`、`Assets/Plugins` 或 `Assets/BuildReport` 内的第三方代码。

更详细的协作和自动化代理规则见 [AGENTS.md](AGENTS.md)。

## 验证建议

仓库已引入 Unity Test Framework：

- Edit Mode（`Assets/Tests/EditMode`）：场景常量与全局状态、拼图和触控切换、地图保存/失败恢复、地图场景引用和字体完整性。
- Play Mode（`Assets/Tests/PlayMode`）：音频开关与生命周期、6 个构建场景加载，以及 Editor 中 `Scan → Navigation` 的按钮跳转和地图信息回填。

Windows Editor 不提供地图场景所需的移动端运动跟踪源。场景冒烟测试仅把 SDK 对该设备限制的特定诊断列为预期，其他错误仍会使测试失败；这些测试不能替代 Android 真机的扫描、上传与定位验证。

运行方式：Test Runner 窗口，或批处理 `Unity.exe -batchmode -runTests -testPlatform EditMode|PlayMode`。

提交功能改动前至少还应检查：

- Unity 完成导入且 Console 无编译错误、缺失脚本或关键空引用。
- 六个构建场景可以按预期切换，返回路径有效。
- 中英文切换后菜单、简介、详情和导航图片正确。
- 音频开关、详情面板、触控旋转/缩放和玉佩拼图可用。
- 各 `.etd` 图像目标可在真机稳定识别，模型和 UI 显示正确。
- Android 构建成功，摄像头权限、横竖屏切换和 EasyAR 初始化正常。
- 若改动空间地图功能，分别验证 `Scan` 的地图保存与 `Navigation` 的加载、开始定位和停止定位。

## 许可

仓库根目录目前没有提供许可证文件。除非项目所有者另行授权，不应假定代码、模型、图片、字体、音频或第三方资源可以对外再分发。
