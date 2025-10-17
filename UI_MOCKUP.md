# E84 Controller Test UI - Visual Mockup

## Main Window Layout (1000x550 pixels)

```
╔═══════════════════════════════════════════════════════════════════════════════════════════════╗
║ E84 控制器測試介面 E84 Controller Test UI                                            [_][□][X] ║
╠═══════════════════════════════════════════════════════════════════════════════════════════════╣
║                                                                                               ║
║  ┌────────────────────────┐  ┌─────────────────────┐  ┌─────────────────────────────────┐   ║
║  │ 控制器 Controller      │  │ PLC 輸出 Outputs    │  │ 事件記錄 Event Log              │   ║
║  ├────────────────────────┤  ├─────────────────────┤  ├─────────────────────────────────┤   ║
║  │ 方向 Direction:        │  │  ┌────────────────┐ │  │ [13:24:15.123] INFO: E84 ...   │   ║
║  │  ⦿ Inbound (進站)      │  │  │ BUSY (忙碌)    │ │  │ [13:24:15.456] INFO: ...       │   ║
║  │  ○ Outbound (出站)     │  │  └────────────────┘ │  │ [13:24:16.789] DEBUG: ...      │   ║
║  │                        │  │  ┌────────────────┐ │  │ [13:24:17.012] INFO: Idle ...  │   ║
║  │  ┌──────────────────┐  │  │  │ HO_AVBL (可接收)│ │  │ [13:24:18.345] DEBUG: ...      │   ║
║  │  │  啟動 Start      │  │  │  └────────────────┘ │  │ [13:24:19.678] INFO: ...       │   ║
║  │  └──────────────────┘  │  │  ┌────────────────┐ │  │                                 │   ║
║  │  ┌──────────────────┐  │  │  │ TRANSFER       │ │  │ Black background with          │   ║
║  │  │  停止 Stop       │  │  │  │ (轉運)         │ │  │ green text (Consolas font)     │   ║
║  │  └──────────────────┘  │  │  └────────────────┘ │  │                                 │   ║
║  │                        │  │  ┌────────────────┐ │  │ Auto-scrolls to bottom         │   ║
║  │ 狀態 State:            │  │  │ CLAMP (夾緊)   │ │  │ Max 1000 lines                  │   ║
║  │      Idle              │  │  └────────────────┘ │  │                                 │   ║
║  │  (blue/green/red)      │  │  ┌────────────────┐ │  │                                 │   ║
║  └────────────────────────┘  │  │ DOCK (對接)    │ │  │                                 │   ║
║                              │  └────────────────┘ │  │                                 │   ║
║  ┌────────────────────────┐  │  ┌────────────────┐ │  │                                 │   ║
║  │ 模擬 PLC 輸入          │  │  │ ABORT (中止)   │ │  │                                 │   ║
║  │ Simulated Inputs       │  │  └────────────────┘ │  │                                 │   ║
║  ├────────────────────────┤  └─────────────────────┘  │                                 │   ║
║  │  ☑ TR_REQ (轉運請求)   │                           │                                 │   ║
║  │                        │  ┌─────────────────────┐  │                                 │   ║
║  │  ☐ VALID (有效)        │  │ 狀態資訊 Status Info │  │                                 │   ║
║  │                        │  ├─────────────────────┤  │                                 │   ║
║  │  ☐ COMPT (完成)        │  │ State: Idle         │  │                                 │   ║
║  │                        │  │ Direction: Inbound  │  │                                 │   ║
║  │  ☐ L_REQ (下層請求)    │  │ Time: 13:24:39      │  │                                 │   ║
║  │                        │  │                     │  │                                 │   ║
║  │  ☐ U_REQ (上層請求)    │  │                     │  │                                 │   ║
║  │                        │  │                     │  │                                 │   ║
║  │  ☐ READY (就緒)        │  │                     │  │                                 │   ║
║  │                        │  │                     │  │                                 │   ║
║  │  ☐ RESET (重置)        │  │                     │  │                                 │   ║
║  │                        │  └─────────────────────┘  │                                 │   ║
║  └────────────────────────┘                           └─────────────────────────────────┘   ║
║                                                                                               ║
╚═══════════════════════════════════════════════════════════════════════════════════════════════╝
```

## Component Details

### Left Column (300px width)

#### Controller Panel (Top, 150px height)
- **Direction Selection**: Radio buttons
  - ⦿ Inbound (進站) - Selected by default
  - ○ Outbound (出站)
- **Control Buttons**: 
  - [啟動 Start] - 90x25px, enabled initially
  - [停止 Stop] - 90x25px, disabled initially
- **State Display**:
  - Label: "狀態 State:"
  - Current State: Large bold text (12pt), color-coded by state
    - Blue: Idle
    - Green: Active states
    - Red: Error/Abort
    - Orange: Resetting

#### Input Simulation Panel (Bottom, 220px height)
- **Title**: "模擬 PLC 輸入 (Simulated Inputs)"
- **Checkboxes** (7 total):
  - ☐ TR_REQ (轉運請求)
  - ☐ VALID (有效)
  - ☐ COMPT (完成)
  - ☐ L_REQ (下層請求)
  - ☐ U_REQ (上層請求)
  - ☐ READY (就緒)
  - ☐ RESET (重置)
- Each checkbox triggers immediate PLC simulation

### Middle Column (250px width)

#### Output Display Panel (Top, 220px height)
- **Title**: "PLC 輸出 (Outputs)"
- **Output Indicators** (6 total):
  - Each indicator: 220x30px label
  - Bold 9.75pt font
  - Border style: FixedSingle
  - Background colors:
    - Gray (LightGray): Inactive
    - Green (Lime): Active
  - Labels:
    1. BUSY (忙碌)
    2. HO_AVBL (可接收)
    3. TRANSFER (轉運)
    4. CLAMP (夾緊)
    5. DOCK (對接)
    6. ABORT (中止)

#### Status Info Panel (Bottom, 150px height)
- **Title**: "狀態資訊 Status Info"
- **Content**: Consolas 8.25pt font
  - Current State
  - Current Direction
  - Timestamp
  - Last Error (if any)
- Updates every 100ms via timer

### Right Column (414px width, full height)

#### Event Log Panel (526px height)
- **Title**: "事件記錄 Event Log"
- **Text Box**:
  - Multiline, read-only
  - Vertical scrollbar
  - Black background (Color.Black)
  - Green text (Color.Lime)
  - Consolas 9pt font
  - Auto-scroll to bottom on new entries
  - Format: `[HH:mm:ss.fff] [LEVEL] Message`
  - Levels: DEBUG, INFO, WARN, ERROR
  - Max 1000 lines (auto-trim)

## Visual States

### Initial State (Application Start)
```
Controller Panel:
  - Inbound selected (radio button filled)
  - Start button: Enabled, normal appearance
  - Stop button: Disabled, grayed out
  - State label: "Idle" in blue

Input Panel:
  - All checkboxes unchecked

Output Panel:
  - All indicators gray (inactive)

Event Log:
  - Startup messages displayed
  - Instructions shown
```

### Running State (After Start)
```
Controller Panel:
  - Direction radio buttons: Disabled
  - Start button: Disabled
  - Stop button: Enabled
  - State label: Updates dynamically with color

Input Panel:
  - All checkboxes enabled and interactive

Output Panel:
  - Indicators change to green when active
  - Real-time updates every 100ms

Event Log:
  - Continuous stream of events
  - State transitions logged
  - Signal changes logged
```

### Active Transfer State Example
```
When TR_REQ is checked and VALID received:

Output Panel:
  ┌────────────────┐
  │ BUSY (忙碌)    │ ← GREEN (Lime background)
  └────────────────┘
  ┌────────────────┐
  │ HO_AVBL (可接收)│ ← GREEN (Lime background)
  └────────────────┘
  ┌────────────────┐
  │ TRANSFER (轉運) │ ← GREEN (Lime background)
  └────────────────┘
  ┌────────────────┐
  │ CLAMP (夾緊)   │ ← GRAY (inactive)
  └────────────────┘
  ┌────────────────┐
  │ DOCK (對接)    │ ← GRAY (inactive)
  └────────────────┘
  ┌────────────────┐
  │ ABORT (中止)   │ ← GRAY (inactive)
  └────────────────┘

State Label:
  "Transfer" in GREEN

Event Log:
  [13:24:15.123] INFO: Idle -> Request
  [13:24:15.456] INFO: Request -> Busy
  [13:24:15.789] DEBUG: Output BUSY -> true
  [13:24:15.790] DEBUG: Output HO_AVBL -> true
  [13:24:16.123] DEBUG: Signal rising: VALID
  [13:24:16.456] INFO: Busy -> Valid
  [13:24:16.789] INFO: Valid -> Transfer
  [13:24:16.790] DEBUG: Output TRANSFER -> true
```

### Error/Abort State
```
Output Panel:
  ┌────────────────┐
  │ ABORT (中止)   │ ← RED/PINK background
  └────────────────┘
  (Other outputs gray)

State Label:
  "Abort" in RED

Event Log:
  [13:24:20.123] ERROR: Timeout: VALID
  [13:24:20.124] WARN: Timeout
  [13:24:20.125] INFO: Busy -> Abort
  [13:24:20.126] DEBUG: Output ABORT -> true
```

## Color Palette

### State Colors
- **Idle**: `Color.Blue` (RGB: 0, 0, 255)
- **Active States**: `Color.Green` (RGB: 0, 128, 0)
- **Complete**: `Color.DarkGreen` (RGB: 0, 100, 0)
- **Error/Abort**: `Color.Red` (RGB: 255, 0, 0)
- **Resetting**: `Color.Orange` (RGB: 255, 165, 0)

### Output Colors
- **Inactive**: `Color.LightGray` (RGB: 211, 211, 211)
- **Active**: `Color.Lime` (RGB: 0, 255, 0)
- **Text**: `Color.Black` (RGB: 0, 0, 0)

### Log Colors
- **Background**: `Color.Black` (RGB: 0, 0, 0)
- **Text**: `Color.Lime` (RGB: 0, 255, 0)

## Fonts

- **Controller State**: Microsoft Sans Serif, 12pt, Bold
- **Output Labels**: Microsoft Sans Serif, 9.75pt, Bold
- **Event Log**: Consolas, 9pt, Regular
- **Status Info**: Consolas, 8.25pt, Regular
- **Default UI**: Microsoft Sans Serif, 8.25pt, Regular

## Interactive Behaviors

### Hover Effects
- Standard Windows Forms hover effects on buttons
- Checkboxes show hand cursor on hover

### Click Feedback
- Buttons show pressed state
- Checkboxes toggle immediately
- Immediate PLC simulation on checkbox change

### Real-time Updates
- Timer ticks every 100ms
- Status snapshot retrieved from controller
- Output displays updated based on snapshot
- State label updated with new state and color
- No flicker due to efficient update logic

### Form Resizing
- Minimum size: 1016x589 pixels
- Event log resizes with window (anchored)
- Other panels maintain fixed size
- Professional layout maintained at all sizes

## Accessibility Features

- Clear bilingual labels (Chinese/English)
- High contrast colors for visibility
- Large, readable fonts
- Logical tab order
- Keyboard navigation support
- Clear visual feedback for all states
- Intuitive layout following left-to-right, top-to-bottom flow

## Error Indicators

1. **Red State Text**: Indicates Error or Abort state
2. **ABORT Output Active**: Visual alarm (green/red)
3. **Error Messages in Log**: Clearly marked with [ERROR] prefix
4. **Last Error in Status**: Shows last error message
5. **State Color Change**: Immediate visual feedback

## Performance Characteristics

- **Update Frequency**: 100ms (10 Hz)
- **Log Capacity**: 1000 lines maximum
- **UI Responsiveness**: Non-blocking async operations
- **Memory Footprint**: Low (< 50 MB typical)
- **CPU Usage**: Low (< 1% typical on modern systems)

This mockup demonstrates a professional, functional test interface that provides comprehensive visibility into the E84 controller's operation while maintaining usability and visual clarity.
