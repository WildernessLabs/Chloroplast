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
- `buildVersion`: optional version string to append to CSS and JavaScript references for cache busting. If not provided, a timestamp in the format `yyyyMMddHHmmss` is automatically generated.

The parameters above will default to the current working directory for the root path, and an `out/` directory in that same path if both parameters are omitted. 

The build command will render all `index.md` files into corresponding `index.html` files, and copy everything else to the same corresponding location (images, styles, etc).

Additionally, the build command will automatically generate sitemap XML files if a `baseUrl` is configured in your `SiteConfig.yml`. This helps with SEO and search engine indexing.

## `host` sub command

The host sub command starts a simple HTML web server. Useful for local preview during development or even authoring. If you update markdown files, the content will automatically be rebuilt so you can just refresh the browser.

```
chloroplast host --root path/to/root --out path/to/html 
```

You can just press the enter key to end this task after stopping the web host with Ctrl+C at any time. 

If you are updating razor templates, you can also just run this command in a separate terminal window and leave it running. That way, as you run the full `build` subcommand, the content and front-end styles will update, and you can just refresh the browser after that build has completed.

### parameters

- `out`: This should be the same value as the `out` parameter used in the `build` command.

The parameter above will default to the `out/` folder in the current working directory, if omitted.

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
