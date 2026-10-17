# Republic - Player Feedback & Community Support

This document outlines official channels and guidelines for players to report issues, suggest policy mechanics, and seek support for Republic.

---

## 1. Official Support & Feedback Channels

- **Issue Tracker**: [GitHub Issues](https://github.com/Don-Walesky/Republic-Game/issues) — Preferred for bug reports, crash logs, and balance feedback.
- **Discussions & Feature Requests**: [GitHub Discussions](https://github.com/Don-Walesky/Republic-Game/discussions) — For gameplay balance debates, economic modeling proposals, and lore contributions.
- **Security Vulnerabilities**: [GitHub Security Advisories](https://github.com/Don-Walesky/Republic-Game/security/advisories) (see `SECURITY.md`).

---

## 2. Bug Reporting Template

When submitting a bug report, please include:
```markdown
### Environment
- **Platform**: Web (Browser name & version) / Windows Desktop / Linux
- **Build Version**: Early Access RC1 (Commit hash if known)

### Description
A clear and concise description of the issue.

### Steps to Reproduce
1. Start/Found country named '...'
2. Navigate to 'Presidential Decision: Taxation'
3. Adjust rate to '...' and click 'Confirm'
4. Observe actual vs expected behavior

### Expected Behavior
What you expected the simulation to do according to constitutional/economic rules.

### Actual Behavior
What actually happened (e.g. status label text, error message, UI state).

### Attached Logs or Save File
- Attach `presidential_desk_state.json` or browser console log if applicable.
```

---

## 3. Supported Troubleshooting Steps

If your country state fails to load:
1. Check that the country name includes the whole word **"Republic"** (e.g., *Republic of Arcadia*).
2. If resuming from a save file, ensure the file path is correct and accessible.
3. If corruption is reported, restore from the latest `.bak` file or use `Tools/backup_restore.ps1`.
