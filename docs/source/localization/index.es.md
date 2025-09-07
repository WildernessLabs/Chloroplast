---
template: Default
title: Localización
subtitle: Contenido multilenguaje, configuración, plantillas y personalización.
machineTranslated: true
---

# Localización

Esta página resume cómo configurar, crear, renderizar y personalizar contenido multilenguaje en Chloroplast sin repetir estructura.

## 1. Configuración

Agrega `defaultLocale` y `supportedLocales` en `SiteConfig.yml`. Si `supportedLocales` se omite, solo se usa el idioma predeterminado. Las páginas sin traducción usan fallback automáticamente.

## 2. Modelo de Renderizado

Cada página (`ContentNode`) tiene: `Locale`, `Translations[]`, `IsMachineTranslated` y `IsFallback` (cuando se sirve el contenido base porque falta una traducción).

## 3. Front Matter

Marca traducciones automáticas:
```
---
machineTranslated: true
---
```
Variantes de nombre son aceptadas.

## 4. Helpers en Plantillas

`@CurrentLocale`, `@IsMachineTranslated`, `@HasTranslation("es")`, `@Href()`, `@LocaleHref()`, `@GetLocalizedPageUrl()`, `@GetCountryFlag()`, `@GetLocaleDisplayName()`.

## 5. Avisos de Traducción

Incluye:
```razor
@await PartialAsync("TranslationWarning")
```
Personaliza creando `TranslationWarning.{locale}.cshtml`.

## 6. Selector de Idioma

Usa un parcial tipo `LocalePicker` que itere `SiteConfig.SupportedLocales` y construya enlaces con `GetLocalizedPageUrl(locale)` + banderas y nombres.

## 7. Personalizar Banderas / Nombres

Sobrescribe `GetCountryFlag` y `GetLocaleDisplayName` para más locales o iconografía distinta.

### Uso de `locale-config.yml`

Para muchos idiomas o para evitar editar código, define un archivo `locale-config.yml` (en la carpeta de plantillas) con un mapa `locales:` donde cada código tiene:

- `flag` – emoji o marcador.
- `displayName` – nombre nativo a mostrar.

La sección `default:` provee valores por omisión cuando no hay coincidencia. Ventajas:

- Agregar locales sin recompilar.
- Unificar nombres (ej. variantes regionales).
- Sustituir emojis por texto si se requiere.

#### Consumo

Tu base de plantillas puede cargar este YAML y usarlo dentro de overrides de `GetCountryFlag()` / `GetLocaleDisplayName()`. Si existen ambos (código + archivo), prioriza datos del archivo para permitir ajustes sin despliegue de binarios.

#### Estrategia

1. Mantener `locale-config.yml` junto a las demás plantillas.
2. Cargar/almacenar en caché en un diccionario por código.
3. Devolver valor configurado o fallback (`default:` / implementación interna).
4. Exponer un helper opcional para enumerar solo locales visibles.

> Aún debes listar los locales activos en `supportedLocales` del `SiteConfig.yml`. El archivo `locale-config.yml` solo controla presentación (banderas, nombres) y no activa la generación de contenido.

## 8. Fallback

Si una traducción falta: se sirve el idioma base, `IsFallback` = true y puedes mostrar aviso. URLs siguen incluyendo el código solicitado para contexto.

## 9. SEO

El sitemap incluye `hreflang` para cada conjunto de traducciones (requiere `baseUrl`).

## 10. Flujo de Trabajo

- Traducir progresivamente.
- Mantener slugs consistentes.
- Marcar traducciones automáticas.
- Centralizar banderas/nombres.

## 11. Lista Rápida

- [ ] Configurar locales en `SiteConfig.yml`
- [ ] Agregar archivos `.es.md` u otras carpetas por idioma
- [ ] Agregar aviso y selector de idioma
- [ ] Añadir parciales de aviso por idioma
- [ ] Personalizar banderas/nombres si es necesario

Volver al [Inicio](/).
