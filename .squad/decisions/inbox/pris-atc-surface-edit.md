# Pris Atc.Test surface edit

## Decision
- Point `README.md` at the surviving canonical Atc.Test permalink: `https://christianhelle.com/2025/07/atc-test-unit-testing-for-net-with-a-touch-of-class.html`.
- Enable `jekyll-redirect-from` in `_config.yml`, `_config_dev.yml`, and `_config_prod.yml` so committed build paths all honor the post's `redirect_from` aliases.

## Why
- The canonical post now lives on the `net` slug, so the README should link directly to the durable URL instead of an older slug family.
- Redirect aliases are declared in front matter on the surviving post, but they do not generate without the plugin being present in the configs that control builds.

## Scope
- No post body edits.
- No test execution.
