# Christian Helle's Blog - GitHub Copilot Instructions

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the information here.

## Overview

Christian Helle's Blog is a Jekyll-based GitHub Pages website featuring a dark theme blog about software development, tools, and programming experiences. The site uses Ruby, Jekyll, and Bundler for static site generation, with automated .NET Playwright tests for quality assurance.

## Working Effectively

### Prerequisites
- Ruby 3.2+ (tested with 3.2.3)
- .NET 8.0 (for testing)
- Git

### Initial Setup and Environment Configuration
Configure Ruby gems to install in user directory:
```bash
export GEM_HOME="$HOME/gems"
export PATH="$HOME/gems/bin:$PATH"
echo 'export GEM_HOME="$HOME/gems"' >> ~/.bashrc
echo 'export PATH="$HOME/gems/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
```

Install Jekyll and Bundler:
```bash
gem install jekyll bundler
```

### Bootstrap, Build, and Test the Repository
```bash
# Install dependencies - takes ~65 seconds
bundle install

# Configure for development
cp _config_dev.yml _config.yml

# Build the site - takes ~5 seconds
bundle exec jekyll build

# Serve development site - starts in ~5 seconds
bundle exec jekyll serve --incremental
# Site available at http://127.0.0.1:4000/
```

### Alternative Quick Start Methods
Use the provided convenience scripts:
```bash
# Linux/macOS
./start.sh

# Windows
./start.ps1

# Or use Makefile
make dev run
```

### Testing
#### .NET Playwright Tests
The repository includes .NET Playwright tests for comprehensive website validation:
```bash
cd tests/playwright

# Build test project - takes ~10 seconds
dotnet build

# Update target framework if needed (from net6.0 to net8.0)
# Add <SuppressTfmSupportBuildErrors>true</SuppressTfmSupportBuildErrors> to .csproj if needed

# Install Playwright CLI and browsers (if running tests locally)
dotnet tool install --global Microsoft.Playwright.CLI
playwright install

# Run tests (requires Jekyll dev server running)
dotnet test
```

#### Manual Validation
Always manually validate website functionality:
- Navigate to http://127.0.0.1:4000/
- Verify dark theme renders correctly
- Test blog post navigation
- Check archive and tag pages
- Verify all links work properly

## Configuration Management

### Development vs Production
- **Development**: Copy `_config_dev.yml` to `_config.yml`
- **Production**: Copy `_config_prod.yml` to `_config.yml`

The main differences:
- Development uses localhost base URL
- Production uses https://christianhelle.com
- Different analytics and optimization settings

### Key Configuration Files
- `_config_dev.yml` - Development configuration
- `_config_prod.yml` - Production configuration  
- `Gemfile` - Ruby dependencies
- `Makefile` - Build automation commands

## Development Workflow

### Making Changes
1. Always start with development configuration: `cp _config_dev.yml _config.yml`
2. Start the development server: `bundle exec jekyll serve --incremental`
3. Make your changes - Jekyll auto-rebuilds with `--incremental`
4. Test changes in browser at http://127.0.0.1:4000/
5. Run validation tests if modifying functionality

### Content Creation
- Blog posts go in `_posts/` directory
- Follow Jekyll naming convention: `YYYY-MM-DD-title.md`
- Use existing posts as templates for front matter
- Images go in `assets/` directory

### Theme and Styling
- Uses customized Minima theme with dark skin
- Theme source: https://github.com/christianhelle/minima (custom fork)
- Layouts in `_layouts/`
- Includes in `_includes/`
- Sass files in `assets/`

## Common Tasks

### Full Clean Build
```bash
# Clean previous build
rm -rf _site

# Install/update dependencies
bundle install

# Configure for development
cp _config_dev.yml _config.yml

# Build and serve
bundle exec jekyll serve --incremental
```

### Production Deployment
The site automatically deploys via GitHub Actions when changes are pushed to the `master` branch. The workflow:
1. Uses Ruby 3.0 and bundles dependencies
2. Copies production configuration
3. Builds site with Jekyll
4. Deploys to GitHub Pages
5. Runs link checker tests

### Updating Dependencies
```bash
# Update bundle
bundle update

# Check for security vulnerabilities
bundle audit
```

## Validation Requirements

### Before Committing Changes
Always validate your changes work correctly:
1. Build succeeds without errors: `bundle exec jekyll build`
2. Development server starts: `bundle exec jekyll serve --incremental`  
3. Site loads correctly at http://127.0.0.1:4000/
4. Navigation and links work
5. Dark theme renders properly
6. No broken links or images

### CI/CD Validation
The GitHub Actions workflow automatically:
- Builds the site with production configuration
- Deploys to GitHub Pages
- Runs link checker tests
- Validates site accessibility

## Repository Structure

### Key Directories
- `_posts/` - Blog post markdown files
- `_layouts/` - Jekyll layout templates
- `_includes/` - Reusable Jekyll includes
- `assets/` - Images, CSS, JavaScript
- `tests/playwright/` - .NET Playwright web tests
- `.github/workflows/` - GitHub Actions CI/CD

### Important Files
- `_config_dev.yml` / `_config_prod.yml` - Jekyll configurations
- `Gemfile` - Ruby dependency definitions
- `start.sh` / `start.ps1` - Quick start scripts
- `Makefile` - Build automation
- `blog.sln` - Visual Studio solution for tests

## Performance Notes

All operations are fast - no long timeouts needed:
- Bundle install: ~65 seconds
- Jekyll build: ~5 seconds  
- Server start: ~5 seconds
- .NET build: ~10 seconds

## Troubleshooting

### Common Issues
- **Bundle install fails**: Ensure `GEM_HOME` and `PATH` are configured correctly
- **Jekyll build fails**: Check that `_config.yml` exists (copy from dev or prod version)
- **Server won't start**: Make sure port 4000 is available
- **.NET tests fail to build**: Update target framework to net8.0 in PlaywrightTests.csproj

### Useful Commands
```bash
# Check Ruby/Jekyll versions
ruby --version
jekyll --version
bundle --version

# Verbose Jekyll build for debugging
bundle exec jekyll build --verbose

# Check bundle dependencies
bundle list

# Clear Jekyll cache
bundle exec jekyll clean
```

## Security and Best Practices

- Never commit sensitive configuration
- Keep dependencies updated for security
- Use development configuration for local work
- Test thoroughly before pushing to master
- Follow Jekyll security best practices
- Validate external links regularly via automated tests