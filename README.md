# Star Citizen Companion (SCVerse & SCLogMate) for Playnite

[![Website](https://img.shields.io/badge/playnite.goover.dev-Showcase%20%26%20Downloads-ea8024?style=flat-square&logo=googlechrome&logoColor=white)](https://playnite.goover.dev)
[![License: AGPL-3.0](https://img.shields.io/badge/License-AGPL--3.0-blue.svg?style=flat-square)](LICENSE)
[![Ko-fi](https://img.shields.io/badge/Ko--fi-Support-F16061?style=flat-square&logo=ko-fi&logoColor=white)](https://ko-fi.com/goover)

A powerful Playnite generic plugin that integrates **SCLogMate**, live **RSI server status**, **session flight debriefings**, and maintenance tools into Playnite.

🌐 **Official Showcase & Direct Downloads**: [https://playnite.goover.dev/](https://playnite.goover.dev/)

---

## Features

- **Automated SCLogMate Background Lifecycle**:
  - Automatically starts **SCLogMate** in the background when Star Citizen starts.
- **Flight Debriefing & Push Notifications**:
  - Displays a clean session summary notification when exiting Star Citizen:
    - 🚀 Pilot's vessel
    - ⏱️ Flight duration
    - 💰 Profit / loss (aUEC)
    - 🎯 Completed missions, kills, and deaths
    - 🏦 Current aUEC wallet balance
- **Live CIG / RSI Server Status (Top Panel)**:
  - Adds a live status indicator directly in the Playnite top panel.
  - One-click dialog showing Platform, Persistent Universe, and Arena Commander status.
- **In-Library Tools & Maintenance Menu**:
  - Right-click any Star Citizen game in your library to access:
    - 🧹 **Shader Cache Cleaner**: Cleans DirectX/Vulkan shaders with 1 click.
    - 💾 **Controls Backup**: Safely backs up `actionmaps.xml` with timestamps.
    - 📸 **Open Screenshots Folder**.
    - 🚀 **Open SCLogMate**.
    - 🌐 **Erkul DPS Calculator**.

---

## Installation

1. Download the latest `.pext` package from the [Releases](https://github.com/gOOvER/Playnite-StarCitizen-Companion/releases) page.
2. Double-click the `.pext` file to install it into Playnite.
3. Restart Playnite.

---

## Building from Source

```powershell
.\build.ps1 -Configuration Release
```

---

## Support & Donate

If you enjoy this plugin and want to support its ongoing development, feel free to buy me a coffee:

[![Ko-fi](https://img.shields.io/badge/Ko--fi-F16061?style=for-the-badge&logo=ko-fi&logoColor=white)](https://ko-fi.com/goover)

---

## License

This project is licensed under the **GNU Affero General Public License v3.0 (AGPL-3.0)**. See [LICENSE](LICENSE) for details.
