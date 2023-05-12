---
template: Default
title: Installing Chloroplast
---

## Dependencies

You must have the `dotnet` CLI installed ... to do so, you need only install _.NET Core_ on your machine. The instructions for that can be found here:

https://docs.microsoft.com/en-us/dotnet/core/install/

## Installing Chloroplast

You can acquire Chloroplast by installing it as either a [local or global 
tool](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-tool-install).

### Wilderness Labs package registry

#### Recommended

Chloroplast can be found on the public nuget.org package registry at:  
https://www.nuget.org/packages/Chloroplast.Tool/

#### Package Repository

However, you can also choose to install it through the Wilderness Labs package registry. This means you'll need to add the authenticated NuGet source as follows if you wish to do so.

```console
dotnet nuget add source https://nuget.pkg.github.com/WildernessLabs/index.json --name "wildernesslabs" --username << your github username >> --password << a personal access token >>
```

### Global

Regardless of which package registry you choose, you can install Chloroplast as a global tool.

```console
dotnet tool install Chloroplast.Tool -g
```

If you install it as a global tool, you can invoke it with the tool name
`chloroplast`.

### Local

Alternatively, you can install it as a local tool by first creating a local
tool manifest in the project folder, as follows:

```console
dotnet new tool-manifest
dotnet tool install Chloroplast.Tool
```

If you install it as a local tool, you must prefix the tool name, so you can invoke it with `dotnet chloroplast`.

### Updating

You can update to the latest release at any point by running the following command (with or without the `-g` depending on your preference):

```console
dotnet tool update Chloroplast.Tool -g
```
