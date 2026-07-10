@echo off
call "C:\Program Files (x86)\Microsoft Visual Studio\2022\BuildTools\VC\Auxiliary\Build\vcvars64.bat" >nul 2>&1
set ROOT=C:\dev\GBF\vendor\GBFRelinkFix
cl /nologo /O2 /DZYDIS_STATIC_BUILD /DZYCORE_STATIC_BUILD ^
  /I "%ROOT%\external\zydis\include" ^
  /I "%ROOT%\external\zydis\src" ^
  /I "%ROOT%\external\zydis\dependencies\zycore\include" ^
  disasm.c ^
  /link "%ROOT%\build\windows\x64\release\zydis.lib" "%ROOT%\build\windows\x64\release\zycore.lib" ^
  /OUT:disasm.exe
