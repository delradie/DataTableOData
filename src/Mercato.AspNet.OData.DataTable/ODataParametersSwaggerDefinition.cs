using Microsoft.OpenApi;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Mercato.AspNet.OData.DataTableExtension
{
    /// <summary>
    /// Adds OData parameters to the Swagger documentation for the given operation
    /// Taken from https://stackoverflow.com/questions/41973356/is-there-a-way-to-get-swashbuckle-to-add-odata-parameters-to-web-api-2-iqueryabl
    /// Updated for aspnetcore
    /// </summary>
    public class ODataParametersSwaggerDefinition
    {
        private static readonly Type QueryableType = typeof(IQueryable);

        /// <summary>
        /// Simple method to append OData parameters to the target operation
        /// </summary>
        /// <param name="operation">Operation object to add parameter information to</param>
        public static void AppendOdataParametersToOperation(OpenApiOperation operation)
        {
            if (operation.Parameters == null)
            {
                operation.Parameters = new List<IOpenApiParameter>();
            }

            if (!operation.Parameters.Any(x => String.Equals(x.Name, "$filter", StringComparison.InvariantCultureIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$filter",
                    Description = "Filter the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                    }
                });
            }

            if (!operation.Parameters.Any(x => String.Equals(x.Name, "$orderby", StringComparison.InvariantCultureIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$orderby",
                    Description = "Order the results using OData syntax.",
                    Required = false,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                    }
                });
            }

            if (!operation.Parameters.Any(x => String.Equals(x.Name, "$skip", StringComparison.InvariantCultureIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$skip",
                    Description = "The number of results to skip.",
                    Required = false,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }

            if (!operation.Parameters.Any(x => String.Equals(x.Name, "$top", StringComparison.InvariantCultureIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$top",
                    Description = "The number of results to return.",
                    Required = false,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }

            if (!operation.Parameters.Any(x => String.Equals(x.Name, "$select", StringComparison.InvariantCultureIgnoreCase)))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = "$select",
                    Description = "Specify the subset of properties to be included in the response.",
                    Required = false,
                    In = ParameterLocation.Query,
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String
                    }
                });
            }

            //Hidden as this is not currently implemented
            //operation.parameters.Add(new Parameter
            //{
            //    name = "$count",
            //    description = "Return the total count.",
            //    required = false,
            //    type = "boolean",
            //    @in = "query"
            //});

        }
    }
}
