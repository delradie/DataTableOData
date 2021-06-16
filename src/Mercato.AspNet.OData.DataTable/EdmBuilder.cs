using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Mercato.AspNet.OData.DataTableExtension
{
    /// <summary>
    /// Extension utility to build an EdmModel for use in validation and parsing from an untyped DataTable
    /// </summary>
    public static class EdmBuilder
    {
        /// <summary>
        /// Creates an EdmModel based on the source table
        /// </summary>
        /// <param name="sourceTable"></param>
        /// <returns></returns>
        public static Tuple<IEdmModel, IEdmType> BuildEdmModel(this DataTable sourceTable, String entityName = null)
        {
            String Namespace = "Dynamic";
            String TypeName = sourceTable.TableName;

            if(!String.IsNullOrWhiteSpace(entityName))
            {
                TypeName = entityName;
            }

            EdmModel Output = new EdmModel();

            EdmEntityType DataSourceModel = new EdmEntityType(Namespace, TypeName);
            List<IEdmStructuralProperty> KeyProperties = new List<IEdmStructuralProperty>();

            foreach (DataColumn SourceColumn in sourceTable.Columns)
            {
                String ColumnName = SourceColumn.ColumnName;
                Type ColumnType = SourceColumn.DataType;
                EdmPrimitiveTypeKind? MappedType = ConvertType(ColumnType);

                if (!MappedType.HasValue)
                {
                    continue;
                }

                IEdmStructuralProperty NewColumn = DataSourceModel.AddStructuralProperty(ColumnName, MappedType.Value);

                if(sourceTable.PrimaryKey != null && sourceTable.PrimaryKey.Any(x=>String.Equals(x.ColumnName, ColumnName, StringComparison.InvariantCultureIgnoreCase)))
                {
                    KeyProperties.Add(NewColumn);
                }
            }

            if (KeyProperties.Count > 0)
            {
                DataSourceModel.AddKeys(KeyProperties);
            }

            Output.AddElement(DataSourceModel);

            return new Tuple<IEdmModel, IEdmType>(Output, DataSourceModel);
        }

        /// <summary>
        /// Maps source types to the Edm types to be used in the output model
        /// </summary>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        public static EdmPrimitiveTypeKind? ConvertType(Type sourceType)
        {
            EdmPrimitiveTypeKind? Output = null;

            if (sourceType == typeof(Byte[]))
            {
                Output = EdmPrimitiveTypeKind.Binary;
            }
            else if (sourceType == typeof(Boolean))
            {
                Output = EdmPrimitiveTypeKind.Boolean;
            }
            else if (sourceType == typeof(Byte))
            {
                Output = EdmPrimitiveTypeKind.Byte;
            }
            else if (sourceType == typeof(Date))
            {
                Output = EdmPrimitiveTypeKind.Date;
            }
            else if (sourceType == typeof(DateTime))
            {
                Output = EdmPrimitiveTypeKind.DateTimeOffset;
            }
            else if (sourceType == typeof(Decimal))
            {
                Output = EdmPrimitiveTypeKind.Decimal;
            }
            else if (sourceType == typeof(Double))
            {
                Output = EdmPrimitiveTypeKind.Double;
            }
            else if (sourceType == typeof(Guid))
            {
                Output = EdmPrimitiveTypeKind.Guid;
            }
            else if (sourceType == typeof(Single))
            {
                Output = EdmPrimitiveTypeKind.Single;
            }
            else if (sourceType == typeof(SByte))
            {
                Output = EdmPrimitiveTypeKind.SByte;
            }
            else if (sourceType == typeof(Int16))
            {
                Output = EdmPrimitiveTypeKind.Int16;
            }
            else if (sourceType == typeof(Int32))
            {
                Output = EdmPrimitiveTypeKind.Int32;
            }
            else if (sourceType == typeof(Int64))
            {
                Output = EdmPrimitiveTypeKind.Int64;
            }
            else if (sourceType == typeof(String))
            {
                Output = EdmPrimitiveTypeKind.String;
            }
            else if (sourceType == typeof(TimeSpan))
            {
                Output = EdmPrimitiveTypeKind.TimeOfDay;
            }
            else if (sourceType == typeof(DateTimeOffset))
            {
                Output = EdmPrimitiveTypeKind.DateTimeOffset;
            }

            return Output;
        }
    }
}
