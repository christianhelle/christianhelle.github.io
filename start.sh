rm -rf _site
cp -f _config_dev.yml _config.yml
bundle install && bundle exec jekyll serve --incremental

