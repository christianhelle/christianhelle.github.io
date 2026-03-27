## Roy - Atc.Test final validation follow-up

- Run `bundle exec jekyll clean` before alias smoke checks whenever post files were deleted or renamed.
- Reason: a normal `jekyll build` can leave stale files in `_site`, which makes removed draft routes appear valid and can hide missing redirect coverage.
- Validation impact: the clean rebuild gave trustworthy route results for the surviving Atc.Test post and its legacy draft aliases.
