@echo off
REM Publish script for Windows deployment
echo ==========================================
echo Publishing Siebel Lead Backup Application
echo ==========================================
echo.

REM Clean previous builds
echo Cleaning previous builds...
dotnet clean --configuration Release

REM Publish the application
echo Publishing application...
dotnet publish --configuration Release --output ./publish --self-contained false --runtime win-x64

echo.
echo ==========================================
echo Publish completed successfully!
echo Output location: ./publish
echo ==========================================
echo.
echo To run the application:
echo 1. Navigate to the publish folder
echo 2. Update appsettings.json with your database connection
echo 3. Run SiebelLeadBackup.Console.exe
echo.
pause
