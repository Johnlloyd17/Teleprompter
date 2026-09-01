# Publishing Teleprompter: Complete Guide

This guide walks you through building your Teleprompter app into an installable **Android APK** file or a **Windows EXE** file that you can share with others. Every step includes exactly where to click and what to type.

> **No programming experience needed.** Just follow each step in order.

---

## Table of Contents

1. [Prerequisites (Things You Need First)](#1-prerequisites-things-you-need-first)
2. [How to Open the Right PowerShell Window](#2-how-to-open-the-right-powershell-window)
3. [Part A: Build an Android APK (for phones/tablets)](#part-a-build-an-android-apk-for-phonestablets)
4. [Part B: Build a Windows EXE (for PCs)](#part-b-build-a-windows-exe-for-pcs)
5. [Using Visual Studio Instead of PowerShell](#3-using-visual-studio-instead-of-powershell)
6. [Finding Your Built Files](#4-finding-your-built-files)
7. [Testing on a Real Android Device](#5-testing-on-a-real-android-device)
8. [Creating a Windows Installer (Optional)](#6-creating-a-windows-installer-optional)
9. [Sharing Your Built App (Zip the Whole Folder)](#65-sharing-your-built-app-zip-the-whole-folder)
10. [App Icon & Splash Screen (Recommended)](#7-app-icon--splash-screen-recommended)
11. [Windows Code Signing (Optional, Avoids SmartScreen Warning)](#8-windows-code-signing-optional-avoids-smartscreen-warning)
12. [Releasing Updates (Version Bumping)](#9-releasing-updates-version-bumping)
13. [Publishing to Google Play (.aab) vs. Sharing Directly (.apk)](#10-publishing-to-google-play-aab-vs-sharing-directly-apk)
14. [Troubleshooting Common Errors](#11-troubleshooting-common-errors)

---

## 1. Prerequisites (Things You Need First)

Before you begin, make sure these are installed on your computer:

| What | How to Check | Where to Download |
|------|-------------|-------------------|
| **Visual Studio 2022** (with .NET desktop development workload) | Open it and check it launches | [visualstudio.microsoft.com](https://visualstudio.microsoft.com/) |
| **.NET 9 SDK** | Open PowerShell and type `dotnet --version` — it should say `9.0.x` | Usually comes with VS 2022 |
| **MAUI workload** | See Step 1a below | Installed via PowerShell |

### Step 1a: Install the MAUI Workload (One-Time Setup)

This is only needed once. If you already build and run the app from Visual Studio, you can skip this.

**Using PowerShell:**
1. Open PowerShell (see [Section 2](#2-how-to-open-the-right-powershell-window) on how)
2. Type the following command and press **Enter**:
   ```powershell
   dotnet workload install maui-android
   ```
3. Wait for it to finish downloading and installing (may take a few minutes).

**Using Visual Studio:**
1. Open Visual Studio 2022.
2. In the top menu bar, click **Tools**.
3. Click **Get Tools and Features...** (this opens the Visual Studio Installer).
4. In the installer window, find your Visual Studio 2022 installation and click the **Modify** button (if available) or look for the **Workloads** tab.
5. Scroll down and make sure **.NET Multi-platform App UI development** is checked (ticked).
6. Click **Modify** if you made any changes, then wait for it to finish.

---

## 2. How to Open the Right PowerShell Window

Throughout this guide, you will need a special PowerShell window that has all the right tools pre-loaded. Here is how to open it:

### Method 1: From Inside Visual Studio (Easiest)

1. Open **Visual Studio 2022**.
2. Load your **Teleprompter** project (if not already open).
3. In the top menu bar, click **Tools**.
4. Hover over **Command Line**.
5. Click **Developer PowerShell**.
6. A blue PowerShell window will appear — this is the window you will use for all commands in this guide.

### Method 2: From the Windows Start Menu

1. Press the **Windows key** on your keyboard (the key with the Windows logo).
2. Type: `Developer PowerShell for VS 2022`
3. Click on **"Developer PowerShell for VS 2022"** when it appears in the search results.
4. A blue PowerShell window will open.

### How to Navigate to Your Project Folder

In the PowerShell window that opened, type the following and press **Enter**:

```powershell
cd "C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter"
```

> **What this does:** It tells PowerShell to work inside your project folder. You
> only need to do this once per PowerShell window session.

---

## Part A: Build an Android APK (for phones/tablets)

This produces a `.apk` file that you can install on any Android phone or tablet.

### Step A1: Create a Signing Keystore (One-Time Only)

A "keystore" is like a digital signature that proves the APK is really from you. You only create this once — reuse the same keystore for every future update of this app.

1. First, let's find where Java (keytool) is on your computer. Run this command in
   PowerShell to check if `JAVA_HOME` is set:

   ```powershell
   $env:JAVA_HOME
   ```

   - **If it prints a path** (like `C:\Program Files\Microsoft\jdk-17...`), use
     this command:

     ```powershell
     & "$env:JAVA_HOME\bin\keytool.exe" -genkeypair -v `
       -keystore teleprompter.keystore -alias teleprompter `
       -keyalg RSA -keysize 2048 -validity 10000
     ```

   - **If it prints nothing or errors**, Java is likely bundled with Android Studio.
     Use this command instead:

     ```powershell
     & "C:\Program Files\Android\Android Studio\jbr\bin\keytool.exe" -genkeypair -v `
       -keystore teleprompter.keystore -alias teleprompter `
       -keyalg RSA -keysize 2048 -validity 10000
     ```

   > **Still not working?** Find `keytool.exe` on your computer manually:
   > 1. Open **File Explorer**.
   > 2. Click in the search bar (top-right) and type: `keytool.exe`
   > 3. Wait for results. Right-click the result and choose **Open file location**.
   > 4. Copy the path from the address bar, then use:
   >    ```powershell
   >    & "PASTE-THE-PATH-HERE\keytool.exe" -genkeypair -v `
   >      -keystore teleprompter.keystore -alias teleprompter `
   >      -keyalg RSA -keysize 2048 -validity 10000
   >    ```

2. PowerShell will ask you to enter a **keystore password**. Type a password you will remember (write it down somewhere safe — you'll need it every time you build), then press **Enter**.

3. It will ask you to **re-enter the password**. Type the same password again and press **Enter**.

4. It will then ask for a **key password for `<teleprompter>`** — you can just press **Enter** to reuse the same keystore password, or set a separate one. Whichever you choose, **write both passwords down**, since Step A2 needs them.

5. It will then ask several questions (your name, organization, city, etc.). You can:
   - Type answers for each one, OR
   - Simply press **Enter** to skip each question and leave them blank.
   - The only important question is the **last one** — it will ask: `Is CN=..., OU=..., O=..., L=..., ST=..., C=... correct?` — type `yes` and press **Enter**.

6. A file called `teleprompter.keystore` will be created in your project folder.

> **What to do with this file:** Keep it safe! If you lose it, you cannot publish
> updates to your app — Android treats an app signed with a different keystore as
> a completely different app, so existing users could never receive your update.
> Do NOT delete it, and do NOT commit it to a public GitHub repository.

### Step A2: Build the Signed APK

**Method 1: PowerShell Command**

1. Make sure you are still in the Developer PowerShell for VS 2022.
2. Make sure you are in the project folder (if not, see "How to Navigate" in Section 2).
3. Copy and paste the following command, but **replace both placeholder passwords**
   with the actual password(s) you created in Step A1 (if you used the same
   password for both the keystore and the key, use it in both places):

   ```powershell
   dotnet publish Teleprompter/Teleprompter.csproj `
     -f net9.0-android -c Release -p:NetMajor=9 `
     -p:AndroidKeyStore=true `
     -p:AndroidSigningKeyStore="C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\teleprompter.keystore" `
     -p:AndroidSigningKeyAlias=teleprompter `
     -p:AndroidSigningKeyPass=johnlloyd17~ `
     -p:AndroidSigningStorePass=johnlloyd17~
   ```

   > ⚠️ **Do not copy this command with placeholder text left in.** Both
   > `YOUR-KEY-PASSWORD-HERE` and `YOUR-STORE-PASSWORD-HERE` must be replaced
   > with your real password(s) from Step A1, or the build will fail with a
   > signing error.

   > 📁 **Use the full path to your keystore, not just `teleprompter.keystore`.**
   > `AndroidSigningKeyStore` is resolved **relative to the project file's folder**
   > (`Teleprompter\Teleprompter\`, where `Teleprompter.csproj` lives) — not the
   > solution root folder you `cd`'d into at the start of the guide. If you
   > created your keystore while sitting in the solution root (as Step A1 has
   > you do), a bare filename like `teleprompter.keystore` won't be found there,
   > and the build fails with:
   > `error XA4310: `$(AndroidSigningKeyStore)` file 'teleprompter.keystore' could not be found.`
   > Using the full absolute path (as shown above) avoids this regardless of
   > which folder you're in when you run the command. Adjust the path if your
   > keystore lives somewhere else on your machine.

4. Press **Enter** and wait. The build process takes 1-5 minutes. You will see lines of text scrolling — this is normal.

5. When it finishes, you should see a message like: `Build succeeded. 0 Warning(s). 0 Error(s).`

**Method 2: Visual Studio (Easier, No Typing)**

See [Section 3: Using Visual Studio Instead of PowerShell](#3-using-visual-studio-instead-of-powershell) below.

### Step A3: Find Your APK File

After building, your APK file is located at:

```
C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\Teleprompter\bin\Release\net9.0-android\publish\
```

The file will be named: **`com.companyname.teleprompter-Signed.apk`**

(If you've already changed the Application ID — see [Section 10](#10-publishing-to-google-play-aab-vs-sharing-directly-apk) — the file name will reflect your new ID instead of `com.companyname.teleprompter`.)

**How to get there quickly:**

1. Open **File Explorer** (the yellow folder icon on your taskbar, or press `Windows key + E`).
2. Click in the address bar at the top of the window.
3. Paste the path above and press **Enter**.
4. You will see your `.apk` file — this is what you share with others.

---

## Part B: Build a Windows EXE (for PCs)

This produces a `.exe` file (or a folder of files) that runs on Windows computers.

### Step B1: Choose Build Type

There are two options:

| Type | Pros | Cons |
|------|------|------|
| **Framework-dependent** (small, ~10 MB) | Small file size | Recipients must have .NET 9 Runtime installed |
| **Self-contained** (large, ~150-200 MB) | Runs on any Windows PC, nothing to install | Larger file size |

> **Recommendation for sharing:** Use **Self-contained** — it works on any PC
> without requiring the recipient to install anything extra.

### Step B2: Build the EXE

**Method 1: PowerShell Command**

1. Open **Developer PowerShell for VS 2022** (see Section 2).
2. Navigate to your project folder:
   ```powershell
   cd "C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter"
   ```

3. **Clean old build folders first (recommended).** If you've built this project
   before — especially the Android build in Part A, or a build that got
   interrupted — leftover files in `bin`/`obj` can cause a
   `NETSDK1152: Found multiple publish output files with the same relative path`
   error (usually pointing at a font or image file). Avoid it by cleaning first:
   ```powershell
   Remove-Item -Recurse -Force "Teleprompter\bin", "Teleprompter\obj"
   ```
   This just deletes generated build output — none of your source code is touched.

4. **For a small build** (recipients need .NET 9 Runtime), type:
   ```powershell
   dotnet publish Teleprompter/Teleprompter.csproj `
     -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9
   ```

5. **OR for a self-contained build** (recipients need nothing), type:
   ```powershell
   dotnet publish Teleprompter/Teleprompter.csproj `
     -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9 `
     -p:SelfContained=true -p:RuntimeIdentifierOverride=win-x64
   ```
   > 📌 **Use `RuntimeIdentifierOverride`, not `RuntimeIdentifier`.** Because
   > Teleprompter is a multi-targeted MAUI project (it builds for both Android
   > and Windows), passing the plain `-p:RuntimeIdentifier=win-x64` property
   > leaks that RID into MSBuild's evaluation of *every* target framework —
   > including the Android one, which uses Mono — and the build fails looking
   > for a nonexistent `Microsoft.NETCore.App.Runtime.Mono.win-x64` package.
   > `RuntimeIdentifierOverride` is the property MAUI provides specifically to
   > scope the RID to only the framework you're publishing with `-f`.

6. Press **Enter** and wait for the build to complete (1-3 minutes).

**Method 2: Visual Studio (Easier, No Typing)**

See [Section 3: Using Visual Studio Instead of PowerShell](#3-using-visual-studio-instead-of-powershell) below.

### Step B3: Find Your EXE File

After building, your files are located at:

```
C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\Teleprompter\bin\Release\net9.0-windows10.0.19041.0\publish\
```

Inside this folder you will find:
- **`Teleprompter.exe`** — the main application file
- Several other `.dll` files (these are required — do not delete them)

**How to get there quickly:**

1. Open **File Explorer**.
2. Click in the address bar at the top.
3. Paste the path above and press **Enter**.

> **Important:** The Windows build produces a **folder** of files, not a single
> file. `Teleprompter.exe` must stay in the same folder as the `.dll` files.
> To share it with someone, zip the **entire `publish` folder**.

---

## 3. Using Visual Studio Instead of PowerShell

If you prefer clicking buttons instead of typing commands, you can build directly in Visual Studio.

### For Android APK:

**Option A: Quick Build (No Signing — for testing only)**

1. Open your **Teleprompter** project in **Visual Studio 2022**.
2. At the top of the window, find the **dropdown menus** on the toolbar:
   - The **first dropdown** (left side) says something like "Teleprompter" — leave it.
   - The **second dropdown** says the platform. Click it and change it to **Android**.
   - The **third dropdown** says "Debug" — click it and change it to **Release**.
3. In the **Solution Explorer** panel (usually on the right side), **right-click** on **Teleprompter** (the project, not the solution).
4. Click **Publish...**.
5. In the dialog that appears, click **Folder** as the publish target, then click **Publish**.
6. Wait for the build to finish (check the bottom of the screen for progress).

**Option B: Signed Build (for sharing with others)**

1. Open your **Teleprompter** project in **Visual Studio 2022**.
2. In the **Solution Explorer** panel (right side), **right-click** on **Teleprompter**.
3. Click **Properties** (at the bottom of the menu).
4. A new tab opens. On the **left side**, click **Android** to expand it.
5. Click **Package Signing**.
6. At the top of the page, find the **Configuration** dropdown and change it to **Release**.
7. **Check (tick) the box** that says something like: *"Sign the .APK package using my keystore details below"*. (Without checking this box, the fields below are ignored!)
8. Fill in the fields:
   - **Keystore path** → Click **Browse...** and navigate to where you saved `teleprompter.keystore`, then select it.
   - **Password** → Type your keystore password.
   - **Alias** → Type: `teleprompter`
   - **Alias password** → Type your key password (from Step A1.4 — may be the same as the keystore password).
9. Click **Save** (press `Ctrl + S`).
10. Now go back to the top toolbar and change the platform dropdown to **Android** and the config dropdown to **Release**.
11. **Right-click** the **Teleprompter** project → **Publish...** → **Folder** → **Publish**.
12. The signed APK will be built and saved.

### For Windows EXE:

1. Open your **Teleprompter** project in **Visual Studio 2022**.
2. At the top toolbar, make sure the platform dropdown says **Windows Machine** and the config dropdown says **Release**.
3. In the **Solution Explorer** panel, **right-click** on **Teleprompter**.
4. Click **Publish...**.
5. Choose **Folder** as the publish target.
6. To get a single self-contained folder that runs on any PC, on the publish profile settings choose **Target runtime: win-x64** and set **Deployment mode: Self-contained**.
7. Click **Publish**.
8. Wait for it to finish. The EXE and its files will appear in the publish folder.

---

## 4. Finding Your Built Files

Here is a quick reference for where files end up:

| What You Built | Where It Is |
|---------------|-------------|
| **Android APK** | `Teleprompter\bin\Release\net9.0-android\publish\com.companyname.teleprompter-Signed.apk` |
| **Windows EXE** | `Teleprompter\bin\Release\net9.0-windows10.0.19041.0\publish\Teleprompter.exe` (+ DLLs) |

> **Quick tip:** You can also find these folders by going to the project in
> **Solution Explorer** → **right-click** the project → **Open Folder in File Explorer**.
> Then navigate to `bin\Release\` and look for the `publish` subfolder.

---

## 5. Testing on a Real Android Device

You can install the APK directly on your Android phone without using Google Play.

### Prerequisites
- Your Android phone (Android 6.0 / API 23 or newer)
- A USB cable
- USB Debugging enabled on your phone (see below)

### How to Enable USB Debugging (One-Time)

1. On your Android phone, open **Settings**.
2. Scroll down and tap **About phone**.
3. Find **Build number** and tap it **7 times quickly**.
4. You will see a message saying "You are now a developer!"
5. Go back to **Settings** → **System** (or **Developer options**).
6. Scroll down and toggle **USB debugging** to ON.
7. Tap **OK** on the confirmation popup.

### Install the APK

1. Connect your phone to your computer with the USB cable.
2. On your phone, tap **Allow** when asked about USB debugging.
3. Open **Developer PowerShell for VS 2022** (see Section 2).
4. Navigate to your project folder:
   ```powershell
   cd "C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter"
   ```
5. Type the following command and press **Enter**:
   ```powershell
   adb install -r Teleprompter\bin\Release\net9.0-android\publish\com.companyname.teleprompter-Signed.apk
   ```
   > If PowerShell says `adb` is not recognized, the Android SDK platform-tools
   > folder isn't on your PATH. It's usually found at
   > `%LOCALAPPDATA%\Android\Sdk\platform-tools\adb.exe` — either add that
   > folder to your PATH, or call `adb.exe` using its full path instead.
6. Wait a moment — you will see "Success" when it is done.
7. The **Teleprompter** app will now appear in your phone's app drawer!

### Alternatively: Share the APK File

If you do not want to use USB, simply send the `.apk` file to your phone:
- Email it to yourself and open the attachment on your phone
- Upload it to Google Drive and download it on your phone
- Use a USB cable to copy the file to your phone's Downloads folder

On your phone, tap the downloaded `.apk` file and follow the prompts to install it. You may need to allow installation from "unknown sources" in your phone's settings.

---

## 6. Creating a Windows Installer (Optional)

The Windows build produces a folder of files, not a single installer. If you want a real `Setup.exe` installer that other users can run like a normal program, you can use **Inno Setup** (free).

### Steps:

1. Download and install **Inno Setup** from: https://jrsoftware.org/isinfo.php
2. Open **Inno Setup Compiler**.
3. Click **File** → **New**.
4. The Setup Wizard opens. Click **Next**.
5. Fill in the **Application Name** (`Teleprompter`), **Publisher**, and **Application Version**.
6. Click **Next**.
7. For the **Application folder**, click **Next** (keep defaults).
8. For the **Application files**, click **Browse...** next to "Application main executable file" and navigate to:
   ```
   C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\Teleprompter\bin\Release\net9.0-windows10.0.19041.0\publish\Teleprompter.exe
   ```
9. In the "Other files and folders" box, browse to the same `publish` folder and add everything in it.
10. Click **Next** through the remaining steps (shortcuts, license, etc.).
11. Click **Finish** — Inno Setup will compile your installer.
12. The final `Setup.exe` will be in the `Output` folder Inno Setup creates.

---

## 6.5. Sharing Your Built App (Zip the Whole Folder)

The `dotnet publish` command for Windows never produces a single portable
`.exe` — it always produces a **folder** containing `Teleprompter.exe` plus
dozens of required `.dll` files (the .NET runtime, MAUI, and WinUI
dependencies). The `.exe` cannot run on its own; it silently fails to launch
if separated from that folder — no error window, nothing happens, even after
clicking "Run anyway" past SmartScreen.

**Do not upload `Teleprompter.exe` by itself anywhere** — not to itch.io, not
by email, not by any file-sharing link. Always share the whole folder:

1. Open the publish folder:
   ```
   Teleprompter\bin\Release\net9.0-windows10.0.19041.0\win10-x64\publish\
   ```
2. Go **inside** that folder, then select everything inside it (`Ctrl+A`) —
   `Teleprompter.exe`, all the `.dll` files, and any resource subfolders.
3. Right-click the selection → **Send to → Compressed (zipped) folder**.
   Zipping the *contents* (not the `publish` folder itself as a single item)
   keeps `Teleprompter.exe` sitting at the top level of the archive, which
   matters for platforms like itch.io that try to auto-detect the executable.
4. Share or upload that single `.zip` file. On itch.io specifically, mark the
   uploaded file as **Windows** in the uploader so the itch.io app knows to
   treat it as a Windows package.

**Is there a way to avoid zipping and ship a truly single-file `.exe`
instead?** In short, no — not reliably. .NET has a `PublishSingleFile`
feature that works for plain console/WPF/WinForms apps, but for .NET MAUI
Windows apps (built on WinUI 3 / Windows App SDK) it remains a known,
long-standing limitation: attempts to combine `PublishSingleFile=true` with
a MAUI Windows target either still leak dependent files out of the "single"
file, or the app crashes on launch, or the window has broken behavior (e.g.
can't be moved). This isn't a flag you're missing — it's a platform
limitation as of the current .NET/MAUI versions. **The zip (or an Inno Setup
installer, Section 6 above) are the two real options** for distributing this
app. Do not spend time chasing `PublishSingleFile` for this project.

---

## 7. App Icon & Splash Screen (Recommended)

If you skip this, your APK and EXE will ship with the default MAUI placeholder icon (the purple/blue puzzle-piece style logo), which looks unfinished to anyone you share the app with.

1. Prepare a square PNG or SVG icon (at least 512×512 px works well) and, optionally, a splash-screen image.
2. In **Solution Explorer**, open the `Resources` folder in your Teleprompter project.
3. Replace the files inside `Resources\AppIcon\` with your own icon image, **keeping the same file names** (e.g. `appicon.svg` and `appiconfg.svg`), or update the paths referenced in `Teleprompter.csproj` under the `<MauiIcon>` and `<MauiSplashScreen>` elements to point at your new files.
4. Rebuild the project (**Build → Rebuild Solution**) so MAUI regenerates the platform-specific icon sizes.
5. Re-run the publish steps in Part A / Part B — the new icon will now appear on the built APK/EXE.

---

## 8. Windows Code Signing (Optional, Avoids SmartScreen Warning)

By default, `Teleprompter.exe` is **unsigned**. When someone else downloads and runs it, Windows SmartScreen will likely show a blue "Windows protected your PC" warning, because the file has no trusted publisher certificate attached. The app will still run (the user can click **More info → Run anyway**), but it looks alarming to a first-time user.

This step is optional — skip it if you're only sharing the app with people you know personally.

**To remove the warning, you need a code-signing certificate:**
1. Buy a code-signing certificate from a Certificate Authority (e.g. DigiCert, Sectigo) — these typically cost money per year and require identity verification. A self-signed certificate will **not** remove the SmartScreen warning for the public; it only helps if you install your own certificate as trusted on the exact machines you control.
2. Once you have a `.pfx` certificate file, sign the EXE using `signtool.exe` (included with the Windows SDK):
   ```powershell
   signtool sign /f "path\to\your-certificate.pfx" /p YOUR-CERT-PASSWORD /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 "Teleprompter.exe"
   ```
3. Re-zip or re-package the `publish` folder with the newly signed `Teleprompter.exe`.

> If this feels like overkill for now, it's fine to skip — just let people you send the EXE to know the SmartScreen warning is expected and safe to bypass.

---

## 9. Releasing Updates (Version Bumping)

When you release a new version, you need to update the version numbers in your project file before building.

### Using Visual Studio:

1. In **Solution Explorer**, **right-click** on **Teleprompter**.
2. Click **Properties**.
3. In the **Application** section (on the General tab), find:
   - **Application version** → This is what users see (e.g., `1.1`)
   - **Application display version** → Change to something like `1.1`
   - **Application version (integer)** → Change to `2` (must be higher than before)
4. Press `Ctrl + S` to save.

### Using a Text Editor:

1. Open `Teleprompter/Teleprompter.csproj` in Notepad or Visual Studio.
2. Find these lines near the top:
   ```xml
   <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
   <ApplicationVersion>1</ApplicationVersion>
   ```
3. Change them to:
   ```xml
   <ApplicationDisplayVersion>1.1</ApplicationDisplayVersion>
   <ApplicationVersion>2</ApplicationVersion>
   ```
4. Save the file.

> **Rules:**
> - `ApplicationDisplayVersion` is what users see (e.g., `1.0`, `1.1`, `2.0`).
> - `ApplicationVersion` must be a **whole number** that increases every time
>   (e.g., 1, 2, 3, 4...). Never reuse an old number.
> - Rebuild and re-sign the APK/EXE **after** bumping the version — the version
>   numbers are baked in at build time.

---

## 10. Publishing to Google Play (.aab) vs. Sharing Directly (.apk)

By default, your `Teleprompter.csproj` file is set up to build a `.apk` — the format you install directly on a phone (via ADB, email, Drive, etc.), which is what this guide covers.

**Before your first public release, also do this (one-time):**

- **Change the Application ID.** It currently defaults to `com.companyname.teleprompter`, which is just a placeholder. Open `Teleprompter.csproj` and change the `<ApplicationId>` value to something unique, such as `com.johnlloyd.teleprompter`. Do this **before** your first release — the Application ID cannot be changed later without creating what Android treats as a brand-new app (existing installs won't update).

**If you instead want to publish through the Google Play Store**, Play requires the `.aab` (Android App Bundle) format instead of a raw `.apk`:

1. Open `Teleprompter.csproj` in a text editor.
2. Find any `<AndroidPackageFormat>apk</AndroidPackageFormat>` line (or similar property forcing APK output).
3. Remove that line, or change its value to `aab`.
4. Rebuild using the same `dotnet publish` command from Step A2 — the output in the `publish` folder will now be a `.aab` file instead of `.apk`.
5. Upload the `.aab` file to the Play Console under your app's release track.

> If you don't see an `<AndroidPackageFormat>` property in your `.csproj` at all, that's fine — MAUI defaults to `.apk` output, so there's nothing to remove; just build normally as shown in Part A.

---

## 11. Troubleshooting Common Errors

### "keytool is not recognized" or "JAVA_HOME is not set"

This is the most common error. It means PowerShell cannot find Java.

**Fix — Try these in order:**

1. **Make sure you are using Developer PowerShell** — Open **Developer PowerShell for VS 2022** (not regular PowerShell). See Section 2 for how.

2. **If `JAVA_HOME` is still empty**, use the Android Studio bundled Java directly:
   ```powershell
   & "C:\Program Files\Android\Android Studio\jbr\bin\keytool.exe" -genkeypair -v `
     -keystore teleprompter.keystore -alias teleprompter `
     -keyalg RSA -keysize 2048 -validity 10000
   ```

3. **If that path does not exist either**, find `keytool.exe` on your computer:
   - Open **File Explorer** → search for `keytool.exe` in the top-right search bar.
   - Right-click the result → **Open file location**.
   - Copy the folder path from the address bar and use:
     ```powershell
     & "PASTE-THE-FOLDER-PATH-HERE\keytool.exe" -genkeypair -v `
       -keystore teleprompter.keystore -alias teleprompter `
       -keyalg RSA -keysize 2048 -validity 10000
     ```

---

### "dotnet: The term 'dotnet' is not recognized"

**Cause:** .NET SDK is not installed or not in your PATH.

**Fix:** Install the .NET 9 SDK from https://dotnet.microsoft.com/download/dotnet/9.0, then restart PowerShell.

---

### "Workload 'maui' is not installed"

**Cause:** The MAUI workload is not installed.

**Fix:** Run this command:
```powershell
dotnet workload install maui-android
```

---

### "A valid publishing profile was not found" (in Visual Studio)

**Cause:** Visual Studio is looking for a publish profile that does not exist.

**Fix:** Instead of clicking "Publish" from the top menu, **right-click the project** in Solution Explorer → **Publish...** → choose **Folder** → **Publish**.

---

### Build fails with "Could not locate the Android SDK"

**Cause:** The Android SDK is not installed or Visual Studio cannot find it.

**Fix:**
1. Open Visual Studio.
2. Go to **Tools** → **Get Tools and Features**.
3. Go to the **Individual Components** tab.
4. Search for `Android` and make sure **Android SDK setup (API 35)** or similar is checked.
5. Click **Modify** and wait for it to install.

---

### Build fails with `error XA4310: ... file 'teleprompter.keystore' could not be found`

**Cause:** `AndroidSigningKeyStore` is resolved relative to the **project folder**
(`Teleprompter\Teleprompter\`), not the solution root folder you `cd`'d into.
If your keystore file sits in the solution root (which is where Step A1 has you
create it) and you passed just `teleprompter.keystore` as the value, MSBuild
looks for it one directory deeper than where it actually is.

**Fix:** Either:
- Use the **full absolute path** to the keystore in the `-p:AndroidSigningKeyStore=`
  argument, e.g. `"C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\teleprompter.keystore"`
  (this is the version used throughout this guide), OR
- Move `teleprompter.keystore` into the `Teleprompter\Teleprompter\` subfolder
  (next to `Teleprompter.csproj`) and keep using the bare filename.

---

### Build fails with a signing / keystore password error

**Cause:** Almost always caused by leaving the placeholder text (`YOUR-KEY-PASSWORD-HERE` / `YOUR-STORE-PASSWORD-HERE`) in the `dotnet publish` command from Step A2 instead of your real password, or a mismatch between the keystore and key passwords.

**Fix:** Re-run the command from Step A2 with both passwords replaced with the actual values you set in Step A1. If you're not sure what they were, you'll need to delete `teleprompter.keystore` and create a new one — but note this means any app already installed on a device with the old keystore can no longer receive updates signed with the new one.

---

### Build fails with `error NETSDK1152: Found multiple publish output files with the same relative path`

**Cause:** MAUI's `resizetizer` step — which processes files under
`Resources\Fonts` and `Resources\Images` — leaves a processed copy in the
`obj` folder (e.g. `obj\Release\...\resizetizer\f\OpenSans-Regular.ttf`). This
error means that processed copy and the original source file both end up in
the publish output at the same relative path, and MSBuild refuses to guess
which one should win. Cleaning `bin`/`obj` does **not** fix this if the file
is genuinely registered twice in the project — confirmed in our case below.

**Confirmed real-world cause (Teleprompter project):** `Teleprompter.csproj`
had **two separate item types** both claiming `OpenSans-Regular.ttf`:
```xml
<MauiFont Include="Resources\Fonts\*" />
<MauiAsset Include="Resources\Fonts\OpenSans-Regular.ttf" LogicalName="OpenSans-Regular.ttf" />
```
- `MauiFont` (a wildcard covering the whole `Fonts` folder) processes the file
  through resizetizer so it can be used as a UI font via `ConfigureFonts()`.
- `MauiAsset` separately copies the *same raw file* into the package so code
  can open it directly at runtime — added here because
  `Teleprompter\Services\FileExportService.cs` calls
  `FileSystem.OpenAppPackageFileAsync("OpenSans-Regular.ttf")` to embed the
  font in exported files.

Both items want to place a file at the exact same output path, which
MSBuild rejects.

**Fix — since the raw asset IS used in code, don't delete it; give it a
distinct output path instead:**

1. Open `Teleprompter\Teleprompter.csproj` in a text editor (e.g. Notepad —
   **do not** paste XML into PowerShell, it isn't a shell command) and change
   the `MauiAsset` line's `LogicalName` to a path the `MauiFont` wildcard
   doesn't also produce:
   ```xml
   <MauiAsset Include="Resources\Fonts\OpenSans-Regular.ttf" LogicalName="ExportFonts/OpenSans-Regular.ttf" />
   ```
2. Open `Teleprompter\Services\FileExportService.cs` (around line 171) and
   update the matching string so it still finds the asset:
   ```csharp
   using var stream = FileSystem.OpenAppPackageFileAsync("ExportFonts/OpenSans-Regular.ttf").GetAwaiter().GetResult();
   ```
   The string in both places must match exactly.
3. Clean and rebuild:
   ```powershell
   Remove-Item -Recurse -Force "Teleprompter\bin", "Teleprompter\obj"
   dotnet publish Teleprompter/Teleprompter.csproj `
     -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9 `
     -p:SelfContained=true -p:RuntimeIdentifierOverride=win-x64
   ```

**If your project instead has a genuine duplicate** (the same font wired up
twice with no code depending on the raw asset), the simpler fix is to just
delete the extra `MauiAsset`/`MauiFont` line rather than renaming it — search
`Teleprompter.csproj` for `MauiFont` and the font's filename to check first:
```powershell
Select-String -Path "Teleprompter\Teleprompter.csproj" -Pattern "OpenSans|MauiFont|MauiAsset"
Get-ChildItem -Recurse -Include *.cs -Path "Teleprompter" | Select-String -Pattern "OpenSans-Regular"
```

---

### Build fails with `error NU1102: Unable to find package Microsoft.NETCore.App.Runtime.Mono.win-x64`

**Cause:** You published with `-p:RuntimeIdentifier=win-x64` on a multi-targeted
MAUI project (one that also targets Android). That property leaks into
MSBuild's evaluation of *every* target framework in the project, so it also
tries to apply `win-x64` to the Android/Mono target — and no such Mono runtime
package exists for that RID, so NuGet fails to find it.

**Fix:** Use `RuntimeIdentifierOverride` instead of `RuntimeIdentifier`:
```powershell
dotnet publish Teleprompter/Teleprompter.csproj `
  -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9 `
  -p:SelfContained=true -p:RuntimeIdentifierOverride=win-x64
```
This is the property MAUI provides specifically to scope a self-contained
RID to a single published framework, without affecting the others.

---

### Windows EXE does not run on another PC ("NET Runtime not found")

**Cause:** You built a framework-dependent version, but the other PC does not have .NET 9 Runtime.

**Fix:** Either:
- Have the other person install .NET 9 Desktop Runtime from https://dotnet.microsoft.com/download/dotnet/9.0, OR
- Rebuild as **self-contained** (see Step B2 — use the self-contained command with `-p:SelfContained=true`).

---

### Windows shows "Windows protected your PC" (SmartScreen) when running the EXE

**Cause:** The EXE is unsigned — this is expected for an app built without a code-signing certificate.

**Fix:** Click **More info → Run anyway**. To remove this warning entirely for other users, see [Section 8: Windows Code Signing](#8-windows-code-signing-optional-avoids-smartscreen-warning).

**If nothing happens after clicking "Run anyway"** (no crash message, no window,
the app just silently does nothing): this almost always means `Teleprompter.exe`
was separated from the `.dll` files it depends on — for example, only the `.exe`
was uploaded somewhere (like an itch.io page) instead of the whole `publish`
folder. See [Section 6.5: Sharing Your Built App](#65-sharing-your-built-app-zip-the-whole-folder)
below — the `.exe` alone is never enough; the whole folder (or an Inno Setup
installer built from it) must be distributed together.

---

### Phone says "App not installed as package appears to be invalid"

**Cause:** This is Android's generic catch-all install error, and it can mean
several different things. The two most common causes:
1. The APK file got **corrupted or truncated during transfer** — some chat
   apps (Messenger, Viber, etc.) silently compress or mangle `.apk` attachments.
2. A **previous install with a different signature** already exists on the
   phone (e.g. an old debug build), and Android refuses to overwrite an app
   with a differently-signed package.

**Fix:**
1. Uninstall any existing version of the app from the phone first (Settings →
   Apps → Teleprompter → Uninstall), even if it looks broken or partial.
2. Reinstall using ADB instead of a file transfer, so you see the real error
   code instead of the vague phone popup:
   ```powershell
   adb install -r "Teleprompter\bin\Release\net9.0-android\publish\com.companyname.teleprompter-Signed.apk"
   ```
   ADB will print something specific like `INSTALL_FAILED_UPDATE_INCOMPATIBLE`
   (signature mismatch — do step 1 again) or `INSTALL_FAILED_INVALID_APK`
   (corrupted file — rebuild and retry, or re-transfer without a chat app).
3. If you were installing by tapping the file from Drive/Gmail/Files instead
   of ADB, confirm that specific app is allowed to install unknown apps:
   **Settings → Apps → [that app] → Install unknown apps → Allow**.
4. Double-check you're installing the **signed Release** APK from
   `bin\Release\net9.0-android\publish\`, not a Debug build.

---

### APK installs but crashes immediately on the phone

**Cause:** Could be an incompatibility with your Android version, or the app was not signed properly.

**Fix:**
- Make sure your phone runs Android 6.0 or newer (the app requires API 23+).
- Make sure you built with a **signed** APK (not a debug build).
- Check the build output for warnings or errors.

---

## Quick Reference: All PowerShell Commands

For convenience, here are all the commands in one place:

```powershell
# Navigate to the project folder
cd "C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter"

# --- ANDROID ---

# Step 1: Create keystore (one-time)
# If JAVA_HOME is set:
& "$env:JAVA_HOME\bin\keytool.exe" -genkeypair -v `
  -keystore teleprompter.keystore -alias teleprompter `
  -keyalg RSA -keysize 2048 -validity 10000

# OR if JAVA_HOME is empty, use Android Studio's bundled Java:
& "C:\Program Files\Android\Android Studio\jbr\bin\keytool.exe" -genkeypair -v `
  -keystore teleprompter.keystore -alias teleprompter `
  -keyalg RSA -keysize 2048 -validity 10000

# Step 2: Build signed APK (replace BOTH passwords with your real ones;
# use the FULL PATH to the keystore, adjusted to your own project location)
dotnet publish Teleprompter/Teleprompter.csproj `
  -f net9.0-android -c Release -p:NetMajor=9 `
  -p:AndroidKeyStore=true `
  -p:AndroidSigningKeyStore="C:\Users\John Lloyd T. Caban\source\repos\Johnlloyd17\Teleprompter\teleprompter.keystore" `
  -p:AndroidSigningKeyAlias=teleprompter `
  -p:AndroidSigningKeyPass=YOUR-KEY-PASSWORD-HERE `
  -p:AndroidSigningStorePass=YOUR-STORE-PASSWORD-HERE

# --- WINDOWS ---

# Build (framework-dependent, small)
dotnet publish Teleprompter/Teleprompter.csproj `
  -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9

# Build (self-contained, large, no dependencies needed)
dotnet publish Teleprompter/Teleprompter.csproj `
  -f net9.0-windows10.0.19041.0 -c Release -p:NetMajor=9 `
  -p:SelfContained=true -p:RuntimeIdentifierOverride=win-x64

# --- INSTALL ON DEVICE ---
adb install -r Teleprompter\bin\Release\net9.0-android\publish\com.companyname.teleprompter-Signed.apk
```

---

## Important Notes

- **Application ID:** The current app ID is `com.companyname.teleprompter`. This is a template placeholder. Before publishing publicly, change it to something unique in `Teleprompter.csproj` (like `com.johnlloyd.teleprompter`) — see [Section 10](#10-publishing-to-google-play-aab-vs-sharing-directly-apk). Change it **before** your first release — you cannot change it later without creating a brand new app.
- **Do NOT use `PublishTrimmed=true`:** Trimming breaks MAUI Windows apps at runtime due to XAML and reflection. Always leave it off.
- **No admin rights needed:** You do not need to run PowerShell as Administrator. All tools are in your user folder.
- **Keep your keystore file safe:** Store `teleprompter.keystore` in a safe place (and back it up). If you lose it, you can never update your app on users' devices.
- **Google Play:** If you want to publish to Google Play instead of sharing the APK directly, see [Section 10](#10-publishing-to-google-play-aab-vs-sharing-directly-apk) — you need to switch the output format from `.apk` to `.aab`.