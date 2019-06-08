using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Description;

namespace Mercato.AspNet.OData.DataTableExtension
{
    /// <summary>
    /// Adds OData parameters to the Swagger documentation for the given operation
    /// Taken from https://stackoverflow.com/questions/41973356/is-there-a-way-to-get-swashbuckle-to-add-odata-parameters-to-web-api-2-iqueryabl
    /// </summary>
    public class ODataParametersSwaggerDefinition : IOperationFilter
    {
        private static readonly Type QueryableType = typeof(IQueryable);

        /// <summary>
        /// Apply the filter to the operation.
        /// </summary>
        /// <param name="operation">The API operation to check.</param>
        /// <param name="schemaRegistry">The swagger schema registry.</param>
        /// <param name="apiDescription">The description of the api method.</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            Type responseType = apiDescription.ResponseType();

            if (responseType.GetInterfaces().Any(i => i == QueryableType))
            {
                AppendOdataParametersToOperation(operation);
            }
        }

        public static void AppendOdataParametersToOperation(Operation operation)
        {
            if (operation.parameters == null)
            {
                operation.parameters = new List<Parameter>();
            }

            operation.parameters.Add(new Parameter
            {
                name = "$filter",
                description = "Filter the results using OData syntax.",
                required = false,
                type = "string",
                @in = "query"
            });

            operation.parameters.Add(new Parameter
            {
                name = "$orderby",
                description = "Order the results using OData syntax.",
                required = false,
                type = "string",
                @in = "query"
            });

            operation.parameters.Add(new Parameter
            {
                name = "$skip",
                description = "The number of results to skip.",
                required = false,
                type = "integer",
                @in = "query"
            });

            operation.parameters.Add(new Parameter
            {
                name = "$top",
                description = "The number of results to return.",
                required = false,
                type = "integer",
                @in = "query"
            });

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
