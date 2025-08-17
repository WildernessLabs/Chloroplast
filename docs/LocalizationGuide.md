# Chloroplast Localization Guide

Chloroplast now supports solid localization features that allow you to create multilingual documentation sites without duplicating configuration or imposing an English-centric view on your content.

## Configuration

Add localization settings to your `SiteConfig.yml`:

```yaml
# Localization configuration
defaultLocale: en          # The primary language of your content
supportedLocales:          # Array of all supported languages
  - en                     # English (default)
  - es                     # Spanish
  - fr                     # French
  - de                     # German
```

The `defaultLocale` specifies which language your original content is written in. This doesn't have to be English - it can be any language that makes sense for your project.

## Content Organization

Chloroplast supports two naming conventions for organizing translated content:

### 1. Filename-based Localization

Add the locale code before the file extension:

```
content/
├── guide.md              # Default language (English)
├── guide.es.md          # Spanish translation
├── guide.fr.md          # French translation
└── guide.de.md          # German translation
```

### 2. Directory-based Localization

Organize content in locale-specific directories:

```
content/
├── guide.md              # Default language (English)
├── es/
│   └── guide.md         # Spanish translation
├── fr/
│   └── guide.md         # French translation
└── de/
    └── guide.md         # German translation
```

## URL Structure

Chloroplast automatically generates locale-aware URLs:

- **Default locale**: `/docs/guide` (no locale prefix)
- **Other locales**: `/es/docs/guide`, `/fr/docs/guide`, `/de/docs/guide`

This structure is SEO-friendly and follows web standards for internationalized sites.

## Content Object Model

Each content node in Chloroplast now includes localization information:

```csharp
public class ContentNode 
{
    public string Locale { get; set; }              // The locale of this content
    public ContentNode[] Translations { get; set; }  // All translation versions
    // ... other properties
}
```

Chloroplast automatically detects and groups translations, so you can access all language versions from any content node.

## Template Helpers

Chloroplast provides several helper methods in your Razor templates:

### URL Generation

```html
<!-- Link to a page in the current locale -->
<a href="@Href("/docs/guide")">Guide</a>

<!-- Link to a specific locale version -->
<a href="@LocaleHref("/docs/guide", "es")">Guía (Español)</a>

<!-- Get URL for current page in different locale -->
<a href="@GetLocalizedPageUrl("fr")">Français</a>
```

### Locale Information

```html
<!-- Get country flag emoji -->
@GetCountryFlag("es")  <!-- Returns 🇪🇸 -->

<!-- Get display name -->
@GetLocaleDisplayName("es")  <!-- Returns "Español" -->
```

### Current Page Information

```html
@{
    var currentLocale = CurrentLocale;  // Direct access from base template
    var supportedLocales = SiteConfig.SupportedLocales;
    var isMachineTranslated = IsMachineTranslated;  // Direct access from base template
}
```

## Machine Translation Support

Chloroplast supports marking content as machine translated to show appropriate warnings:

### Front Matter Configuration

Mark content as machine translated in the front matter:

```yaml
---
title: Mi Página
machineTranslated: true  # or machine_translated: true, or MachineTranslated: true
---
```

### Template Detection

Templates can detect machine translated content:

```html
@if (IsMachineTranslated)
{
    <div class="machine-translation-warning">
        🤖 This content was automatically translated and may contain errors.
    </div>
}
```

## Fallback Behavior

When a user requests a page that doesn't exist in their requested locale, Chloroplast:

1. Falls back to the default language version
2. Includes proper metadata indicating the canonical language
3. Provides template helpers to show translation warnings
4. Maintains proper linking to available translations

## Locale Picker

Chloroplast includes a complete locale picker component. See `LocalePickerComponent.md` for detailed implementation instructions including:

- Accessible HTML structure
- CSS styling
- JavaScript functionality
- Keyboard navigation support
- ARIA attributes for screen readers

## Sitemap Generation

The sitemap generator automatically includes all localized content with proper `hreflang` attributes:

```xml
<url>
  <loc>https://example.com/docs/guide</loc>
  <xhtml:link rel="alternate" hreflang="es" href="https://example.com/es/docs/guide"/>
  <xhtml:link rel="alternate" hreflang="fr" href="https://example.com/fr/docs/guide"/>
  <xhtml:link rel="alternate" hreflang="de" href="https://example.com/de/docs/guide"/>
</url>
```

This helps search engines understand your multilingual content structure.

## Template Base Methods

These properties and methods are directly available in all templates without needing to access specific objects:

### Localization Support

- `@CurrentLocale` - Gets the current page's locale
- `@IsMachineTranslated` - Gets whether the current page was machine translated
- `@HasTranslation("es")` - Checks if the current page has a translation in the specified locale
- `@LocaleHref("/path", "es")` - Generates a locale-aware URL
- `@GetLocalizedPageUrl("es")` - Gets the current page URL in a different language
- `@GetCountryFlag("es")` - Gets the country flag emoji for a locale (can be overridden)
- `@GetLocaleDisplayName("es")` - Gets the native display name for a locale (can be overridden)

## Translation Warnings

To show translation status, include the translation warning partial:

```html
@await PartialAsync("TranslationWarning")
```

This will automatically:
- Show a warning if content isn't available in the current locale  
- Show a machine translation notice if the content was auto-translated
- Use localized messages based on the current language

### Manual Translation Warnings

If you prefer manual control, you can use the template base methods:

```html
@if (!HasTranslation(CurrentLocale) && CurrentLocale != SiteConfig.DefaultLocale)
{
    <div class="translation-warning" role="alert">
        <strong>⚠️ Translation Notice:</strong> 
        This content is not available in @GetLocaleDisplayName(CurrentLocale). 
        You are viewing the @GetLocaleDisplayName(SiteConfig.DefaultLocale) version.
    </div>
}

@if (IsMachineTranslated)
{
    <div class="machine-translation-warning" role="alert">
        <strong>🤖 Machine Translation:</strong> 
        This content was automatically translated and may contain errors.
    </div>
}
```

## Best Practices

1. **Start with your primary language**: Define `defaultLocale` as the language you're most comfortable writing in
2. **Consistent naming**: Choose either filename or directory-based organization and stick with it
3. **Progressive translation**: You don't need to translate everything at once - pages fall back gracefully
4. **Include locale picker**: Add the locale picker to your footer or header for easy language switching
5. **Test with basePath**: If deploying to a subdirectory, test that locale URLs work correctly with your basePath configuration

## Migration from Non-localized Sites

Existing Chloroplast sites can add localization support without breaking changes:

1. Add localization configuration to `SiteConfig.yml`
2. Set `defaultLocale` to match your existing content language
3. Your existing content automatically becomes the default language
4. Add translations incrementally using your preferred naming convention
5. Update templates to include the locale picker when ready

The localization system is designed to be additive - existing sites continue to work exactly as before, with localization features available when you're ready to use them.