rm _config.yml
cp _config_dev.yml _config.yml

bundle install && bundle exec jekyll serve --incremental

rm _config.yml
cp _config_prod.yml _config.yml