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

For the main site's chrome, Chloroplast will look for a template named `SiteFrame.cshtml` by default. This is the outer "frame" or "chrome" that wraps your content.

You can also specify a different frame template for individual content files by adding a `frame:` property to the frontmatter:

```
---
template: TemplateName
frame: CustomFrame
title: Document title
---
```

If no `frame:` is specified, Chloroplast defaults to `SiteFrame.cshtml`.

## Frame Template Example

The SiteFrame template renders the full page structure:

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

## Error Handling

If a specified frame template is not found:
- An error message is logged to the console
- The affected content file is skipped (no incomplete page is rendered)
- The build continues processing other files
- The error is included in the build error summary

This ensures the build process doesn't stop completely due to a missing frame, but you'll be notified of the issue.

# Customizing Templates

When designing custom templates for Chloroplast, focus on leveraging the available template methods and ensuring your templates handle the full range of Chloroplast features effectively.

## Template Structure Reference

For recommended template structure and patterns, refer to the included templates:

- **SiteFrame.cshtml** - The main site chrome template that provides the overall page structure, navigation, and asset loading
- **Default.cshtml** - The default content template that demonstrates content area layout and metadata handling

These templates serve as the de-facto recommended structure and demonstrate best practices for:
- Using `@Href()` for internal navigation links with BasePath handling
- Using `@Asset()` for asset references with automatic cache busting
- Handling content metadata with `@Model.GetMeta()`
- Rendering content with `@Raw(Model.Body)`
- Working with the `@Model.Headers` collection for table of contents

# Template Properties and Methods

Available to all templates are the following functions:

- `@Raw("<some string>")`
  - the `Raw` method will avoid HTML encoding content (which is the default).
- `@Model.Body`
  - This is the main HTML content being rendered in this template. You should output this through the `Raw` method since it will likely contain HTML.
- `@Model.GetMeta("title")`
  - with this you can access any value in the front matter (for example, `title`).
- `@await PartialAsync("TemplateName", Model)`
  - This lets you render sub templates. 
  - the template name parameter will match the razor file name ... so in the example above, it would be looking for a file named `Templates/TemplateName.cshtml`
  - the second parameter is the model to pass to the template (typically `Model` to pass the current model).
  - **Note:** Template names are relative to the `templates_folder`, so you reference them without the `templates/` prefix. For example, use `"TranslationWarning"` not `"templates/TranslationWarning"` (though the latter will work as a fallback).
- `@await PartialAsync("path")`
  - **Smart routing based on file extension:**
    - `.cshtml` extension → renders as a Razor template with current `Model`
    - `.md` extension → renders as markdown file (can define its own template, defaults to `Default`, no `SiteFrame.cshtml`)
    - No extension → checks if a template exists, otherwise treats as markdown file path
  - Examples:
    - `@await PartialAsync("topNav")` → renders template if `topNav.cshtml` exists
    - `@await PartialAsync("source/menu.md")` → renders markdown file
    - `@await PartialAsync("template/nav.cshtml")` → explicitly renders template
- `@Model.Headers` collection
  - Each `h1`-`h6` will be parsed and added to this headers collection.
  - Every `Header` object has the following properties:
    - Level: the numeric value of the header
    - Value: the text
    - Slug: a slug version of the `Value`, which corresponds to an anchor added to the HTML before the header.

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

**⚠️ Cache Busting Conflict Warning**: When using `Asset()`, do not manually append query parameters or call `WithVersion()` on the result, as `Asset()` already includes cache busting. This would result in malformed URLs like `/basePath/assets/script/samples?v=123/mysample.js`.

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

## Localized Partials & Templates

Chloroplast provides helper methods to simplify rendering locale-specific partials (both Markdown content files and Razor template partials) without hard-coding locale-specific logic in every `.cshtml` file.

These helpers keep your templates clean and make it easy for site authors to add localized variants by simply creating files that follow a naming convention (e.g. `menu.es.md`).

### Helper Methods

| Method | Purpose |
| ------ | ------- |
| `ResolveLocalizedContentPath(basePath, locale?)` | Returns the localized relative path if it exists (e.g. `source/menu.es.md`), otherwise the base path. Does not render anything. |
| `InsertLocaleBeforeExtension(path, locale)` | Utility that transforms `file.md` into `file.es.md` (used internally; available if you need custom logic). |
| `LocalizedMarkdownPartialAsync(basePath, locale?)` | Renders a localized Markdown content partial, falling back to the base file. |
| `LocalizedTemplatePartialAsync<TModel>(baseName, model, locale?, fallbackToBase=true)` | Renders a Razor template partial, trying `baseName.{locale}` first when locale != default, then falling back. |

### Usage Examples

Render a localized menu Markdown file (tries `source/menu.<locale>.md` first):

```razor
@await LocalizedMarkdownPartialAsync("source/menu.md")
```

Render a localized Razor partial (e.g. `TranslationWarning.cshtml` + optional `TranslationWarning.es.cshtml`):

```razor
@(await LocalizedTemplatePartialAsync("TranslationWarning", Model))
```

Explicitly target a different locale (useful for language switchers or preview UIs):

```razor
@* Show how this page's menu would look in Spanish *@
@await LocalizedMarkdownPartialAsync("source/menu.md", locale: "es")
```

### File Naming Conventions

For a base file:

```
source/menu.md
```

Add a localized variant by inserting the locale before the extension:

```
source/menu.es.md
source/menu.fr.md
```

For Razor partials (no extension required in calls):

```
TranslationWarning.cshtml
TranslationWarning.es.cshtml
TranslationWarning.fr.cshtml
```

### Fallback Behavior

1. Non-default locale requested → framework looks for localized variant.
2. If localized file exists → it is rendered.
3. If missing → base file is rendered (graceful fallback, no exception).
4. You can force an exception instead of fallback with `fallbackToBase: false` on `LocalizedTemplatePartialAsync`.

### When to Use Which

| Situation | Use |
| --------- | --- |
| Rendering a Markdown content fragment (e.g. a sidebar, footer) | `LocalizedMarkdownPartialAsync` |
| Rendering a Razor partial (dynamic logic, custom model) | `LocalizedTemplatePartialAsync` |
| Just need the resolved path for custom logic (e.g. building a list) | `ResolveLocalizedContentPath` |

### Migrating Existing Logic

If you previously had manual logic like:

```razor
@{
  var baseMenu = "source/menu.md";
  var localized = CurrentLocale != SiteConfig.DefaultLocale ? $"source/menu.{CurrentLocale}.md" : baseMenu;
  // ... existence checks ...
}
@await PartialAsync(menuPathToUse)
```

Replace it with:

```razor
@await LocalizedMarkdownPartialAsync("source/menu.md")
```

And for localized warnings / notices:

```razor
@(await LocalizedTemplatePartialAsync("TranslationWarning", Model))
```

### Customization

You can still build higher-level abstractions on top of these methods (e.g. a `RenderSidebar()` helper) in a derived template base class. The provided helpers deliberately avoid assumptions about specific file names beyond the locale insertion convention.

### Edge Cases & Notes

* If a file has no extension (e.g. `fragment`), a locale variant becomes `fragment.es`.
* `InsertLocaleBeforeExtension` only modifies the final segment; nested paths are preserved.
* The existence check uses the configured site `root` and honors directory separators on the current platform.
* These helpers do not alter URL localization—they strictly manage which source content file is rendered.

---

With these helpers, you can keep templates concise while enabling contributors to add localized content simply by creating appropriately named files.
