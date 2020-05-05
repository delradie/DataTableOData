using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.OData.Edm.Validation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Xml;

namespace Mercato.AspNet.OData.DataTableExtension.Demo.Controllers
{
    [RoutePrefix("api/Test")]
    public class TestController : ApiController
    {
        public class OdataReturn
        {
            [JsonProperty("@odata.context, NullValueHandling = NullValueHandling.Ignore")]
            public String Context { get; set; }
            [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
            public Int32? Count { get; set; }
            [JsonProperty("@odata.nextLink, NullValueHandling = NullValueHandling.Ignore")]
            public String NextPageLink { get; set; }
            [JsonProperty("value, NullValueHandling = NullValueHandling.Ignore")]
            public DataTable Values { get; set; }
        }

        [Route("")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            DataTable Source = TestData.GetData();

            ODataTableFilter.Result Output = Source.ApplyODataQuery(this.Request);

            OdataReturn ReturnData = new OdataReturn()
            {
                Values = Output.Values
            };

            if (Output.RequestedOutputFormat == ODataTableFilter.OutputFormat.DataWithMetaDataAndCount)
            {
                ReturnData.Count = Output.ValueCount;
            }
            else if (Output.RequestedOutputFormat == ODataTableFilter.OutputFormat.DataWithMetaData)
            {
                ReturnData.Context = $"http://localhost:50293/api/Test/$metadata";
            }

            if (!String.IsNullOrWhiteSpace(Output.NextPageQueryString))
            {
                ReturnData.NextPageLink = $"http://localhost:50293/api/Test?{Output.NextPageQueryString}";
            }

            return Ok<OdataReturn>(ReturnData);
        }

        [Route("$metadata")]
        [HttpGet]
        public IHttpActionResult GetMetadata()
        {
            DataTable Source = TestData.GetData();

            Tuple<IEdmModel, IEdmType> InferredEntityModel = Source.BuildEdmModel();

            StringWriter Writer = new StringWriter();
            XmlWriter XWriter = XmlWriter.Create(Writer);

            if (CsdlWriter.TryWriteCsdl(InferredEntityModel.Item1, XWriter, CsdlTarget.OData, out IEnumerable<EdmError> errors))
            {
                XWriter.Flush();
                String XmlOutput = Writer.ToString();

                if (!String.IsNullOrWhiteSpace(XmlOutput))
                {
                    StringContent OutputContent = new StringContent(XmlOutput);
                    OutputContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, XmlOutput, new XmlMediaTypeFormatter()));
                }
            }

            return InternalServerError();
        }
    }
}
