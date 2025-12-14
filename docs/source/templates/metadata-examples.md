---
template: Default
title: Partial Metadata Inheritance - Examples
---

# Partial Metadata Inheritance - Advanced Examples

This page provides real-world examples of using partial metadata inheritance to create dynamic, context-aware components.

## Example 1: Multi-Level Navigation with Active State

This example shows a complete navigation system that highlights the active page and section.

### Content Pages

**home.md**:
```markdown
---
title: Home
activeNav: home
activeSection: main
---

# Welcome to Our Site
```

**docs/getting-started.md**:
```markdown
---
title: Getting Started
activeNav: docs
activeSection: intro
---

# Getting Started Guide
```

**docs/advanced/configuration.md**:
```markdown
---
title: Configuration
activeNav: docs
activeSection: advanced
---

# Advanced Configuration
```

### Navigation Partial

**source/nav.md**:
```markdown
---
template: nav
navItems:
  - id: home
    title: Home
    path: /
  - id: docs
    title: Documentation
    path: /docs
  - id: api
    title: API Reference
    path: /api
---
```

**templates/nav.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
@using Microsoft.Extensions.Configuration

<nav class="main-nav">
@{
    var activeNav = Model.GetMeta("activeNav");
    var navItems = Model.Metadata.GetSection("navItems").GetChildren();
    
    foreach (var item in navItems)
    {
        var id = item["id"];
        var isActive = string.Equals(id, activeNav, StringComparison.OrdinalIgnoreCase);
        var cssClass = isActive ? "nav-link active" : "nav-link";
        
        <a href="@Href(item["path"])" class="@cssClass">
            @item["title"]
            @if (isActive) { <span class="sr-only">(current)</span> }
        </a>
    }
}
</nav>
```

## Example 2: Context-Aware Sidebar

This example creates a sidebar that shows different content based on the current section.

### Content Pages

**guides/intro.md**:
```markdown
---
title: Introduction
section: guides
sidebarContent: getting-started
---

# Introduction to Our Framework
```

### Sidebar Partial

**source/sidebar.md**:
```markdown
---
template: sidebar
---
```

**templates/sidebar.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>

<aside class="sidebar">
@{
    var section = Model.GetMeta("section");
    var sidebarContent = Model.GetMeta("sidebarContent");
    
    <h3>@section.ToUpper() Section</h3>
    
    if (sidebarContent == "getting-started")
    {
        <ul>
            <li><a href="@Href("/guides/intro")">Introduction</a></li>
            <li><a href="@Href("/guides/quickstart")">Quick Start</a></li>
            <li><a href="@Href("/guides/tutorials")">Tutorials</a></li>
        </ul>
    }
    else if (sidebarContent == "reference")
    {
        <ul>
            <li><a href="@Href("/api/classes")">Classes</a></li>
            <li><a href="@Href("/api/methods")">Methods</a></li>
            <li><a href="@Href("/api/properties")">Properties</a></li>
        </ul>
    }
    else
    {
        <p>Related Content</p>
    }
}
</aside>
```

## Example 3: Breadcrumb Navigation

Generate breadcrumbs based on page metadata.

### Content Pages

**products/electronics/phones.md**:
```markdown
---
title: Smartphones
breadcrumbs:
  - title: Home
    path: /
  - title: Products
    path: /products
  - title: Electronics
    path: /products/electronics
  - title: Phones
    path: /products/electronics/phones
---

# Smartphones Catalog
```

### Breadcrumb Partial

**source/breadcrumbs.md**:
```markdown
---
template: breadcrumbs
---
```

**templates/breadcrumbs.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>
@using Microsoft.Extensions.Configuration

<nav aria-label="breadcrumb">
    <ol class="breadcrumb">
@{
    var breadcrumbsSection = Model.Metadata.GetSection("breadcrumbs");
    if (breadcrumbsSection != null && breadcrumbsSection.Exists())
    {
        var crumbs = breadcrumbsSection.GetChildren().ToList();
        for (int i = 0; i < crumbs.Count; i++)
        {
            var crumb = crumbs[i];
            var isLast = i == crumbs.Count - 1;
            var cssClass = isLast ? "breadcrumb-item active" : "breadcrumb-item";
            
            <li class="@cssClass" @(isLast ? "aria-current=\"page\"" : "")>
                @if (!isLast) {
                    <a href="@Href(crumb["path"])">@crumb["title"]</a>
                } else {
                    @crumb["title"]
                }
            </li>
        }
    }
    else
    {
        // Fallback breadcrumb when not specified
        <li class="breadcrumb-item"><a href="@Href("/")">Home</a></li>
        <li class="breadcrumb-item active" aria-current="page">@Model.GetMeta("title")</li>
    }
}
    </ol>
</nav>
```

## Example 4: Conditional Widget Display

Show or hide widgets based on page configuration.

### Content Pages

**blog/post1.md**:
```markdown
---
title: My Blog Post
showComments: true
showRelated: true
showAuthor: true
author: John Doe
authorBio: Software developer and blogger
---

# Blog Post Content
```

### Widgets Partial

**source/widgets.md**:
```markdown
---
template: widgets
---
```

**templates/widgets.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>

<div class="widgets">
@{
    var showComments = Model.GetMeta("showComments");
    var showRelated = Model.GetMeta("showRelated");
    var showAuthor = Model.GetMeta("showAuthor");
    
    if (!string.IsNullOrEmpty(showAuthor) && showAuthor.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        <div class="widget author-widget">
            <h4>About the Author</h4>
            <p><strong>@Model.GetMeta("author")</strong></p>
            <p>@Model.GetMeta("authorBio")</p>
        </div>
    }
    
    if (!string.IsNullOrEmpty(showRelated) && showRelated.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        <div class="widget related-widget">
            <h4>Related Posts</h4>
            <ul>
                <li><a href="#">Related Post 1</a></li>
                <li><a href="#">Related Post 2</a></li>
            </ul>
        </div>
    }
    
    if (!string.IsNullOrEmpty(showComments) && showComments.Equals("true", StringComparison.OrdinalIgnoreCase))
    {
        <div class="widget comments-widget">
            <h4>Comments</h4>
            <p>Comments section placeholder</p>
        </div>
    }
}
</div>
```

## Example 5: Nested Partials with Override

Demonstrate metadata override through multiple levels.

### Content Page

**project/status.md**:
```markdown
---
title: Project Status
theme: dark
priority: high
status: active
---

# Project Status Dashboard
```

### Outer Partial

**source/dashboard.md**:
```markdown
---
template: dashboard
theme: light
showChart: true
---

Dashboard wrapper content
```

**templates/dashboard.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>

<div class="dashboard theme-@Model.GetMeta("theme")">
    <h2>Dashboard</h2>
    <p>Priority: @Model.GetMeta("priority")</p>
    <p>Status: @Model.GetMeta("status")</p>
    
    @if (Model.GetMeta("showChart") == "true")
    {
        @await PartialAsync("source/chart.md")
    }
</div>
```

### Inner Partial

**source/chart.md**:
```markdown
---
template: chart
theme: blue
chartType: bar
---
```

**templates/chart.cshtml**:
```razor
@inherits Chloroplast.Core.Rendering.ChloroplastTemplateBase<Chloroplast.Core.Rendering.RenderedContent>

<div class="chart chart-@Model.GetMeta("chartType") theme-@Model.GetMeta("theme")">
    <p>Chart for status: @Model.GetMeta("status")</p>
    <p>Priority level: @Model.GetMeta("priority")</p>
    <p>Chart theme: @Model.GetMeta("theme")</p>
</div>
```

**Result**:
- Chart sees `theme: blue` (from chart.md, overrides dashboard and page)
- Chart sees `status: active` (from page, inherited through dashboard)
- Chart sees `priority: high` (from page, inherited through dashboard)
- Chart sees `chartType: bar` (from chart.md, unique to chart)

## Best Practices Checklist

- ✅ **Always check for metadata existence** before using it
- ✅ **Provide sensible defaults** for missing values
- ✅ **Document required metadata** in comments
- ✅ **Use descriptive key names** that indicate purpose
- ✅ **Test with and without parent metadata** to ensure graceful fallback
- ✅ **Avoid deep nesting** of partials when possible (2-3 levels max)
- ✅ **Use consistent naming conventions** across your templates
- ✅ **Consider case-insensitivity** when checking metadata values

## Debugging Tips

When troubleshooting metadata inheritance:

1. **Print all metadata keys**:
```razor
@{
    foreach (var kvp in Model.Metadata.AsEnumerable())
    {
        <p>@kvp.Key = @kvp.Value</p>
    }
}
```

2. **Check specific values**:
```razor
<p>activeNav: @(Model.GetMeta("activeNav") ?? "NOT SET")</p>
```

3. **Trace inheritance path**:
```razor
<!-- Content Page: activeNav=@Model.GetMeta("activeNav") -->
```

Add similar comments at each level to trace where values come from.
