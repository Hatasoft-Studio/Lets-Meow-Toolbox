> 🌐 [简体中文](./README.md) · **繁體中文** · [日本語](./README.ja.md) · [English](./README.en.md)

# Let's Meow Toolbox

## 一個二次元現代化 UI、新手友善的 Windows 10 / 11 一體化調校工具箱

> [!WARNING]
>
> **作者提醒！**
>
> 這是一個**Vibe-Coding 作品**！如果你**介意使用 Vibe-Coding 作品**，請你自覺離開，**不要提 Issue 來罵人**！這個程式每次發佈時都會經過作者親測，基本上不會出現大問題，**比其他 Vibe-Coding 作品穩定得多！**
>
> 作者製作這個工具，最初只是為了方便自己重設系統後能更快設定電腦 *（畢竟現在的 Win11 反人類設計太多了！尤其是右鍵選單）*，後來想到會有很多和作者一樣的 <u>「懶人」</u>，於是決定美化一下發出來。**不過，請放心！這個專案作者會一直維護下去的。**
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
> **靜待大佬...**
>
> 作者不是什麼有才之人，如果**有大佬路過後想要「救」一下這個專案的話**，**那就太好了**！

- **技術堆疊**：WPF · .NET 8（LTS）· C# · HandyControl（MIT）
- **作者**：Hatasoft-Studio（B站：[Hatasoft工作室](https://space.bilibili.com/1198166444)）
- **授權條款**：GPL-3.0（見 [LICENSE](./LICENSE)）
- **主程式**：`LetsMeowToolbox.exe`
- **目前版本**：v0.2.0（2026-11-06）
- **優點**：體積小、功能大致夠用、頁面簡潔、說明直白
- **線上文件**：<https://doc.lets-meow-toolbox.hatasoft365.top/>

## 功能分區

| 分區 | 定位 | 說明 |
| --- | --- | --- |
| 🟢 新手區 | 預設不啟用、無需風險確認 | Win11 經典右鍵選單、工作列/檔案總管微調、關閉廣告/遙測/通知推薦、安全 UWP 解除安裝（白名單）、電源/索引/自動播放/縮圖最佳化、一鍵還原 |
| 🟡 進階區 | 勾選風險提示後解鎖 | Windows 更新暫停期限/停用、一鍵還原 |
| 📦 解除安裝程式 | 獨立頁面 | Win32 軟體批次解除安裝、殘留掃描與清理、匯出清理報告，與「解除佔用」連動 |
| 🔓 解除佔用 | 獨立頁面 | 尋找佔用處理程序、關閉程式釋放佔用、開機延遲刪除 |
| 🔴 高風險外掛區 | 外部 DLL 外掛 | 關閉 Defender / 關閉防火牆 / 移除 Edge·WebView2。**主程式本體不含任何高風險程式碼**，僅動態載入 `plugins` 目錄下的外掛 DLL |
| ⚙️ 設定 | 獨立頁面 | 語言包選擇與安裝、檢查更新、外觀、設定預設匯入匯出、日誌目錄、一鍵初始化全部內容、關於與更新日誌 |
| 💖 特別鳴謝 | 獨立頁面 | 本專案整合的其他開源專案 |

## v0.2.0 新增：多語言與軟體更新

### 多語言（外部語言包）

- 簡體中文是**源語言**，直接內嵌在主程式裡，不裝任何語言包也能正常使用。
- 繁體中文、日語、英語以 `.meowlang`（JSON 文字）語言包的形式放在程式目錄的 `lang` 資料夾，官方隨包發佈。
- 設定頁「語言 / Language」卡片：
  - **選擇語言**：開啟語言管理視窗，列出 `lang` 目錄下全部語言包，可檢視詳細資訊並載入所選語言，載入後自動重新啟動生效；
  - **安裝語言包**：選擇 `.meowlang` 檔案，驗證欄位與適用版本後顯示詳細資訊（包名、製作者、適用版本、安裝時間、語言類型、語言編碼、檔案大小、簡介、作者網站），官方語言包帶綠色盾牌標示，第三方語言包需二次確認；安裝帶進度條，成功提示「語言包安裝成功！」，失敗顯示原因與日誌路徑。
  - **同名語言包更新/降級**：自動比較版本號並二次確認；作者或作者網站不一致時拒絕覆寫，需手動刪除舊包。
  - **適用版本**：支援 `*`、`>=0.2.0`、`0.2.0 | 0.2.1 | 0.2.2` 等多版本寫法。
- 官方語言包另以 `v1.1.0-lang` 獨立發佈，方便只更新語言包的使用者單獨下載。

### 軟體更新

- 設定頁「軟體更新」卡片新增「檢查更新」：請求官方更新介面，比較版本後顯示最新版本號、更新內容與「前往官網取得新版本」按鈕；已是最新時提示目前版本。
- **停止支援版本偵測**：介面傳回的不受支援版本清單中包含目前版本時，啟動即彈出「此版本已停止支援」強制更新提示，更新前無法繼續使用本程式。

## 專案結構

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

## 安全設計

- 所有修改、刪除、清理操作**均需使用者手動確認**，無任何自動執行邏輯；殘留、佔用預設不勾選。
- 內建黑名單：系統核心目錄（`C:\Windows`、`System32`、`WinSxS`）與系統核心處理程序禁止強制結束。
- 不加殼、不混淆、無自解壓縮惡意邏輯；底層僅呼叫微軟原生 Win32 API（RestartManager、MoveFileEx、Registry 等）。
- 語言包只是被複製到 `lang` 目錄的 JSON 文字，安裝過程不會執行語言包中的任何程式碼。

## 建置與執行

### 環境
- Visual Studio 2022 Community（含「.NET 桌面開發」工作負載）
- .NET 8 SDK
- NuGet 還原 HandyControl 3.5.1

### 直接執行（偵錯）
在 VS 中開啟 `LetsMeowToolbox.sln`，按 F5 執行 `LetsMeowToolbox.App`。建置時高風險外掛會自動複製到主程式輸出目錄的 `plugins` 資料夾，`lang/**/*.meowlang` 也會自動複製到輸出目錄的 `lang/`。

### 單一檔案發佈（兩種模式）

**模式一：單一檔案 · 相依執行階段（小，約 10~20MB，目標機需裝有 .NET 8 桌面執行階段）**
```powershell
./build/publish-framework-dependent.ps1
# 产物：dist/framework-dependent/LetsMeowToolbox.exe + plugins/ + lang/
```

**模式二：單一檔案 · 自包含（大，約 90~110MB，目標機無需裝 .NET）**
```powershell
./build/publish-self-contained.ps1
# 产物：dist/self-contained/LetsMeowToolbox.exe + plugins/ + lang/
```

也可以手動執行：
```bash
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained false -o dist/framework-dependent
dotnet publish src/LetsMeowToolbox.App -c Release -r win-x64 \
  -p:PublishSingleFile=true --self-contained true  -o dist/self-contained
```
發佈後把 `LetsMeowToolbox.Plugins.HighRisk.dll` 複製到 `dist/…/plugins/` 即可啟用高風險外掛區；`lang/` 目錄由 csproj 自動複製，無需手動處理。

## 更新日誌

### v0.2.0 · 2026-11-06

- 【新增】多語言支援：新增外部語言包機制（`.meowlang`，內容為 JSON 文字），啟動時自動掃描程式目錄下的 `lang` 資料夾。
- 【新增】簡體中文作為源語言直接內嵌於程式本體，無需安裝語言包即可使用。
- 【新增】官方語言包：繁體中文、日語、英語，隨程式發佈在 `lang` 資料夾內。
- 【新增】設定頁新增「選擇語言」按鈕：獨立視窗列出 `lang` 目錄下的全部語言包，可檢視詳細資訊並載入所選語言（載入後自動重新啟動生效）。
- 【新增】設定頁新增「安裝語言包」按鈕：選擇 `.meowlang` 檔案後顯示包名、製作者（官方語言包帶綠色盾牌標示）、適用版本、安裝時間、語言類型、語言編碼、檔案大小、簡介與作者網站。
- 【新增】安裝流程：帶進度條的安裝視窗，成功提示「語言包安裝成功！」，失敗時顯示原因與日誌路徑；第三方語言包需二次確認。
- 【新增】同名語言包更新/降級：自動比較版本號並二次確認；若作者或作者網站不一致則拒絕覆寫，需手動刪除舊語言包。
- 【新增】語言包適用版本支援用 `|` 分隔多個版本號（如 `0.2.0 | 0.2.1 | 0.2.2`）。
- 【新增】設定頁新增「檢查更新」：向官方更新介面請求最新版本號，與目前版本自動比較；有新版本時顯示版本號、更新內容與「前往官網取得新版本」按鈕。
- 【新增】停止支援版本偵測：更新介面傳回的不受支援版本清單中包含目前版本時，啟動即彈出強制更新提示，更新前無法繼續使用本程式。
- 【修正】語言管理視窗清單項目的語言包名稱不顯示、選取後無醒目提示效果的問題。

**完整變更日誌**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v0.1.2...v0.2.0>

### v0.1.2 · 2026-10-24

- 【新增】所有頁面加入網頁風格的右側捲軸：內容超出視窗時自動出現，可上下拖曳滑桿、點擊軌道翻頁，滑桿懸停/拖曳時變粉色，圓角 Win11 樣式。
- 【修正】解除佔用頁佔用處理程序清單在小視窗下高度塌陷、滾輪被內部清單吃掉的問題，現在可隨整頁正常捲動。

**完整變更日誌**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/compare/v.0.1.1...v0.1.2>

### v0.1.1 · 2026-10-24

- 【修正】日誌系統：日誌改為全域記錄，記錄使用者全部設定操作、執行結果與時間（INFO/WARN/ERROR），不再為空白檔案。
- 【新增】設定頁新增「一鍵初始化全部內容」：還原全部功能設定到預設狀態、重設外觀設定、清除設定預設。
- 【新增】版本號體系：自此版起為 v0.1.1，關於頁顯示目前版本；新增「檢視更新日誌」按鈕。
- 【新增】特別鳴謝：每個開源專案新增「前往」按鈕，一鍵開啟該專案首頁/GitHub 儲存庫；新增 EZ-Mirror 項目；修正字型模糊（放大字級、加深顏色）。
- 【新增】介面重組：「解除安裝程式」與「解除佔用」從新手區/進階區拆出，各自獨立為一個導覽分類與獨立頁面。
- 【新增】關於頁新增 GitHub 儲存庫資訊與「前往 GitHub 儲存庫」一鍵按鈕。
- 【修正】二次元模式下開關與按鈕的顏色、文字可讀性最佳化。

**完整變更日誌**：<https://github.com/Hatasoft-Studio/Lets-Meow-Toolbox/commits/v.0.1.1>

## 特別鳴謝

詳見程式內「特別鳴謝」頁面。本專案參考並致謝以下開源專案（均附 GitHub 連結）：

- **HandyControl**（MIT）— UI 控制項庫
- **Open-Shell**（MIT）— 經典右鍵選單思路
- **Bulk Crap Uninstaller**（GPL-3.0）— 解除安裝/殘留掃描思路
- **Win11Debloat**（MIT）— UWP 解除安裝白名單與廣告/遙測機碼取捨標準
- **Sophia Script**（CC-BY-NC-SA-4.0）— 系統設定指令碼思路
- **EZ-Mirror** — 下載加速鏡像

## 常見問題（GUI 無法開啟時如何排解）

> [!WARNING]
> **作者提醒：**
> 遇到任何問題請先[**查看線上文件**](https://doc.lets-meow-toolbox.hatasoft365.top/)，**線上文件可以解決 99% 的問題**！如果因為您遇到問題不看文件而提的 Issue，我會直接關閉的！

本程式已內建**全域例外擷取與日誌**：若啟動或執行出錯，會彈出錯誤對話方塊並寫入日誌，不會「無聲無息打不開」。

- **日誌位置**：`%LocalAppData%\LetsMeowToolbox\logs\meow-日期.log`
  （於檔案總管網址列貼上 `%LocalAppData%\LetsMeowToolbox\logs` 按 Enter 即可）
- **框架相依版打不開**：請先安裝 [.NET 8 桌面執行階段](https://dotnet.microsoft.com/download/dotnet/8.0)（選擇「.NET Desktop Runtime 8.x」）。若不想裝執行階段，請直接使用**自包含（self-contained）**版本，它自帶執行階段、雙擊即用。
- **佈景主題說明**：非 Win11 系統可能會自動退回為純色背景，不影響使用。
- **語言包相關**：語言包安裝失敗、切換後未生效等問題，見[設定與關於](https://doc.lets-meow-toolbox.hatasoft365.top/docs/settings.html)。
