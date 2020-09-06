# Chloroplast Tool

usage:

```
build --root path/to/docs/root --out path/to/out
```

_note_: `root` should contain a `SiteConfig.yml`

## Testing in Visual Studio

During development, it is convenient to set startup arguments in the project startup settings. You can use the `Source/Chloroplast/Chloroplast_Sample` sample content for testing and set a value like this (adjusted for your local path):

```
build --root ~/dev/meadow/Chloroplast/Source/Chloroplast/Chloroplast_Sample --out ~/dev/meadow/Chloroplast/out --force=true
```

The `--force=true` forces a rebuild of every piece of content every time, which is convenient while making template or other pipeline changes, but otherwise not necessary.

## Hosting 

You can run a local webserver in order to test the results of your work. With this repository fetched, you can easily run this with the following command in the repo root.

```
dotnet run --project toolsrc/Chloroplast.Tool/ -- host --out ~/dev/meadow/Chloroplast/out
```