# Security Policy

## Supported versions

Security fixes land on `main`. If you are on an older commit, please update
before reporting unless you can show the issue still reproduces on `main`.

## Reporting a vulnerability

Please do **not** open a public issue for a vulnerability that could be abused.

- Preferred: GitHub **Security Advisory** on
  [MlsMoon/DrawModePlusMLS](https://github.com/MlsMoon/DrawModePlusMLS).
- If advisories are unavailable, email the repository owner listed on GitHub.

Include:

- Unity / URP versions
- A minimal project or exact steps
- Impact (what an attacker or a malicious asset could do)

This plugin is an editor debug tool. Still report anything that could write
unexpected assets, execute code during import, or leak data outside the project.

We will acknowledge a valid report as soon as we can and credit you if you want.
