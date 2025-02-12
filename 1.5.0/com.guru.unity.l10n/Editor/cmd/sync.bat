@echo off
:: get cmd file path
set "DIR=%~dp0"
echo [DIR]: %DIR%
cd %DIR%
:: import env settings
call params.bat
:: print args
echo [1] - FILE: %FILE%
echo [2] - SHEET_ID: %SHEET_ID%
echo [3] - TABLE_NAME:   %TABLE_NAME%
echo [4] - ACTION:   %ACTION%
echo [5] - LOG:  %LOG%

del /a /f %LOG%
:: Call Python Script On Windows system
python l10n %ACTION% --platform unity --path %FILE% --spreadsheet_id %SHEET_ID% --table %TABLE_NAME% > _log.txt
echo "**** l10n sync over ****"
:: Set Log file for reading
ren _log.txt %LOG%