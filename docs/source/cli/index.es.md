---
template: Default
title: Interfaz de Línea de Comandos
machineTranslated: true
---

Chloroplast soporta varios subcomandos. Se recomienda primero hacer `cd` a la carpeta raíz de tu proyecto, y ejecutar los comandos con los valores predeterminados según sea apropiado.

## Subcomando `new`

El subcomando new te permite arrancar un sitio chloroplast copiando archivos a una carpeta de destino.

```
chloroplast new conceptual carpetaObjetivo
```

Esto creará `carpetaObjetivo` (si no existe ya), y copiará archivos de la plantilla `conceptual`. Luego puedes ejecutar los siguientes comandos para construir y previsualizar el sitio localmente:

```
cd carpetaObjetivo
chloroplast build
chloroplast host
```

### parámetros

- `conceptual`: El segundo parámetro posicional después de `new`.
  - Actualmente esto solo soportará `conceptual`, que será una copia de esta documentación.
  - Eventualmente tendremos más plantillas, y eventualmente abriremos a contribuciones de la comunidad.
- `carpetaObjetivo`: el tercer parámetro posicional después de `new` es el nombre de la carpeta objetivo donde se colocarán los archivos de plantilla.

## Subcomando `build`

El subcomando build construirá un sitio Chloroplast

```
chloroplast build --root ruta/a/SiteConfig/ --out ruta/a/salida
```

### parámetros

- `root`: la ruta al directorio que contiene un archivo `SiteConfig.yml`
- `out`: la ruta al directorio donde se generará el HTML resultante.
- `normalizePaths`: por defecto `true`, que normaliza todas las rutas a minúsculas. Establece esto a false para que las rutas de salida coincidan con las mayúsculas y minúsculas de origen.
- `errorLogPath`: ruta opcional para escribir información detallada de errores a un archivo de registro
- `buildVersion`: cadena de versión opcional para agregar a las referencias CSS y JavaScript para invalidar la caché. Si no se proporciona, se genera automáticamente una marca de tiempo en el formato `yyyyMMddHHmmss`.

Los parámetros anteriores utilizarán por defecto el directorio de trabajo actual para la ruta raíz, y un directorio `out/` en esa misma ruta si ambos parámetros se omiten.

El comando build renderizará todos los archivos `index.md` en archivos `index.html` correspondientes, y copiará todo lo demás a la misma ubicación correspondiente (imágenes, estilos, etc).

Además, el comando build ejecuta automáticamente la validación del sitio después de completar la construcción para verificar enlaces rotos y recursos faltantes. El comando build generará automáticamente archivos XML de sitemap si un `baseUrl` está configurado en tu `SiteConfig.yml`. Esto ayuda con el SEO y la indexación de motores de búsqueda.

### Manejo de Errores

El comando build proporciona manejo integral de errores para ayudar a diagnosticar problemas sin sobrecargar la salida de consola:

- **Salida de Consola**: Mensajes de error limpios y concisos que muestran la información esencial necesaria para solucionar problemas
- **Archivo de Registro de Errores**: Información detallada de errores incluyendo trazas completas de pila y código fuente generado

#### Configurando el Registro de Errores

Puedes configurar el registro de errores de dos maneras:

**1. Parámetro de Línea de Comandos:**
```bash
chloroplast build --errorLogPath /ruta/a/registro-errores.txt
```

**2. Configuración en SiteConfig.yml:**
```yaml
# Configuración de registro de errores
errorLogPath: logs/build-errors.log
```

#### Beneficios del Registro de Errores

- **La consola se mantiene legible**: Solo se muestra información esencial de errores en la terminal
- **Información completa de depuración**: Trazas completas de pila, código fuente generado e información detallada de excepciones se escriben al archivo de registro
- **Recolección de múltiples errores**: Todos los errores encontrados durante la construcción se recolectan y muestran juntos
- **Seguimiento de marcas de tiempo**: Cada error incluye información de marca de tiempo para solución de problemas

#### Ejemplo de Salida de Errores

**Salida de Consola:**
```
=== ERRORES DE CONSTRUCCIÓN ===
Se encontraron 1 error(es) durante la construcción:

[20:17:21] Error en 'Inicialización de plantilla': Falló la inicialización del renderizador de contenido
    Falló la compilación de plantilla:
    Error(es):
    - Template.cs(4,26): error CS1010: Nueva línea en constante
    - Template.cs(4,61): error CS1002: ; esperado

Detalles del error escritos en: /tmp/build-errors.log
```

**Contenido del Archivo de Registro de Errores:**
- Trazas completas de pila de excepciones
- Código fuente completo generado para errores de compilación de plantillas
- Rutas de archivos detalladas y contexto de procesamiento
- Marcas de tiempo para todos los errores encontrados

## Subcomando `host`

El subcomando host inicia un servidor web HTML simple. Útil para previsualización local durante el desarrollo o incluso creación. Si actualizas archivos markdown, el contenido se reconstruirá automáticamente para que solo tengas que actualizar el navegador.

El comando host ejecuta automáticamente la validación del sitio antes de iniciar el servidor web para ayudar a identificar cualquier enlace roto o recurso faltante.

```
chloroplast host --root ruta/a/raíz --out ruta/a/html 
```

Puedes simplemente presionar la tecla enter para terminar esta tarea después de detener el servidor web con Ctrl+C en cualquier momento.

Si estás actualizando plantillas razor, también puedes ejecutar este comando en una ventana de terminal separada y dejarlo ejecutándose. De esa manera, mientras ejecutas el subcomando completo `build`, el contenido y los estilos del front-end se actualizarán, y puedes simplemente actualizar el navegador después de que esa construcción se haya completado.

### parámetros

- `out`: Este debe ser el mismo valor que el parámetro `out` usado en el comando `build`.

El parámetro anterior utilizará por defecto la carpeta `out/` en el directorio de trabajo actual, si se omite.

## Subcomando `validate`

El subcomando validate verifica el sitio generado en busca de problemas comunes como enlaces rotos, archivos CSS/JavaScript faltantes e imágenes faltantes. Esto ayuda a detectar problemas de recursos y rutas temprano en el desarrollo.

```
chloroplast validate --out ruta/a/salida
```

Las verificaciones de validación incluyen:
- **Referencias CSS y JavaScript**: Asegura que todas las hojas de estilo y scripts enlazados existan
- **Enlaces de navegación**: Verifica que los enlaces internos apunten a páginas existentes
- **Imágenes**: Verifica que todas las referencias de imágenes apunten a archivos existentes
- **Enlaces de directorios**: Asegura que los enlaces de directorios tengan un archivo index.html

### parámetros

- `out`: la ruta al directorio que contiene la salida del sitio construido (por defecto es `out/` en el directorio actual)

El comando validate se ejecuta automáticamente al final del comando `build` y antes de iniciar el servidor `host`, pero también se puede ejecutar de forma independiente para verificar la salida del sitio.

## Generación de Sitemap

El comando build genera automáticamente archivos XML de sitemap cuando un `baseUrl` está configurado en tu `SiteConfig.yml`. Esto ayuda a los motores de búsqueda a descubrir e indexar el contenido de tu sitio.

### Configuración

Agrega lo siguiente a tu `SiteConfig.yml` para configurar la generación de sitemap:

```yaml
# URL base para tu sitio (requerido para la generación de sitemap)
baseUrl: https://tu-sitio.com

# Configuración opcional de sitemap
sitemap:
  enabled: true              # Habilitar/deshabilitar generación de sitemap (por defecto: true)
  maxUrlsPerSitemap: 50000   # URLs máximas por archivo de sitemap (por defecto: 50000)
```

### Comportamiento

- **Sitemap único**: Para sitios con menos URLs que el umbral configurado, se genera un único archivo `sitemap.xml`
- **Múltiples sitemaps**: Para sitios más grandes, se crean múltiples archivos de sitemap (`sitemap1.xml`, `sitemap2.xml`, etc.) con un archivo índice de sitemap (`sitemap.xml`) que los referencia
- **Filtrado de contenido**: Solo se incluyen archivos HTML en los sitemaps; recursos como CSS, imágenes y otros archivos se excluyen
- **Marcas de tiempo**: Cada entrada de URL incluye una marca de tiempo de última modificación basada en la fecha de modificación del archivo

## Invalidación de Caché

Chloroplast agrega automáticamente parámetros de versión a las referencias CSS y JavaScript para asegurar la invalidación de la caché del navegador cuando se actualizan los recursos. Esta característica ayuda a prevenir que los usuarios vean estilos o scripts obsoletos después de los despliegues.

### Configuración

Agrega lo siguiente a tu `SiteConfig.yml` para controlar el comportamiento de invalidación de caché:

```yaml
# Configuración de invalidación de caché
cacheBusting:
  enabled: true  # Habilitar/deshabilitar invalidación de caché (por defecto: true)
```

### Uso en Plantillas

En tus plantillas Razor, usa el método `WithVersion()` para agregar parámetros de versión a las URLs de recursos:

```html
<!-- CSS con invalidación de caché -->
<link href="@WithVersion("/assets/main.css")" rel="stylesheet" />

<!-- JavaScript con invalidación de caché -->
<script src="@WithVersion("/assets/site.js")" type="text/javascript"></script>
```

### Uso en Línea de Comandos

Puedes especificar una versión de construcción personalizada al ejecutar el comando build:

```bash
# Usar una versión personalizada
chloroplast build --buildVersion "v1.2.3"

# Usar versión de marca de tiempo automática (por defecto)
chloroplast build
```

### Comportamiento

- **Cuando está habilitado**: Las URLs de recursos se anexan con el parámetro `?v={version}` (ej., `/assets/main.css?v=20231215123456`)
- **Cuando está deshabilitado**: Las URLs de recursos permanecen sin cambios (ej., `/assets/main.css`)
- **Versionado automático**: Si no se especifica `--buildVersion`, se genera automáticamente una marca de tiempo en formato `yyyyMMddHHmmss`
- **Manejo de parámetros de consulta**: El parámetro de versión se anexa correctamente con `&` si la URL ya contiene parámetros de consulta