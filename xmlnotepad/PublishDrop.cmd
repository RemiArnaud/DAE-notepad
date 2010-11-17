set TargetDir=%1
echo TargetDir=%TargetDir%

PUSHD "%~dp0"
echo PUSHD "%~dp0"

if not exist "drop" mkdir "drop"


rem if not exist "drop\samples" mkdir "drop\samples"


xcopy /d /f /y "Updates.*" %TargetDir%
xcopy /d /f /y "Updates.*" "drop"

xcopy /d /f /y "*.xsd" %TargetDir%

xcopy /d /f /y "Readme.*" "drop"
xcopy /d /f /y "Help\Help.chm" "drop"
xcopy /d /f /y "Help\Images\xmlicon.*" "drop"

rem xcopy /d /f /y "Samples\*.*" "drop\samples"
rem xcopy /d /f /y "XML NotePad 2007 EULA.rtf" "drop"

xcopy /d /f/y %TargetDir%*.* "drop"