# Roy - ATC smoke rerun

- Reused the already-running Jekyll dev server on `127.0.0.1:4000` instead of restarting it, because it was reachable and owned by `ruby.exe ... jekyll serve --host 127.0.0.1 --port 4000 --incremental`.
- Treated the Playwright red result as a real runtime gate failure, not a harness issue, because the affected post routes are serving generated self-redirect pages instead of rendered post HTML.
