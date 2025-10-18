# E84 RGV/AGV Controller (Active Side) - 使用說明

## 概述

本程式已修改為 **Active側 (RGV/AGV)** 實作，符合主動測（搬運設備端）的E84協定要求。

## 主要變更

### 1. 方向定義
- **Load (入料)**: RGV載入工件到EQ
- **Unload (出料)**: RGV從EQ載出工件

### 2. 信號定義

#### Active側輸出 (RGV → EQ)
- **VALID**: RGV到達EQ時發送，表示交握通訊開始
- **TR_REQ**: RGV接受EQ載入/載出工件要求
- **BUSY**: 確認EQ READY ON後，RGV開始載入或載出工件
- **COMP**: 交握完成，RGV離開交握區

#### Passive側輸入 (EQ → RGV)
- **L_REQ**: EQ要求載入工件的訊號
- **U_REQ**: EQ要求載出工件的訊號
- **READY**: EQ準備完成，允許搬運設備取出或置入工件
- **LC_REQ**: 準備完成可以載入工件進入EQ端
- **UC_REQ**: 準備完成可以載出工件離開EQ端
- **Carrier**: 工件在席檢知
- **EQ_ONLINE**: EQ在線狀態
- **IN_LINE**: EQ併入產線狀態
- **ALARM**: EQ異常狀態
- **IDLE**: EQ無工件並且無運轉
- **RUN**: EQ在運轉生產

### 3. 狀態機

```
Idle → WaitingRequest → TrReqSent → ReadyReceived → Transferring → TransferComplete → WaitingReadyOff → Complete → Idle
                                                                                                    ↓
                                                                                                 Abort → Resetting → Idle
```

### 4. 交握流程

#### RGV執行入料 (Load)

1. EQ端有工件載入需求時，發送 **LC_REQ** 信號ON
2. RGV接收到LC_REQ，將工件載運到EQ位置，發送 **Carrier ID** 到EQ，然後發送 **VALID** 信號ON（延遲0.5秒）
3. EQ端確認有工件載入需求時，發送 **L_REQ** 信號ON，開始交握
4. RGV接收到L_REQ，發送 **TR_REQ** 信號ON（延遲0.5秒）
5. EQ確認交握區RGV可以載入工件，發送 **READY** 信號ON
6. RGV確認READY信號ON，發送 **BUSY** 信號ON，同時開始將工件載入到EQ的交握區（延遲0.5秒）
7. RGV動作完成離開交握區，**BUSY** 信號OFF
8. RGV完成交握區的操作後，發送 **COMP** 信號ON（延遲0.5秒）
9. EQ收到COMP信號ON後，其 **READY**、**L_REQ**、**LC_REQ** 信號OFF
10. RGV在READY信號OFF後，**COMP** 信號OFF（延遲0.5秒）
11. COMP信號OFF後，**VALID** OFF

#### RGV執行出料 (Unload)

1. EQ端有工件載出需求時，先發送 **Carrier ID** 到RGV，然後發送 **UC_REQ** 信號ON
2. RGV接收到UC_REQ，移動到EQ位置，發送 **VALID** 信號ON（延遲0.5秒）
3. EQ端有工件載出需求時，發送 **U_REQ** 信號ON，開始交握
4. RGV接收到U_REQ，發送 **TR_REQ** 信號ON（延遲0.5秒）
5. EQ確認交握區RGV可以載出工件，發送 **READY** 信號ON
6. RGV確認READY信號ON，發送 **BUSY** 信號ON，同時開始從EQ的交握區將工件載出（延遲0.5秒）
7. RGV動作完成離開交握區，**BUSY** 信號OFF
8. RGV完成交握區的操作後，發送 **COMP** 信號ON（延遲0.5秒）
9. EQ收到COMP信號ON後，其 **READY**、**U_REQ**、**UC_REQ** 信號OFF
10. RGV在READY信號OFF後，**COMP** 信號OFF（延遲0.5秒）
11. COMP信號OFF後，**VALID** OFF

### 5. Timeout檢查

Active側(RGV)監控以下超時：

- **T1 = 5秒**: 發出VALID後等待L_REQ或U_REQ
- **T3 = 5秒**: 發出TR_REQ後等待READY（依現場可調整）
- **T5 = 10秒**: BUSY ON後執行搬運動作的時間（依現場可調整）
- **T6 = 5秒**: 發出COMP後等待READY OFF

### 6. 異常檢知

Active側(RGV)會檢測以下異常：

1. L_REQ信號及U_REQ信號同時ON
2. TR_REQ信號未ON，READY信號先ON
3. BUSY信號ON時，READY信號OFF
4. RGV移動到EQ Port時，LC_REQ或UC_REQ OFF

### 7. 異常處理

- **異常發生在BUSY ON之前**: EQ端的L_REQ/U_REQ及READY信號都OFF，RGV端的交握信號都OFF以結束交握時序
- **異常發生在BUSY ON之後**: EQ端的L_REQ/U_REQ及READY信號都OFF，RGV端的交握信號都OFF以結束交握時序，BUSY訊號於RGV退出交握區後OFF
- **EQ端ONLINE OFF**: RGV交握時序亦結束
- **Timeout**: 以警告(Warning)等級處理，程序繼續直到交握結束

## 使用測試UI

### 1. 啟動應用程式
執行編譯後的 `E84.Controller.Core.exe`

### 2. 選擇方向
- **Load (入料)**: RGV載入工件到EQ
- **Unload (出料)**: RGV從EQ載出工件

### 3. 啟動控制器
點擊 **"啟動 Start"** 按鈕

### 4. 模擬Load流程

1. 先勾選 **EQ_ONLINE** (模擬EQ在線)
2. 勾選 **LC_REQ** (模擬EQ可接受載入工件)
3. 觀察 **VALID** 輸出變綠（延遲0.5秒）
4. 勾選 **L_REQ** (模擬EQ要求載入)
5. 觀察 **TR_REQ** 輸出變綠（延遲0.5秒）
6. 勾選 **READY** (模擬EQ準備完成)
7. 觀察 **BUSY** 輸出變綠（延遲0.5秒）
8. 等待搬運動作完成（約10秒），**BUSY** OFF
9. 觀察 **COMP** 輸出變綠（延遲0.5秒）
10. 取消勾選 **READY** (模擬EQ收到COMP後READY OFF)
11. 觀察所有輸出依序OFF，返回Idle狀態

### 5. 模擬Unload流程

1. 選擇 **Unload** 方向
2. 先勾選 **EQ_ONLINE** (模擬EQ在線)
3. 勾選 **UC_REQ** (模擬EQ可接受載出工件)
4. 觀察 **VALID** 輸出變綠（延遲0.5秒）
5. 勾選 **U_REQ** (模擬EQ要求載出)
6. 觀察 **TR_REQ** 輸出變綠（延遲0.5秒）
7. 勾選 **READY** (模擬EQ準備完成)
8. 觀察 **BUSY** 輸出變綠（延遲0.5秒）
9. 等待搬運動作完成（約10秒），**BUSY** OFF
10. 觀察 **COMP** 輸出變綠（延遲0.5秒）
11. 取消勾選 **READY** (模擬EQ收到COMP後READY OFF)
12. 觀察所有輸出依序OFF，返回Idle狀態

### 6. 測試超時

#### T1超時測試（VALID後等待L_REQ/U_REQ）
1. 勾選 **LC_REQ** 或 **UC_REQ**
2. 觀察 **VALID** ON
3. 不要勾選 **L_REQ** 或 **U_REQ**
4. 等待5秒後，系統應進入Abort狀態

#### T3超時測試（TR_REQ後等待READY）
1. 完成到 **TR_REQ** ON的步驟
2. 不要勾選 **READY**
3. 等待5秒後，系統應進入Abort狀態

### 7. 測試錯誤條件

#### 測試L_REQ和U_REQ同時ON
1. 同時勾選 **L_REQ** 和 **U_REQ**
2. 系統應立即進入Abort狀態

#### 測試READY在TR_REQ之前ON
1. 在未勾選 **TR_REQ** 前先勾選 **READY**
2. 系統應檢測到錯誤並進入Abort狀態

## 配置參數

可在 `E84Config` 中調整以下參數：

```csharp
SignalResponseDelayMs = 500;    // 信號回覆延遲 (0.5秒)
T1_WaitLReqUReqMs = 5000;       // T1超時
T3_WaitReadyMs = 5000;          // T3超時
T5_TransferActionMs = 10000;    // T5搬運動作時間
T6_WaitReadyOffMs = 5000;       // T6超時
```

## 注意事項

1. **所有交握訊號都延遲0.5秒回覆**（符合規格要求）
2. **BUSY ON時，必須等待離開交握區才能OFF**
3. **EQ_ONLINE OFF時，交握時序會結束**
4. **LC_REQ或UC_REQ在交握過程中OFF會導致失敗**
5. **超時處理為Warning等級，程序會繼續直到交握結束**

## 開發環境

- .NET Framework 4.8
- Windows Forms
- Mono 6.8+ (Linux) 或 Visual Studio 2017+ (Windows)

## 建置

### Linux (使用 Mono):
```bash
xbuild E84.Controller.sln /p:Configuration=Debug
```

### Windows (使用 MSBuild):
```bash
msbuild E84.Controller.sln /p:Configuration=Debug
```

## 執行

### Linux (使用 Mono):
```bash
mono E84.Controller.Core/bin/Debug/E84.Controller.Core.exe
```

### Windows:
直接執行 `E84.Controller.Core.exe`

## 故障排除

如果遇到問題：
1. 檢查事件記錄面板查看詳細訊息
2. 確認 **EQ_ONLINE** 已勾選
3. 按照正確的順序勾選信號
4. 注意觀察0.5秒的延遲時間
5. 檢查是否觸發超時或錯誤條件
