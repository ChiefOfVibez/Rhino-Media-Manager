# Install .NET 7 SDK - Required for Building

## ⚠️ .NET 7 SDK Not Found

The build requires .NET 7 SDK to be installed.

---

## Quick Install Steps

### Download & Install .NET 7 SDK

**Direct Download Link:**
https://dotnet.microsoft.com/en-us/download/dotnet/7.0

**Choose:** .NET 7.0 SDK (Windows x64 Installer)

**Or use this direct link:**
https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.410-windows-x64-installer

---

### Installation Steps

1. **Download** the installer (about 200 MB)
2. **Run** the installer
3. **Follow** the installation wizard (default settings are fine)
4. **Restart** your terminal/IDE after installation

---

### Verify Installation

After installing, open a **new PowerShell window** and run:

```powershell
dotnet --version
```

**Expected output:**
```
7.0.xxx
```

---

## Alternative: Use Visual Studio

If you have **Visual Studio 2022**, it includes the .NET 7 SDK:

1. Open Visual Studio
2. Go to **Tools → Get Tools and Features**
3. Ensure **.NET desktop development** workload is installed
4. Ensure **.NET 7.0 Runtime** is checked

---

## After Installing

Once .NET 7 SDK is installed, you can build:

### Option 1: Command Line
```powershell
cd "e:\CURSOR\__MATT MEDIA MANAGER ERP\Bosch Products DB in excel"
dotnet build BoschMediaBrowser.sln --configuration Debug
```

### Option 2: Visual Studio
1. Open `BoschMediaBrowser.sln`
2. Press **Ctrl+Shift+B** to build
3. Check Output window for success

---

## Next Steps

After building successfully, the plugin will be at:
```
src\BoschMediaBrowser.Rhino\bin\Debug\net7.0\BoschMediaBrowser.Rhino.rhp
```

Then drag this .rhp file into Rhino 8!

---

**Install .NET 7 SDK first, then we can build!** 🚀
