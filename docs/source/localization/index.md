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

### Manual Snippets
If you prefer not to use the partials, you can add explicit notices directly in a template:

```razor
@* Missing translation (fallback) *@
@if (IsFallback && CurrentLocale != SiteConfig.DefaultLocale)
{
	<div class="translation-warning" role="alert">
		<strong>⚠️ Translation Notice:</strong>
		This content is not available in @GetLocaleDisplayName(CurrentLocale).
		You are viewing the @GetLocaleDisplayName(SiteConfig.DefaultLocale) version.
		<a href="@GetLocalizedPageUrl(SiteConfig.DefaultLocale)" class="translation-warning-link">
			View in @GetLocaleDisplayName(SiteConfig.DefaultLocale)
		</a>
	</div>
}

@* Machine translation notice *@
@if (IsMachineTranslated)
{
	<div class="machine-translation-warning" role="alert">
		<strong>🤖 Machine Translation:</strong>
		This content was automatically translated and may contain errors.
	</div>
}
```

Suggested minimal styling (merge / adapt to your main stylesheet):

```css
.translation-warning, .machine-translation-warning {
	background: #fff3cd;
	border: 1px solid #ffeaa7;
	border-radius: 4px;
	padding: 12px;
	margin: 16px 0;
	color: #856404;
	font-size: 14px;
}
.translation-warning-link { color: #856404; text-decoration: underline; font-weight: 500; }
.translation-warning-link:hover { color: #533f03; }
```

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

### Reference Implementation
Below is an inline reference you can copy into a partial named `LocalePicker.cshtml` (the separate component doc has been inlined here for convenience):

```razor
@{
	var supportedLocales = SiteConfig.SupportedLocales;
	var currentLocale = Model?.Node?.Locale ?? SiteConfig.DefaultLocale;
}

@if (supportedLocales.Length > 1)
{
	<div class="locale-picker">
		<button type="button" class="locale-picker-toggle" aria-expanded="false" aria-haspopup="true">
			@GetCountryFlag(currentLocale) @GetLocaleDisplayName(currentLocale)
			<span class="locale-picker-arrow">▼</span>
		</button>
		<ul class="locale-picker-menu" role="menu" aria-hidden="true">
			@foreach (var locale in supportedLocales)
			{
				var isActive = locale == currentLocale;
				var localizedUrl = GetLocalizedPageUrl(locale);
				<li role="none">
					<a href="@localizedUrl" role="menuitem" class="locale-picker-item @(isActive ? "active" : "")" @(isActive ? "aria-current=\"page\"" : "")>
						@GetCountryFlag(locale) @GetLocaleDisplayName(locale)
					</a>
				</li>
			}
		</ul>
	</div>
}
```

Include the partial where desired:

```razor
@await PartialAsync("LocalePicker")
```

#### CSS (Add to your main stylesheet)
```css
.locale-picker { position: relative; display: inline-block; }
.locale-picker-toggle { background:#f8f9fa; border:1px solid #dee2e6; border-radius:4px; padding:8px 12px; cursor:pointer; display:flex; align-items:center; gap:8px; font-size:14px; color:#495057; }
.locale-picker-toggle:hover { background:#e9ecef; border-color:#adb5bd; }
.locale-picker-arrow { font-size:10px; transition: transform .2s ease; }
.locale-picker[aria-expanded="true"] .locale-picker-arrow { transform: rotate(180deg); }
.locale-picker-menu { position:absolute; top:100%; right:0; background:#fff; border:1px solid #dee2e6; border-radius:4px; box-shadow:0 2px 8px rgba(0,0,0,.1); list-style:none; margin:4px 0 0; padding:4px 0; min-width:150px; z-index:1000; display:none; }
.locale-picker[aria-expanded="true"] .locale-picker-menu { display:block; }
.locale-picker-item { display:block; padding:8px 12px; color:#495057; text-decoration:none; font-size:14px; white-space:nowrap; }
.locale-picker-item:hover { background:#f8f9fa; color:#212529; }
.locale-picker-item.active { background:#007bff; color:#fff; }
.locale-picker-item.active:hover { background:#0056b3; }
```

#### JavaScript (defer or inline near the end of `body`)
```javascript
document.addEventListener('DOMContentLoaded', () => {
  const picker = document.querySelector('.locale-picker');
  if (!picker) return;
  const toggle = picker.querySelector('.locale-picker-toggle');
  const menu = picker.querySelector('.locale-picker-menu');
  const open = () => { toggle.setAttribute('aria-expanded','true'); menu.setAttribute('aria-hidden','false'); picker.setAttribute('aria-expanded','true'); };
  const close = () => { toggle.setAttribute('aria-expanded','false'); menu.setAttribute('aria-hidden','true'); picker.setAttribute('aria-expanded','false'); };
  toggle.addEventListener('click', e => { e.preventDefault(); e.stopPropagation(); (toggle.getAttribute('aria-expanded')==='true') ? close() : open(); });
  document.addEventListener('click', e => { if (!picker.contains(e.target)) close(); });
  document.addEventListener('keydown', e => { if (e.key==='Escape') { close(); toggle.focus(); } });
  menu.addEventListener('keydown', e => {
	const items = menu.querySelectorAll('.locale-picker-item');
	const list = Array.from(items);
	const idx = list.indexOf(document.activeElement);
	if (e.key==='ArrowDown') { e.preventDefault(); list[(idx+1)%list.length].focus(); }
	else if (e.key==='ArrowUp') { e.preventDefault(); list[(idx-1+list.length)%list.length].focus(); }
	else if (e.key===' ' || e.key==='Enter') { if (document.activeElement.classList.contains('locale-picker-item')) { e.preventDefault(); document.activeElement.click(); } }
  });
});
```

Accessibility notes:
* Uses ARIA menu roles for screen reader clarity.
* Keyboard navigation (ArrowUp/Down, Enter/Space, Escape) supported.
* `aria-current="page"` marks the active locale.

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

Example excerpt:

```xml
<url>
	<loc>https://example.com/docs/guide</loc>
	<xhtml:link rel="alternate" hreflang="es" href="https://example.com/es/docs/guide" />
	<xhtml:link rel="alternate" hreflang="fr" href="https://example.com/fr/docs/guide" />
	<xhtml:link rel="alternate" hreflang="de" href="https://example.com/de/docs/guide" />
</url>
```

## 10. Authoring Workflow Tips & Best Practices

- Start with your primary language: set `defaultLocale` to the language you author in.
- Translate incrementally; untranslated pages work immediately via fallback.
- Prefer consistent slug names across locales to maximize automatic grouping.
- Choose one organization style (filename suffix OR sub-directory) for clarity.
- Include the locale picker early for user discoverability.
- Test with a configured `basePath` (e.g., GitHub Pages) to ensure prefixed locale URLs resolve.
- Add machine translation flags early so you remember to revisit content later.
- Keep custom locale assets (flags, names) centralized (helpers or `locale-config.yml`).
- Document a review process for machine-translated pages to graduate them to human-reviewed.
- Periodically audit sitemap output to verify `hreflang` integrity after adding locales.

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

## 12. Migration from Existing Non-localized Sites

You can add localization without breaking existing deployments:

1. Add `defaultLocale` and `supportedLocales` to `SiteConfig.yml` (include the default in the list).
2. Your existing content is automatically treated as the default locale.
3. Add translated files using your chosen naming strategy (`file.es.md` or `es/file.md`).
4. Introduce the locale picker & translation warning partials when ready (site builds fine without them).
5. Add machine translation flags where applicable so you can queue human review later.
6. Rebuild and confirm new localized URLs plus sitemap `hreflang` entries.

No reorganization of existing default-language content is required; localization is additive and opt-in.
