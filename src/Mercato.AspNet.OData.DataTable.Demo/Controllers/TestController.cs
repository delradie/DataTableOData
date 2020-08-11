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
    [RoutePrefix("api")]
    public class TestController : ApiController
    {
        [Route("Test")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            DataTable Source = TestData.GetData();

            ODataTableFilter.Result Output = Source.ApplyODataQuery(this.Request);

            String AddressBase = $"{this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}:{this.Request.RequestUri.Port}";

            String EndpointAddress = $"{AddressBase}/api/Test";
            String MetaDataAddress = $"{AddressBase}/api/$metadata";

            ODataReturn ReturnData = new ODataReturn(Output, EndpointAddress, MetaDataAddress);

            return Ok<ODataReturn>(ReturnData);
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
