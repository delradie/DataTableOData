using Microsoft.AspNet.OData.Query;
using System;
using System.Data;
using Microsoft.OData.Edm;

using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Mercato.AspNet.OData.DataTableExtension;
using System.Net.Http;
using Microsoft.AspNet.OData.Extensions;

namespace Mercato.AspNet.OData.DataTableExtension.Demo.Controllers
{
    public class TestController : ApiController
    {
        // GET: api/Test
        public IHttpActionResult Get()
        {
            DataTable Source = TestData.GetData();

            Tuple<IEdmModel, IEdmType> SourceModel = Source.BuildEdmModel();

            ODataPath Path = Request.ODataProperties().Path;

            ODataQueryContext SourceContext = new ODataQueryContext(SourceModel.Item1, SourceModel.Item2, Path);

            ODataQueryOptions Query = new ODataQueryOptions(SourceContext, Request);

            DataTable Output = Source.Apply(Query);

            return Ok<DataTable>(Output);
        }      
    }
}
