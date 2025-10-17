# E84 Controller Test UI - Quick Start Guide

## UI Layout Overview

The test UI is divided into several sections:

```
┌─────────────────────────────────────────────────────────────────────────┐
│ E84 控制器測試介面 E84 Controller Test UI                                │
├──────────────────────────┬───────────────────┬──────────────────────────┤
│ 控制器 Controller        │ PLC 輸出 Outputs  │ 事件記錄 Event Log       │
│ ┌──────────────────────┐ │ ┌───────────────┐ │ ┌──────────────────────┐ │
│ │ 方向 Direction:      │ │ │ BUSY (忙碌)   │ │ │ [HH:mm:ss.fff] INFO: │ │
│ │ ○ Inbound (進站)     │ │ │ HO_AVBL       │ │ │ State transition...  │ │
│ │ ○ Outbound (出站)    │ │ │ TRANSFER      │ │ │ [HH:mm:ss.fff] ...   │ │
│ │                      │ │ │ CLAMP (夾緊)  │ │ │                      │ │
│ │ [啟動 Start]         │ │ │ DOCK (對接)   │ │ │                      │ │
│ │ [停止 Stop]          │ │ │ ABORT (中止)  │ │ │                      │ │
│ │                      │ │ └───────────────┘ │ │                      │ │
│ │ 狀態 State: Idle     │ │                   │ │                      │ │
│ └──────────────────────┘ │ 狀態資訊 Status   │ │                      │ │
│                          │ ┌───────────────┐ │ │                      │ │
│ 模擬 PLC 輸入 (Inputs)   │ │ State: Idle   │ │ │                      │ │
│ ┌──────────────────────┐ │ │ Direction:... │ │ │                      │ │
│ │ □ TR_REQ (轉運請求)  │ │ │ Time: ...     │ │ │                      │ │
│ │ □ VALID (有效)       │ │ │               │ │ │                      │ │
│ │ □ COMPT (完成)       │ │ └───────────────┘ │ │                      │ │
│ │ □ L_REQ (下層請求)   │ │                   │ │                      │ │
│ │ □ U_REQ (上層請求)   │ │                   │ │                      │ │
│ │ □ READY (就緒)       │ │                   │ │                      │ │
│ │ □ RESET (重置)       │ │                   │ │                      │ │
│ └──────────────────────┘ │                   │ └──────────────────────┘ │
└──────────────────────────┴───────────────────┴──────────────────────────┘
```

## Test Scenarios

### Scenario 1: Basic Inbound Transfer

**Purpose**: Test a successful inbound material transfer from AMHS to equipment.

**Steps**:
1. Select **Inbound (進站)** radio button
2. Click **啟動 Start** button
3. Check **TR_REQ** checkbox (AMHS requests transfer)
   - Expected: **BUSY** and **HO_AVBL** outputs turn green
   - Expected: State changes to **Request** → **Busy**
4. Check **VALID** checkbox (AMHS confirms valid carrier)
   - Expected: **TRANSFER** output turns green
   - Expected: State changes to **Valid** → **Transfer**
5. Check **COMPT** checkbox (Transfer complete)
   - Expected: All outputs turn off (gray)
   - Expected: State changes to **Complete** → **Idle**
6. Uncheck all input checkboxes to reset

**Expected Result**: Complete state cycle without errors, all outputs properly activated and deactivated.

---

### Scenario 2: Basic Outbound Transfer

**Purpose**: Test a successful outbound material transfer from equipment to AMHS.

**Steps**:
1. Select **Outbound (出站)** radio button
2. Click **啟動 Start** button
3. Check **L_REQ** checkbox (Request lower level transfer)
   - Expected: **CLAMP** output turns green
   - Expected: State changes to **Request** → **Busy**
   - Wait ~1.5 seconds for clamp action
4. Observe **DOCK** output turns green automatically
   - Expected: State remains in **Busy**
5. Check **READY** checkbox (AMHS ready to receive)
   - Expected: **TRANSFER** output turns green
   - Expected: State changes to **Valid** → **Transfer**
6. Check **COMPT** checkbox (Transfer complete)
   - Expected: All outputs turn off
   - Expected: State changes to **Complete** → **Idle**

**Expected Result**: Complete outbound cycle with proper clamp and dock sequence.

---

### Scenario 3: Timeout Handling

**Purpose**: Test timeout behavior when expected signals are not received.

**Steps**:
1. Select **Inbound (進站)** radio button
2. Click **啟動 Start** button
3. Check **TR_REQ** checkbox
   - Expected: **BUSY** and **HO_AVBL** turn green
4. **DO NOT** check **VALID** checkbox
5. Wait for timeout (~10 seconds)
   - Expected: **ABORT** output turns red
   - Expected: State changes to **Abort**
   - Expected: Event log shows timeout error
6. Check **RESET** checkbox to recover
   - Expected: **ABORT** turns off
   - Expected: State changes to **Resetting** → **Idle**

**Expected Result**: Controller properly detects timeout and enters abort state, then recovers with RESET.

---

### Scenario 4: Error Recovery

**Purpose**: Test manual abort and reset functionality.

**Steps**:
1. Start controller in either direction
2. Begin a transfer sequence (check TR_REQ or L_REQ)
3. During transfer, observe **ABORT** output behavior
4. If abort occurs (timeout or other error):
   - Check **RESET** checkbox
   - Verify state transitions through **Resetting** to **Idle**
5. Verify all outputs are cleared
6. Start a new transfer cycle to confirm recovery

**Expected Result**: System cleanly recovers from error states and can resume normal operation.

---

### Scenario 5: Rapid State Changes

**Purpose**: Test controller behavior with rapid input changes.

**Steps**:
1. Start controller in **Inbound** mode
2. Rapidly check and uncheck **TR_REQ** multiple times
3. Observe debounce behavior (50ms default)
4. Check complete transfer sequence with minimal delays:
   - TR_REQ → VALID → COMPT (check all quickly)
5. Observe event log for proper sequencing

**Expected Result**: Debounce prevents spurious state changes, controller follows proper sequence regardless of rapid inputs.

---

## Color Coding Reference

### State Colors
- **Blue**: Idle state (ready for operation)
- **Green**: Active transfer states (Request, Busy, Valid, Transfer)
- **Dark Green**: Complete state
- **Red**: Error states (Abort, Error)
- **Orange**: Resetting state

### Output Colors
- **Green (Lime)**: Output is active/ON
- **Gray**: Output is inactive/OFF

## Event Log Messages

Common log messages you'll see:

```
[INFO] Idle -> Request              # State transition
[INFO] PLC connected                # Successful PLC connection
[DEBUG] Signal rising: TR_REQ       # Input signal activated
[DEBUG] Signal falling: TR_REQ      # Input signal deactivated
[DEBUG] Output BUSY -> true         # Output activated
[WARN] Interlock failed: ...        # Safety interlock check failed
[ERROR] Timeout: VALID              # Timeout waiting for signal
```

## Tips and Best Practices

1. **Always start with Idle state**: Ensure controller is in Idle before beginning a new transfer
2. **Follow the sequence**: Don't skip steps in the transfer sequence
3. **Watch the event log**: It provides detailed information about state machine behavior
4. **Use RESET for recovery**: If you get stuck in Abort/Error state, use RESET to recover
5. **Observe timing**: Some outputs (like CLAMP) have built-in delays
6. **Stop before closing**: Always stop the controller before closing the application

## Troubleshooting

### Controller won't start
- Ensure a direction is selected
- Check event log for error messages
- Try restarting the application

### Outputs not changing
- Verify controller is started (Stop button should be enabled)
- Check input signals are properly checked
- Review event log for state transitions

### Stuck in Abort state
- Check RESET input checkbox
- Wait for Resetting state to transition to Idle
- If still stuck, stop and restart controller

### Event log too full
- Log automatically limits to 1000 lines
- Stop and restart controller to clear log

## Advanced Usage

### Simulating Communication Failures
The test UI uses FakePlc which allows testing without real hardware. For production:
- Replace FakePlc with MxComponentPlc for Mitsubishi PLCs
- Implement custom IE84Plc for other PLC types

### Customizing Timeouts
Modify E84Config parameters in Form1.cs InitializeController method:
```csharp
_config = new E84Config
{
    WaitValidMs = 15000,  // Increase timeout to 15 seconds
    WaitComptMs = 20000,  // Increase completion timeout
    // ... other settings
};
```

### Adding Safety Interlocks
Replace AlwaysPassInterlock with custom implementation:
```csharp
public class CustomInterlock : IE84SafetyInterlock
{
    public bool Check(E84State state, out string reason)
    {
        // Check door sensors, e-stops, etc.
        if (/* safety condition fails */)
        {
            reason = "Safety condition not met";
            return false;
        }
        reason = string.Empty;
        return true;
    }
}
```

## State Machine Diagram

```
                    ┌──────────┐
                    │   Idle   │
                    └────┬─────┘
                         │ TR_REQ/L_REQ/U_REQ
                         ↓
                    ┌─────────┐
                    │ Request │
                    └────┬────┘
                         │ Assert BUSY/CLAMP
                         ↓
                    ┌──────┐
                    │ Busy │←──────┐
                    └───┬──┘       │
                        │ VALID   │ Timeout
                        ↓         │
                    ┌───────┐     │
                    │ Valid │     │
                    └───┬───┘     │
                        │ Assert TRANSFER
                        ↓         │
                  ┌──────────┐   │
                  │ Transfer │   │
                  └────┬─────┘   │
                       │ COMPT   │
                       ↓         │
                  ┌──────────┐   │
                  │ Complete │   │
                  └────┬─────┘   │
                       │         │
                       └─────────┤
                                 │
                            ┌────┴────┐
                            │  Abort  │
                            └────┬────┘
                                 │ RESET
                                 ↓
                           ┌──────────┐
                           │ Resetting│
                           └────┬─────┘
                                │
                                └─────→ Back to Idle
```

## Support

For issues or questions:
1. Check the event log for detailed error messages
2. Review README.md for architecture details
3. Consult SEMI E84 specification for protocol details
4. Submit issues to the project repository
