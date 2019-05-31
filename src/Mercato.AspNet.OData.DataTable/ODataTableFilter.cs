using Microsoft.AspNet.OData.Query;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mercato.AspNet.OData.DataTableExtension
{
    /// <summary>
    /// Functionality to apply a validated OData query to an untyped ADO.Net DataTable
    /// </summary>
    public static class ODataTableFilter
    {
        /// <summary>
        /// Extension method on DataTable to apply the input query
        /// </summary>
        /// <param name="sourceData">Source data</param>
        /// <param name="criteria">OData options</param>
        /// <returns>A DataTable containing the output from applying the OData options to the input data</returns>
        public static DataTable Apply(this DataTable sourceData, ODataQueryOptions criteria)
        {
            DataView Output = new DataView(sourceData);

            if (criteria.Filter != null)
            {
                Output.RowFilter = TranslateFilter(criteria.Filter);
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

                if(criteria.Top != null)
                {
                    OutputRows = OutputRows.Take(criteria.Top.Value);
                }

                OutputTable = OutputRows.CopyToDataTable();
                OutputTable.TableName = sourceData.TableName;
            }

            return OutputTable;
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

                    return ConstNode.LiteralText;
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

    }
}
