using System;
using System.Data;

namespace Mercato.AspNet.OData.DataTableExtension.Demo
{
    public static class TestData
    {
        public static DataTable GetData()
        {
            DataTable Output = new DataTable("DataSource");

            Output.Columns.Add("ID", typeof(Int32));
            Output.Columns.Add("Description", typeof(String));
            Output.Columns.Add("Value", typeof(Decimal));
            Output.Columns.Add("LastUpdated", typeof(DateTime));
            Output.Columns.Add("LastUpdatedBy", typeof(Int32));

            DataRow NewRow = Output.NewRow();

            NewRow["ID"] = 1;
            NewRow["Description"] = "Mark";
            NewRow["Value"] = 1000000;
            NewRow["LastUpdated"] = DateTime.Now.AddMonths(-1);
            NewRow["LastUpdatedBy"] = 1;

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 2;
            NewRow["Description"] = "Neil";
            NewRow["Value"] = 10000000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-1);
            NewRow["LastUpdatedBy"] = 1;

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 3;
            NewRow["Description"] = "Tom";
            NewRow["Value"] = 10000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-5);
            NewRow["LastUpdatedBy"] = 2;

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 4;
            NewRow["Description"] = "Jamie";
            NewRow["Value"] = 500000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-10);
            NewRow["LastUpdatedBy"] = 2;

            Output.Rows.Add(NewRow);

            NewRow = Output.NewRow();

            NewRow["ID"] = 5;
            NewRow["Description"] = "Neil";
            NewRow["Value"] = 500000;
            NewRow["LastUpdated"] = DateTime.Now.AddDays(-10);
            NewRow["LastUpdatedBy"] = 2;

            Output.Rows.Add(NewRow);

            return Output;
        }
    }
}