# Epam.GraphQL
![Licence](https://img.shields.io/github/license/epam/epam-graphql.svg)

[![Build artifacts](https://github.com/epam/epam-graphql/actions/workflows/build.yml/badge.svg)](https://github.com/epam/epam-graphql/actions/workflows/build.yml)
[![Publish code](https://github.com/epam/epam-graphql/actions/workflows/publish.yml/badge.svg)](https://github.com/epam/epam-graphql/actions/workflows/publish.yml)

![Size](https://img.shields.io/github/repo-size/epam/epam-graphql.svg)
![Activity](https://img.shields.io/github/commit-activity/w/epam/epam-graphql)
![Activity](https://img.shields.io/github/commit-activity/m/epam/epam-graphql)
![Activity](https://img.shields.io/github/commit-activity/y/epam/epam-graphql)

## Overview

**Epam.GraphQL** is a set of .NET libraries which provides high-level way for building GraphQL APIs with a few lines of code, including (but not limited to) CRUD, batching, complex sorting and filtering, pagination.
We have built **Epam.GraphQL** on top of [GraphQL.NET](https://github.com/graphql-dotnet/graphql-dotnet/) to simplify developing GraphQL API layer:
  * It is used by a dozen internal EPAM applications, battle-tested on complex tasks
  * Highly declarative; can be seen as Low-Code platform done right
  * Serves as architecture backbone for the whole app
  * Makes APIs aligned and metadata-rich – allowing future features like admin UI generation
 
## Features

* Supports EntityFrameworkCore (e.g. querying only necessary fields from the database, disable change tracking when needed)
* Declarative CRUD (but you can write your own manual mutations as well)
* Gracefully solves [N+1 problem](https://medium.com/the-marcy-lab-school/what-is-the-n-1-problem-in-graphql-dd4921cb3c1a) by nested entities query batching with zero boilerplate code (but you can write your batches by yourself)
* [Relay connections](https://relay.dev/graphql/connections.htm) out-of-the-box
* Declarative filtering and search
* Declarative sorting
* Entity automapping
* Master-details relationship between entities with a few lines of code
* Security
* ... and many more

## Documentation

* [Get Started](docs/01-get-started.md)
* Entity Framework Core Usage
  * [Automapping](docs/02-ef-core/01.automapping.md)

## Packages

| Package                          | Downloads                                                                                                                                     | NuGet Latest                                                                                                                                 |
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------- |
| Epam.GraphQL                     | [![Nuget](https://img.shields.io/nuget/dt/Epam.GraphQL)](https://www.nuget.org/packages/Epam.GraphQL)                                         | [![Nuget](https://img.shields.io/nuget/v/Epam.GraphQL)](https://www.nuget.org/packages/Epam.GraphQL)                                         |
| Epam.GraphQL.EntityFrameworkCore | [![Nuget](https://img.shields.io/nuget/dt/Epam.GraphQL.EntityFrameworkCore)](https://www.nuget.org/packages/Epam.GraphQL.EntityFrameworkCore) | [![Nuget](https://img.shields.io/nuget/v/Epam.GraphQL.EntityFrameworkCore)](https://www.nuget.org/packages/Epam.GraphQL.EntityFrameworkCore) |
| Epam.GraphQL.MiniProfiler        | [![Nuget](https://img.shields.io/nuget/dt/Epam.GraphQL.MiniProfiler)](https://www.nuget.org/packages/Epam.GraphQL.MiniProfiler)               | [![Nuget](https://img.shields.io/nuget/v/Epam.GraphQL.MiniProfiler)](https://www.nuget.org/packages/Epam.GraphQL.MiniProfiler)               |
| Epam.GraphQL.SystemTextJson      | [![Nuget](https://img.shields.io/nuget/dt/Epam.GraphQL.SystemTextJson)](https://www.nuget.org/packages/Epam.GraphQL.SystemTextJson)           | [![Nuget](https://img.shields.io/nuget/v/Epam.GraphQL.SystemTextJson)](https://www.nuget.org/packages/Epam.GraphQL.SystemTextJson)           |
| Epam.GraphQL.NewtonsoftJson      | [![Nuget](https://img.shields.io/nuget/dt/Epam.GraphQL.NewtonsoftJson)](https://www.nuget.org/packages/Epam.GraphQL.NewtonsoftJson)           | [![Nuget](https://img.shields.io/nuget/v/Epam.GraphQL.NewtonsoftJson)](https://www.nuget.org/packages/Epam.GraphQL.NewtonsoftJson)           |


## License
[MIT](LICENSE.md)