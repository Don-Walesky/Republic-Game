# Republic - Privacy Policy

**Effective Date:** October 17, 2026  
**Applicability:** Republic Simulation Engine, Desktop Client, and WebAssembly Early Access Candidate.

---

## 1. Overview & Core Privacy Commitment

Republic is an open political and nation-building simulation. We believe in privacy by design and data minimization:
- **No Personal Tracking**: We do not use third-party analytics trackers, tracking cookies, advertising IDs, or fingerprinting scripts.
- **Local Sovereignty**: In single-player mode, all country state, presidential decisions, and simulation saves are stored exclusively on your local file system (`republic_game.sav`, `presidential_desk_state.json`, or Godot `user://` storage).
- **Zero Account Wall**: Single-player gameplay does not require registering an account, providing an email, or surrendering personal identity.

---

## 2. Information We Process

### 2.1 Local Simulation Data
When you play Republic locally:
- **Sovereign Identifiers**: Your chosen country name, cabinet appointee preferences, and policy decisions are stored in local save files.
- **Save Location**: Stored on your own machine. Deleting the save file or clearing browser local storage permanently removes all associated simulation data.

### 2.2 Peer-to-Peer & Multiplayer Networking
When you host or join a multiplayer session:
- **Network Identifiers**: An ephemeral `ClientId` (UUID) is generated or read from local storage to identify your session.
- **IP Address**: Direct TCP connections between host and client expose IP addresses to each other as inherent to peer networking. Republic does not centrally log or sell your IP address.
- **Country State Transmission**: Authoritative simulation snapshots (tax rate, approval rating, trade proposals, diplomatic messages) are transmitted across connected peers to synchronize the shared world clock.

---

## 3. Advertising & Monetization Disclosures

- **No Active Ad Networks**: The Republic Early Access build contains architectural stubs and placeholder UI for future ethical monetization research. **No active advertisements, ad-network SDKs, trackers, or real payment processors are installed or running in the game.**
- Any future monetization features will be explicitly announced with opt-in user controls.

---

## 4. Data Security & Storage

- All local files are standard JSON and binary records managed by your local operating system user permissions.
- In multiplayer, game servers run authoritative state checks to prevent malicious alteration of other players' sovereign treasuries or institutions.

---

## 5. Contact & Data Rights

Under GDPR, CCPA, and global privacy frameworks:
- Because local data is stored exclusively on your device, you have complete control to inspect, export, or delete your data at any time by deleting the game files or clearing browser storage.
- For privacy inquiries or vulnerability reports, contact the maintainers via GitHub Issues at [https://github.com/Don-Walesky/Republic-Game](https://github.com/Don-Walesky/Republic-Game).
