# AGENTS.md

本文适用于仓库根目录及其所有子目录，用于约束在 ARMuseum 项目中工作的自动化代理和协作者。若更深层目录以后出现新的 `AGENTS.md`，以距离目标文件最近的规则为准。

## 项目定位

- 这是一个 Unity `2021.3.9f1c1` 移动端 AR 博物馆项目，主要目标平台为 Android。
- 当前业务 AR SDK 是 EasyAR Sense `4.6.2+3022.b15d4240`。
- 核心业务代码位于 `Assets/Resources/Scripts`；核心场景位于 `Assets/Scenes`。
- `Assets/Common` 是保留的 Vuforia 示例公共资源，`Assets/Plugins` 和 `Assets/BuildReport` 主要是第三方代码。除非任务明确要求，不要修改这些目录。
- 项目包含大量二进制模型、图片、音频和动画资源。优先做最小、可审查的改动。

## 先读后改

开始任务前至少检查：

1. `git status --short`，识别并保留用户已有改动。
2. `README.md`、`Packages/manifest.json`、`ProjectSettings/ProjectVersion.txt` 和 `ProjectSettings/EditorBuildSettings.asset`。
3. 与任务相关的场景、预制体、脚本及其 `.meta` 文件。
4. 修改场景名、对象名、资源名或公共方法前，使用 `rg` 搜索字符串引用和 GUID 引用。

不要把 Unity 生成目录当作源文件。`Library`、`Temp`、`Obj`、`Build`、`Builds`、`Logs` 和 `UserSettings` 不应提交。

## 关键目录边界

| 路径 | 作用 | 修改原则 |
| --- | --- | --- |
| `Assets/Resources/Scripts` | 项目业务逻辑 | 主要代码修改区，保持现有场景绑定兼容 |
| `Assets/Scenes` | 应用与地图场景 | 尽量通过 Unity Editor 修改并逐场景验证 |
| `Assets/Resources/Prefabs` | 菜单、详情、导航等预制体 | 检查实例覆盖和序列化字段 |
| `Assets/Resources/UI` | 中英文 UI 图片 | 保持目录和 `_cn`/`_en` 命名约定 |
| `Assets/StreamingAssets` | EasyAR 目标图片与 `.etd` 数据 | 成对维护，避免改名和压缩处理破坏识别 |
| `Assets/XR/Settings` | EasyAR 设置及服务凭据 | 视为敏感配置，不在输出中暴露值 |
| `Assets/Common` | Vuforia 示例代码/资源 | 第三方边界，默认不改 |
| `Assets/Plugins` | Ingame Debug Console | 第三方边界，默认不改 |
| `Assets/BuildReport` | Build Report Tool | 第三方边界，默认不改 |
| `Packages` | UPM 依赖 | 更新清单后让 Unity 正常解析锁文件 |
| `ProjectSettings` | Unity 工程和构建设置 | 只做任务要求的定点修改 |

## 场景与导航约束

构建列表的有效顺序为：

1. `Beginning`
2. `NanYueWenDi`
3. `MeiYuDaGuan`
4. `ShengHuoQiJu`
5. `HaiLuYangFan`
6. `Navigation`

`Scan` 是稀疏空间地图构建场景，当前不在构建列表。修改构建场景时必须同步检查 `SceneNavigation` 中的场景跳转逻辑和 UI Button 绑定。

导航面板里的 `GTYYBtn` / `BQCMBtn`（宫廷雅乐 / 兵器车马）目前仅作为主题标签展示，对应展馆场景尚不存在；不要默认启用这些入口，也不要为了消除引用而创建空场景，只有在需求明确且内容齐备时再补充。

## Unity 资源与序列化规则

- 所有位于 `Assets` 下的资源必须与其 `.meta` 文件一起移动、重命名或提交；保留 GUID 稳定。
- 不要手工重新生成 `.meta`，不要只移动资源而遗漏 `.meta`。
- 场景和预制体优先使用匹配版本的 Unity Editor 修改。确需文本编辑 YAML 时，只做局部改动，并检查 YAML 结构、fileID、GUID 和换行未被批量重写。
- 修改 `MonoBehaviour` 的字段名或类型会影响已有序列化数据。需要重命名时考虑 `FormerlySerializedAs`，并检查所有场景和预制体实例。
- 被 UnityEvent、Button 或动画事件调用的 `public` 方法可能没有普通 C# 调用方；搜索不到代码引用不代表可以删除或重命名。
- 大多数业务脚本已把 `GameObject.Find` 硬编码路径改为 `[SerializeField]` 字段，但 `Navigation/BuildController` 仍用 `GameObject.Find`（其 `Scan` 场景缺 `SceneMaster` 预制体，路径已失效）。层级调整前必须搜索并同步调用方。
- `Resources.Load` 路径依赖文件位置和命名。中英文 UI 保持 `<name>_cn`、`<name>_en` 后缀以及现有 `cn`/`en` 目录结构。
- EasyAR 目标数据使用目标图片与 `.etd` 文件配对。修改目标图后应使用正确 EasyAR 工具重新生成数据并在真机验证。
- 不要无目的地触发全项目重导入、材质升级、纹理重新压缩或序列化版本升级。

## 代码约定

- 保持 Unity `2021.3` 支持的 C#/.NET API 范围，不引入要求更高 Unity 或运行时版本的语法和包。
- `MonoBehaviour` 文件名与类型名保持一致。新组件优先使用清晰的 PascalCase 类型/方法名和 camelCase 局部变量。
- Inspector 引用优先使用 `[SerializeField] private`；不要把仅为序列化而存在的字段无理由扩大为公共 API。
- 在 `Awake`、`Start`、`OnEnable`、`OnDisable` 中成对管理事件订阅，避免重复注册和场景切换后的泄漏。
- 对 `FindObjectOfType`、`GameObject.Find`、`Resources.Load` 和 Inspector 引用增加必要的空值保护，但不要在未验证场景绑定时进行大范围重构。
- 保留由 UI 和场景序列化绑定的方法签名。需要破坏性调整时，同时更新所有场景、预制体和 UnityEvent。
- 新日志应说明场景或功能上下文，避免每帧输出；不要记录许可证、API Key、API Secret、地图服务密钥或其他敏感值。
- 业务脚本存在 ASCII/UTF-8 与 GBK 兼容编码混用。读取中文乱码时先按 GBK 检查；不要把转码、全文件格式化和功能改动混在同一提交中。
- 删除无用代码前确认没有 UnityEvent、动画事件、反射或序列化引用。

## EasyAR 与 Vuforia

- `Packages/manifest.json` 中 EasyAR 当前使用开发机绝对路径 `file:D:/EasyARSenseUnityPlugin_4.6.2+3022.b15d4240/package`。在其他机器上验证前需要将该依赖改为有效路径或恢复同一路径。
- 不要静默升级 EasyAR。升级时应单独说明版本、迁移影响，并验证图像跟踪、稀疏空间地图、Android 构建和所有 EasyAR 场景组件。
- `Assets/XR/Settings/EasyAR Settings.asset` 含许可证和服务配置。读取时必须遮蔽敏感值；不要把值复制到回复、日志、测试快照或新文档。
- `Assets/Common` 的 Vuforia 程序集通过 `VUFORIA_ENABLED` 和包版本定义保护。不要为了让 EasyAR 功能工作而安装 Vuforia，也不要移除这些条件约束，除非任务明确要求迁移或清理。

## 大文件与第三方资源

- 仓库当前没有 `.gitattributes`/Git LFS 配置，但包含大型 FBX、PSD、BMP、PNG 和 MP3。不要无理由复制、重新导出或替换大文件。
- 二进制资源无法有效文本审查。替换时记录来源、用途、尺寸/压缩变化，并验证 Unity Import Settings 和运行时显示。
- 保留第三方文件的许可证头和原始结构。修改第三方代码必须是最小补丁，并在交付说明中明确指出。
- 根目录未提供统一许可证，不要假定项目素材或第三方资源允许外部分发。

## 文件删除安全规则

禁止批量删除文件或目录。

不要使用：

- `del /s`
- `rd /s`
- `rmdir /s`
- `Remove-Item -Recurse`
- `rm -rf`

需要删除文件时，只能一次删除一个已经核对过的明确文件路径，例如：

```powershell
Remove-Item "C:\path\to\file.txt"
```

如果任务需要批量删除文件，应停止操作并请用户手动删除。不得删除仓库根目录、`Assets`、`Packages`、`ProjectSettings` 或任何未解析的变量/通配符目标。

## 验证流程

仓库已引入 Unity Test Framework：

- Edit Mode（`Assets/Tests/EditMode`）：`SceneNames`、`AppState` 纯逻辑测试。
- Play Mode（`Assets/Tests/PlayMode`）：`SceneLoader` 逐一加载 Build Settings 中 6 个应用场景的冒烟测试。

批处理运行方式：

```powershell
& "<Unity-Editor-Path>\Unity.exe" -batchmode -nographics -projectPath "<repo-root>" -runTests -testPlatform EditMode -testResults results.xml
```

根据改动范围执行尽可能完整的验证：

### 纯文档改动

- 检查 Markdown 结构、链接、路径、版本号和场景清单。
- 使用 `git diff --check` 检查空白错误。

### C# 改动

- 使用正确 EasyAR 依赖打开项目，等待 Unity 编译完成。
- 确认 Console 无新增编译错误、Missing Script、关键空引用和重复事件异常。
- 进入受影响场景执行 Play Mode 冒烟测试。

若本机有匹配版本 Unity Editor，可用以下形式做无界面导入/编译检查；替换为真实 Editor 路径，不要把其他名为 `unity.exe` 的工具误当 Unity Editor：

```powershell
& "<Unity-Editor-Path>\Unity.exe" -batchmode -quit -projectPath "<repo-root>" -logFile -
```

### 场景、预制体或资源改动

- 打开并保存受影响的每个场景，确认没有 Missing Script/Prefab。
- 检查 Inspector 引用、UnityEvent、动画控制器、资源加载路径和预制体覆盖。
- 检查 `ProjectSettings/EditorBuildSettings.asset` 中的场景顺序没有意外变化。

### AR、触控、音频或构建改动

- 在 Android 真机验证摄像头权限、EasyAR 初始化和目标识别。
- 验证单指旋转、双指缩放、UI 拖拽、语言切换、音频开关和场景返回路径。
- 修改空间地图时同时验证 `Scan` 的保存回调和 `Navigation` 的加载、定位、停止定位流程。
- 修改发布配置时至少完成一次目标架构的 Android 构建，并记录实际执行的验证，不得声称未运行的测试已通过。

## 完成标准

交付前确认：

- 改动只覆盖用户请求，没有覆盖或整理无关的用户改动。
- 没有泄露 EasyAR 或其他服务凭据。
- 新增/移动的 Unity 资源包含正确 `.meta`；根目录 Markdown 等非 Asset 文件不需要 `.meta`。
- 硬编码的场景名、对象路径、资源路径和 UI 绑定已同步。
- 已执行与风险相称的检查，并如实说明未能执行的真机、Unity 或构建验证。
- `git diff --check` 通过，最终差异中没有意外的大型二进制文件或批量序列化改写。

## 提交信息与 PR 说明规范

### 提交信息

- 提交信息必须基于实际改动，不得编造测试结果、Issue 编号、部署情况或其他未验证信息。
- 优先匹配仓库现有提交风格和主要语言；仅在仓库已使用 Conventional Commits 或用户明确要求时采用该格式，否则使用中文格式 `type: 中文概括式标题`。
- 标题类型取 `feat`、`fix`、`style`、`refactor`、`perf`、`test`、`docs`、`build`、`ci`、`chore`、`security`；标题 50–72 字符，描述改动性质，不使用文件名作为类型或标题主体。
- 小型内聚改动只写标题；复杂改动在空行后写正文，说明背景与原因、主要改动点，以及用户可见、兼容性、数据迁移、测试、风险或限制方面的影响。
- 不逐文件罗列改动，不包含本机绝对路径、临时目录或工具缓存路径。
- 只有确实执行过测试或检查时才写"已通过……验证"；未运行测试时不要在提交信息中写"未测试"。

### Pull Request 标题与正文

- PR 标题和正文必须基于实际分支差异、提交记录和已知上下文，不得编造测试结果、Issue 编号、兼容性结论、部署情况或其他未验证信息。
- 优先匹配仓库现有 PR 风格和主要语言；无法判断时使用简体中文。
- PR 标题概括整个变更的主要目的或用户可见结果，简洁明确，通常不超过 72 字符，不以句号结尾。仓库使用类型前缀时采用 `type: 中文概括式标题`。
- PR 正文默认采用以下结构，只填充实际存在的内容，不生成空章节、不虚构内容：
  - `## 背景`：要解决的问题、现象、需求或修改原因。
  - `## 主要改动`：关键行为、结构或界面变化及必要理由；多个独立改动用项目符号分行，不逐文件罗列。
  - `## 影响与注意事项`：用户可见影响、兼容性、数据或迁移影响、风险、限制和回滚注意事项；无实质影响时简要说明。
  - `## 验证`：仅列出实际执行过的测试或检查并简述结果；未运行测试时写"未运行（未要求）"。
- 仅在上下文确实存在关联 Issue、迁移步骤、截图要求或部署说明时增加相应章节。
- 正文重点解释为什么修改、改了什么以及产生什么影响，保持详细但避免重复 PR 标题、提交记录或文件清单。
