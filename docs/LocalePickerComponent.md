# Locale Picker Component

This is a sample Razor partial template that demonstrates how to implement a locale picker for Chloroplast sites with localization support.

## Usage

Create a file called `LocalePicker.cshtml` in your templates directory with the following content:

```html
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
                    <a href="@localizedUrl" 
                       role="menuitem" 
                       class="locale-picker-item @(isActive ? "active" : "")"
                       @(isActive ? "aria-current=\"page\"" : "")>
                        @GetCountryFlag(locale) @GetLocaleDisplayName(locale)
                    </a>
                </li>
            }
        </ul>
    </div>
}
```

## CSS Styling

Add this CSS to style the locale picker:

```css
.locale-picker {
    position: relative;
    display: inline-block;
}

.locale-picker-toggle {
    background: #f8f9fa;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    padding: 8px 12px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 14px;
    color: #495057;
    text-decoration: none;
}

.locale-picker-toggle:hover {
    background: #e9ecef;
    border-color: #adb5bd;
}

.locale-picker-arrow {
    font-size: 10px;
    transition: transform 0.2s ease;
}

.locale-picker[aria-expanded="true"] .locale-picker-arrow {
    transform: rotate(180deg);
}

.locale-picker-menu {
    position: absolute;
    top: 100%;
    right: 0;
    background: white;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    list-style: none;
    margin: 4px 0 0 0;
    padding: 4px 0;
    min-width: 150px;
    z-index: 1000;
    display: none;
}

.locale-picker[aria-expanded="true"] .locale-picker-menu {
    display: block;
}

.locale-picker-item {
    display: block;
    padding: 8px 12px;
    color: #495057;
    text-decoration: none;
    font-size: 14px;
    white-space: nowrap;
}

.locale-picker-item:hover {
    background: #f8f9fa;
    color: #212529;
}

.locale-picker-item.active {
    background: #007bff;
    color: white;
}

.locale-picker-item.active:hover {
    background: #0056b3;
}
```

## JavaScript

Add this JavaScript to make the locale picker interactive:

```javascript
document.addEventListener('DOMContentLoaded', function() {
    const localePicker = document.querySelector('.locale-picker');
    if (!localePicker) return;

    const toggle = localePicker.querySelector('.locale-picker-toggle');
    const menu = localePicker.querySelector('.locale-picker-menu');

    function openMenu() {
        toggle.setAttribute('aria-expanded', 'true');
        menu.setAttribute('aria-hidden', 'false');
        localePicker.setAttribute('aria-expanded', 'true');
    }

    function closeMenu() {
        toggle.setAttribute('aria-expanded', 'false');
        menu.setAttribute('aria-hidden', 'true');
        localePicker.setAttribute('aria-expanded', 'false');
    }

    toggle.addEventListener('click', function(e) {
        e.preventDefault();
        e.stopPropagation();
        
        if (toggle.getAttribute('aria-expanded') === 'true') {
            closeMenu();
        } else {
            openMenu();
        }
    });

    // Close menu when clicking outside
    document.addEventListener('click', function(e) {
        if (!localePicker.contains(e.target)) {
            closeMenu();
        }
    });

    // Close menu on escape key
    document.addEventListener('keydown', function(e) {
        if (e.key === 'Escape') {
            closeMenu();
            toggle.focus();
        }
    });

    // Handle keyboard navigation
    menu.addEventListener('keydown', function(e) {
        const items = menu.querySelectorAll('.locale-picker-item');
        const currentIndex = Array.from(items).indexOf(document.activeElement);

        switch (e.key) {
            case 'ArrowDown':
                e.preventDefault();
                const nextIndex = currentIndex < items.length - 1 ? currentIndex + 1 : 0;
                items[nextIndex].focus();
                break;
            case 'ArrowUp':
                e.preventDefault();
                const prevIndex = currentIndex > 0 ? currentIndex - 1 : items.length - 1;
                items[prevIndex].focus();
                break;
            case 'Enter':
            case ' ':
                e.preventDefault();
                if (document.activeElement.classList.contains('locale-picker-item')) {
                    document.activeElement.click();
                }
                break;
        }
    });
});
```

## Include in Template

To include the locale picker in your main template (e.g., in the footer), use:

```html
@await PartialAsync("LocalePicker")
```

## Warning for Untranslated Content

If you want to show a warning when content is not localized, you can add this to your main content template:

```html
@{
    var currentLocale = Model?.Node?.Locale ?? SiteConfig.DefaultLocale;
    var defaultLocale = SiteConfig.DefaultLocale;
    var isTranslated = currentLocale == defaultLocale || 
                      (Model?.Node?.Translations?.Any(t => t.Locale == currentLocale) ?? false);
}

@if (!isTranslated && currentLocale != defaultLocale)
{
    <div class="translation-warning" role="alert">
        <strong>⚠️ Translation Notice:</strong> 
        This content is not available in @GetLocaleDisplayName(currentLocale). 
        You are viewing the @GetLocaleDisplayName(defaultLocale) version.
        <a href="@GetLocalizedPageUrl(defaultLocale)" class="translation-warning-link">
            View in @GetLocaleDisplayName(defaultLocale)
        </a>
    </div>
}
```

Add CSS for the warning:

```css
.translation-warning {
    background: #fff3cd;
    border: 1px solid #ffeaa7;
    border-radius: 4px;
    padding: 12px;
    margin: 16px 0;
    color: #856404;
    font-size: 14px;
}

.translation-warning-link {
    color: #856404;
    text-decoration: underline;
    font-weight: 500;
}

.translation-warning-link:hover {
    color: #533f03;
}
```

This provides a complete, accessible locale picker component with proper keyboard navigation, ARIA attributes, and visual feedback for users.