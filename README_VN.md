<p align="center">
  <img src="https://raw.githubusercontent.com/Trongdepzai-dev/Xorium-Stealer-Pulsar/main/abyss_intro.png" alt="Xorium Pulsar Abyss Edition" width="800">
</p>

# 🌑 XORIUM STEALER PULSAR [ABYSS EDITION] 🌑
### *Khi Bóng Tối Nuốt Chửng Mọi Hệ Thống.*

[![Rust](https://img.shields.io/badge/Lõi-Rust_Shadow-orange?style=for-the-badge&logo=rust)]()
[![C#](https://img.shields.io/badge/Loader-C%23_AbyssPower-blue?style=for-the-badge&logo=csharp)]()
[![Status](https://img.shields.io/badge/Trạng_Thái-UNDETECTED-brightgreen?style=for-the-badge)]()
[![Privilege](https://img.shields.io/badge/Đặc_Quyền-NT_AUTHORITY%5CSYSTEM-red?style=for-the-badge)]()
[![Kiến Trúc](https://img.shields.io/badge/Kiến_Trúc-Hybrid_Kernel-blueviolet?style=for-the-badge)]()

---

## 💀 TẠI SAO LẠI CHỌN XORIUM ABYSS?

Bạn đã chán ngấy những công cụ "FUD" rẻ tiền bị phát hiện chỉ sau một bản cập nhật Windows? Bạn mệt mỏi với những phần mềm bị EDR khóa chặt ngay khi vừa thực thi?
**Xorium Stealer Pulsar: Abyss Edition** không chỉ là một Stealer. Nó là một **Thực Thể Kernel** được thiết kế để **Tàng Hình và Hủy Diệt**.

Chúng tôi đã kết hợp sức mạnh thô của **C# .NET 8** với sự tàng hình tuyệt đối của **Rust Nightly Driver**.
Kết quả? Một loại mã độc không chỉ đánh cắp dữ liệu—nó **THỐNG TRỊ** toàn bộ hệ điều hành từ bên trong Vực Thẳm.

---

## ⚡ CÁC TÍNH NĂNG "VỰC THẲM" (ABYSS FEATURES)

### 1. 👻 PROJECT SHADOW (Siêu Tàng Hình)
Hầu hết các Stealer đều bị bắt bởi heuristics. Xorium sử dụng **Custom Rust Stealth Core** (`shadow.sys`).
- **DKOM Cloaking**: Biến mất hoàn toàn khỏi PsActiveProcessHead. Không Task Manager nào thấy được.
- **ETW-TI Blinding**: Làm mù hệ thống Threat Intelligence của Windows.
- **DSE Heresy**: Tắt Driver Signature Enforcement để nạp bất kỳ driver nào.

### 2. 👑 SYSTEM RITUAL (Nghi Thức Quyền Lực)
- **Token Theft LPE**: Chiếm đoạt Token của các tiến trình hệ thống như `lsass.exe`.
- **Ghost Elevation**: Tự động nâng quyền lên `NT AUTHORITY\SYSTEM` mà không cần UAC.
- **PPL Protection**: Biến Pulsar thành một tiến trình được bảo vệ (Protected Process), không thể bị tắt bởi bất kỳ ai.

### 3. 🌑 VOID WALKER (Rootkit Kernel Ring 0)
Vũ khí cuối cùng để kiểm soát tuyệt đối OS.
- **EDR Cataclysm**: Xóa sạch mọi Callbacks của EDR (CrowdStrike, SentinelOne, v.v.) khỏi Kernel.
- **Kernel Keylogger**: Bắt phím ở cấp độ driver, vượt qua mọi sandbox.
- **Network Ghosting**: Ẩn cổng kết nối C2 khỏi mọi công cụ như `netstat` hay `Process Hacker`.

---

## 📜 DANH SÁCH LỆNH CẤM (VẬN HÀNH QUA C2)

### Forbidden Commands (C2-Operational)
| Sign | Force | Effect | Parameters |
| :--- | :--- | :--- | :--- |
| `collect` | **Scraper** | Quét 150+ mục tiêu (Browser, Wallet, etc.) | N/A |
| `shadow_fullstealth`| **Ritual** | Ẩn tiến trình + Driver + Tắt ETW (Tất cả trong 1). | N/A |
| `shadow_ghost` | **Ritual** | Nâng quyền SYSTEM + Ẩn tiến trình. | N/A |
| `shadow_nuke_edr` | **Cataclysm**| Xóa sạch EDR callbacks + Vô hiệu hóa ETW. | N/A |
| `shadow_hide_c2port`| **Net** | Ẩn cổng kết nối khỏi netstat/viewers. | `port` |
| `shadow_inject_apc` | **Infect** | Injection shellcode bí mật qua APC. | `pid\|path` |
| `shadow_inject_hijack`| **Infect** | Kỹ thuật injection thread-hijacking đỉnh cao. | `pid\|path` |
| `shadow_bypass_hvci` | **Bypass** | Vô hiệu hóa Hypervisor Code Integrity. | N/A |
| `shadow_uefi_persist`| **Curse** | Cài đặt UEFI bootkit tồn tại vĩnh viễn. | N/A |
| `kernel_hide_port` | **Kernel** | Ẩn cổng TCP/UDP cấp độ thấp. | `proto\|port` |
| `kernel_clean_callbacks`| **Kernel** | Xóa nhanh các callback Process/Thread/Image. | N/A |
| `kernel_ghost_reg` | **Kernel** | Ẩn key/value registry khỏi mọi trình xem. | `key\|value` |
| `kernel_hide_thread` | **Kernel** | Ẩn thread cụ thể khỏi hệ thống. | `tid` |
| `kernel_hide_module` | **Kernel** | Ẩn DLL trong một tiến trình mục tiêu. | `pid\|modName` |
| `kernel_terminate` | **Kernel** | Kết thúc tiến trình bằng quyền Kernel tối thượng. | `pid` |
| `kernel_block_driver` | **Kernel** | Chặn các driver bảo mật không cho load. | `driverName` |
| `kernel_protect_reg_key`| **Shield** | Khóa registry key chống chỉnh sửa. | `keyPath` |
| `kernel_protect_reg_val`| **Shield** | Khóa registry value chống chỉnh sửa. | `key\|value` |
| `kernel_hvci_bypass` | **Bypass** | (Raw) Kích hoạt thử nghiệm HVCI Bypass. | N/A |
| `kernel_uefi_persist`| **Curse** | (Raw) Kích hoạt thử nghiệm UEFI Persistence. | N/A |
| `kernel_antivm` | **Shield** | Kiểm tra sâu Sandbox/VM qua Kernel. | N/A |

> [!TIP]
> **Manual Rituals**: Các lệnh như `kernel_protect_process` và `kernel_signature_process` đã được nạp sẵn vào Driver nhưng cần thao tác thủ công hoặc cập nhật C2 dispatcher để thực thi từ xa qua console.

---

## 🛠️ HỆ THỐNG BUILD VỰC THẲM

Chúng tôi đã tối ưu hóa mọi thứ. Không còn lỗi environment phiền phức.

- **Windows**: `.\build.ps1` (Tự động phát hiện WDK, Rust, .NET SDK)
- **Linux/WSL**: `./build.sh` (Build chéo cho Windows một cách mượt mà)

---

## 🔮 LỘ TRÌNH PHÁT TRIỂN (Roadmap)
- [x] **Abyss Edition Overhaul**: Giao diện và lệnh được kiểm định 100%.
- [x] **Shadow Integration**: Lõi Rust và C# hoạt động hoàn hảo.
- [x] **EDR Nuke Engine**: Vô hiệu hóa những rào cản bảo mật hiện đại nhất.
- [ ] **HVCI Pierce**: Đâm thủng hàng rào Hypervisor Code Integrity.
- [ ] **UEFI Resurrection**: Tồn tại ngay cả khi xóa ổ cứng.

---

## ⚠️ LỜI CẢNH BÁO TỪ VỰC THẲM
*Công cụ này được tạo ra cho mục đích Red Team nghiên cứu bảo mật. Nhà phát triển không chịu trách nhiệm cho bất kỳ hành vi lạm dụng nào. Một khi bạn đã bước vào Vực Thẳm, sẽ không có đường lui.*

---

<p align="center">
  <b>[ ⭐ STAR REPO NÀY ĐỂ MỞ KHÓA SỨC MẠNH ]</b><br>
  <i>Làm chủ bóng tối. Sử dụng Xorium.</i>
</p>

