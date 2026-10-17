# Security Policy

Republic takes the security, data integrity, and authoritative validity of the simulation seriously.

---

## 1. Supported Versions

Security and data integrity patches are applied to the active development branches:

| Version | Status | Notes |
|---|---|---|
| `main` (Early Access RC) | :white_check_mark: Active | Supported with active fixes and authoritative simulation validation |
| Releases < 1.0.0 | :x: Deprecated | Alpha iterations without full authoritative validation |

---

## 2. Authoritative Simulation Security Architecture

Republic operates under an authoritative host model:
1. **Zero Client Trust on Protected State**:
   - Clients are never permitted to declare or alter their own sovereign Treasury balance, institutional capacity, or bilateral trade settlement outcomes.
   - All state modifications are checked by the authoritative host engine (`RepublicHost` / `RepublicEngine`).
2. **Idempotent Decision Requests**:
   - Consequential executive decisions (taxation, capital projects, appointments, emergency decrees) utilize unique request IDs (`RequestId`) and idempotency checks to prevent double-spending or replay attacks.
3. **Session Reconnect Authentication**:
   - Reconnecting sessions verify sovereign country ownership against previous credentials (`PreviousClientId`) before granting command authorization.
4. **Resilient Local Persistence**:
   - Corrupt or malformed save files are isolated with timestamped `.bak` emergency backups before replacement to prevent silent data loss or arbitrary code execution via deserialization.

---

## 3. Reporting a Vulnerability

If you discover a security vulnerability, buffer exploit, or authoritative desync flaw in Republic:

1. **Do not create a public issue**.
2. Privately submit details through GitHub Security Advisories at [https://github.com/Don-Walesky/Republic-Game/security/advisories](https://github.com/Don-Walesky/Republic-Game/security/advisories) or contact the core maintainers directly.
3. Include:
   - Reproduction steps or proof of concept.
   - Affected version or commit hash.
   - Potential impact on client integrity or authoritative simulation state.

We will acknowledge receipt within 48 hours and work with you on a coordinated fix.

