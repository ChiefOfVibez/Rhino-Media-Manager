@echo off
echo ====================================================
echo Bosch Product Database - Web UI
echo ====================================================
echo.

cd /d "%~dp0"

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Python is not installed or not in PATH
    echo Please install Python 3.8+ from https://www.python.org/
    pause
    exit /b 1
)

echo Checking Python version...
python --version

echo.
echo Checking dependencies...

REM Check if virtual environment exists
if not exist "venv" (
    echo Creating virtual environment...
    python -m venv venv
    if errorlevel 1 (
        echo ERROR: Failed to create virtual environment
        echo Continuing with global Python...
    ) else (
        echo Virtual environment created successfully
    )
)

REM Activate virtual environment if it exists
if exist "venv\Scripts\activate.bat" (
    echo Activating virtual environment...
    call venv\Scripts\activate.bat
) else (
    echo Using global Python installation...
)

REM Install/update dependencies
echo Installing dependencies from requirements.txt...
pip install -r requirements.txt --quiet --upgrade
if errorlevel 1 (
    echo.
    echo WARNING: Some dependencies failed to install
    echo Trying again without --quiet flag...
    pip install -r requirements.txt --upgrade
)

echo.
echo ====================================================
echo Starting server...
echo Auto-opening browser at http://localhost:8000
echo.
echo Press Ctrl+C to stop the server
echo ====================================================
echo.

REM Kill any existing Python server on port 8000
for /f "tokens=5" %%a in ('netstat -aon ^| find ":8000" ^| find "LISTENING"') do taskkill /F /PID %%a >nul 2>&1

REM Wait a moment for port to be released
timeout /t 1 /nobreak >nul

REM Start server in background
start /B python server.py

REM Wait 3 seconds for server to start
echo Waiting for server to initialize...
timeout /t 3 /nobreak >nul

REM Open default browser
start http://localhost:8000

echo.
echo ====================================================
echo Server running! Browser should open automatically.
echo If not, manually open: http://localhost:8000
echo.
echo To stop the server, close this window or press Ctrl+C
echo ====================================================
echo.

REM Keep window open and show server output
python server.py

pause
