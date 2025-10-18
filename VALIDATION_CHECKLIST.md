# E84 Active Side Implementation - Validation Checklist

## Build Validation
- [x] Project builds successfully with xbuild/Mono
- [x] No compilation errors
- [x] Only non-critical warnings (unused fields)

## Code Structure Validation

### Models/Enums.cs
- [x] E84Direction: Load, Unload
- [x] E84State: 11 states for Active side state machine
- [x] E84FaultCode: T1-T6 timeout codes + error codes

### Models/E84Config.cs
- [x] SignalResponseDelayMs = 500 (0.5s requirement)
- [x] T1_WaitLReqUReqMs = 5000
- [x] T3_WaitReadyMs = 5000
- [x] T5_TransferActionMs = 10000
- [x] T6_WaitReadyOffMs = 5000

### Controller/E84Controller.cs
- [x] ScheduleDelayedSignal() method for 0.5s delays
- [x] ProcessDelayedSignal() method
- [x] CheckErrorConditions() for Active side errors
- [x] State machine with 11 states
- [x] Load sequence implementation
- [x] Unload sequence implementation
- [x] Timeout monitoring (T1, T3, T5, T6)
- [x] Error recovery logic

### Form1.cs
- [x] Updated I/O mappings:
  - Active outputs: VALID, TR_REQ, BUSY, COMP
  - Passive inputs: L_REQ, U_REQ, READY, LC_REQ, UC_REQ, Carrier, EQ_ONLINE, IN_LINE, ALARM, IDLE, RUN
- [x] Radio buttons: radioLoad, radioUnload
- [x] Config with correct timeout values

### Form1.Designer.cs
- [x] 11 input checkboxes for Passive (EQ) signals
- [x] 4 output labels for Active (RGV) signals
- [x] Radio buttons for Load/Unload
- [x] Proper layout and grouping

## Specification Compliance

### Signal Definitions (A->P: Active to Passive)
- [x] VALID - RGV到达EQ时ON，表示交握通讯开始
- [x] TR_REQ - RGV接受EQ载入/载出工件要求
- [x] BUSY - 确认READY ON后，开始载入或载出工件
- [x] COMP - 交握完成，RGV离开交握区

### Signal Definitions (P->A: Passive to Active)
- [x] L_REQ - EQ要求载入工件
- [x] U_REQ - EQ要求载出工件
- [x] READY - EQ准备完成
- [x] LC_REQ - 准备完成可以载入工件
- [x] UC_REQ - 准备完成可以载出工件
- [x] Carrier - 工件在席检知
- [x] EQ_ONLINE - EQ在线状态
- [x] IN_LINE - EQ并入产线
- [x] ALARM - EQ异常
- [x] IDLE - EQ无工件且无运转
- [x] RUN - EQ运转生产

### Load Sequence (入料)
1. [x] EQ发送LC_REQ
2. [x] RGV发送Carrier ID和VALID (延迟0.5s)
3. [x] EQ发送L_REQ
4. [x] RGV发送TR_REQ (延迟0.5s)
5. [x] EQ发送READY
6. [x] RGV发送BUSY (延迟0.5s)并开始载入
7. [x] RGV完成后BUSY OFF
8. [x] RGV发送COMP (延迟0.5s)
9. [x] EQ的READY、L_REQ、LC_REQ OFF
10. [x] RGV的COMP OFF (延迟0.5s)
11. [x] VALID OFF

### Unload Sequence (出料)
1. [x] EQ发送Carrier ID和UC_REQ
2. [x] RGV发送VALID (延迟0.5s)
3. [x] EQ发送U_REQ
4. [x] RGV发送TR_REQ (延迟0.5s)
5. [x] EQ发送READY
6. [x] RGV发送BUSY (延迟0.5s)并开始载出
7. [x] RGV完成后BUSY OFF
8. [x] RGV发送COMP (延迟0.5s)
9. [x] EQ的READY、U_REQ、UC_REQ OFF
10. [x] RGV的COMP OFF (延迟0.5s)
11. [x] VALID OFF

### Timeout Monitoring (Active Side)
- [x] T1 (5s): VALID → L_REQ/U_REQ
- [x] T3 (5s): TR_REQ → READY
- [x] T5 (10s): BUSY → Transfer Complete
- [x] T6 (5s): COMP → READY OFF

### Error Detection (Active Side)
- [x] L_REQ和U_REQ同时ON
- [x] TR_REQ未ON，READY先ON
- [x] BUSY ON时，READY OFF
- [x] 移动到EQ Port时，LC_REQ或UC_REQ OFF
- [x] EQ_ONLINE OFF时结束交握

### Error Recovery
- [x] 异常发生在BUSY ON之前：所有信号OFF
- [x] 异常发生在BUSY ON之后：BUSY于退出交握区后OFF，其他信号OFF
- [x] EQ端OnLine OFF：交握时序结束
- [x] Timeout处理为Warning等级

## Documentation
- [x] RGV_ACTIVE_GUIDE.md - Comprehensive Chinese user guide
- [x] IMPLEMENTATION_SUMMARY.md - Technical implementation summary
- [x] VALIDATION_CHECKLIST.md - This file

## Test Scenarios (To be executed)

### Scenario 1: Normal Load
1. [ ] Start controller in Load mode
2. [ ] Check EQ_ONLINE
3. [ ] Check LC_REQ → Verify VALID ON after ~0.5s
4. [ ] Check L_REQ → Verify TR_REQ ON after ~0.5s
5. [ ] Check READY → Verify BUSY ON after ~0.5s
6. [ ] Wait ~10s → Verify BUSY OFF, COMP ON after ~0.5s
7. [ ] Uncheck READY → Verify COMP OFF after ~0.5s, VALID OFF
8. [ ] Verify return to Idle state

### Scenario 2: Normal Unload
1. [ ] Start controller in Unload mode
2. [ ] Check EQ_ONLINE
3. [ ] Check UC_REQ → Verify VALID ON after ~0.5s
4. [ ] Check U_REQ → Verify TR_REQ ON after ~0.5s
5. [ ] Check READY → Verify BUSY ON after ~0.5s
6. [ ] Wait ~10s → Verify BUSY OFF, COMP ON after ~0.5s
7. [ ] Uncheck READY → Verify COMP OFF after ~0.5s, VALID OFF
8. [ ] Verify return to Idle state

### Scenario 3: T1 Timeout
1. [ ] Check LC_REQ → VALID ON
2. [ ] Don't check L_REQ
3. [ ] Wait 5 seconds
4. [ ] Verify timeout error and abort

### Scenario 4: T3 Timeout
1. [ ] Complete up to TR_REQ ON
2. [ ] Don't check READY
3. [ ] Wait 5 seconds
4. [ ] Verify timeout error and abort

### Scenario 5: T6 Timeout
1. [ ] Complete up to COMP ON
2. [ ] Don't uncheck READY
3. [ ] Wait 5 seconds
4. [ ] Verify timeout error and abort

### Scenario 6: L_REQ and U_REQ Both ON
1. [ ] Check both L_REQ and U_REQ simultaneously
2. [ ] Verify immediate abort

### Scenario 7: READY Before TR_REQ
1. [ ] Check READY before TR_REQ is ON
2. [ ] Verify immediate abort

### Scenario 8: EQ_ONLINE OFF During Transfer
1. [ ] Start normal Load sequence
2. [ ] During transfer, uncheck EQ_ONLINE
3. [ ] Verify abort and cleanup

### Scenario 9: LC_REQ/UC_REQ OFF During Transfer
1. [ ] Start normal Load sequence
2. [ ] After VALID ON, uncheck LC_REQ
3. [ ] Verify abort

## Summary

✅ **Implementation Status**: Complete and ready for testing

✅ **Build Status**: Successfully builds with no errors

✅ **Specification Compliance**: All requirements implemented

✅ **Documentation**: Complete user and technical documentation

✅ **Next Steps**: Manual testing with UI simulator or integration with actual EQ equipment

---

**Note**: The manual test scenarios marked with [ ] should be executed by the user with the Windows Forms UI or during integration testing with actual equipment.
