@echo off
title Laser Print & Cut - Register Add-in
setlocal enabledelayedexpansion

echo ============================================
echo   Laser Print & Cut - CorelDRAW Add-in
echo   Registro da DLL
echo ============================================
echo.

:: Detect architecture
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set ARCH=x64
    set REGASM=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe
) else (
    set ARCH=x86
    set REGASM=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\regasm.exe
)

set DLL_PATH=%~dp0src\LaserPrintCut\bin\Release\LaserPrintCut.dll

if not exist "%DLL_PATH%" (
    echo [ERRO] DLL nao encontrada em:
    echo   %DLL_PATH%
    echo.
    echo Faca o build primeiro com: dotnet build -c Release
    pause
    exit /b 1
)

echo [1/3] Registrando DLL no COM Interop...
"%REGASM%" "%DLL_PATH%" /codebase
if %ERRORLEVEL% neq 0 (
    echo [ERRO] Falha ao registrar a DLL. Execute como Administrador.
    pause
    exit /b %ERRORLEVEL%
)
echo   OK - DLL registrada com sucesso.

echo [2/3] Registrando nas versoes do CorelDRAW...

:: CorelDRAW Versions Registry Paths
:: X7 (17) = 17.0, X8 (18) = 18.0, 2017 (19) = 19.0, 2018 (20) = 20.0
:: 2019 (21) = 21.0, 2020 (22) = 22.0, 2021 (23) = 23.0
:: 2022 (24) = 24.0, 2023 (25) = 25.0

set VERSIONS=17.0 18.0 19.0 20.0 21.0 22.0 23.0 24.0 25.0
set REGISTERED=0

for %%v in (%VERSIONS%) do (
    set "REG_PATH=HKEY_CURRENT_USER\Software\Corel\CorelDRAW\%%v\AddIns"
    
    reg query "!REG_PATH!" >nul 2>&1
    if !ERRORLEVEL! equ 0 (
        echo   Registrando no CorelDRAW %%v...
        reg add "!REG_PATH!\LaserPrintCut.Addin" /ve /d "Laser Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInName" /d "Laser Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInDescription" /d "Gera contornos de corte e preparacao para LightBurn Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInCLSID" /d "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInAuthor" /d "LaserPrintCut Team" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInVersion" /d "1.0.0.0" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInDLL" /d "%DLL_PATH%" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInStartup" /d "1" /t REG_DWORD /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInLoadBehavior" /d "3" /t REG_DWORD /f >nul
        set /a REGISTERED+=1
        echo     OK
    )
)

echo [3/3] Registrando no HKEY_LOCAL_MACHINE...
for %%v in (%VERSIONS%) do (
    set "REG_PATH=HKEY_LOCAL_MACHINE\SOFTWARE\Corel\CorelDRAW\%%v\AddIns"
    
    reg query "!REG_PATH!" >nul 2>&1
    if !ERRORLEVEL! equ 0 (
        echo   Registrando no CorelDRAW %%v...
        reg add "!REG_PATH!\LaserPrintCut.Addin" /ve /d "Laser Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInName" /d "Laser Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInDescription" /d "Gera contornos de corte e preparacao para LightBurn Print & Cut" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInCLSID" /d "{A1B2C3D4-E5F6-7890-ABCD-EF1234567890}" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInAuthor" /d "LaserPrintCut Team" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInVersion" /d "1.0.0.0" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInDLL" /d "%DLL_PATH%" /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInStartup" /d "1" /t REG_DWORD /f >nul
        reg add "!REG_PATH!\LaserPrintCut.Addin" /v "AddInLoadBehavior" /d "3" /t REG_DWORD /f >nul
        set /a REGISTERED+=1
        echo     OK
    )
)

if !REGISTERED! gtr 0 (
    echo.
    echo ============================================
    echo   Registro concluido com sucesso!
    echo   Registrado em !REGISTERED! versao(oens) do CorelDRAW
    echo.
    echo   Reinicie o CorelDRAW para ativar o add-in.
    echo   Acesse: Ferramentas ^> Opcoes ^> VBA Add-Ins
    echo ============================================
) else (
    echo.
    echo [AVISO] Nenhuma versao do CorelDRAW encontrada no registro.
    echo         Instale o CorelDRAW primeiro ou ajuste as versoes.
)

pause