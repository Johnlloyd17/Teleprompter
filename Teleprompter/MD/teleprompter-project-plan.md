# Teleprompter App — Project Plan
**Platform:** .NET MAUI (Android, iOS, Windows, macOS)
**Database:** SQLite (fully offline, local-first)
**Architecture:** MVVM (CommunityToolkit.Mvvm)

---

## 1. Project Overview

A cross-platform teleprompter app that lets users write/import scripts and scroll them
at a controlled speed while recording or presenting, with full offline functionality —
no internet or backend server required.

---

## 2. Core Functional Requirements

### 2.1 Script Management
- Create, edit, delete, duplicate scripts
- Script library / list view with search and folders or tags
- Import scripts from `.txt`, `.docx`, `.pdf`
- Export scripts to `.txt` / `.pdf`
- Auto-save while editing

### 2.2 Teleprompter Display / Playback
- Smooth auto-scrolling text with adjustable speed (WPM-based or px/sec)
- Play / Pause / Stop / Reset controls
- Rewind / fast-forward, jump to line
- Countdown before start (3-2-1)
- Progress indicator (time remaining / % scrolled)
- Mirror mode (horizontal flip) for physical beam-splitter rigs
- Adjustable font size, font family, line spacing, text alignment
- Adjustable background color, text color, contrast presets (for glass reflection)
- Adjustable margins / reading guide line or center marker
- Voice-activated scroll (optional, later phase — speech recognition matches spoken words to script position)

### 2.3 Remote Control / Input
- Bluetooth remote / clicker support (Play/Pause/Next mapped to system media keys where possible)
- On-screen tap zones (tap left/right of screen = speed down/up)
- Keyboard shortcuts (Windows/macOS): Space = play/pause, arrows = speed

### 2.4 Camera / Recording (optional advanced phase)
- Camera preview behind/overlaid with scrolling text (using MAUI camera APIs or platform-specific)
- Basic video recording synced with teleprompter session

### 2.5 Settings & Personalization
- Global default settings (speed, font, colors, mirror mode)
- Per-script override of settings
- Theme: Light / Dark / custom
- Multi-language UI (optional)

### 2.6 Offline Data & Sync
- 100% offline core functionality (SQLite)
- Optional future: cloud backup/sync (Google Drive/iCloud/custom API) — not required for MVP

---

## 3. Non-Functional Requirements
- Smooth scrolling performance (60fps target) even with long scripts
- Cross-platform consistent UI (MAUI Shell + platform-specific tweaks)
- Data persists fully offline — app must be usable with no network at all
- App state restore (resume last script/session)
- Accessibility: large text support, high-contrast mode

---

## 4. Tech Stack

| Layer | Choice |
|---|---|
| Framework | .NET MAUI (.NET 8/9) |
| Language | C# |
| UI Pattern | MVVM via `CommunityToolkit.Mvvm` |
| Navigation | .NET MAUI Shell |
| Local DB | SQLite via `sqlite-net-pcl` (or EF Core + `Microsoft.EntityFrameworkCore.Sqlite`) |
| File import/export | `CommunityToolkit.Maui` FilePicker, DocX/PDF parsing libs |
| DI | `Microsoft.Extensions.DependencyInjection` (built into MAUI) |
| Speech (optional) | `Microsoft.CognitiveServices.Speech` or platform native speech APIs (offline-limited) |

---

## 5. Database Design (SQLite — Offline)

SQLite fits this app well because all core data (scripts + settings) is small, structured,
and local. Store the `.db3` file under `FileSystem.AppDataDirectory`.

### Tables

**Scripts**
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | Autoincrement |
| Title | TEXT | |
| Content | TEXT | Full script body |
| FolderId | INTEGER | FK → Folders (nullable) |
| CreatedAt | DATETIME | |
| UpdatedAt | DATETIME | |
| WordCount | INTEGER | For estimated read time |

**Folders / Categories**
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| Name | TEXT | |
| CreatedAt | DATETIME | |

**ScriptSettings** (per-script override)
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| ScriptId | INTEGER | FK → Scripts |
| ScrollSpeed | REAL | |
| FontSize | INTEGER | |
| FontFamily | TEXT | |
| MirrorMode | BOOLEAN | |
| TextColor | TEXT | Hex |
| BackgroundColor | TEXT | Hex |
| Alignment | TEXT | Left/Center/Right |

**AppSettings** (global defaults, single row or key-value)
| Column | Type | Notes |
|---|---|---|
| Key | TEXT PK | e.g. "DefaultSpeed" |
| Value | TEXT | |

**PlaybackHistory** (optional, for "resume last session")
| Column | Type | Notes |
|---|---|---|
| Id | INTEGER PK | |
| ScriptId | INTEGER | FK |
| LastPositionPercent | REAL | |
| LastOpenedAt | DATETIME | |

---

## 6. Suggested Project Structure

```
TeleprompterApp/
├── TeleprompterApp.sln
├── TeleprompterApp/
│   ├── App.xaml / App.xaml.cs
│   ├── AppShell.xaml
│   ├── MauiProgram.cs              # DI registration, SQLite init
│   │
│   ├── Models/
│   │   ├── ScriptModel.cs
│   │   ├── FolderModel.cs
│   │   ├── ScriptSettingsModel.cs
│   │   └── AppSettingModel.cs
│   │
│   ├── Data/
│   │   ├── AppDbContext.cs         # or SQLiteAsyncConnection wrapper
│   │   ├── DatabaseService.cs      # CRUD operations
│   │   └── Migrations/
│   │
│   ├── ViewModels/
│   │   ├── ScriptListViewModel.cs
│   │   ├── ScriptEditorViewModel.cs
│   │   ├── TeleprompterPlayerViewModel.cs
│   │   └── SettingsViewModel.cs
│   │
│   ├── Views/
│   │   ├── ScriptListPage.xaml
│   │   ├── ScriptEditorPage.xaml
│   │   ├── TeleprompterPlayerPage.xaml
│   │   └── SettingsPage.xaml
│   │
│   ├── Services/
│   │   ├── IFileImportService.cs / FileImportService.cs
│   │   ├── IScrollEngine.cs / ScrollEngine.cs
│   │   └── IRemoteControlService.cs
│   │
│   ├── Controls/
│   │   └── ScrollingTextView.xaml (custom control)
│   │
│   ├── Resources/
│   │   ├── Styles/
│   │   ├── Fonts/
│   │   └── Images/
│   │
│   └── Platforms/
│       ├── Android/
│       ├── iOS/
│       ├── Windows/
│       └── MacCatalyst/
│
└── TeleprompterApp.Tests/
```

---

## 7. Core Screens (Pages)

1. **Script List Page** — library of saved scripts, search, folders, new/import buttons
2. **Script Editor Page** — text editor, word count, per-script settings shortcut
3. **Teleprompter Player Page** — the actual scrolling display + playback controls + mirror toggle
4. **Settings Page** — global defaults, theme, remote setup
5. **(Optional) Camera/Record Page** — teleprompter overlay + recording controls

---

## 8. Development Phases (Roadmap)

**Phase 1 — Foundation**
- Set up MAUI project, Shell navigation, DI
- SQLite integration + Scripts/Folders CRUD
- Basic script list + editor UI

**Phase 2 — Core Teleprompter Engine**
- Scrolling text engine with adjustable speed
- Playback controls (play/pause/reset/rewind)
- Font/color/mirror settings applied live

**Phase 3 — Personalization & Persistence**
- Per-script settings saved to SQLite
- Global app settings + theme (light/dark)
- Resume last session (PlaybackHistory)

**Phase 4 — Input & Import/Export**
- File import (.txt/.docx/.pdf) and export
- Bluetooth remote / keyboard shortcut support

---

## 9. Open Questions to Decide Later
- Do you want per-script settings, or just one global setting profile for MVP?
- Which platforms are priority for v1 (Android only first, or all four)?
- Is recording/camera overlay in scope for MVP or a stretch goal?
- Local-only forever, or cloud sync planned for a future version?
