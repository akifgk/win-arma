# Win-Arma Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-10-29

### 🎉 Initial Release

#### ✨ Added
- **Spotlight-style launcher** — `Alt + Space` global hotkey, animated open/close with expanding result list
- **App search** — scans Windows Start Menu shortcuts for installed applications
- **File search** — Windows Search index integration with common folders fallback
- **Web search** — Google, Bing, DuckDuckGo, Yandex, Brave with prefix shortcuts (`g `, `y `, `gh `, `w `, `b `)
- **Calculator** — supports expressions, `sqrt()`, `^` (power), `%` (percentage)
- **System commands** — kilitle (lock), kapat (shutdown), uyku (sleep), yeniden başlat (restart)
- **Clipboard history** — tracks last 50 copied items, triggered with `pano` or `cb`, persisted to `%AppData%\WinArma\clipboard_history.json`
- **Quick Look** — `Shift+Space` / `F3` to preview images, text/code files, and show file info cards
- **Text snippets** — `!mail`, `!iban`, `!tel`, `!adres`, `!tarih`, `!saat`, `!shrug`, `!guid`
- **Settings window** — full settings UI accessible via tray icon right-click
  - All features individually toggleable (on/off switches)
  - Theme selection, hotkey customization, search engine choice
  - Max results slider (5–20)
  - Windows startup toggle (registry-based)
- **4 Themes** — Dark, Midnight, Amoled, Light (Apple-style white)
- **Custom logo & icon** — generated programmatically via WPF `DrawingVisual`, multi-size `.ico` (16–256px) + `logo.png`
- **System tray** — tray icon with right-click context menu (Open, Settings, About, Exit)
- **Settings persistence** — JSON settings at `%AppData%\WinArma\settings.json`
- **Windows startup** — optional auto-start via registry `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`

---

## [Unreleased]

### 🔮 Planned
- Custom snippet editor in Settings UI
- Plugin system for third-party result providers
- More themes (Nord, Solarized, Dracula)
- Fuzzy search ranking improvements
- Multi-monitor positioning support
