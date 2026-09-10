# ELAN WDT — Visual Studio 2017

C# Console 專案，使用 .NET Framework 4.7.2、C# 7.0 與傳統 csproj。預設 VID=04F3、PID=0C8C，可透過設定檔或命令列修改。

## 建置

1. 使用 Windows 與 Visual Studio 2017（建議更新至 15.9），安裝「.NET 桌面開發」工作負載及 .NET Framework 4.7.2 Developer Pack／Targeting Pack。只有 Runtime 不足以提供編譯參考組件。
2. 開啟儲存庫根目錄的 `ElanWdt.sln`。
3. 選擇 `Release | Any CPU`，執行「建置方案」。
4. 輸出為 `ElanWdt\bin\Release\elan_WDT.exe` 及 `elan_WDT.exe.config`，部署時兩者一起複製。目標 Windows 需有相容的 .NET Framework Runtime。

也可在 VS2017 Developer Command Prompt 執行：

```bat
msbuild ElanWdt.sln /p:Configuration=Release /p:Platform="Any CPU"
```

## 設定 VID / PID

開發時修改 `ElanWdt/App.config` 的 `appSettings`；部署後修改 EXE 旁的 `elan_WDT.exe.config`，不需重新編譯：

```xml
<appSettings>
  <add key="VID" value="04F3" />
  <add key="PID" value="0C8C" />
</appSettings>
```

命令列個別覆蓋設定檔；鍵與十六進位字母不分大小寫。接受可選的 `0x` 前綴，移除前綴後必須恰為四位十六進位數字。

```bat
REM 以下非 help 指令會實際執行硬體操作！確認目標與協定後再執行。
elan_WDT.exe
elan_WDT.exe /vid:04F3 /pid:0C8C
elan_WDT.exe /vid:0x04F3 /pid:0x0C8C
REM 只覆蓋 PID，VID 仍讀取設定檔
elan_WDT.exe /pid:0C8C
REM 以下僅顯示說明，不碰裝置或服務，也不需管理員權限
elan_WDT.exe /?
elan_WDT.exe --help
```

原始程式中未使用的 `/f`、`/d`、`/v` 已移除。未知、重複、空值或格式錯誤的參數會被拒絕。程式不會等待按鍵。

## 執行流程與重要警告

- 實際執行必須從「以系統管理員身分執行」的命令提示字元啟動；VS 偵錯時也需適當提升權限。
- 先驗證設定並找到唯一符合的裝置，再停止 Windows Biometric Service (`WbioSrvc`)，最後傳送 WDT 封包。
- 找不到或找到多個符合介面時中止。服務停止或等待失敗時，不會送出 WDT。每次服務狀態等待最多 10 秒。
- **沿用原始行為：停止服務後不會自動重新啟動，即使後續發生錯誤。這會影響依賴該服務的生物辨識功能。** 測試結束、裝置恢復正常且符合環境政策後，請由操作者在 Windows「服務」管理工具手動恢復服務。
- 僅列舉 GUID `e2b5183a-99ea-4cc3-ad6b-80ca8d715b80` 的 Biometric Reader 介面，不是所有 USB／HID 裝置。
- 沿用原始程式以介面路徑中的 `VID_`／`PID_` 比對。介面路徑不是通用硬體識別契約；若驅動的路徑沒有這些字串，需要改用裝置屬性查詢。多個同型裝置需另設選擇機制；本版會拒絕多個匹配。
- **修改 VID/PID 只改變目標選擇，不代表其他裝置支援這組私有協定。** 請僅在確認支援此協定的 ELAN 測試裝置上操作。
- IOCTL 沿用 `(0x44u << 16) | (0x850u << 2)`；WDT 封包為 `40 27 57 44 54 52 53 54`，輸出緩衝區為兩個零位元組。
- 保留 `In_Boot_Code` 方法（封包 `42 01 52 55 4E 49 41 50`），但主程式不會呼叫，也未提供命令列入口。
- `CreateFileW` 的 desiredAccess 保留為 0；接受哪些存取權限仍由驅動決定。
- 使用同步 `DeviceIoControl`，本版没有 IOCTL 逾時／取消機制；服務的等待逾時不限制驅動 I/O 時間。
- IOCTL 成功只表示該呼叫成功，不代表已驗證重置完成；裝置重置或斷線也可能影響呼叫結果。需在實機確認預期結果。

## Exit code

| 值 | 意義 |
| --- | --- |
| 0 | 顯示說明或 IOCTL 呼叫成功（不是硬體重置驗證） |
| 1 | 權限、裝置、設定檔讀取、服務或原生 API 等執行失敗 |
| 2 | 參數或 VID/PID 值不合法／缺少 |

## 驗證狀態

此提交未在 Visual Studio 2017 編譯，也未執行硬體或服務測試；不宣稱已通過。請依 `TESTING.md` 驗證。無自動重啟服務、額外韌體操作或實機測試腳本。
