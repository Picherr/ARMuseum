# 手动回归检查清单

> 用途：每次重构阶段合并前逐项回归，作为「行为等价」的验收依据。
> 说明：AR 识别、触控、摄像头、空间地图能力依赖真机，无法在 Editor 内完整验证。

## A. 编辑器静态检查

- [ ] Unity 完成导入，Console 无编译错误
- [ ] Console 无「Missing Script」或缺失脚本告警
- [ ] 无关键空引用（NullReferenceException）在启动路径上
- [ ] `Assets/Common`、`Assets/Plugins`、`Assets/BuildReport` 第三方代码未被改动

## B. 编辑器运行检查（六个构建场景）

场景切换顺序：`Beginning → NanYueWenDi / MeiYuDaGuan / ShengHuoQiJu / HaiLuYangFan / Navigation`

- [ ] `Beginning` 启动页正常，背景音频正常，可进入各主题入口
- [ ] `NanYueWenDi` 南越文帝主题可进入、返回路径有效
- [ ] `MeiYuDaGuan` 美玉大观：详情面板、玉佩拼图拖拽可用
- [ ] `ShengHuoQiJu` 生活器具主题可进入、返回路径有效
- [ ] `HaiLuYangFan` 海上丝绸之路主题可进入、返回路径有效
- [ ] `Navigation` 导航场景可加载（不要求定位成功，见 E 节）

## C. 功能检查

- [ ] 中英文切换：菜单、简介、详情、导航图片均随语言切换正确
- [ ] 音频开关：静音/恢复状态正确，图标切换正确
- [ ] 详情面板：打开/关闭正常
- [ ] 触控：单指旋转、双指缩放、地图拖动及缩放正常
- [ ] WireFrame 展示序列：目标激活时面板显隐正确

## D. Android 构建检查

- [ ] 构建场景及顺序与 `ProjectSettings/EditorBuildSettings.asset` 一致
- [ ] Android 构建成功（IL2CPP、ARMv7 + ARM64、最低 API 22）
- [ ] 摄像头权限、网络权限正常
- [ ] 横竖屏切换正常
- [ ] EasyAR 初始化正常，无许可证报错
- [ ] 各 `.etd` 图像目标在真机稳定识别，模型与 UI 显示正确

## E. 空间地图专项（仅改动导航功能时）

- [ ] `Scan` 场景：扫描环境进入 Tracking 状态后可保存地图，`MapID`/`MapName` 写入 `PlayerPrefs`
- [ ] `Navigation` 场景：使用相同地图 ID/名称可加载
- [ ] 开始定位 / 停止定位正常，定位结果文本提示正确
- [ ] Scan → Navigation 的 `MapID`/`MapName` 自动传递生效（依赖 `LocalizeController` 的 `PlayerPrefs` 读取逻辑）
