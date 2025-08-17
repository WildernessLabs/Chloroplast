---
template: Default
title: Plantillas
machineTranslated: true
---

Las plantillas de Chloroplast están construidas usando las plantillas Razor de ASP.NET.

# Configuración

El archivo `SiteConfig.yml` te permite configurar la ubicación de las plantillas y las carpetas para encontrar archivos de contenido para procesar.

```yaml
# plantillas razor
templates_folder: templates

areas:
  - source_folder: /source
    output_folder: /
```

# Front matter de Markdown

La plantilla será definida por el front matter del contenido.

```
---
template: NombrePlantilla
title: Título del documento
---
```

Si este valor es incorrecto, u omitido, buscará por defecto una plantilla llamada `Default.cshtml`. Se asume que estas plantillas son solo para el área de contenido en sí misma.

# SiteFrame.cshtml

Para el chrome principal del sitio, Chloroplast buscará una plantilla llamada `SiteFrame.cshtml` ... esto renderizará

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

# Personalizando Plantillas

Al diseñar plantillas personalizadas para Chloroplast, enfócate en aprovechar los métodos de plantilla disponibles y asegurar que tus plantillas manejen efectivamente toda la gama de características de Chloroplast.

## Referencia de Estructura de Plantillas

Para la estructura de plantillas recomendada y patrones, consulta las plantillas incluidas:

- **SiteFrame.cshtml** - La plantilla principal del chrome del sitio que proporciona la estructura general de la página, navegación y carga de recursos
- **Default.cshtml** - La plantilla de contenido predeterminada que demuestra el diseño del área de contenido y el manejo de metadatos

Estas plantillas sirven como la estructura recomendada de facto y demuestran las mejores prácticas para:
- Usar `@Href()` para enlaces de navegación interna con manejo de BasePath
- Usar `@Asset()` para referencias de recursos con invalidación automática de caché
- Manejar metadatos de contenido con `@Model.GetMeta()`
- Renderizar contenido con `@Raw(Model.Body)`
- Trabajar con la colección `@Model.Headers` para tabla de contenidos

# Propiedades y Métodos de Plantilla

Disponibles para todas las plantillas están las siguientes funciones:

- `@Raw("<alguna cadena>")`
  - el método `Raw` evitará la codificación HTML del contenido (que es el predeterminado).
- `@Model.Body`
  - Este es el contenido HTML principal que se está renderizando en esta plantilla. Debes generar esto a través del método `Raw` ya que probablemente contendrá HTML.
- `@Model.GetMeta("title")`
  - con esto puedes acceder a cualquier valor en el front matter (por ejemplo, `title`).
- `@await PartialAsync("NombrePlantilla", "valor del modelo")`
  - Esto te permite renderizar sub plantillas.
  - el parámetro del nombre de plantilla coincidirá con el nombre del archivo razor... así que en el ejemplo anterior, estaría buscando un archivo llamado `Templates/NombrePlantilla.cshtml`
  - el segundo parámetro puede ser cualquier tipo de objeto realmente, lo que sea que la plantilla en cuestión esté esperando.
- Colección `@Model.Headers`
  - Cada `h1`-`h6` será parseado y agregado a esta colección de headers.
  - Cada objeto `Header` tiene las siguientes propiedades:
    - Level: el valor numérico del header
    - Value: el texto
    - Slug: una versión slug del `Value`, que corresponde a un ancla agregada al HTML antes del header.
- `@await PartialAsync("Docs/area/menu.md")`
  - Esta sobrecarga del método `PartialAsync` asume que estás renderizando un archivo markdown.
  - El archivo markdown puede definir su propia plantilla (por defecto será `Default`), y solo renderizará los contenidos de esa plantilla específica (es decir, no `SiteFrame.cshtml`).

## Métodos de URL y Recursos

Para generar URLs y referencias de recursos en tus plantillas:

- `@Href("/ruta/a/pagina")`
  - Prefija una URL relativa al sitio con el BasePath configurado
  - Usa esto para enlaces de navegación interna del sitio
  - Ejemplo: `<a href="@Href("/about")">Acerca de</a>`
- `@Asset("/ruta/a/recurso.css")`
  - Construye una URL de recurso con BasePath e invalidación automática de caché con versión
  - Combina tanto la aplicación de BasePath como la adición de parámetro de versión
  - **Importante**: No agregues manualmente parámetros de consulta a los resultados de Asset(), ya que esto entrará en conflicto con el parámetro de versión automático
  - Ejemplo: `<link href="@Asset("/css/main.css")" rel="stylesheet" />`

### Ejemplos de Métodos de URL

```html
<!-- Enlaces de navegación usando Href -->
<nav>
    <a href="@Href("/")">Inicio</a>
    <a href="@Href("/docs")">Documentación</a>
    <a href="@Href("/about")">Acerca de</a>
</nav>

<!-- Referencias de recursos usando Asset (preferido para CSS/JS) -->
<link href="@Asset("/css/main.css")" rel="stylesheet" />
<script src="@Asset("/js/app.js")"></script>

<!-- Enfoque manual usando Href + WithVersion para más control -->
<link href="@WithVersion(Href("/css/main.css"))" rel="stylesheet" />
```

**⚠️ Advertencia de Conflicto de Invalidación de Caché**: Al usar `Asset()`, no agregues manualmente parámetros de consulta o llames `WithVersion()` en el resultado, ya que `Asset()` ya incluye invalidación de caché. Esto resultaría en URLs malformadas como `/basePath/assets/script/samples?v=123/mysample.js`.

## Métodos de Invalidación de Caché

Para manejar el versionado de recursos e invalidación de caché:

- `@WithVersion("/ruta/a/recurso.css")`
  - Agrega un parámetro de versión a las URLs de recursos para invalidación de caché (ej., `/ruta/a/recurso.css?v=20231215123456`)
  - Solo agrega el parámetro de versión cuando la invalidación de caché está habilitada en la configuración
  - Maneja correctamente los parámetros de consulta existentes usando `&` en lugar de `?` cuando es apropiado
- `@BuildVersion`
  - Obtiene la cadena de versión de construcción actual, o `null` si la invalidación de caché está deshabilitada
  - Útil para mostrar información de versión o lógica personalizada de invalidación de caché

### Ejemplo de Invalidación de Caché

```html
<!-- Enfoque recomendado usando WithVersion -->
<link href="@WithVersion("/assets/main.css")" rel="stylesheet" />
<script src="@WithVersion("/assets/site.js")"></script>

<!-- Enfoque manual usando BuildVersion -->
@if (BuildVersion != null)
{
    <link href="/assets/main.css?v=@BuildVersion" rel="stylesheet" />
}
else
{
    <link href="/assets/main.css" rel="stylesheet" />
}
```