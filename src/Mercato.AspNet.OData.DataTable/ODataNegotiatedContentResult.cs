using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Mercato.AspNet.OData.DataTableExtension
{
    public class ODataReturnNegotiatedContentResult : ODataNegotiatedContentResult<ODataReturn>
    {
        public ODataReturnNegotiatedContentResult(ODataReturn content, ControllerBase controller)
            : base(content, controller)
        { }

        public ODataReturnNegotiatedContentResult(ODataReturn content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
            : base(content, contentNegotiator, request, formatters)
        { }
    }

    public class ODataNegotiatedContentResult<T> : OkNegotiatedContentResult<T>
    {
        public ODataNegotiatedContentResult(T content, ControllerBase controller)
        : base(content, controller) { }

        public ODataNegotiatedContentResult(T content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
     : base(content, contentNegotiator, request, formatters) { }


        public const string ODataServiceVersionHeader = "OData-Version";

        public override async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            base.Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            HttpResponseMessage response = await base.ExecuteAsync(cancellationToken);

            response.Headers.TryAddWithoutValidation(
                ODataServiceVersionHeader,
                ODataUtils.ODataVersionToString(ODataVersion.V4));

            return response;
        }
    }
}
