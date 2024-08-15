Remove-Item -Recurse -Force _site
Copy-Item _config_dev.yml _config.yml
bundle install && bundle exec jekyll serve --incremental