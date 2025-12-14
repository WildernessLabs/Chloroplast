---
template: Default
title: Command Line Interface
---

Chloroplast supports various sub commands. It is recommended to first `cd` into your project's root folder, and run the commands with defaults as appropriate.

## `new` sub command

The new subcommand allows you to bootstrap a chloroplast site by 
copying files to a destination folder.

```
chloroplast new conceptual targetFolder
```

This will create `targetFolder` (if it doesn't already exist), and 
copy files from the `conceptual` template. You can then run the
following commands to build and preview the site locally:

```
cd targetFolder
chloroplast build
chloroplast host
```

### parameters

- `conceptual`: The second positional parameter after `new`.
  - This will currently only support `conceptual`, which will be a copy of these docs.
  - Eventually we will have more templates, and eventually open up to community contributions.
- `targetFolder`: the third positional parameter after `new` is the name of the target folder where the template files will be placed.

## `build` sub command

The build subcommand will build a Chloroplast site

```
chloroplast build --root path/to/SiteConfig/ --out path/to/out
```

### parameters

- `root`: the path to the directory that contains a `SiteConfig.yml` file
- `out`: the path to the directory where the resulting HTML will be output.
- `normalizePaths`: defaults to `true`, which normalizes all paths to lower case. Set this to false for output paths to match the source casing.
- `errorLogPath`: optional path to write detailed error information to a log file
- `buildVersion`: optional version string to append to CSS and JavaScript references for cache busting. If not provided, a timestamp in the format `yyyyMMddHHmmss` is automatically generated.

The parameters above will default to the current working directory for the root path, and an `out/` directory in that same path if both parameters are omitted. 

The build command will render all `index.md` files into corresponding `index.html` files, and copy everything else to the same corresponding location (images, styles, etc).

Additionally, the build command automatically runs site validation after completing the build to check for broken links and missing assets. The build command will automatically generate sitemap XML files if a `baseUrl` is configured in your `SiteConfig.yml`. This helps with SEO and search engine indexing.

### Error Handling

The build command provides comprehensive error handling to help diagnose issues without overwhelming the console output:

- **Console Output**: Clean, concise error messages showing the essential information needed to fix problems
- **Error Log File**: Detailed error information including full stack traces and generated source code

#### Configuring Error Logging

You can configure error logging in two ways:

**1. Command Line Parameter:**
```bash
chloroplast build --errorLogPath /path/to/error-log.txt
```

**2. SiteConfig.yml Configuration:**
```yaml
# Error logging configuration
errorLogPath: logs/build-errors.log
```

#### Error Log Benefits

- **Console stays readable**: Only essential error information is shown in the terminal
- **Complete debugging info**: Full stack traces, generated source code, and detailed exception information are written to the log file
- **Multiple error collection**: All errors encountered during the build are collected and displayed together
- **Timestamp tracking**: Each error includes timestamp information for troubleshooting

#### Example Error Output

**Console Output:**
```
=== BUILD ERRORS ===
Found 1 error(s) during build:

[20:17:21] Error in 'Template initialization': Failed to initialize content renderer
    Template compilation failed:
    Error(s):
    - Template.cs(4,26): error CS1010: Newline in constant
    - Template.cs(4,61): error CS1002: ; expected

Error details written to: /tmp/build-errors.log
```

**Error Log File Contents:**
- Complete exception stack traces
- Full generated source code for template compilation errors
- Detailed file paths and processing context
- Timestamps for all errors encountered

## `host` sub command

The host sub command starts a simple HTML web server. Useful for local preview during development or even authoring. If you update markdown files, the content will automatically be rebuilt so you can just refresh the browser.

The host command automatically runs site validation before starting the web server to help identify any broken links or missing assets.

```
chloroplast host --root path/to/root --out path/to/html 
```

You can just press the enter key to end this task after stopping the web host with Ctrl+C at any time. 

If you are updating razor templates, you can also just run this command in a separate terminal window and leave it running. That way, as you run the full `build` subcommand, the content and front-end styles will update, and you can just refresh the browser after that build has completed.

### parameters

- `out`: This should be the same value as the `out` parameter used in the `build` command.

The parameter above will default to the `out/` folder in the current working directory, if omitted.

## `validate` sub command

The validate subcommand checks the generated site for common issues such as broken links, missing CSS/JavaScript files, and missing images. This helps catch asset and path problems early in development.

```
chloroplast validate --out path/to/out
```

The validation checks include:
- **CSS and JavaScript references**: Ensures all linked stylesheets and scripts exist
- **Navigation links**: Verifies internal links point to existing pages
- **Images**: Checks that all image references point to existing files
- **Directory links**: Ensures directory links have an index.html file

### parameters

- `out`: the path to the directory containing the built site output (defaults to `out/` in the current directory)

The validate command is automatically run at the end of the `build` command and before starting the `host` server, but can also be run standalone to spot-check site output.

## Sitemap Generation

The build command automatically generates sitemap XML files when a `baseUrl` is configured in your `SiteConfig.yml`. This helps search engines discover and index your site's content.

### Configuration

Add the following to your `SiteConfig.yml` to configure sitemap generation:

```yaml
# Base URL for your site (required for sitemap generation)
baseUrl: https://your-site.com

# Optional sitemap configuration
sitemap:
  enabled: true              # Enable/disable sitemap generation (default: true)
  maxUrlsPerSitemap: 50000   # Maximum URLs per sitemap file (default: 50000)
```

### Behavior

- **Single sitemap**: For sites with fewer URLs than the configured threshold, a single `sitemap.xml` file is generated
- **Multiple sitemaps**: For larger sites, multiple sitemap files are created (`sitemap1.xml`, `sitemap2.xml`, etc.) with a sitemap index file (`sitemap.xml`) that references them
- **Content filtering**: Only HTML files are included in sitemaps; assets like CSS, images, and other files are excluded
- **Timestamps**: Each URL entry includes a last modified timestamp based on the file's modification date

## Cache Busting

Chloroplast automatically adds version parameters to CSS and JavaScript references to ensure browser cache invalidation when assets are updated. This feature helps prevent users from seeing stale styles or scripts after deployments.

### Configuration

Add the following to your `SiteConfig.yml` to control cache busting behavior:

```yaml
# Cache busting configuration
cacheBusting:
  enabled: true  # Enable/disable cache busting (default: true)
```

### Usage in Templates

In your Razor templates, use the `WithVersion()` method to add version parameters to asset URLs:

```html
<!-- CSS with cache busting -->
<link href="@WithVersion("/assets/main.css")" rel="stylesheet" />

<!-- JavaScript with cache busting -->
<script src="@WithVersion("/assets/site.js")" type="text/javascript"></script>
```

### Command Line Usage

You can specify a custom build version when running the build command:

```bash
# Use a custom version
chloroplast build --buildVersion "v1.2.3"

# Use automatic timestamp version (default)
chloroplast build
```

### Behavior

- **When enabled**: Asset URLs are appended with `?v={version}` parameter (e.g., `/assets/main.css?v=20231215123456`)
- **When disabled**: Asset URLs remain unchanged (e.g., `/assets/main.css`)
- **Automatic versioning**: If no `--buildVersion` is specified, a timestamp in format `yyyyMMddHHmmss` is automatically generated
- **Query parameter handling**: The version parameter is properly appended with `&` if the URL already contains query parameters
