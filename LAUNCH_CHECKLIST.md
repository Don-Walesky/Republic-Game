# Republic - Early Access Candidate Launch Checklist

This document provides the operational deployment and hosting checklist for launching the Republic Godot client as an Early Access candidate on the web.

---

## 1. Web Export Verification

Before deploying to any hosting environment, verify that the web client builds cleanly without missing assets or script errors:

- [ ] **Export Presets**: Confirm `client/godot/export_presets.cfg` contains the `Web` preset configured with `custom_html_shell="res://shell.html"`.
- [ ] **Run Web Export**:
  ```powershell
  godot --headless --path client/godot --export-release "Web" "builds/web/index.html"
  ```
- [ ] **Verify Generated Artifacts** in `client/godot/builds/web/`:
  - `index.html` (entrypoint shell)
  - `index.pck` (engine pack)
  - `index.png` & `index.icon.png` (app icons and splash)
  - `index.apple-touch-icon.png` (iOS touch icon)
- [ ] **Verify Automated Test Suite**:
  ```powershell
  godot --headless --path client/godot -s test_presidential_desk.gd
  ```
  Ensure all 24 automated checks pass.

---

## 2. Self-Hosting Options

The exported web build consists of static HTML, JavaScript, WebAssembly, and PNG/PCK assets. It can be hosted on standard static platforms:

### Option A: Cloudflare Pages (Recommended for Global CDN & DDoS Protection)
- **Framework Preset**: None (Static HTML).
- **Build Output Directory**: `client/godot/builds/web`.
- **Advantages**: Built-in global edge caching, automatic SSL, and zero server maintenance.

### Option B: Nginx on VPS / Dedicated Server
Place the contents of `client/godot/builds/web` into `/var/www/republic` and configure the site:
```nginx
server {
    listen 80;
    server_name play.republicgame.com;
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name play.republicgame.com;

    root /var/www/republic;
    index index.html;

    # SSL certificates
    ssl_certificate /etc/letsencrypt/live/play.republicgame.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/play.republicgame.com/privkey.pem;

    # Godot 4 WebAssembly & SharedArrayBuffer requirements
    add_header Cross-Origin-Opener-Policy "same-origin";
    add_header Cross-Origin-Embedder-Policy "require-corp";

    location / {
        try_files $uri $uri/ /index.html;
    }

    # Cache immutable game pack
    location ~* \.(pck|wasm)$ {
        expires 7d;
        add_header Cache-Control "public, no-transform";
    }
}
```

### Option C: Caddy Web Server (Automatic HTTPS)
```caddy
play.republicgame.com {
    root * /var/www/republic
    file_server

    header {
        Cross-Origin-Opener-Policy "same-origin"
        Cross-Origin-Embedder-Policy "require-corp"
    }
}
```

---

## 3. Custom Domain & DNS Setup

- [ ] **DNS Records**:
  - Apex domain (`republicgame.com`): `A` record pointing to web server or `CNAME` for CDN.
  - Subdomain (`play.republicgame.com`): Dedicated hostname for client web instance.
- [ ] **Time-To-Live (TTL)**: Set to 300s (5 minutes) during initial launch rollout.

---

## 4. HTTPS & Security Headers (Mandatory for Godot 4 Web)

Godot 4 Web export relies on modern browser WebAssembly and SharedArrayBuffer features, which require strict HTTPS and Cross-Origin Isolation headers:

- [ ] **Enforce HTTPS**: All HTTP traffic must redirect to HTTPS (301 redirect).
- [ ] **Cross-Origin Opener Policy (COOP)**:
  ```http
  Cross-Origin-Opener-Policy: same-origin
  ```
- [ ] **Cross-Origin Embedder Policy (COEP)**:
  ```http
  Cross-Origin-Embedder-Policy: require-corp
  ```
- [ ] **Content Security Policy (CSP)**:
  Allow WebAssembly execution (`'wasm-unsafe-eval'` or `'unsafe-eval'`) and script loads from your ad provider domain (e.g. `https://pagead2.googlesyndication.com`).

---

## 5. Web Ad Placement Notes

- [ ] **Ad Unit Slot**:
  - The presidential desk layout reserves a dedicated 728x70 leaderboard area (`AdPlaceholder`).
  - Keep ad units positioned outside or below critical gameplay telemetry (Country, Treasury, Tax controls) to prevent accidental clicks.
- [ ] **Google AdSense Integration**:
  - Inject the AdSense client library script into `client/godot/shell.html`.
  - Place `<ins class="adsbygoogle">` with approved publisher ID (`ca-pub-XXXXXXXXXXXXXXXX`) and slot ID.
- [ ] **Ad Block Handling**:
  - Ensure the canvas container displays gracefully if ad units fail to load or are blocked by privacy extensions.

---

## 6. Executive Decrees & Buffs IAP: Next Implementation Steps

Phase 5 establishes simulated purchase hooks writing `presidential_desk_buff_request.json`. For the full Early Access commercial launch:

- [ ] **Payment Gateway**:
  - Integrate Stripe Checkout or PayPal JavaScript SDK for web payment processing.
  - Generate one-time checkout sessions for $0.99 (Administrative Efficiency) and $1.99 (Investor Confidence).
- [ ] **Backend Webhook & Receipt Validation**:
  - Implement an authenticated webhook handler in `.NET` backend that validates completed payment receipts.
- [ ] **Authoritative Buff Application in `Republic.Core`**:
  - Create `BuffState` entity tracking active buffs, duration timers, and multipliers.
  - "Administrative Efficiency": Apply multiplier to National Yield cycle calculations.
  - "Investor Confidence": Apply stability and approval bonuses in `ApplyTaxDecision` and periodic cycles.
- [ ] **Client Expiration Sync**:
  - Export remaining buff duration in `presidential_desk_state.json` to show a countdown timer in the Godot UI.
