using System.Data;
using System.Web.Http;
using Mercato.AspNet.OData.DataTableExtension;

namespace Mercato.AspNet.OData.DataTableExtension.Demo.Controllers
{
    public class TestController : ApiController
    {
        // GET: api/Test
        public IHttpActionResult Get()
        {
            DataTable Source = TestData.GetData();

            DataTable Output = Source.ApplyODataQuery(this.Request);

            return Ok<DataTable>(Output);
        }
    }
}
