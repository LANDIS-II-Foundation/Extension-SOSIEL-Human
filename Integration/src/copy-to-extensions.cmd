@echo off

rem  Set environment variables about specific library
set sourceDir=%1

set targetDir=C:\Program Files\LANDIS-II\v6\bin\extensions

set files=Landis.Extension.SosielHuman.dll CsvHelper.dll Newtonsoft.Json.dll
 
(for %%a in (%files%) do ( 
   xcopy /Q /Y "%sourceDir%\%%a" "%targetDir%"
))

pause

