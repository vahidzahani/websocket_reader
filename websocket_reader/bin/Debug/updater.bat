
@echo off
set sourceFile=tmpupdate.exe
set targetFile=websocketprinter.exe
echo Updating ...
timeout /T 2 /nobreak >nul
copy /Y "%sourceFile%" "%targetFile%"
start "" "%targetFile%"
