@echo off
title Laser Print & Cut - Unregister Add-in
setlocal enabledelayedexpansion

echo ============================================
echo   Laser Print & Cut - CorelDRAW Add-in
echo   Remocao do Registro
echo ============================================
echo.

:: Detect architecture
if "%PROCESSOR_ARCHITECTURE%"=="AMD64" (
    set REGASM=%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\regasm.exe
) else (
    set REGASM=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\regasm.exe
)

set DLL_PATH=%~dp0src\LaserPrintCut\bin\Release\LaserPrintCut.dll

echo [1/3] Removendo registro COM...
if exist "%DLL_PATH%" (
    "%REGASM%" "%DLL_PATH%" /unregister
    echo   OK - COM unregistered
) else (
    echo   [AVISO] DLL nao encontrada, pulando...
)

echo [2/3] Removendo chaves do registro do CorelDRAW...

set VERSIONS=17.0 18.0 19.0 20.0 21.0 22.0 23.0 24.0 25.0
set REMOVED=0

for %%v in (%VERSIONS%) do (
    set "REG_PATH=HKEY_CURRENT_USER\Software\Corel\CorelDRAW\%%v\AddIns\LaserPrintCut.Addin"
    reg delete "!REG_PATH!" /f >nul 2>&1
    if !ERRORLEVEL! equ 0 set /a REMOVED+=1

    set "REG_PATH=HKEY_LOCAL_MACHINE\SOFTWARE\Corel\CorelDRAW\%%v\AddIns\LaserPrintCut.Addin"
    reg delete "!REG_PATH!" /f >nul 2>&1
    if !ERRORLEVEL! equ 0 (
        set /a REMOVED+=1
    ) else (
        set "REG_PATH=HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Corel\CorelDRAW\%%v\AddIns\LaserPrintCut.Addin"
        reg delete "!REG_PATH!" /f >nul 2>&1
        if !ERRORLEVEL! equ 0 set /a REMOVED+=1
    )
)

echo [3/3] Limpando cache de assemblies...
if exist "%windir%\Microsoft.NET\assembly\GAC_MSIL\LaserPrintCut" (
    rmdir /s /q "%windir%\Microsoft.NET\assembly\GAC_MSIL\LaserPrintCut" 2>nul
    echo   OK - Cache limpo
)

echo.
echo ============================================
echo   Remocao concluida!
echo   Chaves removidas: %REMOVED%
echo ============================================
pause