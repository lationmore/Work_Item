# Work_Item

## 室內無人機換燈泡模擬 MVP

C# + Unity 桌面鍵盤操作範例，包含可導覽的室內大廳 3D 場景、簡化四旋翼飛行、
追蹤攝影機、模擬斷電聯鎖與完整換燈泡任務。場景、材質與模型皆由 Unity Editor
選單以原生幾何產生，不需要下載外部素材或額外 Render Pipeline 相依。儲存庫已具備
Unity 專案結構，可由 Unity Hub 直接開啟；Unity Package Manager 會還原 Visual Studio
整合套件及其相依套件。

### 啟動

1. 在 Windows 安裝 **Unity Hub** 與 **Unity 2022.3.62f3 LTS**。
   專案版本記錄於 `ProjectSettings/ProjectVersion.txt`；可從 Unity 官方下載封存取得。
   若改用較新的 2022.3 LTS 修補版本，請先備份並重新完成驗收，不要降回未修補的舊版本。
2. 安裝 **Visual Studio 2022**，在 Visual Studio Installer 勾選
   **「使用 Unity 的遊戲開發」（Game development with Unity）**，包含 Visual Studio Tools for Unity。
3. 在 **Unity Hub → Projects → Add → Add project from disk** 選取本儲存庫根目錄
   （同時包含 `Assets`、`Packages`、`ProjectSettings` 的資料夾），使用上述 Editor 開啟。
   **不需要另建專案或複製 Assets，也不是先用 VS 開啟資料夾建置。**
4. 首次開啟需連網讓 Unity Package Manager 還原套件；Unity 會建立 `Library`、
   `Packages/packages-lock.json` 與其餘預設 Project Settings。專案使用 **Built-in Render Pipeline**，
   不需安裝 URP／HDRP。等右下角匯入與編譯完成，確認 Console 沒有紅色錯誤。
5. 在 **Edit → Project Settings → Player → Other Settings** 確認
   **Active Input Handling** 為 **Input Manager (Old)** 或 **Both**，若有修改則依提示重啟。
   本專案使用舊式鍵盤輸入，未加入新版 Input System 套件。
6. 執行 **Tools → Indoor Drone → Validate Development Environment**，先檢查：
   - Unity Editor 是否為 **2022.3.62f3**
   - **Visual Studio Editor 2.0.22** 是否已由 Package Manager 還原
   - **Active Input Handling** 是否仍為 **Input Manager (Old)** 或 **Both**
   - 是否已存在可啟動的示範場景，以及該場景是否已加入 Build Settings
7. 執行 **Tools → Indoor Drone → Create Demo Scene**。
   場景會儲存在 `Assets/IndoorDrone/Generated/Demo/IndoorDrone.unity`；
   重複產生會使用新的資料夾，不覆寫舊場景，並會自動把新產生的場景加入 **Build Settings**。
   新版示範場景會建立以照片構圖為靈感的大廳：深灰石材地坪、淺色圓形地坪嵌飾、
   奶油色天花板、玻璃門窗、中央老鷹雕像、盆栽與右側深色石材牆，同時保留原本換燈泡任務。
   點選 **Play**，再點 Game 視窗取得鍵盤焦點。

### 使用 VS2022 編輯與偵錯

1. 在 Unity 的 **Edit → Preferences → External Tools → External Script Editor**
   選擇 **Visual Studio 2022**。這是每台電腦的偏好設定，不在儲存庫內硬編碼 VS 安裝路徑。
2. 按 **Regenerate project files**，再使用 **Assets → Open C# Project**，
   或雙擊 Project 視窗中的 C# 腳本，以 VS2022 開啟 Unity 產生的方案。
3. 在 VS2022 設定中斷點，使用 **Attach to Unity** 連接目前 Editor，回 Unity 按 Play。
   若沒有此按鈕，請檢查 Visual Studio Tools for Unity 是否安裝完成。
4. 若補全或 UnityEngine 參考異常，確認 Package Manager 中 **Visual Studio Editor 2.0.22**
   已還原，然後重新產生專案檔；不要手動加入 Unity DLL 或把腳本轉成 Console 專案。
5. 若更換 Unity 版本、刪除 `Library`、或第一次在新電腦開啟專案，請重新執行
   **Validate Development Environment**，再產生方案檔與 Attach 偵錯。

`.sln`／`.csproj` 由 Unity 依本機環境產生且不提交版本控制。
VS2022 用於編輯與偵錯；腳本編譯、場景執行與遊戲輸出仍以 **Unity Editor** 為準，
不能只靠 VS 的 Build／F5 產生或啟動模擬平台。

### 建置 Windows 執行檔

1. `Create Demo Scene` 會把剛產生的場景自動加入 **File → Build Settings**。
   若你手動開啟其他場景、或保留了多個舊的 Generated Demo，請確認清單中只勾選
   這次要啟動的示範場景；必要時再按 **Add Open Scenes** 更新。
2. 選 **PC, Mac & Linux Standalone → Windows → x86_64**，必要時按 **Switch Platform**。
3. 在 Player Settings 使用 **Mono** 作為 MVP 的 Scripting Backend，再按 **Build And Run**，
   選擇 `Builds/Windows` 下的輸出位置。若改用 IL2CPP，需另裝 Unity Hub 的
   **Windows Build Support (IL2CPP)** 以及對應的 VS C++ 建置工具與 Windows SDK。
4. 發送執行檔時保留整個輸出資料夾（包含 `_Data`、`UnityPlayer.dll` 等），不可只複製 `.exe`。
   MVP 的重新開始方式是停止／重新進入 Play；獨立執行檔則關閉後重新啟動。

### 操作

| 按鍵 | 功能 |
| --- | --- |
| Space | 起飛至目前位置上方約 0.8 公尺 |
| W / S | 世界座標 +Z / −Z 移動 |
| A / D | 世界座標 −X / +X 移動 |
| R / F | 上升／下降 |
| 左 Shift | 慢速精準移動 |
| 放開移動鍵 | 自動定點懸停 |
| E | 在目前任務目標附近執行拆卸、回收、領取或安裝 |
| P | 模擬電源切換；未完成安裝不可重新送電 |
| L | 自動移動至起降點上方並降落 |

### 任務與驗收

目標座標是**無人機機身中心**的位置，場景中的小色球表示懸停點。
畫面顯示目前目標、距離、高度、速度、碰撞次數與操作提示；介面使用英文以免依賴中文字型。
大廳是依單張照片估算尺寸與構圖後，以 Unity 原生幾何做出的**風格化近似 3D 重建**；
可辨識主要元素，但不是實測尺寸或擬真的一比一掃描。

1. 起飛，按 P 斷開模擬電源，移動至黃色 SOCKET `(0, 3.05, 2)`。
2. 放開按鍵穩定懸停，在距離不超過 **0.25 m**、速度不超過 **0.25 m/s** 時按 E 拆下舊燈泡。
3. 飛到紅色 RECYCLE `(3, 0.95, 2)`，穩定後按 E 回收。
4. 飛到綠色 SUPPLY `(-3, 0.95, 2)`，穩定後按 E 領取新燈泡。
5. 回到 SOCKET，穩定後按 E 安裝；離開燈座目標至少 **0.8 m**，再按 P 恢復模擬供電。
6. 確認室內點光源亮起，按 L 返回青色起降點，落地後顯示 COMPLETE。

反向驗收：

- 通電時按 E 不得拆卸；離目標太遠、飛行太快、降落途中也不得操作。
- 舊燈泡未回收不得取得新品；未安裝新品或未離開燈座不得送電。
- 空燈座不顯示燈泡；搬運時燈泡顯示在機身上方；安裝後搬運中的燈泡消失。
- 完成安裝但未送電／未返航降落，不得顯示任務完成。
- 復電後再次斷電，必須重新復電才能完成任務。
- 牆、地板、天花板、工作台與機身有碰撞；重複產生場景不得覆寫已產生的資產。

### 大廳場景重點

- 深灰拋光石材地板搭配淺色圓形地坪嵌飾，提供起飛後往返任務點的淨空。
- 奶油色天花板含嵌燈與通風格柵；任務燈座嵌入天花板中央前方。
- 後方灰框玻璃門窗、暖金棕色裝飾石材板、中央老鷹雕像與底座、兩側白色盆栽，
  以及右側深色石材柱牆，皆為實際 3D 幾何，不是背景貼圖。
- 環境常亮由 ambient / 大廳照明提供，任務中的 `P` 只切換工作燈，確保斷電後仍可操作。

### 結構與限制

- `Scripts/DroneController.cs`：Rigidbody 加速度控制、重力補償、懸停、飛行邊界與返航降落。
- `Scripts/BulbReplacementMission.cs`：任務狀態、模擬電源聯鎖、搬運物品及即時 HUD。
- `Scripts/DroneCamera.cs`：跟隨攝影機。
- `Editor/IndoorDroneSceneBuilder.cs`：以 Unity 原生幾何產生可儲存場景。

以上腳本位於 `Assets/IndoorDrone`。`ProjectSettings/ProjectVersion.txt` 固定 Editor 版本，
`Packages/manifest.json` 宣告 VS2022 整合與模擬所需的 Unity 內建模組。
所有既有腳本與資料夾均附帶固定 GUID 的 `.meta`，避免不同電腦產生不一致的資產識別。
首次成功開啟後，請將 Unity 產生的 `Packages/packages-lock.json`、其餘 `ProjectSettings`
及需要共享的場景／材質連同 `.meta` 提交；不要提交 `Library` 或 VS 暫存檔。

這是**純軟體流程驗證**：機身鎖定旋轉，拆裝是離散互動，並未模擬旋翼空氣動力、
螺紋扭矩、電路、視覺辨識、機械手臂或燈泡破碎。搬運物品不改變重量，
懸停目標不是實體障礙物；返航沒有避障規劃，碰撞只統計、不判定損壞。
初始燈泡故障，因此初始電源雖為 ON，燈仍不亮；環境光保持可見以利斷電操作。
大廳視覺是依單張參考照片估計比例後製作的風格化近似，不宣稱精確量測或擬真材質校正。

**不可用於真實市電維修、飛控部署或安全認證。**

### 驗證狀態

儲存庫尚無自動化測試或 CI 建置流程。開發沙箱未安裝 Unity Editor／VS2022，
無法在此還原 Unity 套件、產生方案、編譯 Unity API、執行 Play Mode 或輸出 Windows 執行檔。
此沙箱仍未安裝 Unity Editor／VS2022，因此**未在此實際驗證** Hub 開啟、套件還原、
方案產生、Attach to Unity 偵錯、Play Mode、或 Windows Build。此次儲存庫狀態包含：

- Editor 端的 **Validate Development Environment** 檢查，讓乾淨 checkout 可明確看出
  Unity 版本、VS 套件、舊式輸入設定與示範場景／Build Settings 是否到位。
- `Create Demo Scene` 產生後自動將當前示範場景加入 Build Settings，減少首次建置遺漏。
- `Create Demo Scene` 目前會建立可導覽的大廳 3D 場景與持久化材質資產，保留既有換燈泡任務流程。

請在本機依序執行：

1. Unity Hub 以 **2022.3.62f3** 開啟專案。
2. 等待 Package Manager 與 Script Compilation 完成。
3. 執行 **Validate Development Environment**，確認沒有阻塞項目。
4. 執行 **Create Demo Scene**，按 **Play** 跑完「任務與驗收」。
5. 在 VS2022 重新產生方案，使用 **Attach to Unity** 驗證中斷點。
6. 進行 Windows Build，於輸出執行檔重跑同一套任務流程。

### 手動驗收檢查表

- [ ] `Validate Development Environment` 顯示 Unity 版本、VS 套件、輸入模式與示範場景／Build Settings 均符合要求。
- [ ] `Create Demo Scene` 產生新的 `Assets/IndoorDrone/Generated/Demo*` 資料夾，不覆寫舊資產。
- [ ] 關閉並重新開啟剛儲存的 `IndoorDrone.unity` 後，大廳幾何、材質、玻璃、雕像、盆栽與標記仍完整存在。
- [ ] 重複產生第二個 Demo Scene 後，新的 `IndoorDrone.unity` 仍自動加入 Build Settings。
- [ ] Play Mode 中可看見 3D 大廳，而不是平面背景；相機可跟隨無人機完成起飛、巡航與返航。
- [ ] 無人機可抵達 `SOCKET (0, 3.05, 2)`、`RECYCLE (3, 0.95, 2)`、`SUPPLY (-3, 0.95, 2)`，
      不會被雕像、盆栽或右側石材牆卡死，且仍可按 `L` 安全返航降落。
- [ ] 牆、地板、天花板、底座、櫃體與主要裝飾會產生合理碰撞；盆栽葉片不造成過重碰撞負擔。
- [ ] 完整走完斷電、拆舊燈泡、回收、領新品、安裝、離開燈座、復電、返航降落流程。
- [ ] 驗證所有反向驗收案例：通電不可拆卸、未回收不可領新品、未安裝／未離開燈座不可復電、未返航不可完成。
- [ ] 工作燈在任務復電前保持熄滅，但大廳 ambient / 常亮照明仍足以辨識路徑與 HUD。
