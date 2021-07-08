using Microsoft.OData;
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
    [RoutePrefix("api/v2")]
    public class TestController : ApiController
   {
        [Route("Test")]
        [HttpGet]
        public IHttpActionResult Get()
        {
            DataTable Source = TestData.GetData();

            ODataTableFilter.Result Output = Source.ApplyODataQuery(this.Request);

            String AddressBase = $"{this.Request.RequestUri.Scheme}://{this.Request.RequestUri.Host}:{this.Request.RequestUri.Port}";

            String EndpointAddress = $"{AddressBase}/api/v2/Test";
            String MetaDataAddress = $"{AddressBase}/api/v2/$metadata#Test";

            ODataReturn ReturnData = new ODataReturn(Output, EndpointAddress, MetaDataAddress);

            ReturnData.PatchUpValueTypes();

            return ReturnData.GenerateResponseMessage(this);
        }

        [Route("")]
        [HttpGet]
        public IHttpActionResult GetMetadataRoot()
        {
            return GetMetadata();
        }

        [Route("$metadata")]
        [HttpGet]
        public IHttpActionResult GetMetadata()
        {
            DataTable Source = TestData.GetData();

            Tuple<IEdmModel, IEdmType> InferredEntityModel = Source.BuildEdmModel("Test", "Test");

            StringWriter Writer = new StringWriter();
            XmlWriter XWriter = XmlWriter.Create(Writer);

            XWriter.WriteProcessingInstruction("xml", "version='1.0'");

            if (CsdlWriter.TryWriteCsdl(InferredEntityModel.Item1, XWriter, CsdlTarget.OData, out IEnumerable<EdmError> errors))
            {
                XWriter.Flush();
                String XmlOutput = Writer.ToString();

                if (!String.IsNullOrWhiteSpace(XmlOutput))
                {
                    HttpResponseMessage Output = new HttpResponseMessage();

                    Output.StatusCode = HttpStatusCode.OK;
                    Output.Content = new StringContent(XmlOutput);
                    Output.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");
                    Output.Content.Headers.TryAddWithoutValidation(
                        ODataNegotiatedContentResult.ODataServiceVersionHeader,
                        ODataUtils.ODataVersionToString(ODataVersion.V4));


                    //StringContent OutputContent = new StringContent(XmlOutput);
                    //OutputContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/xml");

                    return ResponseMessage(Output);
                }
            }

            return InternalServerError();
        }
    }
}
