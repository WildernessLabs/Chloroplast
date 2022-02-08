---
template: Default
title: API Docs
---

Chloroplast supports documenting .NET APIs.

**PLEASE NOTE:** This functionality is still in development and not fully released yet.

## Build Process
The API build process is inherently a multi-step process, for which we'll use a few different tools:

- [mdoc](https://github.com/mono/api-doc-tools/): This tool takes compiled .NET assemblies, and produces an XML format called EcmaXml, which represents all of the type and namespace metadata, and gives you an opportunity to document your APIs.
- msbuild: We are going to use msbuild to fetch mdoc itself, along with all the APIs that you need documented.
- Build Server: The glue that will drive and automate this process. In this document, we will use Github Actions to demonstrate the capabilities, but this can honestly be built with any other build pipeline such as Azure DevOps.

## Using MSBuild to Drive APIs

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
        <MdocVersion>5.8.2</MdocVersion>
        <MdocPath>$(NuGetPackageRoot)mdoc\$(MdocVersion)\tools\mdoc.exe</MdocPath>
        <OutEcmaXml>tmp/EcmaXml</OutEcmaXml>

        <ChloroplastCoreVersion>0.5.0</ChloroplastCoreVersion>
        <ChloroplastCorePath>$(NuGetPackageRoot)chloroplast.core\$(ChloroplastCoreVersion)\lib\$(TargetFramework)\Chloroplast.Core.dll</ChloroplastCorePath>
        
        <MiniRazorPath>$(NuGetPackageRoot)minirazor\2.0.3\lib\$(TargetFramework)\</MiniRazorPath>
        <ConfigurationNugetPath>$(NuGetPackageRoot)Microsoft.Extensions.Configuration.Abstractions\5.0.0\lib\netstandard2.0\</ConfigurationNugetPath>

        <ApiPaths>$(ChloroplastCorePath)</ApiPaths>
        <DependencyPaths>-L $(NuGetPackageRoot)</DependencyPaths>
    </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="mdoc" Version="$(MdocVersion)" />

    <PackageReference Include="Chloroplast.Core" Version="$(ChloroplastCoreVersion)" />

  </ItemGroup>

    <Target Name="mdocupdate" DependsOnTargets="restore">
        <Message Text="Output At: $(OutEcmaXml)" Importance="high"/>
        <Message Text="API paths: $(ApiPaths)" Importance="high"/>
        <Exec Command="$(MdocPath) update $(ApiPaths) -out $(OutEcmaXml) $(DependencyPaths) --debug" />
      </Target>
</Project>
```

## Using Github Actions to Automate Builds

```yaml
name: Content Build

on:
  workflow_dispatch:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v2

      - name: Setup .NET Core
        uses: actions/setup-dotnet@v1
        with:
          dotnet-version: 3.1.x
          
      - name: Add private GitHub registry to NuGet
        run: dotnet nuget add source https://nuget.pkg.github.com/WildernessLabs/index.json --name "wildernesslabs" --username wildlingdev --password ${{ secrets.WILDLINGDEV_PAT }} --store-password-in-clear-text
      
      - name: Build APIs with mdoc
        run: |
          cd $GITHUB_WORKSPACE
          dotnet restore mdocbuild.csproj
          msbuild mdocbuild.csproj /t:mdocupdate
      
      - name: Install Chloroplast
        run: |
          dotnet new tool-manifest
          dotnet tool install Chloroplast.Tool
        
      # Runs a set of commands using the runners shell
      - name: Run a multi-line script
        run: |
          cd $GITHUB_WORKSPACE
          ls .
          dotnet chloroplast
          ls .
          
      - name: Upload a Build Artifact
        uses: actions/upload-artifact@v2.1.3
        with:
          name: output
          path: out/*
          
      - name: Publish to Web
        if: github.ref == 'refs/heads/main'
        uses: bacongobbler/azure-blob-storage-upload@v1.2.0
        with:
          source_dir: out
          container_name: $web
          connection_string: ${{ secrets.STORAGE_CONNECTION }}
          sync: true

```

## Supporting DocXML/Triple Slash docs

WIP ...