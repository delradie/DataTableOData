using Newtonsoft.Json;
using System;
using System.Data;
using System.Web.Http;

namespace Mercato.AspNet.OData.DataTableExtension.Demo.Controllers
{
    public class TestController : ApiController
    {
        public class OdataReturn
        {
            [JsonProperty("@odata.context")]
            public String Context { get; set; }
            [JsonProperty("@odata.count")]
            public Int32? Count { get; set; }
            [JsonProperty("@odata.nextLink")]
            public String NextPageLink { get; set; }
            [JsonProperty("value")]
            public DataTable Values { get; set; }
        }

        // GET: api/Test
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
    }
}
