﻿using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;

namespace Mercato.AspNet.OData.DataTableExtension
{
    public class ODataNegotiatedContentResult : OkNegotiatedContentResult<ODataReturn>
    {
        public ODataNegotiatedContentResult(ODataReturn content, ApiController controller) 
        :base(content, controller){ }

        public ODataNegotiatedContentResult(ODataReturn content, IContentNegotiator contentNegotiator, HttpRequestMessage request, IEnumerable<MediaTypeFormatter> formatters)
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
