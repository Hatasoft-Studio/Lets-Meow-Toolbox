> 🌐 [简体中文](./README.md) · [繁體中文](./README.zh-Hant.md) · [日本語](./README.ja.md) · **English**

# Let's Meow Toolbox

## A modern anime-style UI, beginner-friendly all-in-one tuning toolbox for Windows 10 / 11

> [!WARNING]
>
> **Author's note!**
>
> This is a **Vibe-Coding project**! If you **mind using Vibe-Coding projects**, please leave on your own accord, and **don't open Issues just to flame me**! This program is personally tested by the author before every release and basically won't have major issues, **far more stable than other Vibe-Coding projects!**
>
> The author originally built this tool just to make it faster to configure a computer after reinstalling the system *(after all, Win11 has way too many anti-human designs nowadays! Especially the context menu)*, and later realized there would be many <u>"lazy people"</u> just like the author, so decided to polish it up and publish it. **But rest assured! The author will keep maintaining this project.**
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
> **Waiting for a big shot...**
>
> The author isn't a particularly talented person. If **a big shot passes by and wants to "save" this project**, **that would be wonderful**!

- **Tech stack**: WPF · .NET 8 (LTS) · C# · HandyControl (MIT)
- **Author**: Hatasoft-Studio (Bilibili: [Hatasoft Studio](https://space.bilibili.com/1198166444))
- **License**: GPL-3.0 (see [LICENSE](./LICENSE))
- **Main program**: `LetsMeowToolbox.exe`
- **Current version**: v0.2.0 (2026-11-06)
- **Strengths**: small size, features are basically sufficient, clean pages, straightforward instructions
- **Online documentation**: <https://doc.lets-meow-toolbox.hatasoft365.top/>

## Feature Zones

| Zone | Role | Description |
| --- | --- | --- |
| 🟢 Beginner Zone | Disabled by default, no risk confirmation required | Win11 classic context menu, taskbar/File Explorer fine-tuning, disable ads/telemetry/notification suggestions, safe UWP uninstall (whitelist), power/indexing/autoplay/thumbnail optimization, one-click restore |
| 🟡 Advanced Zone | Unlocked after ticking the risk warning | Windows Update pause period/disable, one-click restore |
| 📦 Uninstaller | Standalone page | Batch uninstall of Win32 software, leftover scanning and cleanup, export cleanup report, integrated with "Release Lock" |
| 🔓 Release Lock | Standalone page | Find the process holding the lock, close the program to release the lock, delete on reboot |
| 🔴 High-Risk Plugin Zone | External DLL plugins | Disable Defender / disable firewall / remove Edge·WebView2. **The main program itself contains no high-risk code**; it only dynamically loads plugin DLLs from the `plugins` directory |
| ⚙️ Settings | Standalone page | Language pack selection and installation, Check for updates, appearance, config preset import/export, log directory, one-click reset of all content, About and changelog |
| 💖 Special Thanks | Standalone page | Other open-source projects integrated into this project |

## New in v0.2.0: Multilingual Support and Software Updates

### Multilingual (external language packs)

- Simplified Chinese is the **source language**, embedded directly in the main program, so it works normally without installing any language pack.
- Traditional Chinese, Japanese, and English are shipped as `.meowlang` (JSON text) language packs in the `lang` folder of the program directory, released officially together with the program.
- The "Language" card on the Settings page:
  - **Select language**: opens the language management window, listing all language packs in the `lang` directory; you can view details and load the selected language, which takes effect after an automatic restart;
  - **Install language pack**: select a `.meowlang` file; after validating the fields and compatible version, the details are displayed (package name, author, compatible version, install time, language type, language code, file size, description, author website). Official language packs carry a green shield badge; third-party language packs require a second confirmation. Installation shows a progress bar, with the success message "Language pack installed successfully!" and, on failure, the reason and log path.
  - **Update/downgrade of a same-named language pack**: version numbers are compared automatically and a second confirmation is required; if the author or author website differs, overwriting is refused and the old pack must be deleted manually.
  - **Compatible version**: supports multi-version notations such as `*`, `>=0.2.0`, `0.2.0 | 0.2.1 | 0.2.2`.
- Official language packs are additionally released separately as `v1.1.0-lang`, so users who only want to update the language packs can download them individually.

### Software Updates

- The "Software Updates" card on the Settings page now has "Check for updates": it requests the official update API, compares versions, and then shows the latest version number, update details, and a "Get the new version from the official website" button; if already up to date, it shows the current version.
- **End-of-support version detection**: if the current version is in the unsupported-version list returned by the API, a forced-update prompt "This version is no longer supported" appears at startup, and the program cannot be used until it is updated.

## Project Structure

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

## Security Design

- All modification, deletion, and cleanup operations **require manual user confirmation**; there is no automatic execution logic whatsoever. Leftovers and locks are unchecked by default.
- Built-in blacklist: forced termination is prohibited for core system directories (`C:\Windows`, `System32`, `WinSxS`) and core system processes.
- No packing, no obfuscation, no self-extracting malicious logic; underneath it only calls native Microsoft Win32 APIs (RestartManager, MoveFileEx, Registry, etc.).
- A language pack is nothing more than JSON text copied into the `lang` directory; the installation process never executes any code from the language pack.

## Build and Run

### Environment
- Visual Studio 2022 Community (with the ".NET desktop development" workload)
- .NET 8 SDK
- NuGet restores HandyControl 3.5.1

### Run directly (debug)
Open `LetsMeowToolbox.sln` in VS and press F5 to run `LetsMeowToolbox.App`. During the build, the high-risk plugins are automatically copied to the `plugins` folder in the main program output directory, and `lang/**/*.meowlang` is also automatically copied to `lang/` in the output directory.

### Single-file publish (two modes)

**Mode 1: Single file · framework-dependent (small, about 10~20MB, the target machine must have the .NET 8 Desktop Runtime installed)**
```powershell
./build/publish-framework-dependent.ps1
# 产物：dist/framework-dependent/LetsMeowToolbox.exe + plugins/ + lang/
```

**Mode 2: Single file · self-contained (large, about 90~110MB, the target machine does not need .NET)**
```powershell
./build/publish-self-contained.ps1
# 产物：dist/self-contained/LetsMeowToolbox.exe + plugins/ + lang/
```

You can also run the commands manually:
```bash
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained false -o dist/framework-dependent
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true  -o dist/self-contained
```
After publishing, copy `LetsMeowToolbox.Plugins.HighRisk.dll` to `dist/…/plugins/` to enable the High-Risk Plugin Zone; the `lang/` directory is copied automatically by the csproj, so no manual handling is needed.

## Changelog

### v0.2.0 · 2026-11-06

- [Added] Multilingual support: new external language pack mechanism (`.meowlang`, containing JSON text); the `lang` folder in the program directory is scanned automatically at startup.
- [Added] Simplified Chinese as the source language is embedded directly in the program itself, so it can be used without installing any language pack.
- [Added] Official language packs: Traditional Chinese, Japanese, and English, shipped in the `lang` folder together with the program.
- [Added] New "Select language" button on the Settings page: a standalone window lists all language packs in the `lang` directory, where you can view details and load the selected language (it takes effect after an automatic restart).
- [Added] New "Install language pack" button on the Settings page: after selecting a `.meowlang` file, it shows the package name, author (official language packs carry a green shield badge), compatible version, install time, language type, language code, file size, description, and author website.
- [Added] Installation flow: an installation window with a progress bar, a success message "Language pack installed successfully!", and on failure the reason and log path; third-party language packs require a second confirmation.
- [Added] Update/downgrade of a same-named language pack: version numbers are compared automatically with a second confirmation; if the author or author website differs, overwriting is refused and the old language pack must be deleted manually.
- [Added] Language pack compatible versions support `|`-separated multiple version numbers (e.g. `0.2.0 | 0.2.1 | 0.2.2`).
- [Added] New "Check for updates" on the Settings page: requests the latest version number from the official update API and automatically compares it with the current version; when a new version is available, it shows the version number, update details, and a "Get the new version from the official website" button.
- [Added] End-of-support version detection: when the current version is in the unsupported-version list returned by the update API, a forced-update prompt appears at startup, and the program cannot be used until it is updated.
- [Fixed] In the language management window, the language pack names of list items were not displayed, and there was no highlight effect when selected.

**Full Changelog**: <https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v0.1.2...v0.2.0>

### v0.1.2 · 2026-10-24

- [Added] A web-style right-side scrollbar was added to all pages: it appears automatically when the content exceeds the window; you can drag the thumb up and down and click the track to page through, and the thumb turns pink on hover/drag, in a rounded Win11 style.
- [Fixed] On the Release Lock page, the process-lock list collapsed in height in small windows and the mouse wheel was swallowed by the inner list; it now scrolls normally along with the whole page.

**Full Changelog**: <https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v.0.1.1...v0.1.2>

### v0.1.1 · 2026-10-24

- [Fixed] Logging system: logs are now recorded globally, recording all of the user's settings operations, execution results, and timestamps (INFO/WARN/ERROR), instead of empty files.
- [Added] New "One-click reset of all content" on the Settings page: restores all feature settings to their default state, resets appearance settings, and clears config presets.
- [Added] Version numbering system: this version is v0.1.1, and the About page shows the current version; a new "View changelog" button was added.
- [Added] Special Thanks: each open-source project now has a "Visit" button that opens that project's homepage/GitHub repository in one click; an EZ-Mirror entry was added; blurry fonts were fixed (larger font size, darker color).
- [Added] UI reorganization: "Uninstaller" and "Release Lock" were split out of the Beginner/Advanced Zones, each becoming its own navigation category and standalone page.
- [Added] The About page now shows GitHub repository information and a one-click "Go to GitHub repository" button.
- [Fixed] Improved the color and text readability of switches and buttons in anime mode.

**Full Changelog**: <https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/commits/v.0.1.1>

## Special Thanks

See the "Special Thanks" page inside the program. This project references and thanks the following open-source projects (all with GitHub links):

- **HandyControl** (MIT) — UI control library
- **Open-Shell** (MIT) — classic context menu approach
- **Bulk Crap Uninstaller** (GPL-3.0) — uninstall/leftover-scanning approach
- **Win11Debloat** (MIT) — UWP uninstall whitelist and ad/telemetry registry key conventions
- **Sophia Script** (CC-BY-NC-SA-4.0) — system settings script approach
- **EZ-Mirror** — download acceleration mirror

## FAQ (how to troubleshoot when the GUI won't open)

> [!WARNING]
> **Author's note:**
> If you run into any problem, please first [**check the online documentation**](https://doc.lets-meow-toolbox.hatasoft365.top/), **the online documentation can solve 99% of problems**! Any Issue opened because you didn't read the docs after hitting a problem will be closed directly!

This program has built-in **global exception capture and logging**: if an error occurs at startup or during operation, an error dialog pops up and the error is written to the log, so it will never "silently fail to open".

- **Log location**: `%LocalAppData%\LetsMeowToolbox\logs\meow-日期.log`
  (paste `%LocalAppData%\LetsMeowToolbox\logs` into the File Explorer address bar and press Enter)
- **The framework-dependent build won't open**: please install the [.NET 8 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) first (choose ".NET Desktop Runtime 8.x"). If you don't want to install the runtime, just use the **self-contained** build, which bundles the runtime and works with a double-click.
- **Theme note**: on non-Win11 systems the background may automatically fall back to a solid color; this does not affect usage.
- **Language pack related**: for issues such as a failed language pack installation or changes not taking effect after switching, see [Settings and About](https://doc.lets-meow-toolbox.hatasoft365.top/docs/settings.html).
