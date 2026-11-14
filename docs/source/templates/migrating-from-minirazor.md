---
template: Default
title: Migrating Templates from MiniRazor
---

# Migrating Templates from MiniRazor

Chloroplast used to compile templates with MiniRazor. MiniRazor is deprecated, so current Chloroplast releases compile templates with RazorLight, which uses the standard ASP.NET Razor parser — the same one ASP.NET Core MVC and Blazor use.

Most templates work unchanged. This page covers the constructs that do not survive the move, how to recognize them, and how to rewrite them.

## What still works

The migration kept backward compatibility for the most common legacy patterns:

- `@using MiniRazor` still resolves, and `RawString` still works — a compatibility type stands in for the old MiniRazor one, so templates that build a `RawString` in a code block and write it out render as before.
- Templates that inherit from `ChloroplastTemplateBase<T>` keep all of their helpers: `@Raw(...)`, `@Href(...)`, `@Asset(...)`, `PartialAsync`, metadata access, and so on.
- Plain C# statements in `@{ }` blocks (variable declarations, conditionals that set variables, and similar) work as before.

## What breaks

### Helper methods declared in `@{ }` blocks

MiniRazor allowed a template to declare a helper method inside a `@{ }` block, and — critically — allowed the method body to contain literal markup mixed with `@` expressions, like a miniature template:

```razor
@{
    RawString RenderNodes(IConfigurationSection section)
    {
        var children = section.GetChildren();
        if (children.Any())
        {
        <ul class="nav-menu">
            @foreach(var item in children)
            {
                <li class="nav-item">
                    <a href="@Href("/" + item["path"])" class="nav-link">@item["title"]</a>
                </li>
            }
        </ul>
        }
        return new RawString("");
    }
}
```

Standard Razor does not support this, in either of its code sections:

- `@{ }` blocks may only contain C# **statements**. A method declaration is not a statement.
- `@functions { }` blocks hold member declarations, but their contents are **pure C#** — literal markup is not allowed inside a method body.

RazorLight copies a `@{ }` block into the generated template class verbatim, so the markup and the unprocessed `@foreach` / `@if` transitions land in the middle of generated C#. The C# compiler then shreds the result, and no part of the helper survives: the method is never really declared, and calls to it fail as well.

### How the failure looks

The build reports the error once per content file that (transitively) uses the broken template, and the build as a whole fails:

```
Error in 'index.md': Failed to process content file
    Details: Failed to compile generated Razor template:
- (11:9) Invalid expression term '<'
- (12:12) ; expected
- (17:20) 'GeneratedTemplate.GeneratedTemplate(item.ContainsKey, (?, ?))' must declare a body because it is not marked abstract, extern, or partial
- (25:19) The type or namespace name 'RawString' could not be found (are you missing a using directive or an assembly reference?)
- (16:28) Operator '/' cannot be applied to operands of type 'string' and 'string'
- (7:14) 'RenderNodes(IConfigurationSection)': not all code paths return a value
```

The line numbers refer to positions in your template file. The tell-tale signs are errors about `<` in expression position, fragments of your markup or expressions showing up as if they were method declarations (`GeneratedTemplate(...)` entries), and your helper's name appearing in "not all code paths return a value" or "does not exist in the current context" errors. If you see this pattern, look for a method declared in a `@{ }` block in the named template — the errors are a symptom, the helper is the cause.

Note that template compilation failures now **fail the build**. Earlier releases caught template errors and wrote the error text into the generated page, so a site with a broken template could appear to build while shipping exception text as page content. If a site "used to build" and now fails with errors like the above, the template was almost certainly already broken — the failure is just visible now.

### Encoding differences

Output written with a plain `@expression` is HTML-encoded by RazorLight, and its encoder also encodes non-ASCII characters as HTML entities — an em dash in a title or subtitle comes out as `&#x2014;` instead of `—`. Browsers render both identically. If you need a value written byte-for-byte, output it with `@Raw(...)`, as before.

## How to migrate a helper method

Move the method into a `@functions { }` block, and build its return value in C# instead of markup:

1. Move the method declaration out of `@{ }` and into `@functions { }`.
2. Replace literal markup in the method body with `StringBuilder` appends (add `@using System.Text`).
3. Replace `@expression` output inside the markup with appends of the value, HTML-encoded — `WebUtility.HtmlEncode(...)` (add `@using System.Net`) matches what Razor would have done when writing the expression into markup.
4. Keep returning `RawString` for the assembled markup, so call sites (`@RenderNodes(...)`) work unchanged — `Write` on the template base emits a `RawString` without encoding it a second time.

### Worked example: a recursive navigation menu

This is a real site menu template, before and after. The "before" version is the MiniRazor style above; it renders a nested `<ul>` from the site's `navTree` metadata, recursing into items that have children.

**Before (MiniRazor):**

```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
@using System.Linq;
@using System.Collections.Generic;
@using Microsoft.Extensions.Configuration;
@using Chloroplast.Core.Extensions;
@using MiniRazor;
@{
    RawString RenderNodes(IConfigurationSection section)
    {
        var children = section.GetChildren();
        if (children.Any())
        {
        <ul class="nav-menu">
            @foreach(var item in children)
            {
                <li class="nav-item">
                    <a href="@Href("/" + item["path"])" class="nav-link">@item["title"]</a>
                    @if (item.ContainsKey("items"))
                    {
                        @RenderNodes(item.GetSection("items"))
                    }
                </li>
            }
        </ul>
        }
        return new RawString("");
    }
}

<div class="sidebar" id="leftnav">
    @RenderNodes(Model.Metadata.GetSection ("navTree"))
</div>
```

**After (RazorLight):**

```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
@using System.Linq
@using System.Collections.Generic
@using Microsoft.Extensions.Configuration
@using Chloroplast.Core.Extensions
@using System.Net
@using System.Text
@using MiniRazor
@functions {
    private RawString RenderNodes(IConfigurationSection section)
    {
        var children = section.GetChildren().ToArray();
        if (children.Length == 0)
        {
            return new RawString("");
        }

        var builder = new StringBuilder();
        builder.Append("<ul class=\"nav-menu\">");
        foreach (var item in children)
        {
            builder.Append("<li class=\"nav-item\">");
            builder.Append("<a href=\"");
            builder.Append(WebUtility.HtmlEncode(Href("/" + item["path"])));
            builder.Append("\" class=\"nav-link\">");
            builder.Append(WebUtility.HtmlEncode(item["title"]));
            builder.Append("</a>");
            if (item.ContainsKey("items"))
            {
                builder.Append(RenderNodes(item.GetSection("items")).ToString());
            }
            builder.Append("</li>");
        }
        builder.Append("</ul>");
        return new RawString(builder.ToString());
    }
}

<div class="sidebar" id="leftnav">
    @RenderNodes(Model.Metadata.GetSection("navTree"))
</div>
```

The rendered menu is the same list of links with the same classes and hrefs. Two cosmetic differences to expect:

- The MiniRazor version preserved the template's literal indentation and newlines between tags; the `StringBuilder` version emits compact markup with no whitespace between elements. Browsers render both the same way.
- Dynamic values are encoded with `WebUtility.HtmlEncode`, which encodes quotes as `&quot;`. In attribute values (like the `href` above) that is correct, safe HTML.

## Migration checklist

1. Build the site with a current Chloroplast. If it builds clean, you are done — most templates need no changes.
2. If you get "Failed to compile generated Razor template" errors, open the named template and search for `@{` blocks that declare a method (a return type and a method name with parentheses, followed by a body). Also check templates the failing one depends on — frames and partials share the failure.
3. Move each such method to `@functions { }` and rewrite markup in its body as `StringBuilder` appends, encoding dynamic values with `WebUtility.HtmlEncode`.
4. Rebuild. The remaining differences from the old output should only be whitespace between tags and entity-encoded characters, both of which render identically in a browser.
