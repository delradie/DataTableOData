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

            DataTable Output = Source.ApplyODataQuery(Request);

            return Ok<DataTable>(Output);
        }      
    }
}
