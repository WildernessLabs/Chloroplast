---
template: Default
title: Command Line Interface
---

Chloroplast supports various sub commands. It is recommended to first `cd` into your project's root folder, and run the commands with defaults as appropriate.

## `new` sub command

The new subcommand allows you to bootstrap a chloroplast site by 
copying files to a destination folder.

```
chloroplast new conceptual targetFolder
```

This will create `targetFolder` (if it doesn't already exist), and 
copy files from the `conceptual` template. You can then run the
following commands to build and preview the site locally:

```
cd targetFolder
chloroplast build
chloroplast host
```

### parameters

- `conceptual`: The second positional parameter after `new`.
  - This will currently only support `conceptual`, which will be a copy of these docs.
  - Eventually we will have more templates, and eventually open up to community contributions.
- `targetFolder`: the third positional parameter after `new` is the name of the target folder where the template files will be placed.

## `build` sub command

The build subcommand will build a chloroplast site

```
chloroplast build --root path/to/SiteConfig/ --out path/to/out
```

### parameters

- `root`: the path to the directory that contains a `SiteConfig.yml` file
- `out`: the path to the directory where the resulting HTML will be output.
- `normalizePaths`: defaults to `true`, which normalizes all paths to lower case. Set this to false for output paths to match the source casing.

The parameters above will default to the current working directory for the root path, and an `out/` directory in that same path if both parameters are omitted. 

The build command will render all `index.md` files into corresponding `index.html` files, and copy everything else to the same corresponding location (images, styles, etc).

## `host` sub command

The host sub command starts a simple HTML web server. Useful for local preview during development or even authoring. If you update markdown files, the content will automatically be rebuilt so you can just refresh the browser.

```
chloroplast host --root path/to/root --out path/to/html 
```

You can just press the enter key to end this task after stopping the web host with ctrl+c at any time. 

If you are updating razor templates, you can also just run this command in a separate terminal window and leave it running. That way, as you run the full `build` subcommand, the content and front-end styles will update, and you can just refresh the browser after that build has completed.

### parameters

- `out`: This should be the same value as the `out` parameter used in the `build` command.

The parameter above will default to the `out/` folder in the current working directory, if omitted.
