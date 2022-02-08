# Chloroplast
Wilderness Labs docs engine that converts and hosts markdown and (soon) XML API docs into HTML.

## Getting Started

First [Install](/docs/source/Installing/index.md) the `choloroplast.tool` dotnet tool.

Then run the following commands to: create a new chloroplast project at `targetFolder`, then build it, and run the local chloroplast webserver to preview the docs.

```
chloroplast new conceptual targetFolder

cd targetFolder

chloroplast build
chloroplast host
```

Once you're done with that, you can modify that default template, and [read the docs](/docs/).
