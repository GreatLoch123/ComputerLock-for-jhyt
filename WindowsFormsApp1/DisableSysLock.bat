@echo off&color 17

if exist "%SystemRoot%\SysWOW64" path %path%;%windir%\SysNative;%SystemRoot%\SysWOW64;%~dp0

bcdedit >nul

if '%errorlevel%' NEQ '0' (goto UACPrompt) else (goto UACAdmin)

:UACPrompt

%1 start "" mshta vbscript:createobject("shell.application").shellexecute("""%~0""","::",,"runas",1)(window.close)&exit

exit /B

:UACAdmin

cd /d "%~dp0"

:CheckRegistry
:: 设置注册表路径
set "regPath=HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\System"
set "regValue=DisableLockWorkstation"

:: 查询当前值
for /f "tokens=3" %%a in ('reg query "%regPath%" /v "%regValue%" 2^>nul ^| findstr /i "%regValue%"') do (
    set currentValue=%%a
)

:: 检查值并执行操作
if "%currentValue%"=="0x1" (
    reg add "%regPath%" /v "%regValue%" /t REG_DWORD /d 0 /f >nul
) else if "%currentValue%"=="0x0" (
    reg add "%regPath%" /v "%regValue%" /t REG_DWORD /d 1 /f >nul
) else (
    echo 未找到注册表值，可能是首次运行。正在创建并设置为默认值...
    reg add "%regPath%" /v "%regValue%" /t REG_DWORD /d 0 /f >nul
    echo 系统锁屏功能已设置为默认值（启用）。
)
