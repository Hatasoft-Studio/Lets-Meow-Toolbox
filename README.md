# Let's Meow Toolbox  

## 一个二次元现代化 UI、新手友好的 Windows 10 / 11 一体化调优工具箱

> [!WARNING]
>
> **作者提醒！**
>
> 这是一个**Vibe-Coding作品**！如果你**介意使用Vibe-Coding作品**，请你自觉离开，**不要提issue来骂人**！这个程序每次发布时都会经过作者亲测，基本上不会出现大问题，**比别的Vibe-Coding作品稳定得多！**
>
>  
>
> 作者制作这个当初只是为了方便自己重置系统后能更好地快速配置自己的电脑而突然兴起搞的*（毕竟现在的Win11反人类设计太多了！尤其是右键菜单）* ，后面想到会有很多像作者一样的<u>“懒人”</u>*（没有贬低的意思，毕竟作者也是这样的人）*，所以，就决定美化一下发出来了。**不过，请放心！这个项目作者会一直维护下去的。**毕竟...
>
>  
>
> ```python
> total_token_allowance = 114514
> i = input("Queue another message...")
> if i == "我要做一个二次元现代化 UI、新手友好的 Windows 10 / 11 一体化调优工具箱。":
>     print("AI：这得花不少Token，先生。")
>     i2 = input("确定要继续吗？(y/N)")
>     if i2 == "y":
>         total_token_allowance = 0
>         print(f"AI：恭喜你!你没钱了！")
>     else:
>         print("AI：emmm...我已经做完了...你不早说！")
>         total_token_allowance = 0
>         print(f"AI：恭喜你!你没钱了！")
> else:
>         print("AI：emmm...我已经做完了...你不早说！")
>         total_token_allowance = 0
>         print(f"AI：恭喜你!你没钱了！")
> ```
>
> 



> [!NOTE]
>
> **静待大佬...**
>
> 作者不是什么有才之人，如果**有大佬路过后想要“救”一下这个项目的话**，**那就太好了**！



- **技术栈**：WPF · .NET 8（LTS）· C# · HandyControl（MIT）
- **作者**：Hatasoft-Studio（B站：[Hatasoft工作室](https://space.bilibili.com/1198166444)）
- **许可协议**：GPL-3.0（见 [LICENSE](./LICENSE)）
- **主程序**：`LetsMeowToolbox.exe`
- **优点：**

​	体积小、功能基本够用、页面简洁、说明直白

## 功能分区

| 分区 | 定位 | 说明 |
| --- | --- | --- |
| 🟢 新手区 | 默认不启用、无需风险确认 | Win11 经典右键菜单、任务栏/资源管理器微调、关闭广告/遥测/通知推荐、安全 UWP 卸载（白名单）、电源/索引/自动播放/缩略图优化、一键还原 |
| 🟡 高级区 | 勾选风险提示后解锁 | Windows 更新暂停期限/禁用、一键还原 |
| 📦 卸载程序 | 独立页面 | Win32 软件批量卸载、残留扫描与清理、导出清理报告，与「解除占用」联动 |
| 🔓 解除占用 | 独立页面 | 查找占用进程、关闭程序释放占用、开机延迟删除 |
| 🔴 高危插件区 | 外置 DLL 插件 | 关闭 Defender / 关闭防火墙 / 移除 Edge·WebView2。**主程序本体不含任何高危代码**，仅动态加载 `plugins` 目录下的插件 DLL |
| ⚙️ 设置 | 独立页面 | 外观、配置预设导入导出、日志目录、一键初始化全部内容、关于与更新日志 |
| 💖 特别鸣谢 | 独立页面 | 本项目集成的其他开源项目 |

## 项目结构

```
LetsMeowToolbox/
├── LetsMeowToolbox.sln
├── LICENSE                    # GPL-3.0
├── build/
│   ├── publish-framework-dependent.ps1   # 单文件·依赖运行时
│   └── publish-self-contained.ps1        # 单文件·自包含
├── src/
│   ├── LetsMeowToolbox.App/               # 主程序 (LetsMeowToolbox.exe)
│   ├── LetsMeowToolbox.PluginAbstractions/# 插件契约接口
│   └── LetsMeowToolbox.Plugins.HighRisk/  # 🔴 高危插件（外置 DLL）
```

## 安全设计

- 所有修改、删除、清理操作**均需用户手动确认**，无任何自动执行逻辑；残留、占用默认不勾选。
- 内置黑名单：系统核心目录（`C:\Windows`、`System32`、`WinSxS`）与系统核心进程禁止强制结束。
- 不加壳、不混淆、无自解压恶意逻辑；底层仅调用微软原生 Win32 API（RestartManager、MoveFileEx、Registry 等）。

## 构建与运行

### 环境
- Visual Studio 2022 Community（含“.NET 桌面开发”工作负载）
- .NET 8 SDK
- NuGet 还原 HandyControl 3.5.1

### 直接运行（调试）
在 VS 中打开 `LetsMeowToolbox.sln`，F5 运行 `LetsMeowToolbox.App`。构建时高危插件会自动复制到主程序输出目录的 `plugins` 文件夹。

### 单文件发布（两种模式）

**模式一：单文件 · 依赖运行时（小，约 10~20MB，目标机需装有 .NET 8 桌面运行时）**
```powershell
./build/publish-framework-dependent.ps1
# 产物：dist/framework-dependent/LetsMeowToolbox.exe + plugins/
```

**模式二：单文件 · 自包含（大，约 90~110MB，目标机无需装 .NET）**
```powershell
./build/publish-self-contained.ps1
# 产物：dist/self-contained/LetsMeowToolbox.exe + plugins/
```

也可以手动执行：
```bash
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained false -o dist/framework-dependent
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true  -o dist/self-contained
```
发布后把 `LetsMeowToolbox.Plugins.HighRisk.dll` 复制到 `dist/…/plugins/` 即可启用高危插件区。

## 特别鸣谢

详见程序内「💖 特别鸣谢」页面。本项目参考并致谢以下开源项目（均附 GitHub 链接）：

- **HandyControl**（MIT）— UI 控件库
- **Open-Shell**（MIT）— 经典右键菜单思路
- **Bulk Crap Uninstaller**（GPL-3.0）— 卸载/残留扫描思路
- **Win11Debloat**（MIT）— UWP 卸载白名单与广告/遥测键位口径
- **Sophia Script**（CC-BY-NC-SA-4.0）— 系统设置脚本思路

## 常见问题（GUI 无法打开时如何排查）

本程序已内置**全局异常捕获与日志**：若启动或运行出错，会弹出错误对话框并写入日志，不会“静默打不开”。

- **日志位置**：`%LocalAppData%\LetsMeowToolbox\logs\meow-日期.log`
  （资源管理器地址栏粘贴 `%LocalAppData%\LetsMeowToolbox\logs` 回车即可）
- **框架依赖版打不开**：请先安装 [.NET 8 桌面运行时](https://dotnet.microsoft.com/download/dotnet/8.0)（选择 “.NET Desktop Runtime 8.x”）。若不想装运行库，请直接使用**自包含（self-contained）**版本，它自带运行库、双击即用。
- **主题说明**：非 Win11 系统可能会自动回退为纯色背景，不影响使用。

