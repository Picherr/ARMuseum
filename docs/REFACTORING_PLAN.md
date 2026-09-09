# ARMuseum 工程级重构计划

> 状态：已批准（2026-09-09）
> 范围：工程级重构（代码 + 工程结构 + 测试 + LFS + 文档）
> 基准仓库：`E:\gitrepo\ARMuseum`（唯一工作副本，`E:\unity project\ARMuseum` 为编辑器临时副本，忽略）

## 1. 目标与原则

### 目标
1. 消除 God object、全局可变状态、硬编码引用等核心坏味道。
2. 用命名空间 + 程序集定义（asmdef）把业务代码与第三方代码隔离。
3. 修复已知 Bug 与死代码。
4. 建立 Edit Mode / Play Mode 测试与可复用的手动回归清单。
5. 用 Git LFS 管理模型、纹理、音频等大文件。
6. 全程保持行为等价：重构不改产品功能，每阶段可编译、可运行、可回滚。

### 原则
- **小步提交**：不同的改动点分成不同提交；一个提交只做一件事。
- **行为等价优先**：重构与修 Bug 分开提交，修 Bug 单独可追溯。
- **不碰第三方**：`Assets/Common`、`Assets/Plugins`、`Assets/BuildReport` 内的第三方代码保持原样（本计划第 3 点确认：Vuforia 示例代码保留、隔离）。
- **资源与 `.meta` 成对提交**：重命名/移动必须带 `.meta`。

## 2. 分支与提交策略

### 分支模型
- 基准分支：`master`（当前，与 `origin/master` 同步）。
- 集成分支：`refactor/engineering`（长期，从 `master` 切出，各阶段分支合并到此）。
- 阶段分支：每个阶段一条，从 `refactor/engineering` 切出，完成后以 PR 合并回 `refactor/engineering`。
- 最终：全部回归通过后，`refactor/engineering` 合并回 `master`。

| 阶段 | 分支名 |
| --- | --- |
| Phase 0 基线 + 安全网 | `refactor/phase0-baseline` |
| Phase 1 低风险清理 | `refactor/phase1-cleanup` |
| Phase 2 解除硬编码 | `refactor/phase2-decouple` |
| Phase 3 拆分与状态管理 | `refactor/phase3-split` |
| Phase 4 测试 | `refactor/phase4-tests` |
| Phase 5 文档收尾 | `refactor/phase5-docs` |

### 提交信息规范
- 标题格式：`type: 中文概括式标题`，50–72 字符。
- `type` 取值：`refactor` / `fix` / `style` / `test` / `docs` / `build` / `chore` / `feat` / `security`。
- 复杂改动在标题后空一行写正文（背景/原因、主要改动点、影响）。
- 不逐文件罗列、不写本机绝对路径、不编造验证结果。

### 提交拆分规则
- 每个「改动点」一个提交，标题描述改动性质而非文件名。
- 纯移动/重命名单独成提交，不混入逻辑修改。
- 编码转换、命名空间引入这类大范围机械改动，单独成提交，避免污染业务改动 diff。
- 每阶段结束后本地运行 `git status` 确认无遗漏，再合并。

## 3. 分阶段计划

---

### Phase 0 — 基线 + 安全网

**分支**：`refactor/phase0-baseline`（从 `refactor/engineering` 切出）

**目的**：在动手前建立可回滚的基线，并把大文件纳入 LFS。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 0.1 | `chore` | 添加 Git LFS 规则并跟踪大文件 | 新增 `.gitattributes`，为 `Assets/Resources/Models`、`Assets/Resources/Animations`、`Assets/Resources/Audio`、`Assets/Resources/UI`、`Assets/Resources/WireFrame`、`Assets/StreamingAssets` 下的大二进制（模型 `.fbx/.obj`、纹理 `.png/.psd/.bmp/.tga`、音频、`.etd`）配置 LFS 规则 | 需本地 `git lfs install`，且 GitHub 仓库侧已开启 LFS。**历史大文件重写需 `git lfs migrate`，会改写提交历史，合并前需 force push，需确认远端无他人分支依赖** |
| 0.2 | `docs` | 新增工程级重构计划文档 | 新增 `docs/REFACTORING_PLAN.md`（本文档） | 无 |
| 0.3 | `docs` | 新增手动回归检查清单 | 把 README「验证建议」固化为 `docs/REGRESSION_CHECKLIST.md`，作为每阶段回归依据 | 无 |

**验证**：
- `git lfs track` 列出的规则生效，`git lfs ls-files` 能看到跟踪文件（若执行了 migrate）。
- 分支可从 `master` 干净切出，无未提交改动。

---

### Phase 1 — 低风险清理

**分支**：`refactor/phase1-cleanup`

**目的**：只做机械、低风险、可独立验证的清理与 Bug 修复，不改变架构。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 1.1 | `refactor` | 抽取场景名常量并替换字符串字面量 | 新增 `SceneNames` 常量类；替换 `GameController`、`NavigationController`、`NavInfoController` 等脚本中的 `"NanYueWenDi"`、`"MeiYuDaGuan"`、`"HaiLuYangFan"`、`"ShengHuoQiJu"`、`"Navigation"`、`"Beginning"` 字符串 | 低风险。所有字符串字面量必须一一对应替换，不可遗漏，否则场景跳转失效 |
| 1.2 | `refactor` | 移除无效场景跳转与死代码 | 删除 `GameController.changeToGTYY` / `changeToBQCM`（场景不存在）、`changeToMM` 空转间接层、未使用的 `using static UnityEngine.GraphicsBuffer` | 低风险。需确认无 UI 按钮绑定这些方法（README 已说明不要在 UI 绑定） |
| 1.3 | `fix` | 修复定位地图读取 PlayerPrefs 的条件 | `LocalizeController`：`inputID.text.Length < 0` → `inputID.text.Length == 0` | 行为变化（原本恒不读取）。需真机/场景验证 Scan→Navigation 的自动传递 |
| 1.4 | `fix` | 移除 GameController 中无效的 Sprite 实例化 | `GameController.Audio()` 中的 `Instantiate(sprite1)` / `Instantiate(sprite2)`（Sprite 非 GameObject）删除 | 低风险，消除运行时告警/潜在泄漏 |
| 1.5 | `style` | 统一业务脚本编码为 UTF-8 | 将 `Assets/Resources/Scripts` 下脚本统一为 UTF-8（保留中文注释与字符串原义） | 中风险。README 已提示 GBK 兼容风险；只做编码规范化，**不做格式化/重排**，diff 应最小 |
| 1.6 | `refactor` | 为业务脚本引入命名空间 | 为 `Assets/Resources/Scripts` 下业务脚本加 `ARMuseum.*` 命名空间（按目录：`ARMuseum.Navigation`、`ARMuseum.Audio`、`ARMuseum.UI` 等）并补 `using` | 中风险。纯机械包裹，但会改动每个文件；需编译通过、无命名冲突 |

**验证**：
- 每步 `git commit` 前确认 Unity Console 无编译错误。
- 阶段末跑一遍手动回归清单中「场景切换 / 中英文切换 / 音频开关」。

---

### Phase 2 — 解除硬编码引用

**分支**：`refactor/phase2-decouple`

**目的**：把运行期脆弱引用改为编译期安全引用，但不动类职责结构（拆分类留给 Phase 3）。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 2.1 | `refactor` | 抽取 SceneLoader 服务并收敛场景跳转 | 新增 `SceneLoader`（封装 `SceneManager.LoadScene`）；`GameController.changeToXXX` 改为调用 `SceneLoader`，其余脚本直接 `LoadScene` 处统一收敛 | 中风险。涉及多处跳转入口，需逐场景回归返回路径 |
| 2.2 | `refactor` | 用协程替换字符串 Invoke 调用 | `GameController` 等脚本的 `Invoke("changeToMM", …)`、`Invoke("AudioTurnOff", …)`、`Invoke("TouchTipOff", …)` 改为协程 | 低风险。行为等价，消除字符串方法名的编译期不安全 |
| 2.3 | `refactor` | 业务脚本 GameObject.Find 改为序列化字段 | `NavigationController`、`NavInfoController`、`MenuUIController`、`LanguageSwitch`、`MapSizeController` 等脚本的 `GameObject.Find` 改为 `[SerializeField]` + Inspector 拖拽 | 中风险。**Inspector 引用需在 Unity 内重新拖拽**，否则运行时空引用；`GameController` 的 `Find` 留到 Phase 3 拆分时一并处理 |
| 2.4 | `refactor` | Resources.Load 改为序列化引用 | `GameController.Audio()` 等 `Resources.Load<Sprite>` 改为 `[SerializeField] Sprite` 引用 | 中风险。需在 Inspector 重新指定资源 |

**验证**：
- Unity 内重新接线所有序列化字段，Console 无空引用告警。
- 真机冒烟：触控旋转/缩放、地图拖动、中英文切换、音频开关。

---

### Phase 3 — 拆分 God object 与状态管理

**分支**：`refactor/phase3-split`

**目的**：拆解 `GameController`，用正式状态管理取代 `Variables` 静态全局，整合音频，并用 asmdef 隔离业务代码。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 3.1 | `refactor` | 拆分 GameController 为职责单一的服务 | 拆出 `SceneNavigationService`、各场景 AR Presenter（`NanYueWenDiPresenter`/`MeiYuDaGuanPresenter`/`HaiLuYangFanPresenter`）、`GamePanelController`、`TipController`；`GameController` 内 `GameObject.Find`/`imagetarget` 逻辑迁到对应 Presenter 并序列化 | 高风险。需逐个场景验证 WireFrame 显隐、拼图交互、提示；`index`/`MYDGUpdate` 的循环边界需仔细核对并加注释 |
| 3.2 | `refactor` | 用 AppState 单例取代 Variables 静态全局 | 删除 `Variables`；新增 `AppState`（`IsChinese`、`IsAudioOn`），通过单例或 DI 访问 | 中风险。全局状态访问点（`LanguageSwitch`、`GameController.Audio`、`NavInfoController` 等）全部替换，编译期应能暴露遗漏 |
| 3.3 | `refactor` | 整合音频脚本为 AudioService | `AudioManager` + `BGMController` + `DontDestroyBgm` 合并为 `AudioService`（统一背景音乐 + 音效 + 跨场景存活） | 中风险。需验证背景音乐跨场景不中断、开关状态一致 |
| 3.4 | `build` | 为业务代码添加程序集定义 | 新增 `Assets/Resources/Scripts/ARMuseum.Runtime.asmdef`，业务代码从 `Assembly-CSharp` 隔离，显式引用 `easyar` 等依赖 | 中风险。需检查第三方代码对业务代码的引用（若有，需补引用或调整 asmdef 范围），Unity 编译通过为准 |

**验证**：
- 六个构建场景全部切换正常，返回路径有效。
- 中英文切换、音频开关、详情面板、触控、拼图、地图导航逐项回归。
- Android 构建成功，真机 AR 识别、EasyAR 初始化正常。

---

### Phase 4 — 测试

**分支**：`refactor/phase4-tests`

**目的**：为纯逻辑与场景加载建立自动化测试，防止后续回归。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 4.1 | `build` | 引入 Unity Test Framework 与测试程序集 | `com.unity.test-framework` 依赖；新增 `Assets/Tests` 及 EditMode/PlayMode 测试 asmdef | 低风险 |
| 4.2 | `test` | 为场景路由与状态切换添加 Edit Mode 测试 | 覆盖 `SceneNames` 常量、`SceneLoader` 路由、`AppState` 状态切换 | 低风险，纯逻辑 |
| 4.3 | `test` | 为语言切换逻辑添加 Edit Mode 测试 | 覆盖 `LanguageSwitch` 的中英文路径推导逻辑 | 低风险，纯逻辑 |
| 4.4 | `test` | 添加场景加载 Play Mode 冒烟测试 | 覆盖六个构建场景的加载与基本返回 | 中风险，需 Unity 环境运行 |

**验证**：
- `Test Runner` 全绿；Play Mode 测试可在编辑器跑通。
- 测试本身不依赖真机摄像头（AR 识别仍靠手动清单）。

---

### Phase 5 — 文档收尾

**分支**：`refactor/phase5-docs`

**目的**：让文档反映重构后的真实结构。

**提交拆分**：

| # | type | 提交标题 | 改动点 | 影响 / 风险 |
| --- | --- | --- | --- | --- |
| 5.1 | `docs` | 更新 README 目录结构与架构说明 | 更新目录结构、脚本组织（命名空间/asmdef）、SceneLoader/AudioService/AppState 架构说明 | 无 |
| 5.2 | `docs` | 更新 AGENTS.md 协作规则 | 同步命名空间、asmdef、引用规范等新约定 | 无 |

**验证**：文档与实际结构一致，无失效路径引用。

---

## 4. 风险与回滚

- **大文件 LFS 历史重写**：`git lfs migrate` 会改写历史，需 force push。若远端已被多人依赖，改为「只对新增大文件启用 LFS」，不重写历史。
- **序列化字段重接线**：Phase 2/3 把运行期引用改为 Inspector 引用后，场景/预制体的引用必须重新拖拽，漏接会导致空引用。以 Console 空引用告警 + 手动清单为准。
- **编码转换**：只做规范化，不格式化，避免大范围无关 diff。
- **回滚**：每个阶段独立分支，可单独 revert 合并提交；`refactor/engineering` 在最终回归前不合并回 `master`。

## 5. 执行顺序与依赖

```
master ──▶ refactor/engineering ──▶ phase0-baseline ──▶ phase1-cleanup ──▶ phase2-decouple
                                           │                                    │
                                           └──────────────▶ phase3-split ◀────┘
                                                                  │
                                            phase5-docs ◀── phase4-tests
                                                                  │
                                              refactor/engineering ──▶ master
```

依赖关系：
- Phase 1 依赖 Phase 0 的基线分支。
- Phase 2 依赖 Phase 1 的命名空间/常量（避免冲突）。
- Phase 3 依赖 Phase 2 的 SceneLoader 与协程。
- Phase 4 依赖 Phase 3 拆出的可测逻辑。
- Phase 5 依赖 Phase 3 的最终结构。
