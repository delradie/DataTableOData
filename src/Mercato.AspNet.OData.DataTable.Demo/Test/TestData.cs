using System;
using System.Data;

namespace Mercato.AspNet.OData.DataTableExtension.Demo
{
    public static class TestData
    {
        public static DataTable GetData()
        {
            DataTable Output = new DataTable("Test");

            DataColumn IDColumn = Output.Columns.Add("ID", typeof(Int32));
            Output.Columns.Add("Description", typeof(String));
            Output.Columns.Add("Value", typeof(Decimal));
            Output.Columns.Add("LastUpdated", typeof(DateTime));
            Output.Columns.Add("LastUpdatedBy", typeof(Int32));
            Output.Columns.Add("ExternalId", typeof(Guid));

            Output.PrimaryKey = new DataColumn[] { IDColumn };

            DataRow NewRow = Output.NewRow();

            NewRow["ID"] = 1;
            NewRow["Description"] = "Mark";
            NewRow["Value"] = 1000000;
            NewRow["LastUpdated"] = DateTime.Now.AddMonths(-1);
            NewRow["LastUpdatedBy"] = 1;
            NewRow["ExternalId"] = new Guid("4ad2ded8-ae7b-4e20-bfce-dc569445d94c");
            
            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 2;
            NewRow["Description"] = "Neil";
            NewRow["Value"] = 10000000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-1);
            NewRow["LastUpdatedBy"] = 1;
            NewRow["ExternalId"] = new Guid("73daee5c-cb68-4938-a83f-ef87a08af7cb");           

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 3;
            NewRow["Description"] = "Tom";
            NewRow["Value"] = 10000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-5);
            NewRow["LastUpdatedBy"] = 2;
            NewRow["ExternalId"] = new Guid("dd0fed53-04f2-41ff-bc31-493f8441d4fc");

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 4;
            NewRow["Description"] = "Jamie";
            NewRow["Value"] = 500000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-10);
            NewRow["LastUpdatedBy"] = 2;
            NewRow["ExternalId"] = new Guid("13e89c3c-54c7-443a-877f-b76ef49d7067");

            Output.Rows.Add(NewRow);
            
            NewRow = Output.NewRow();

            NewRow["ID"] = 5;
            NewRow["Description"] = "Neil";
            NewRow["Value"] = 500000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-10);
            NewRow["LastUpdatedBy"] = 2;
            NewRow["ExternalId"] = new Guid("e022acd1-f78f-41ee-ab45-ea1c32d792f7");

            Output.Rows.Add(NewRow);

            return Output;
        }
    }
}