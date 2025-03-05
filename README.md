# 医院政企锁屏软件（ComputerLock）
---
# 功能介绍（Features）
---
-  无操作时自动锁屏 √
-  可修改密码  √
-  管理员可以直接修改密码 √
-   开机自启   √
-   接管系统锁屏快捷键Win+L 实现一键锁屏 
-   锁屏状态下屏蔽系统按键 √
## 权限说明 (About Permission)
- 程序在锁屏时，需要通过注册表屏蔽掉任务管理器，因此需要管理员权限运行。
- 程序通过 user32 设置全局快捷键、禁用部分按键，因此部分杀毒软件会报病毒。
- When the program is on the lock screen, the Task Manager needs to be disabled (via the registry), so it needs administrator privileges to run.
- The program uses user32 to set global shortcut keys and disable some keys, so some anti-virus software will report viruses.
