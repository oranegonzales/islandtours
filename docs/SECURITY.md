# Security notes

This project applies defense-in-depth controls suitable for a portfolio and development environment.

## Implemented controls

- PBKDF2-HMAC-SHA256 password hashes with unique salts and a 600,000-iteration work factor.
- Automatic upgrade of plaintext and lower-work-factor passwords after successful authentication.
- Per-account failed-login counters and timed lockout.
- Encrypted Forms Authentication tickets with `HttpOnly` and `SameSite=Strict`; `Secure` is set on HTTPS requests.
- Central page authorization for administrator, staff, client and public routes.
- View-state MAC/event validation plus session-bound `ViewStateUserKey`.
- Parameterized SQL in the repaired authentication, administration, refund, receipt and transportation paths.
- Conditional transportation updates to avoid overwriting concurrent changes.
- Request size limits, generic remote errors and response headers for framing, MIME sniffing, referrer and browser permissions.
- Local/environment-based API-key configuration; tracked configuration contains no key.

## Production checklist

- Terminate only over HTTPS and set `requireSSL="true"` for all cookies.
- Supply the connection string and API keys through a managed secret store.
- Configure the same protected `machineKey` across every web node.
- Use distributed session state and distributed login throttling when running more than one node.
- Replace `TrustServerCertificate=True` with certificate validation appropriate to the SQL environment.
- Restrict the database principal to the minimum required permissions.
- Add centralized, access-controlled log collection and alerting.
- Run dependency, static-analysis, dynamic and penetration tests before handling real customer or payment data.
- Revoke the historical Fixer credential that appeared in repository history.

Please report a suspected vulnerability privately to the repository owner rather than opening a public issue with exploit details.
