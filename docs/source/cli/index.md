---
template: Default
title: Command Line Interface
---

Chloroplast supports various sub commands. It is recommended to first `cd` into your project's root folder, and run the commands with defaults as appropriate. This simplifies 

# `build` sub command

The build subcommand will build a chloroplast site

```
chloroplast build --root path/to/SiteConfig/ --out path/to/out
```

## parameters

- `root`: the path to the directory that contains a `SiteConfig.yml` file
- `out`: the path to the directory where the resulting HTML will be output.
- `normalizePaths`: defaults to `true`, which normalizes all paths to lower case. Set this to false for output paths to match the source casing.

The parameters above will default to the current working directory for the root path, and an `out/` directory in that same path if both parameters are omitted. 

The build command will render all `index.md` files into corresponding `index.html` files, and copy everything else to the same corresponding location (images, styles, etc).

# `host` sub command

The host sub command starts a simple HTML web server. Useful for local preview during development or even authoring.

```
chloroplast host --out path/to/html
```

You can just press the enter key to end this task at any time. For simplicity's sake, you can also just run this command in a separate terminal window and leave it running. That way, as you rebuild the content or update 
the front-end styles, you can just refresh the browser after that buil has completed.

## parameters

- `out`: This should be the same value as the `out` parameter used in the `build` command.

The parameter above will default to the `out/` folder in the current working directory, if omitted.
