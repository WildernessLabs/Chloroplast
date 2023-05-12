# Chloroplast
Wilderness Labs docs engine that converts and hosts Markdown and (soon) XML API docs into HTML.

## Getting Started

First [Install](/docs/source/Installing/index.md) the `choloroplast.tool` dotnet tool.

Then run the following commands to: create a new Chloroplast project at `targetFolder`, then build it, and run the local Chloroplast webserver to preview the docs.

```
chloroplast new conceptual targetFolder

cd targetFolder

chloroplast build
chloroplast host
```

Once you're done with that, you can modify that default template, and [read the docs](/docs/).

## Host the Chloroplast docs

If you want to see Chloroplast in action, you can even host the Chloroplast docs themselves.

1. Clone the Chloroplast repo.

    ```
    git clone https://github.com/WildernessLabs/Chloroplast.git
    cd Chloroplast
    ```

1. Install Chloroplast.

    There are [several ways to install Chloroplast](https://github.com/WildernessLabs/Chloroplast/blob/main/docs/source/Installing/index.md#package-repository). This will install it from NuGet as a global tool.

    ```
    dotnet tool install Chloroplast.Tool --global
    ```

1. Navigate to the Chloroplast docs.

    ```
    cd docs
    ```

1. Build and host the docs.

    ```
    chloroplast build
    chloroplast host
    ```

    This will build and then start hosting the docs from `localhost:5000`, launching a browser to preview them.
