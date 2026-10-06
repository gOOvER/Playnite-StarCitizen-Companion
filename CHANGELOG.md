# Changelog

All notable changes to the Star Citizen Companion plugin for Playnite will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.0.0] - 2026-10-06

### Changed
- Production release 1.0.0 milestone.
- Modernized and optimized flight debriefing storage, sidebar widgets, and telemetry handlers.
- Updated PlayniteSDK dependency to 6.18.0.
- Synchronized package release with playnite.goover.dev ecosystem.

## [0.1.0] - 2026-10-02

### Added
- **Companion Lifecycle & SCLogMate Integration**:
  - Optional background launch of **SCLogMate** upon Star Citizen startup (`AutoStartSCLogMate` setting, disabled by default).
  - Process shutdown monitoring and flight debriefing notifications (session duration, aUEC balance delta, combat stats).
  - Quick launch button in sidebar and main menu.
- **RSI Server Status**:
  - Live RSI Platform, Persistent Universe, Electronic Access, and Community status polling.
  - Playnite Top Panel widget displaying active operational status and incident alerts.
- **Sidebar Integration**:
  - Dedicated Star Citizen view in Playnite's left navigation sidebar.
  - Interactive dashboard showing live pilot profile, current channel, server status, and quick links.
- **Maintenance Tools & Game Utilities**:
  - Shader cache cleaner (DX11 / Vulkan pipelines).
  - Quick access to `USER` folder, controls bindings, and game logs.
  - In-game `r_displayinfo` toggler (Levels 0 through 3).
  - Patch notes reader and direct links to RSI comm-links.
- **Configuration & Persistence**:
  - Full Playnite configuration GUI for SCLogMate paths, telemetry toggles, and notification preferences.
- **CI/CD Automation**:
  - GitHub Actions CI/CD workflows: automated build & `.pext` package verification (`build.yml`).
  - Automated release pipeline (`release.yml`) with checksums and changelog extraction.
