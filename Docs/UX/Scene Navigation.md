# Scene Navigation

Interface transitions and scene hierarchy for **Republic**.

---

## 1. Scene & Interface Hierarchy

```
Root
├── Authentication & World Connection
│   ├── Sign In / Connect to Persistent World
│   ├── Country Founding Dialog (if first-time player)
│   │   ├── Name, Flag, and Starting Geography
│   │   └── Independence Day Timestamp Confirmation
│   └── National Briefing Modal (if returning player)
│
├── Executive Workspace (Main Game Scene)
│   ├── Presidential Desk View (Phone, Terminal, Dossiers, Calendar)
│   ├── National Telemetry & Yield HUD
│   ├── National Map & Provincial View
│   │
│   ├── Decision Spaces (Overlays / Dedicated Rooms)
│   │   ├── Cabinet Chamber
│   │   ├── Economic Briefing Hall
│   │   ├── Situation Room
│   │   ├── Diplomatic Reception Hall
│   │   ├── Intelligence Office
│   │   └── Press Briefing Room
│   │
│   └── System Options (Audio, Display, Accessibility)
```

---

## 2. Navigation Patterns

### Primary Navigation (Executive Command Hub)
- **Executive Workspace**: Always accessible as the home command centre.
- **Decision Spaces**: Open as focused executive rooms or detailed full-screen consoles.
- **National Map**: Toggled seamlessly from the desk view to examine geography, infrastructure, and borders.
- **National Briefing**: Dismissible upon review, accessible at any time from the executive terminal.

### Persistent Simulation Notice
The simulation operates on a continuous global clock in **West Africa Time (WAT / UTC+1)**. There are no pause or speed modifier hotkeys. Exiting the client disconnects the session while the nation continues its persistent simulation in the background.

---

## 3. Keyboard & Shortcut Navigation

- `H` — Return to Executive Desk (Home)
- `C` — Cabinet Chamber
- `E` — Economic Briefing Hall & Projects
- `M` — Military & Situation Room
- `D` — Diplomatic Reception Hall
- `I` — Intelligence Office
- `P` — Press & Public Sentiment
- `Tab` / `Shift+Tab` — Cycle through active desk notifications
- `Enter` — Confirm executive action
- `ESC` — System / Settings menu