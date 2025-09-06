---
template: Default
title: Localization
subtitle: Multi-language content, configuration, templates, and customization.
---

# Localization

Chloroplast provides first-class localization so you can build multilingual sites without duplicating structure or forcing an English-first workflow. This page consolidates how to configure, author, render, and customize localized content.

## 1. Configuration

Add localization keys in `SiteConfig.yml`:

- `defaultLocale`: The primary (authoring) language. Defaults to `en` when omitted.
- `supportedLocales`: List of all locales you want to serve. Must include the default. If omitted, Chloroplast treats the default as the only supported locale.

You can add/remove locales incrementally; untranslated pages gracefully fall back to the default language.

### File Naming & Organization
You can localize content by either:

1. Filename suffix: `guide.md`, `guide.es.md`, `guide.fr.md`
2. Sub‑directories per locale: `guide.md`, `es/guide.md`, `fr/guide.md`

Mixing styles works, but choose one for consistency when possible.

### URL Rules
- Default locale: no locale prefix (`/cli/`).
- Non‑default locale: `/{locale}/...` (e.g., `/es/cli/`).
- Base path (GitHub Pages etc.) is applied after locale logic, so `/Chloroplast/es/cli/` is valid when a base path is configured.

## 2. Rendering Model

Every page is represented by a `ContentNode` that includes:

- `Locale` – the page's locale.
- `Translations[]` – sibling translated nodes.
- `IsMachineTranslated` – set via front matter (see below).
- `IsFallback` – true when Chloroplast synthesized a locale version because no authored translation exists.

During build, Chloroplast groups translations and ensures each locale gets a resolvable URL. If a requested translation does not exist, the framework serves the default version while preserving locale context so templates can show a warning.

## 3. Front Matter Flags

Mark machine translations so users understand quality expectations:

```
---
title: Mi Página
machineTranslated: true  
---
```

Valid key variations (all equivalent): `machineTranslated`, `machine_translated`, `MachineTranslated`.

## 4. Template Helpers (Available in Razor Templates)

Common helpers exposed by the template base:

- `@CurrentLocale` – current page locale.
- `@IsMachineTranslated` – whether front matter flagged it.
- `@HasTranslation("es")` – true if a translation exists.
- `@Href("/path")` – base path aware URL.
- `@LocaleHref("/path", "es")` – locale + base path aware URL.
- `@GetLocalizedPageUrl("es")` – URL for the current page in another locale (falls back gracefully).
- `@GetCountryFlag("es")` – emoji flag (overridable method).
- `@GetLocaleDisplayName("es")` – display name (overridable method).

Use these to build pickers, cross‑links, breadcrumbs, or notices without manual path logic.

## 5. Translation Warning Partial

Add a translation/machine‑translation notice automatically by including the partial:

```razor
@await PartialAsync("TranslationWarning")
```

Behavior:
- Shows a missing translation warning when a user is viewing a fallback (`IsFallback`) for their locale.
- Shows a machine translation notice when `IsMachineTranslated` is set.
- Localized messaging: provide `TranslationWarning.{locale}.cshtml` for each supported language (an English + Spanish implementation is already included in the starter templates).

### Customizing the Warning
Copy the existing partial and modify the text/markup. If you need richer logic, replicate the minimal conditions used internally (`has translation?`, `IsMachineTranslated`) and style to your needs.

## 6. Locale Picker

Implement a user‑selectable language menu via a Razor partial (see the reference component documented separately). Key concepts:

- Iterate `SiteConfig.SupportedLocales`.
- Mark the active locale (e.g., `aria-current="page"`).
- Use `GetLocalizedPageUrl(locale)` to link each option.
- Use `GetCountryFlag(locale)` + `GetLocaleDisplayName(locale)` for concise, accessible labels.

Include the picker where appropriate:

```razor
@await PartialAsync("LocalePicker")
```

Keyboard + ARIA patterns are shown in the component reference (`LocalePickerComponent.md`).

## 7. Customizing Flags & Names

Default mappings intentionally cover only a minimal set (currently English and Spanish) and fall back to a generic globe emoji and uppercase locale code. To customize:

1. Create a derived template base OR override methods directly in a specific Razor template via an inline `@functions` / code block.
2. Override `GetCountryFlag(locale)` and/or `GetLocaleDisplayName(locale)` with your own switch / lookup table (ISO 639/3166, database, etc.).

Tip: Keep mapping logic centralized so all partials (picker, warnings, navigation) stay consistent.

### Using `locale-config.yml`

For larger sets of locales (or to avoid recompiling templates) you can define a data-driven mapping in the template folder via `locale-config.yml` (see the starter file under `templates/`). It provides a `locales:` map whose keys are locale codes and whose values specify:

- `flag` – the emoji or short marker to show.
- `displayName` – the native-language name you want rendered.

There is also a `default:` section used when a locale code is not explicitly listed (falls back to a globe + generic label by default). This file lets you:

- Add many locales without changing C# or Razor code.
- Standardize naming (e.g., choose between “Português (BR)” vs “Português”).
- Remove or replace flags with text-only markers if emojis are undesired.

#### How It’s Consumed

Templates may read and deserialize this YAML (for example in a custom base template) and then surface the values when implementing `GetCountryFlag()` / `GetLocaleDisplayName()`. If you provide both overridden methods AND a `locale-config.yml`, prefer the file’s values inside those overrides so authors can update display names without code changes.

#### Custom Strategy

1. Keep `locale-config.yml` alongside other templates so it ships with your theme.
2. In your template base override, lazy-load and cache the parsed YAML into a dictionary keyed by locale.
3. Return the config value or fall back to the built-ins and finally to the `default:` entry.
4. Optionally expose a helper (e.g., `@GetAllConfiguredLocales()`) that enumerates only those you actually want in the picker (could differ from `supportedLocales` if you stage future locales).

> You still must list every actively served locale in `supportedLocales` inside `SiteConfig.yml` for routing and build output. The `locale-config.yml` file is only for presentation layer data (flags, display names) and doesn’t control which locales get generated.

## 8. Fallback Behavior & UX

When a locale page is requested but no authored translation exists:

1. Chloroplast serves the default language.
2. `IsFallback` is true allowing a notice.
3. Picker still shows other locales; the current locale remains the requested one so the user understands context.
4. SEO: canonical and `hreflang` entries (via sitemap) reflect available translations.

## 9. Sitemaps & SEO

Generated sitemaps include `hreflang` alternates for each set of translations so search engines properly index language variants. Ensure `baseUrl` is configured for sitemap generation.

## 10. Authoring Workflow Tips

- Translate incrementally; untranslated pages work immediately via fallback.
- Prefer consistent slug names across locales to maximize automatic grouping.
- Add machine translation flags early so you remember to revisit content later.
- Keep custom locale assets (flags, names) in one code path for future additions.

## 11. Quick Checklist

- [ ] Add `defaultLocale` + `supportedLocales` to `SiteConfig.yml`
- [ ] Add / update localized markdown files (`file.es.md` or `es/file.md`)
- [ ] Include `@await PartialAsync("TranslationWarning")` and locale picker
- [ ] Provide `TranslationWarning.{locale}.cshtml` partials for each language
- [ ] Override flag/display helpers if you need custom names or icons
- [ ] Run build and validate localized URLs & sitemap

---
Need the implementation reference? See the existing component and guide files in the repo (`LocalizationGuide.md`, `LocalePickerComponent.md`).

Return to [Home](/).
