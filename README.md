# Kitchen Inventory
A selfhosted site for managing the contents of your kitchen

## Development

Kitchen Inventory can be developed using a few potential IDEs (Integrated development environment) depending on your preference. Due to the project's current architecture binding redirects are not much of a concern.

> Note: All setups will require installing the [.NET 8.0 SDK (current LTS as of 5/2024)](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to support building the codebase.

### Supported IDES

The following IDEs support .NET development but also have different tradeoffs depending on your needs.

- [Jetbrains Rider](https://www.jetbrains.com/rider/)
- [Visual Studio 2022 with ASP.NET workload](https://visualstudio.microsoft.com/)
- [Visual Studio Code](https://code.visualstudio.com/)

`Visual Studio Code`, also referred to as `VSCode`, is crossplatform and the lighest IDE option but offers less intellisense help than the other two options.
`Visual Studio 2022` offers the most rich intellisense and feature support while only supporting Windows development and is commonly paired with tools such as [ReSharper](https://www.jetbrains.com/resharper/).
`JetBrains Rider` is a crossplatform IDE that supports a majority of the checks and features Visual Studio provides but is not an official Microsoft tool so it can sometimes miss the newest features. Rider does also provide the types of features Resharper is used for natively in the IDE.