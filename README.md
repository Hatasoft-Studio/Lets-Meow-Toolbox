# Let's Meow Toolbox

## 一个二次元现代化 UI、新手友好的 Windows 10 / 11 一体化调优工具箱

> [!WARNING]
>
> **作者提醒！**
>
> 这是一个**Vibe-Coding 作品**！如果你**介意使用 Vibe-Coding 作品**，请你自觉离开，**不要提 Issue 来骂人**！这个程序每次发布时都会经过作者亲测，基本上不会出现大问题，**比别的 Vibe-Coding 作品稳定得多！**
>
> 作者制作这个工具，最初只是为了方便自己重置系统后能更快配置电脑 *（毕竟现在的 Win11 反人类设计太多了！尤其是右键菜单）*，后来想到会有很多和作者一样的 <u>"懒人"</u>，于是决定美化一下发出来。**不过，请放心！这个项目作者会一直维护下去的。**
>
> ```python
> total_token_allowance = 114514
> i = input("Queue another message...")
> if i == "我要做一个二次元现代化 UI、新手友好的 Windows 10 / 11 一体化调优工具箱。":
>     print("AI：这得花不少Token，先生。")
>     i2 = input("确定要继续吗？(y/N)")
>     if i2 == "y":
>         total_token_allowance = 0
>         print("AI：恭喜你!你没钱了！")
>     else:
>         print("AI：emmm...我已经做完了...你不早说！")
>         total_token_allowance = 0
>         print("AI：恭喜你!你没钱了！")
> else:
>     print("AI：emmm...我已经做完了...你不早说！")
>     total_token_allowance = 0
>     print("AI：恭喜你!你没钱了！")
> ```

> [!NOTE]
>
> **静待大佬...**
>
> 作者不是什么有才之人，如果**有大佬路过后想要"救"一下这个项目的话**，**那就太好了**！

- **技术栈**：WPF · .NET 8（LTS）· C# · HandyControl（MIT）
- **作者**：Hatasoft-Studio（B站：[Hatasoft工作室](https://space.bilibili.com/1198166444)）
- **许可协议**：GPL-3.0（见 [LICENSE](./LICENSE)）
- **主程序**：`LetsMeowToolbox.exe`
- **当前版本**：v0.2.0（2026-11-06）
- **优点**：体积小、功能基本够用、页面简洁、说明直白
- **在线文档**：<https://doc.lets-meow-toolbox.hatasoft365.top/>

## 功能分区

| 分区 | 定位 | 说明 |
| --- | --- | --- |
| 🟢 新手区 | 默认不启用、无需风险确认 | Win11 经典右键菜单、任务栏/资源管理器微调、关闭广告/遥测/通知推荐、安全 UWP 卸载（白名单）、电源/索引/自动播放/缩略图优化、一键还原 |
| 🟡 高级区 | 勾选风险提示后解锁 | Windows 更新暂停期限/禁用、一键还原 |
| 📦 卸载程序 | 独立页面 | Win32 软件批量卸载、残留扫描与清理、导出清理报告，与「解除占用」联动 |
| 🔓 解除占用 | 独立页面 | 查找占用进程、关闭程序释放占用、开机延迟删除 |
| 🔴 高危插件区 | 外置 DLL 插件 | 关闭 Defender / 关闭防火墙 / 移除 Edge·WebView2。**主程序本体不含任何高危代码**，仅动态加载 `plugins` 目录下的插件 DLL |
| ⚙️ 设置 | 独立页面 | 语言包选择与安装、检查更新、外观、配置预设导入导出、日志目录、一键初始化全部内容、关于与更新日志 |
| 💖 特别鸣谢 | 独立页面 | 本项目集成的其他开源项目 |

## v0.2.0 新增：多语言与软件更新

### 多语言（外置语言包）

- 简体中文是**源语言**，直接内嵌在主程序里，不装任何语言包也能正常使用。
- 繁体中文、日语、英语以 `.meowlang`（JSON 文本）语言包的形式放在程序目录的 `lang` 文件夹，官方随包发布。
- 设置页「语言 / Language」卡片：
  - **选择语言**：打开语言管理窗口，列出 `lang` 目录下全部语言包，可查看详情并加载所选语言，加载后自动重启生效；
  - **安装语言包**：选择 `.meowlang` 文件，校验字段与适用版本后展示详情（包名、制作者、适用版本、安装时间、语言类型、语言编码、文件大小、简介、作者网站），官方语言包带绿色盾牌标识，第三方语言包需二次确认；安装带进度条，成功提示「语言包安装成功！」，失败显示原因与日志路径。
  - **同名语言包更新/降级**：自动比较版本号并二次确认；作者或作者网站不一致时拒绝覆盖，需手动删除旧包。
  - **适用版本**：支持 `*`、`>=0.2.0`、`0.2.0 | 0.2.1 | 0.2.2` 等多版本写法。
- 官方语言包另以 `v1.1.0-lang` 独立发布，方便只更新语言包的用户单独下载。

### 软件更新

- 设置页「软件更新」卡片新增「检查更新」：请求官方更新接口，比较版本后展示最新版本号、更新内容与「前往官网获取新版本」按钮；已是最新时提示当前版本。
- **停止支持版本检测**：接口返回的不受支持版本列表中包含当前版本时，启动即弹出「此版本已停止支持」强制更新提示，更新前无法继续使用本程序。

## 项目结构

```
LetsMeowToolbox/
├── LetsMeowToolbox.sln
├── LICENSE                    # GPL-3.0
├── build/
│   ├── publish-framework-dependent.ps1   # 单文件·依赖运行时
│   └── publish-self-contained.ps1        # 单文件·自包含
├── lang/                                  # 外置语言包（官方：zh-Hant / ja / en）
├── src/
│   ├── LetsMeowToolbox.App/               # 主程序 (LetsMeowToolbox.exe)
│   │   └── Localization/zh-Hans.meowlang  # 内嵌源语言（简体中文）
│   ├── LetsMeowToolbox.PluginAbstractions/# 插件契约接口
│   └── LetsMeowToolbox.Plugins.HighRisk/  # 🔴 高危插件（外置 DLL）
```

## 安全设计

- 所有修改、删除、清理操作**均需用户手动确认**，无任何自动执行逻辑；残留、占用默认不勾选。
- 内置黑名单：系统核心目录（`C:\Windows`、`System32`、`WinSxS`）与系统核心进程禁止强制结束。
- 不加壳、不混淆、无自解压恶意逻辑；底层仅调用微软原生 Win32 API（RestartManager、MoveFileEx、Registry 等）。
- 语言包只是被复制到 `lang` 目录的 JSON 文本，安装过程不会执行语言包中的任何代码。

## 构建与运行

### 环境
- Visual Studio 2022 Community（含".NET 桌面开发"工作负载）
- .NET 8 SDK
- NuGet 还原 HandyControl 3.5.1

### 直接运行（调试）
在 VS 中打开 `LetsMeowToolbox.sln`，F5 运行 `LetsMeowToolbox.App`。构建时高危插件会自动复制到主程序输出目录的 `plugins` 文件夹，`lang/**/*.meowlang` 也会自动复制到输出目录的 `lang/`。

### 单文件发布（两种模式）

**模式一：单文件 · 依赖运行时（小，约 10~20MB，目标机需装有 .NET 8 桌面运行时）**
```powershell
./build/publish-framework-dependent.ps1
# 产物：dist/framework-dependent/LetsMeowToolbox.exe + plugins/ + lang/
```

**模式二：单文件 · 自包含（大，约 90~110MB，目标机无需装 .NET）**
```powershell
./build/publish-self-contained.ps1
# 产物：dist/self-contained/LetsMeowToolbox.exe + plugins/ + lang/
```

也可以手动执行：
```bash
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained false -o dist/framework-dependent
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true  -o dist/self-contained
```
发布后把 `LetsMeowToolbox.Plugins.HighRisk.dll` 复制到 `dist/…/plugins/` 即可启用高危插件区；`lang/` 目录由 csproj 自动复制，无需手动处理。

## 更新日志

### v0.2.0 · 2026-11-06

- 【新增】多语言支持：新增外置语言包机制（`.meowlang`，内容为 JSON 文本），启动时自动扫描程序目录下的 `lang` 文件夹。
- 【新增】简体中文作为源语言直接内嵌于程序本体，无需安装语言包即可使用。
- 【新增】官方语言包：繁体中文、日语、英语，随程序发布在 `lang` 文件夹内。
- 【新增】设置页新增「选择语言」按钮：独立窗口列出 `lang` 目录下的全部语言包，可查看详情并加载所选语言（加载后自动重启生效）。
- 【新增】设置页新增「安装语言包」按钮：选择 `.meowlang` 文件后展示包名、制作者（官方语言包带绿色盾牌标识）、适用版本、安装时间、语言类型、语言编码、文件大小、简介与作者网站。
- 【新增】安装流程：带进度条的安装窗口，成功提示「语言包安装成功！」，失败时显示原因与日志路径；第三方语言包需二次确认。
- 【新增】同名语言包更新/降级：自动比较版本号并二次确认；若作者或作者网站不一致则拒绝覆盖，需手动删除旧语言包。
- 【新增】语言包适用版本支持用 `|` 分隔多个版本号（如 `0.2.0 | 0.2.1 | 0.2.2`）。
- 【新增】设置页新增「检查更新」：向官方更新接口请求最新版本号，与当前版本自动比较；有新版本时展示版本号、更新内容与「前往官网获取新版本」按钮。
- 【新增】停止支持版本检测：更新接口返回的不受支持版本列表中包含当前版本时，启动即弹出强制更新提示，更新前无法继续使用本程序。
- 【修复】语言管理窗口列表项的语言包名称不显示、选中后无高亮效果的问题。

**Full Changelog**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v0.1.2...v0.2.0>

### v0.1.2 · 2026-10-24

- 【新增】所有页面加入网页风格的右侧滚动条：内容超出窗口时自动出现，可上下拖动滑块、点击轨道翻页，滑块悬停/拖动时变粉色，圆角 Win11 样式。
- 【修复】解除占用页占用进程列表在小窗口下高度塌陷、滚轮被内部列表吞掉的问题，现在可随整页正常滚动。

**Full Changelog**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v.0.1.1...v0.1.2>

### v0.1.1 · 2026-10-24

- 【修复】日志系统：日志改为全局记录，记录用户全部设置操作、执行结果与时间（INFO/WARN/ERROR），不再为空文件。
- 【新增】设置页新增「一键初始化全部内容」：还原全部功能设置到默认状态、复位外观设置、清空配置预设。
- 【新增】版本号体系：自此版起为 v0.1.1，关于页显示当前版本；新增「查看更新日志」按钮。
- 【新增】特别鸣谢：每个开源项目新增「访问」按钮，一键打开该项目主页/GitHub 仓库；新增 EZ-Mirror 条目；修复字体模糊（加大字号、加深颜色）。
- 【新增】界面重组：「卸载程序」与「解除占用」从新手区/高级区拆出，各自独立为一个导航分类与独立页面。
- 【新增】关于页新增 GitHub 仓库信息与「前往 GitHub 仓库」一键按钮。
- 【修复】二次元模式下开关与按钮的颜色、文字可读性优化。

**Full Changelog**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/commits/v.0.1.1>

## 特别鸣谢

详见程序内「特别鸣谢」页面。本项目参考并致谢以下开源项目（均附 GitHub 链接）：

- **HandyControl**（MIT）— UI 控件库
- **Open-Shell**（MIT）— 经典右键菜单思路
- **Bulk Crap Uninstaller**（GPL-3.0）— 卸载/残留扫描思路
- **Win11Debloat**（MIT）— UWP 卸载白名单与广告/遥测键位口径
- **Sophia Script**（CC-BY-NC-SA-4.0）— 系统设置脚本思路
- **EZ-Mirror** — 下载加速镜像

## 常见问题（GUI 无法打开时如何排查）

> [!WARNING]
> **作者提醒：**
> 遇到任何问题请先[**查看在线文档**](https://doc.lets-meow-toolbox.hatasoft365.top/)，**在线文档可以解决 99% 的问题**！如果因为您遇到问题不看文档而提的 Issue，我会直接关闭的！

本程序已内置**全局异常捕获与日志**：若启动或运行出错，会弹出错误对话框并写入日志，不会"静默打不开"。

- **日志位置**：`%LocalAppData%\LetsMeowToolbox\logs\meow-日期.log`
  （资源管理器地址栏粘贴 `%LocalAppData%\LetsMeowToolbox\logs` 回车即可）
- **框架依赖版打不开**：请先安装 [.NET 8 桌面运行时](https://dotnet.microsoft.com/download/dotnet/8.0)（选择 ".NET Desktop Runtime 8.x"）。若不想装运行库，请直接使用**自包含（self-contained）**版本，它自带运行库、双击即用。
- **主题说明**：非 Win11 系统可能会自动回退为纯色背景，不影响使用。
- **语言包相关**：语言包安装失败、切换后未生效等问题，见[设置与关于](https://doc.lets-meow-toolbox.hatasoft365.top/docs/settings.html)。
