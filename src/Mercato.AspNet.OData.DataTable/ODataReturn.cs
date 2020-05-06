using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Web.Http;

namespace Mercato.AspNet.OData.DataTableExtension
{
    public class ODataReturn
    {
        [JsonProperty("@odata.context", NullValueHandling = NullValueHandling.Ignore)]
        public String Context { get; set; }
        [JsonProperty("@odata.count", NullValueHandling = NullValueHandling.Ignore)]
        public Int32? Count { get; set; }
        [JsonProperty("@odata.nextLink", NullValueHandling = NullValueHandling.Ignore)]
        public String NextPageLink { get; set; }
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public DataTable Values { get; set; }

        public ODataReturn()
        { }

        public ODataReturn(ODataTableFilter.Result content, String endpointAddress, String metadataAddress)
        {
            Values = content.Values;
            Context = metadataAddress;


            if (content.RequestedOutputFormat == ODataTableFilter.OutputFormat.DataWithMetaDataAndCount)
            {
                Count = content.ValueCount;
            }

            if (!String.IsNullOrWhiteSpace(content.NextPageQueryString))
            {
                NextPageLink = $"{endpointAddress}?{content.NextPageQueryString}";
            }
        }
    }
}
