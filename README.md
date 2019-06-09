﻿﻿﻿﻿﻿Mercato.AspNet.OData.DataTable
=========

Adds support for [OData](https://odata.github.io/) query syntax over ADO.Net [DataTables](https://docs.microsoft.com/en-us/dotnet/api/system.data.datatable) in WebApi projects.

This is an initial implementation to allow for core functionality in cases where there is no fixed structure for the datatable at design time, meaning that use of the standard Microsoft.AspNet.OData nuget is precluded by the lack of a fixed EdmModel or underlying Entity source.

The implementation as it stands currently supports the following elements of the OData syntax:

* $filter - supporting all standard operators with the exception of has
* $select - not including support for $expand as there is no concept of related entities
* $top
* $skip
* $orderby

At the time of writing there is no support for functions implemented

## Usage ##

### Getting Started ###

    Install-Package Mercato.AspNet.OData.DataTableExtension

This will install the standard Microsoft OData dependencies

### OData Initialisation ###

To correctly intitialise the OData parsing for your API project add the following to your startup:

```csharp
using Microsoft.AspNet.OData.Extensions;

...

        public static void Register(HttpConfiguration config)
        {
            config.EnableDependencyInjection();

			...
        }


```

In your controller, once you have your populated DataTable:

```csharp
using Mercato.AspNet.OData.DataTableExtension;
...

            DataTable BaseData = ....;

            DataTable Output = BaseData.ApplyODataQuery(Request);
```

##Swagger Support##


The class ODataParametersSwaggerDefinition adds support for indicating the supported OData parameters in OpenApi (Swagger) documentation using [Swashbuckle](https://github.com/domaindrivendev/Swashbuckle).

The class implements the IOperationFilter interface to allow use according to the Swashbuckle documentation. In addition a static method is exposed to allow for direct addition of the parameters if building the operation representations manually.

```csharp
using Mercato.AspNet.OData.DataTableExtension;

…

            Swashbuckle.Swagger.Operation TargetOperation = ….;
            ODataParametersSwaggerDefinition.AppendOdataParametersToOperation(TargetOperation);
```
