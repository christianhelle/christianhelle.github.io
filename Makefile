all: clean dev run

prepare:
	sudo apt-get install ruby-full build-essential zlib1g-dev
	echo '# Install Ruby Gems to ~/gems' >> ~/.bashrc
	echo 'export GEM_HOME="$HOME/gems"' >> ~/.bashrc
	echo 'export PATH="$HOME/gems/bin:$PATH"' >> ~/.bashrc
	source ~/.bashrc
	gem install jekyll bundler

clean:
	rm -rf _site

dev:
	cp -f _config_dev.yml _config.yml

prod:
	cp _config_prod.yml _config.yml

run:
	bundle install && bundle exec jekyll serve --incremental

