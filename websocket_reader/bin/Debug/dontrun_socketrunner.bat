@echo off
setlocal

set url=https://projfa.ir/test_socket/
set downloadPath=%~dp0
set tempFile=%downloadPath%ver_tmp.txt
set verFile=%downloadPath%ver.txt

powershell -command "(New-Object System.Net.WebClient).DownloadFile('%url%ver.txt', '%tempFile%')"

set /p tempVer=<"%tempFile%"
set /p ver=<"%verFile%"

if "%tempVer%" NEQ "%ver%" (
    taskkill /F /IM websocketprinter.exe
    powershell -command "(New-Object System.Net.WebClient).DownloadFile('%url%websocketprinter.exe', '%downloadPath%websocketprinter.exe')"
    start "" "%downloadPath%websocketprinter.exe"
)

del "%tempFile%"

endlocal
