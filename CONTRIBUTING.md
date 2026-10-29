# Contributing to Win-Arma

Katkıda bulunmak istediğiniz için teşekkürler! / Thanks for your interest in contributing!

---

## 🚀 Getting Started

1. **Fork** this repository
2. **Clone** your fork:
   ```bash
   git clone https://github.com/YOUR_USERNAME/win-arma.git
   cd win-arma
   ```
3. Create a **feature branch**:
   ```bash
   git checkout -b feature/amazing-feature
   ```
4. Make your changes and **build** to verify:
   ```bash
   dotnet build
   ```
5. **Commit** with a clear message:
   ```bash
   git commit -m "feat: add amazing feature"
   ```
6. **Push** and open a **Pull Request**:
   ```bash
   git push origin feature/amazing-feature
   ```

---

## 📋 Commit Convention

| Prefix | Usage |
|--------|-------|
| `feat:` | New feature |
| `fix:` | Bug fix |
| `ui:` | UI / visual change |
| `perf:` | Performance improvement |
| `docs:` | Documentation |
| `refactor:` | Code refactoring |
| `chore:` | Build, CI, dependencies |

---

## 🐛 Reporting Bugs

Please open a [GitHub Issue](../../issues) with:
- Windows version
- .NET runtime version (`dotnet --version`)
- Steps to reproduce
- Expected vs actual behavior
- Screenshot if applicable

---

## 💡 Feature Requests

Open an issue with the `enhancement` label. Describe the use case clearly.

---

## 🏗️ Development Notes

- Target framework: **net9.0-windows**
- UI: **WPF** (Windows Presentation Foundation)
- Settings stored in: `%AppData%\WinArma\settings.json`
- Clipboard history: `%AppData%\WinArma\clipboard_history.json`
- Icon/logo generated programmatically on first run via `Utils/IconGenerator.cs`

---

## ✅ Pull Request Checklist

- [ ] Code compiles without errors (`dotnet build`)
- [ ] No new compiler warnings introduced
- [ ] UI changes tested on both Dark and Light themes
- [ ] Settings window updated if new configurable option added
- [ ] README updated if new feature is user-facing

---

*Win-Arma is MIT licensed. By contributing, you agree your contributions will be licensed under the same MIT License.*
