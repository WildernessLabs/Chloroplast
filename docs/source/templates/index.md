---
template: Default
title: Templating
---

Chloroplast templates are built using ASP.NET's Razor templates.

# Configuration

The `SiteConfig.yml` file lets you configure the location of templates and the folders to find content files to process.

```yaml
# razor templates
templates_folder: templates

areas:
  - source_folder: /source
    output_folder: /
```

# Markdown front matter

The template will be defined by the content's front matter. 

```
---
template: TemplateName
title: Document title
---
```

If this value is either incorrect, or omitted, it will default to look for a template named `Default.cshtml`. These templates are assumed to be just for the content area itself.

# SiteFrame.cshtml

For the main site's chrome, Chloroplast will look for a template named `SiteFrame.cshtml` ... this will render

```
<html>
<head>
    <title>@Model.GetMeta("Title")</title>
</head>
<body>
    <div class="maincontent">
    @Raw(Model.Body)
    </div>
</body>
</html>
```

# Customizing Templates

When creating custom templates for Chloroplast, consider the following design elements and structure:

## Essential Template Elements

Your templates should typically include these key elements:

### Site Structure (SiteFrame.cshtml)
- **HTML document structure**: `<html>`, `<head>`, `<body>` tags
- **Meta tags**: Including title, viewport, and SEO metadata
- **Asset references**: CSS stylesheets and JavaScript files
- **Navigation**: Main site navigation menu
- **Content area**: Where `@Raw(Model.Body)` will be rendered
- **Footer**: Site footer with links and information

### Navigation and Menus
- **Main navigation**: Primary site navigation, often using `@Href()` for internal links
- **Breadcrumbs**: For hierarchical navigation using `@Model.Headers` or custom logic
- **Side navigation**: For documentation or category-based sites
- **Table of contents**: Auto-generated from `@Model.Headers` collection

### Content Templates (Default.cshtml, etc.)
- **Article structure**: Headers, content body, metadata display
- **Code syntax highlighting**: For technical documentation
- **Image and media handling**: Responsive image containers
- **Interactive elements**: Search, filtering, or dynamic content

## Template Structure Example

Here's a complete example showing recommended structure:

```html
<!-- SiteFrame.cshtml -->
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>@Model.GetMeta("Title") - My Site</title>
    <link href="@Asset("/css/main.css")" rel="stylesheet" />
</head>
<body>
    <nav class="main-nav">
        <a href="@Href("/")">Home</a>
        <a href="@Href("/docs")">Docs</a>
        <a href="@Href("/about")">About</a>
    </nav>
    
    <main class="content">
        @Raw(Model.Body)
    </main>
    
    <footer>
        <p>&copy; 2024 My Site. Built with Chloroplast.</p>
    </footer>
    
    <script src="@Asset("/js/app.js")"></script>
</body>
</html>
```

```html
<!-- Default.cshtml (content template) -->
<article>
    <header>
        <h1>@Model.GetMeta("Title")</h1>
        @if (Model.GetMeta("author") != null)
        {
            <p class="author">By @Model.GetMeta("author")</p>
        }
    </header>
    
    @if (Model.Headers.Any())
    {
        <nav class="table-of-contents">
            <h2>Contents</h2>
            <ul>
                @foreach (var header in Model.Headers)
                {
                    <li><a href="#@header.Slug">@header.Value</a></li>
                }
            </ul>
        </nav>
    }
    
    <div class="content-body">
        @Raw(Model.Body)
    </div>
</article>
```

# Template Properties and Methods

Available to all templates are the following functions:

- `@Raw("<some string>")`
  - the `Raw` method will avoid HTML encoding content (which is the default).
- `@Model.Body`
  - This is the main HTML content being rendered in this template. You should output this through the `Raw` method since it will likely contain HTML.
- `@Model.GetMeta("title")`
  - with this you can access any value in the front matter (for example, `title`).
- `@await PartialAsync("TemplateName", "model value")`
  - This lets you render sub templates. 
  - the template name parameter will match the razor fil.e name ... so in the example above, it would be looking for a file named `Templates/TemplateName.cshtml`
  - the second parameter can be any kind of object really, whatever the template in question is expecting.
- `@Model.Headers` collection
  - Each `h1`-`h6` will be parsed and added to this headers collection.
  - Every `Header` object has the following properties:
    - Level: the numeric value of the header
    - Value: the text
    - Slug: a slug version of the `Value`, which corresponds to an anchor added to the HTML before the header.
- `@await PartialAsync("Docs/area/menu.md")`
  - This overload of the `PartialAsync` method assumes you're rendering a markdown file.
  - The markdown file can define its own template (will default to `Default`), and will only render the contents of that specific template (ie. no `SiteFrame.cshtml`).

## URL and Asset Methods

For generating URLs and asset references in your templates:

- `@Href("/path/to/page")`
  - Prefixes a site-relative URL with the configured BasePath
  - Use this for internal site navigation links
  - Example: `<a href="@Href("/about")">About</a>`
- `@Asset("/path/to/asset.css")`
  - Builds an asset URL with BasePath and automatic cache-busting version
  - Combines both BasePath application and version parameter addition
  - **Important**: Do not manually append query parameters to Asset() results, as this will conflict with the automatic version parameter
  - Example: `<link href="@Asset("/css/main.css")" rel="stylesheet" />`

### URL Method Examples

```html
<!-- Navigation links using Href -->
<nav>
    <a href="@Href("/")">Home</a>
    <a href="@Href("/docs")">Documentation</a>
    <a href="@Href("/about")">About</a>
</nav>

<!-- Asset references using Asset (preferred for CSS/JS) -->
<link href="@Asset("/css/main.css")" rel="stylesheet" />
<script src="@Asset("/js/app.js")"></script>

<!-- Manual approach using Href + WithVersion for more control -->
<link href="@WithVersion(Href("/css/main.css"))" rel="stylesheet" />
```

**⚠️ Cache Busting Conflict Warning**: When using `Asset()`, do not manually append query parameters or call `WithVersion()` on the result, as `Asset()` already includes cache busting. This would result in malformed URLs like `/css/main.css?v=123&custom=param&v=123`.

## Cache Busting Methods

For managing asset versioning and cache invalidation:

- `@WithVersion("/path/to/asset.css")`
  - Adds a version parameter to asset URLs for cache busting (e.g., `/path/to/asset.css?v=20231215123456`)
  - Only adds the version parameter when cache busting is enabled in configuration
  - Properly handles existing query parameters by using `&` instead of `?` when appropriate
- `@BuildVersion`
  - Gets the current build version string, or `null` if cache busting is disabled
  - Useful for displaying version information or custom cache busting logic

### Cache Busting Example

```html
<!-- Recommended approach using WithVersion -->
<link href="@WithVersion("/assets/main.css")" rel="stylesheet" />
<script src="@WithVersion("/assets/site.js")"></script>

<!-- Manual approach using BuildVersion -->
@if (BuildVersion != null)
{
    <link href="/assets/main.css?v=@BuildVersion" rel="stylesheet" />
}
else
{
    <link href="/assets/main.css" rel="stylesheet" />
}
```
