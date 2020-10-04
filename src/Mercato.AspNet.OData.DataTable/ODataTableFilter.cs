using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Text;
using Routing = Microsoft.AspNet.OData.Routing;

namespace Mercato.AspNet.OData.DataTableExtension
{
    /// <summary>
    /// Functionality to apply a validated OData query to an untyped ADO.Net DataTable
    /// </summary>
    public static class ODataTableFilter
    {
        public enum OutputFormat
        {
            DataWithMetaData,
            DataWithMetaDataAndCount
        }

        public class Result
        {
            public DataTable Values { get; set; }
            public Int32 ValueCount { get; set; }
            public String NextPageQueryString { get; set; }
            public OutputFormat RequestedOutputFormat { get; set; }
        }

        public static Result ApplyODataQuery(this DataTable sourceData, HttpRequestMessage request, Tuple<IEdmModel, IEdmType> datasourceEdmProperties = null)
        {
            Tuple<IEdmModel, IEdmType> SourceModel = datasourceEdmProperties ?? sourceData.BuildEdmModel();

            Routing.ODataPath Path = request.ODataProperties().Path;

            ODataQueryContext SourceContext = new ODataQueryContext(SourceModel.Item1, SourceModel.Item2, Path);

            ODataQueryOptions Query = new ODataQueryOptions(SourceContext, request);

            return sourceData.Apply(Query);
        }

        /// <summary>
        /// Extension method on DataTable to apply the input query
        /// </summary>
        /// <param name="sourceData">Source data</param>
        /// <param name="criteria">OData options</param>
        /// <returns>A DataTable containing the output from applying the OData options to the input data</returns>
        public static Result Apply(this DataTable sourceData, ODataQueryOptions criteria)
        {
            OutputFormat RequestedFormat = OutputFormat.DataWithMetaData;
            Int32 OutputCount = 0;
            String NextPageQuery = null;

            DataView Output = new DataView(sourceData);

            if (criteria.Filter != null)
            {
                Output.RowFilter = TranslateFilter(criteria.Filter);
            }

            OutputCount = Output.Count;

            if (criteria.Count?.Value ?? false)
            {
                RequestedFormat = OutputFormat.DataWithMetaDataAndCount;
            }

            if (criteria.OrderBy?.OrderByClause != null)
            {
                Output.Sort = TranslateOrderBy(criteria.OrderBy.OrderByClause);
            }

            DataTable OutputTable = Output.ToTable();

            if (criteria.Skip != null || criteria.Top != null)
            {
                IEnumerable<DataRow> OutputRows = OutputTable.Rows.Cast<DataRow>();

                if (criteria.Skip != null)
                {
                    OutputRows = OutputRows.Skip(criteria.Skip.Value);
                }

                if (criteria.Top != null)
                {
                    OutputRows = OutputRows.Take(criteria.Top.Value);

                    NextPageQuery = GenerateNextPageQueryString(criteria, OutputCount);
                }

                OutputTable = OutputRows.CopyToDataTable();
                OutputTable.TableName = sourceData.TableName;
            }

            if (criteria.SelectExpand?.SelectExpandClause?.SelectedItems != null)
            {
                OutputTable = ApplySelect(OutputTable, criteria.SelectExpand.SelectExpandClause);
            }

            return new Result()
            {
                Values = OutputTable,
                ValueCount = OutputCount,
                RequestedOutputFormat = RequestedFormat,
                NextPageQueryString = NextPageQuery
            };
        }

        private static String GenerateNextPageQueryString(ODataQueryOptions criteria, Int32 rowCount)
        {
            if (rowCount == 0)
            {
                return String.Empty;
            }

            if(criteria.Top == null || criteria.Top.Value <= 0)
            {
                return String.Empty;
            }

            Int32 Skip = criteria.Top.Value;

            if(criteria.Skip != null && criteria.Skip.Value > 0)
            {
                Skip += criteria.Skip.Value;
            }

            if(Skip >= rowCount)
            {
                return String.Empty;
            }

            StringBuilder QueryStringBuilder = new StringBuilder(512);

            if(criteria.Filter != null && !String.IsNullOrWhiteSpace(criteria.Filter.RawValue))
            {
                QueryStringBuilder.Append($"&$filter={criteria.Filter.RawValue}");
            }

            if (criteria.Count != null && !String.IsNullOrWhiteSpace(criteria.Count.RawValue))
            {
                QueryStringBuilder.Append($"&$count={criteria.Count.RawValue}");
            }

            if (criteria.SelectExpand != null && !String.IsNullOrWhiteSpace(criteria.SelectExpand.RawSelect))
            {
                QueryStringBuilder.Append($"&$select={criteria.SelectExpand.RawSelect}");
            }

            if (criteria.SelectExpand != null && !String.IsNullOrWhiteSpace(criteria.SelectExpand.RawExpand))
            {
                QueryStringBuilder.Append($"&$expand={criteria.SelectExpand.RawExpand}");
            }

            QueryStringBuilder.Append($"&$top={criteria.Top.RawValue}&$skip={Skip}");

            String Output = QueryStringBuilder.ToString(1, QueryStringBuilder.Length - 1);

            return Output;
        }

        /// <summary>
        /// Builds a RowFilter expression based on the input query options
        /// </summary>
        /// <param name="query">Filter criteria</param>
        /// <returns>An ADO.Net compliant filter expression</returns>
        private static String TranslateFilter(FilterQueryOption query)
        {
            if (query?.FilterClause?.Expression == null)
            {
                return String.Empty;
            }

            return ParseNode(query.FilterClause.Expression);
        }

        /// <summary>
        /// Parses the node from the OData syntax tree to a String output
        /// </summary>
        /// <param name="node">Input node</param>
        /// <returns>Translated output to string</returns>
        private static String ParseNode(SingleValueNode node)
        {
            switch (node.Kind)
            {
                case QueryNodeKind.BinaryOperator:
                    BinaryOperatorNode BONode = node as BinaryOperatorNode;

                    String Left = ParseNode(BONode.Left);
                    String Right = ParseNode(BONode.Right);

                    String OperatorFormat = TranslateOperator(BONode.OperatorKind);

                    if (String.IsNullOrWhiteSpace(OperatorFormat))
                    {
                        return String.Empty;
                    }

                    return String.Format(OperatorFormat, Left, Right);
                case QueryNodeKind.Constant:
                    ConstantNode ConstNode = node as ConstantNode;

                    return ConvertLiteralText(ConstNode);
                case QueryNodeKind.SingleValuePropertyAccess:
                    SingleValuePropertyAccessNode SinglePropertyNode = node as SingleValuePropertyAccessNode;

                    return SinglePropertyNode.Property.Name;
                case QueryNodeKind.Convert:
                    SingleValueNode Node = (node as ConvertNode)?.Source;

                    return ParseNode(Node);
                    //case QueryNodeKind.SingleValueFunctionCall:
                    //    SingleValueFunctionCallNode FuncNode = node as SingleValueFunctionCallNode;

                    //    FuncNode.p

                    //    String OperatorFormat = TranslateOperator(BONode.OperatorKind);


                    //    return String.Empty;

                    //    break;
            }

            return String.Empty;
        }

        private static String ConvertLiteralText(ConstantNode node)
        {
            String Output = node?.LiteralText;

            switch (node.TypeReference.PrimitiveKind())
            {
                case EdmPrimitiveTypeKind.Guid:
                    Output = $"'{Output}'";
                    break;
                case EdmPrimitiveTypeKind.Date:
                case EdmPrimitiveTypeKind.DateTimeOffset:
                    Output = $"#{Output}#";
                    break;
            }

            return Output;
        }

        /// <summary>
        /// Generates a format string representing the ADO.Net handling of the specified operator
        /// </summary>
        /// <param name="comparison">Requested operator</param>
        /// <returns>String format to use in ADO syntax</returns>
        private static String TranslateOperator(BinaryOperatorKind comparison)
        {
            String Output = String.Empty;

            switch (comparison)
            {
                case BinaryOperatorKind.Or:
                    Output = "({0}) OR ({1})";
                    break;
                case BinaryOperatorKind.And:
                    Output = "({0}) AND ({1})";
                    break;
                case BinaryOperatorKind.Equal:
                    Output = "{0} = {1}";
                    break;
                case BinaryOperatorKind.NotEqual:
                    Output = "{0} <> {1}";
                    break;
                case BinaryOperatorKind.GreaterThan:
                    Output = "{0} > {1}";
                    break;
                case BinaryOperatorKind.GreaterThanOrEqual:
                    Output = "{0} >= {1}";
                    break;
                case BinaryOperatorKind.LessThan:
                    Output = "{0} < {1}";
                    break;
                case BinaryOperatorKind.LessThanOrEqual:
                    Output = "{0} <= {1}";
                    break;
                case BinaryOperatorKind.Add:
                    Output = "{0} + {1}";
                    break;
                case BinaryOperatorKind.Subtract:
                    Output = "{0} - {1}";
                    break;
                case BinaryOperatorKind.Multiply:
                    Output = "{0} * {1}";
                    break;
                case BinaryOperatorKind.Divide:
                    Output = "{0} / {1}";
                    break;
                case BinaryOperatorKind.Modulo:
                    Output = "{0} % {1}";
                    break;
                    //case BinaryOperatorKind.Has:
                    //TODO: Implement has operator
                    //break;
            }

            return Output;
        }

        /// <summary>
        /// Generates the ADO sort expression based on the order by OData criteria 
        /// </summary>
        /// <param name="orderBy"></param>
        /// <returns></returns>
        private static String TranslateOrderBy(OrderByClause orderBy)
        {
            if (orderBy?.Expression == null)
            {
                return String.Empty;
            }

            String Output = String.Empty;

            Output = ParseNode(orderBy.Expression);

            if (String.IsNullOrWhiteSpace(Output))
            {
                Output = String.Empty;
            }

            String Direction = String.Empty;

            switch (orderBy.Direction)
            {
                case OrderByDirection.Ascending:
                    Direction = "ASC";
                    break;
                case OrderByDirection.Descending:
                    Direction = "DESC";
                    break;
            }

            Output = $"{Output} {Direction}";

            if (orderBy.ThenBy != null)
            {
                String ThenBy = TranslateOrderBy(orderBy.ThenBy);

                if (!String.IsNullOrWhiteSpace(ThenBy))
                {
                    Output = Output + ", " + ThenBy;
                }
            }

            return Output;
        }

        /// <summary>
        /// Restricts the output table to only the columns specified in the OData $select parameter
        /// </summary>
        /// <param name="source"></param>
        /// <param name="selectCriteria"></param>
        /// <returns></returns>
        private static DataTable ApplySelect(DataTable source, SelectExpandClause selectCriteria)
        {
            if (selectCriteria.AllSelected)
            {
                return source;
            }

            List<String> ColumnNames = new List<String>();

            foreach (SelectItem item in selectCriteria.SelectedItems)
            {
                if (item is PathSelectItem)
                {
                    PathSelectItem PathItem = (PathSelectItem)item;

                    IEnumerable<String> Path = PathItem.SelectedPath.Select(x => x.Identifier.ToUpperInvariant());

                    foreach (String PathIdent in Path)
                    {
                        if (!ColumnNames.Contains(PathIdent))
                        {
                            ColumnNames.Add(PathIdent);
                        }
                    }
                }
            }

            List<DataColumn> DeleteColumns = new List<DataColumn>();

            foreach (DataColumn sourceColumn in source.Columns)
            {
                if (!ColumnNames.Any(x => String.Equals(x, sourceColumn.ColumnName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    DeleteColumns.Add(sourceColumn);
                }
            }

            if (DeleteColumns.Count > 0)
            {
                foreach (DataColumn Target in DeleteColumns)
                {
                    source.Columns.Remove(Target.ColumnName);
                }
            }

            return source;
        }
    }
}
