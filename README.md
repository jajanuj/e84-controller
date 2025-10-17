# E84 Controller 測試介面

## 概述

E84 Controller 是一個實作 SEMI E84 協定的控制器核心，用於自動化物料搬運系統 (AMHS) 中的設備與設備間的握手通訊。本專案包含完整的測試 UI，可用於模擬和測試 E84 狀態機的行為。

## 功能特色

### 核心功能
- **完整的 E84 狀態機實作**：支援 Idle、Request、Busy、Valid、Transfer、Complete、Abort、Error、Resetting 等狀態
- **雙向支援**：支援 Inbound (進站) 和 Outbound (出站) 兩種方向
- **PLC 抽象層**：透過 IE84Plc 介面支援多種 PLC 類型
- **安全互鎖檢查**：透過 IE84SafetyInterlock 介面實作安全檢查
- **可配置的時序參數**：支援自定義逾時、去抖動、輪詢間隔等參數
- **事件回呼機制**：提供狀態轉換、信號變化、逾時等事件通知

### 測試 UI 功能
- **方向選擇**：選擇 Inbound 或 Outbound 模式
- **控制器啟動/停止**：開始和停止 E84 控制器運行
- **PLC 輸入模擬**：透過核取方塊模擬以下 PLC 輸入信號：
  - TR_REQ (轉運請求)
  - VALID (有效)
  - COMPT (完成)
  - L_REQ (下層請求)
  - U_REQ (上層請求)
  - READY (就緒)
  - RESET (重置)
- **PLC 輸出顯示**：即時顯示以下 PLC 輸出狀態：
  - BUSY (忙碌)
  - HO_AVBL (可接收)
  - TRANSFER (轉運)
  - CLAMP (夾緊)
  - DOCK (對接)
  - ABORT (中止)
- **狀態顯示**：
  - 當前狀態 (State)
  - 方向 (Direction)
  - 時間戳記
  - 錯誤資訊
- **事件記錄**：即時顯示所有事件，包括狀態轉換、信號變化、錯誤等

## 如何使用測試 UI

### 1. 啟動應用程式
執行 `E84.Controller.Core.exe` 啟動測試介面。

### 2. 選擇方向
- 選擇 **Inbound (進站)** 或 **Outbound (出站)**
- Inbound 模式：模擬接收來自 AMHS 的物料
- Outbound 模式：模擬將物料送出給 AMHS

### 3. 啟動控制器
- 點擊 **"啟動 Start"** 按鈕開始控制器
- 控制器將進入 Idle 狀態並開始監聽輸入信號

### 4. 模擬 E84 握手流程

#### Inbound 流程範例：
1. 勾選 **TR_REQ** (模擬 AMHS 請求轉運)
2. 觀察輸出：**BUSY** 和 **HO_AVBL** 應該變成綠色
3. 勾選 **VALID** (模擬 AMHS 確認物料有效)
4. 觀察輸出：**TRANSFER** 應該變成綠色
5. 勾選 **COMPT** (模擬轉運完成)
6. 控制器返回 Idle 狀態

#### Outbound 流程範例：
1. 勾選 **L_REQ** 或 **U_REQ** (模擬請求下層或上層)
2. 觀察輸出：**CLAMP** 應該變成綠色
3. 觀察輸出：**DOCK** 應該變成綠色
4. 勾選 **READY** (模擬 AMHS 就緒)
5. 觀察輸出：**TRANSFER** 應該變成綠色
6. 勾選 **COMPT** (模擬轉運完成)
7. 控制器返回 Idle 狀態

#### 錯誤恢復流程：
1. 如果發生錯誤或中止 (ABORT 輸出變成紅色)
2. 勾選 **RESET** 信號
3. 控制器將進入 Resetting 狀態後返回 Idle

### 5. 觀察狀態變化
- **狀態標籤**會顯示當前狀態並使用顏色編碼：
  - 藍色：Idle
  - 綠色：Request, Busy, Valid, Transfer
  - 深綠色：Complete
  - 紅色：Abort, Error
  - 橘色：Resetting
- **輸出指示器**會在信號啟動時變成綠色
- **事件記錄**會顯示所有狀態轉換和事件的詳細資訊

### 6. 停止控制器
- 點擊 **"停止 Stop"** 按鈕停止控制器
- 所有輸入將被重置

## 架構說明

### 核心元件

#### E84Controller
主要的狀態機控制器，負責：
- 管理 E84 狀態轉換
- 讀取 PLC 輸入並執行去抖動
- 寫入 PLC 輸出
- 處理逾時和錯誤
- 執行安全互鎖檢查

#### FakePlc
測試用的 PLC 模擬器，實作 IE84Plc 介面：
- 在記憶體中模擬 PLC 位元和字
- 支援模擬連線中斷
- 無需實體 PLC 硬體即可測試

#### E84IoMap
定義邏輯信號名稱與實體 PLC 位址的映射：
- 輸入映射：TR_REQ -> X100, VALID -> X101, 等等
- 輸出映射：BUSY -> Y200, HO_AVBL -> Y201, 等等
- 支援信號反向設定

#### AlwaysPassInterlock
測試用的安全互鎖實作：
- 永遠回傳通過 (用於測試環境)
- 生產環境應實作實際的安全檢查邏輯

### E84 狀態機

```
Idle -> Request -> Busy -> Valid -> Transfer -> Complete -> Idle
                                                    ↓
                                                  Abort -> Resetting -> Idle
```

## 配置參數

在 `E84Config` 中可以調整以下參數：

- **PollIntervalMs** (預設: 100ms)：控制器輪詢間隔
- **DebounceMs** (預設: 50ms)：輸入去抖動時間
- **WaitTrReqMs** (預設: 10000ms)：等待 TR_REQ 的逾時時間
- **WaitValidMs** (預設: 10000ms)：等待 VALID 的逾時時間
- **WaitComptMs** (預設: 15000ms)：等待 COMPT 的逾時時間
- **ClampMs** (預設: 1500ms)：夾緊動作時間
- **DockMs** (預設: 3000ms)：對接動作時間
- **PlcReconnectDelayMs** (預設: 1000ms)：PLC 重連延遲
- **PlcReconnectMaxAttempts** (預設: 3)：PLC 重連最大次數

## 開發環境

- .NET Framework 4.8
- Windows Forms
- Visual Studio 2017 或更高版本

## 專案結構

```
E84.Controller.Core/
├── Controller/
│   └── E84Controller.cs          # 核心控制器邏輯
├── Exceptions/
│   └── Exceptions.cs              # 自訂例外類型
├── Interfaces/
│   ├── IE84IoMap.cs               # I/O 映射介面
│   ├── IE84Logger.cs              # 記錄器介面
│   ├── IE84Plc.cs                 # PLC 介面
│   └── IE84SafetyInterlock.cs     # 安全互鎖介面
├── Logging/
│   └── ConsoleE84Logger.cs        # Console 記錄器實作
├── Mapping/
│   └── E84IoMap.cs                # I/O 映射實作
├── Models/
│   ├── E84Config.cs               # 配置模型
│   ├── E84StatusSnapshot.cs       # 狀態快照模型
│   ├── Enums.cs                   # 列舉定義
│   └── PlcDevice.cs               # PLC 裝置模型
├── Plc/
│   ├── AlwaysPassInterlock.cs     # 測試用互鎖實作
│   ├── FakePlc.cs                 # PLC 模擬器
│   └── MxComponentPlc.cs          # MX Component PLC 實作
├── Form1.cs                       # 測試 UI 主窗體
├── Form1.Designer.cs              # UI 設計器檔案
├── Form1.resx                     # UI 資源檔案
└── Program.cs                     # 應用程式進入點
```

## 授權

請參考專案授權檔案。

## 貢獻

歡迎提交 Pull Request 或回報 Issue。
