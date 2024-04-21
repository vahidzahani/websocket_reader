setlocal enabledelayedexpansion

set "inputFile=websocket_reader\bin\Debug\ver.txt"
set /p version=<"%inputFile%"


md Update!version!\Setup
md Update!version!\Update

copy "socket_setup\Debug\socket_setup.msi" Update!version!\Setup\
copy "socket_setup\Debug\setup.exe" Update!version!\Setup\
copy "websocket_reader\bin\Debug\websocketprinter.exe" Update!version!\Update\
copy "websocket_reader\bin\Debug\ver.txt" Update!version!\Update\





