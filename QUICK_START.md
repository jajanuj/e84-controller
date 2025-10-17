# E84 Controller Test UI - Quick Start

## 🚀 Getting Started (3 Steps)

### Step 1: Build and Run
```bash
# Open in Visual Studio
E84.Controller.sln

# Build (Ctrl+Shift+B)
# Run (F5)
```

### Step 2: Start Controller
1. Select direction: **Inbound** or **Outbound**
2. Click **"啟動 Start"**
3. Watch the state change to **Idle** (blue)

### Step 3: Test a Transfer
**For Inbound:**
```
☑ TR_REQ  → Watch BUSY & HO_AVBL turn green
☑ VALID   → Watch TRANSFER turn green
☑ COMPT   → Watch all outputs turn off, return to Idle
```

**For Outbound:**
```
☑ L_REQ   → Watch CLAMP turn green
Wait      → Watch DOCK turn green
☑ READY   → Watch TRANSFER turn green
☑ COMPT   → Watch all outputs turn off, return to Idle
```

---

## 📊 UI Layout at a Glance

```
┌─────────────┬──────────────┬──────────────┐
│ Controller  │ Outputs      │ Event Log    │
│ [Direction] │ [6 Signals]  │ [Scrolling]  │
│ [Start/Stop]│ Visual LEDs  │ Timestamps   │
│ [State]     ├──────────────┤ Color-coded  │
├─────────────┤ Status Info  │              │
│ Inputs      │ [Details]    │              │
│ [7 Signals] │              │              │
└─────────────┴──────────────┴──────────────┘
```

---

## 🎯 Signal Reference

### Inputs (Checkboxes - YOU Control)
| Signal | Chinese | Purpose | When to Use |
|--------|---------|---------|-------------|
| TR_REQ | 轉運請求 | Transfer Request | Start Inbound transfer |
| VALID  | 有效 | Carrier Valid | Confirm carrier in Inbound |
| COMPT  | 完成 | Complete | Finish any transfer |
| L_REQ  | 下層請求 | Lower Request | Start Outbound to lower |
| U_REQ  | 上層請求 | Upper Request | Start Outbound to upper |
| READY  | 就緒 | AMHS Ready | AMHS ready in Outbound |
| RESET  | 重置 | Reset Error | Recover from Abort/Error |

### Outputs (Displays - Controller Controls)
| Signal | Chinese | Meaning | Color When Active |
|--------|---------|---------|-------------------|
| BUSY | 忙碌 | System Busy | Green (Lime) |
| HO_AVBL | 可接收 | Handoff Available | Green (Lime) |
| TRANSFER | 轉運 | Transfer in Progress | Green (Lime) |
| CLAMP | 夾緊 | Carrier Clamped | Green (Lime) |
| DOCK | 對接 | Docked Position | Green (Lime) |
| ABORT | 中止 | Error/Abort State | Green (Lime) |

---

## 🔄 State Machine Quick Reference

```
        ┌─────────┐
   ┌───→│  Idle   │←────┐
   │    └────┬────┘     │
   │         │          │
   │         ↓          │
   │    ┌─────────┐     │
   │    │ Request │     │
   │    └────┬────┘     │
   │         ↓          │
   │    ┌──────┐        │
   │    │ Busy │        │
   │    └───┬──┘        │
   │        ↓           │
   │    ┌───────┐       │
   │    │ Valid │       │
   │    └───┬───┘       │
   │        ↓           │
   │   ┌──────────┐     │
   │   │ Transfer │     │
   │   └────┬─────┘     │
   │        ↓           │
   │   ┌──────────┐     │
   │   │ Complete │─────┘
   │   └──────────┘
   │
   │   ┌───────┐
   └───│ Abort │ (via RESET)
       └───────┘
```

---

## 🎨 Color Codes

| Color | Meaning |
|-------|---------|
| 🔵 Blue | Idle (ready) |
| 🟢 Green | Active transfer states |
| 🔴 Red | Error/Abort |
| 🟠 Orange | Resetting |
| ⬜ Gray | Inactive output |
| 🟩 Lime | Active output |

---

## ⚡ Common Workflows

### 1. Normal Inbound Transfer
```
Start → TR_REQ → VALID → COMPT → Back to Idle
Time: ~1 second
```

### 2. Normal Outbound Transfer
```
Start → L_REQ → Wait 1.5s → READY → COMPT → Back to Idle
Time: ~3 seconds
```

### 3. Timeout → Recovery
```
Start → TR_REQ → [Wait 10s, no VALID] → ABORT → RESET → Idle
Time: ~10 seconds
```

### 4. Quick Test Cycle
```
1. Inbound mode
2. Start
3. Check: TR_REQ, VALID, COMPT (rapid)
4. Verify: Returns to Idle
5. Repeat
```

---

## ⚠️ Troubleshooting Quick Fix

| Problem | Solution |
|---------|----------|
| Won't start | Check direction selected |
| Stuck in Abort | Check RESET signal |
| No output change | Verify controller started (Stop button enabled) |
| Too many logs | Stop/restart to clear |
| Can't click inputs | Make sure controller is started |

---

## 💡 Pro Tips

1. **Watch the Event Log** - It tells you everything that's happening
2. **Color = Status** - Blue=Ready, Green=Active, Red=Error
3. **Timing Matters** - Some states have built-in delays (CLAMP ~1.5s, DOCK ~3s)
4. **RESET is Your Friend** - Gets you out of error states
5. **Try Both Modes** - Inbound and Outbound have different sequences
6. **Test Timeouts** - Don't complete a sequence to see timeout handling

---

## 📚 More Information

- **Full Guide**: [TEST_UI_GUIDE.md](TEST_UI_GUIDE.md) - 5 detailed test scenarios
- **Architecture**: [README.md](README.md) - How it all works
- **Visual Design**: [UI_MOCKUP.md](UI_MOCKUP.md) - Detailed UI specifications
- **Changes**: [CHANGES.md](CHANGES.md) - What was implemented

---

## 🎯 Success Checklist

- [ ] Application starts without errors
- [ ] Can select Inbound/Outbound
- [ ] Start button works
- [ ] Can check input signals
- [ ] Outputs change color when active
- [ ] State changes visible
- [ ] Event log shows messages
- [ ] Can complete full transfer cycle
- [ ] Can trigger and recover from abort
- [ ] Stop button works

If all checked ✅, you're ready to test!

---

## 🆘 Need Help?

1. Check the **Event Log** panel (right side) for error messages
2. Review **TEST_UI_GUIDE.md** for detailed scenarios
3. Check **CHANGES.md** for implementation details
4. Refer to SEMI E84 specification for protocol details

---

## 📝 Quick Test Script

Copy/paste this test sequence:

```
Test 1: Basic Inbound
1. Select: Inbound
2. Click: Start
3. Check: TR_REQ ✓
4. Verify: BUSY + HO_AVBL green
5. Check: VALID ✓
6. Verify: TRANSFER green
7. Check: COMPT ✓
8. Verify: Back to Idle
✅ Pass if returned to Idle

Test 2: Basic Outbound
1. Select: Outbound
2. Click: Start
3. Check: L_REQ ✓
4. Verify: CLAMP green
5. Wait: 2 seconds
6. Verify: DOCK green
7. Check: READY ✓
8. Verify: TRANSFER green
9. Check: COMPT ✓
10. Verify: Back to Idle
✅ Pass if returned to Idle

Test 3: Timeout
1. Select: Inbound
2. Click: Start
3. Check: TR_REQ ✓
4. Wait: 10 seconds (don't check VALID)
5. Verify: ABORT turns green, State = Abort
6. Check: RESET ✓
7. Verify: Back to Idle
✅ Pass if recovered to Idle
```

---

**That's it! You're ready to test the E84 Controller. Happy Testing! 🎉**
