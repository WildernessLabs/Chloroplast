---
template: Default
title: Templating
---

Chloroplast templates are built using ASP.NET's Razor templates.

# Configuration

The `SiteConfig.yml` file lets you configure the location of templates.

```
# razor templates
templates_folder: Templates
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
