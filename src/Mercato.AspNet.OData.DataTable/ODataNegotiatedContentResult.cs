using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

using System.Threading.Tasks;

namespace Mercato.AspNet.OData.DataTableExtension
{
    public class ODataReturnNegotiatedContentResult : ODataNegotiatedContentResult<ODataReturn>
    {
        public ODataReturnNegotiatedContentResult(ODataReturn content)
            : base(content)
        { }
    }

    public class ODataNegotiatedContentResult<T> : OkObjectResult
    {
        public ODataNegotiatedContentResult(T content)
        : base(content) { }


        public const string ODataServiceVersionHeader = "OData-Version";

        public override void ExecuteResult(ActionContext context)
        {
            if (context.HttpContext.Request.Headers.ContainsKey("Accept"))
            {
                context.HttpContext.Request.Headers.Remove("Accept");
            }

            context.HttpContext.Request.Headers.Append("Accept", "application/json");

            context.HttpContext.Response.Headers.Append(
                ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(ODataVersion.V4));

            base.ExecuteResult(context);
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            if (context.HttpContext.Request.Headers.ContainsKey("Accept"))
            {
                context.HttpContext.Request.Headers.Remove("Accept");
            }

            context.HttpContext.Request.Headers.Append("Accept", "application/json");

            context.HttpContext.Response.Headers.Append(
                ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(ODataVersion.V4));

            await base.ExecuteResultAsync(context);
        }
    }
}
