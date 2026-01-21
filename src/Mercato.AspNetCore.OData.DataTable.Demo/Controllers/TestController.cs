using Mercato.AspNet.OData.DataTableExtension;
using Mercato.AspNetCore.OData.DataTableExtension.Demo.Test;

using Microsoft.AspNetCore.Mvc;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

using System.Data;
using System.Net;
using System.Text;
using System.Xml;

using EdmError = Microsoft.OData.Edm.Validation.EdmError;

namespace Mercato.AspNetCore.OData.DataTableExtension.Demo.Controllers;

[ApiController]
[Route("api")]
public class TestController : ControllerBase
{
    [Route("Test")]
    [HttpGet]
    public IActionResult Get()
    {
        DataTable Source = TestData.GetData();

        ODataTableFilter.Result Output = Source.ApplyODataQuery(this.Request);

        String AddressBase = $"{this.Request.Scheme}://{this.Request.Host.Host}:{this.Request.Host.Port}";

        //Path to this endpoint - used for generating Next links when returning paged data
        String EndpointAddress = $"{AddressBase}/api/Test";
        //Path to the metadata for this entity's context, including reference to this specific entity set
        String MetaDataAddress = $"{AddressBase}/api/$metadata#Test";

        ODataReturn ReturnData = new ODataReturn(Output, EndpointAddress, MetaDataAddress);

        //Workaround method to convert data columns that are not based on Edm mappable types - currently only known one is DateTime, which is mapped to DateTimeOffset
        ReturnData.PatchUpValueTypes();

        //Returns a specialised OkNegotiatedContentResult that ensures JSON serialisation, and included the OData-Version header
        // http://docs.oasis-open.org/odata/odata/v4.01/odata-v4.01-part1-protocol.html#_Toc31358862
        return ReturnData.GenerateResponseMessage();
    }

    [Route("Test/$count")]
    [HttpGet]
    public IActionResult GetCount()
    {
        DataTable Source = TestData.GetData();

        ODataTableFilter.Result Output = Source.ApplyODataQuery(this.Request);

        Output.RequestedOutputFormat = ODataTableFilter.OutputFormat.DataWithMetaDataAndCount;

        String AddressBase = $"{this.Request.Scheme}://{this.Request.Host.Host}:{this.Request.Host.Port}";

        //Path to this endpoint - used for generating Next links when returning paged data
        String EndpointAddress = $"{AddressBase}/api/Test";
        //Path to the metadata for this entity's context, including reference to this specific entity set
        String MetaDataAddress = $"{AddressBase}/api/$metadata#Test";

        ODataReturn ReturnData = new ODataReturn(Output, EndpointAddress, MetaDataAddress);

        return ReturnData.GenerateCountResponseMessage();
    }

    [Route("")]
    [HttpGet]
    public IActionResult GetMetadataRoot()
    {
        return GetMetadata();
    }

    [Route("$metadata")]
    [HttpGet]
    public IActionResult GetMetadata()
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
                HttpContext.Response.Headers.Append(
                    ODataReturnNegotiatedContentResult.ODataServiceVersionHeader,
                    ODataUtils.ODataVersionToString(ODataVersion.V4));

                return this.Content(XmlOutput, "application/xml");
            }
        }

        return this.StatusCode((Int32)HttpStatusCode.InternalServerError);
    }
}
