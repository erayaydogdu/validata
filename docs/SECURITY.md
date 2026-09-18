# Security Advisory

## Dependabot Alerts: Resolved

GitHub reported **96 open vulnerabilities** on the default branch (2 critical, 58 high, 31 moderate, 5 low), all of them npm packages in `src/Validata.Web/package-lock.json`. They are now resolved; **1 remains**.

### What was fixed, and how

| Step | Result |
|------|--------|
| `npm audit fix` (no breaking changes) in `src/Validata.Web` | 67 → 37 local advisories (3 critical → 1) |
| Angular `19.2` → `20.3` upgrade (`@angular/*`, `@angular/cli`, `@angular-devkit/build-angular`, TypeScript ~5.9.3) | 37 → 5 local advisories (0 critical, 0 high) |
| GitHub alert count on the default branch | 96 → 1 |

Most alerts came from the Angular 19 build toolchain (transitive `tar`, `vite`, `webpack-dev-server`, `rollup`, `postcss`, `sigstore`, `socket.io`) plus `@angular/core|common|compiler` XSS/DoS advisories that GitHub marks as having **no patch in the 19.x line** — the only fix for those is a major upgrade.

### Remaining alerts

1. **`uuid@8.3.2` — moderate, development scope.**
   Transitive path: `@angular-devkit/build-angular` → `webpack-dev-server` → `sockjs` → `uuid`.
   The patch (11.1.1) is a major version that `sockjs` cannot take (it requires `uuid@^8`), so there is no in-range fix. The advisory concerns `v3/v5/v6` when a caller-supplied `buf` is passed; `sockjs` only calls `v4`, so the vulnerable path is not reachable here.

2. **5 moderate advisories remain in local `npm audit`** (`uuid`, `sockjs`, `webpack-dev-server`, `@angular-devkit/build-webpack`, `@angular-devkit/build-angular`). All are dev/build tooling with `fixAvailable: false` at Angular 20; clearing them requires Angular 21.

### Risk assessment

- **Production impact: none.** These are `devDependencies` — they are not shipped to the runtime bundle or the production container.
- **Attack vector:** the local build/dev-server environment only.

### Verifying the current state

```bash
# Local advisories, including devDependencies
npm audit --prefix src/Validata.Web

# Production dependencies only
npm audit --prefix src/Validata.Web --omit=dev
```

Live alert list: https://github.com/erayaydogdu/validata/security/dependabot

### Standing recommendation

- Run `npm ci` rather than `npm install` for reproducible builds (the frontend Dockerfile does this).
- Only trusted users should run install/build steps; prefer sandboxed build environments.
