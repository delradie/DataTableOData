using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Data;

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

        /// <summary>
        /// Updates the types in the Values datatable to types that will serialise to OData v4 compatible formats
        /// </summary>
        public void PatchUpValueTypes()
        {
            try
            {
                if (Values == null || Values.Columns.Count == 0)
                {
                    return;
                }

                Dictionary<String, Type> RequiredUpdates = new Dictionary<String, Type>();

                foreach (DataColumn Column in Values.Columns)
                {
                    if (Column.DataType == typeof(DateTime))
                    {
                        RequiredUpdates.Add(Column.ColumnName, typeof(DateTimeOffset));
                    }
                }

                if (RequiredUpdates.Count > 0)
                {
                    foreach (KeyValuePair<String, Type> Update in RequiredUpdates)
                    {
                        ConvertColumnType(Values, Update.Key, Update.Value);
                    }
                }
            }
            catch (Exception exc)
            {
                throw;
            }
        }

        private static void ConvertColumnType(DataTable dt, string columnName, Type newType)
        {
            Boolean UseDateTimeOffsetConversion = false;

            try
            {
                using (DataColumn dc = new DataColumn(columnName + "_new", newType))
                {
                    // Add the new column which has the new type, and move it to the ordinal of the old column
                    int ordinal = dt.Columns[columnName].Ordinal;
                    Type SourceType = dt.Columns[columnName].DataType;

                    UseDateTimeOffsetConversion = (SourceType == typeof(DateTime) && newType == typeof(DateTimeOffset));

                    dt.Columns.Add(dc);
                    dc.SetOrdinal(ordinal);

                    // Get and convert the values of the old column, and insert them into the new
                    foreach (DataRow dr in dt.Rows)
                    {
                        //if (dr.IsNull(dc.ColumnName))
                        //{
                        //    continue;
                        //}

                        if (UseDateTimeOffsetConversion)
                        {
                            dr[dc.ColumnName] = (DateTimeOffset)((DateTime)dr[columnName]);
                        }
                        else
                        {
                            dr[dc.ColumnName] = Convert.ChangeType(dr[columnName], newType);
                        }
                    }

                    // Remove the old column
                    dt.Columns.Remove(columnName);

                    // Give the new column the old column's name
                    dc.ColumnName = columnName;
                }
            }
            catch (Exception exc)
            {
                throw;
            }
        }
    }
}
