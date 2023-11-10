using Dapper.AutoQuery.Lib.Exceptions;
using Dapper.AutoQuery.Lib.Queries;

namespace Dapper.AutoQuery
{
    public sealed class DAQQueryGenParameterException : ArgumentNullException
    {
        public DAQQueryGenParameterException(string queryName, string paramName) 
            : base($"{queryName}: {paramName} cannot be empty.")
        {
        }
    } 
    public class SQLServerQueryLibrary : IQueryLibrary
    {
        public string? DeleteByIdList(string tableName, string keyInSet)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(DeleteByIdList), nameof(tableName));
            }

            if (string.IsNullOrEmpty(keyInSet))
            {
                throw new DAQQueryGenParameterException(nameof(DeleteByIdList), nameof(keyInSet));
            }

            var sql = $"""
                DELETE FROM {tableName} 
                WHERE 
                    {keyInSet} 
                """;

            return sql;
        }

        public string? DeleteSingle(string tableName, string keyEquality)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(DeleteSingle), nameof(tableName));
            }

            if (string.IsNullOrEmpty(keyEquality))
            {
                throw new DAQQueryGenParameterException(nameof(DeleteSingle), nameof(keyEquality));
            }

            var sql = $"""
                DELETE FROM {tableName} 
                WHERE 
                    {keyEquality} 
                """;

            return sql;
        }

        public string CreateIdTempTable(string tempTableName, string sqlIdType, string sqlIdName = "Id")
        {
            if (string.IsNullOrEmpty(tempTableName))
            {
                throw new DAQQueryGenParameterException(nameof(CreateIdTempTable), nameof(tempTableName));
            }

            if (string.IsNullOrEmpty(sqlIdType))
            {
                throw new DAQQueryGenParameterException(nameof(CreateIdTempTable), nameof(sqlIdType));
            }

            var sql = $"CREATE TABLE {tempTableName}({sqlIdName} {sqlIdType})";

            return sql;
        }

        public string GetIdsAndDropIdTempTable(string tempTableName)
        {
            if (string.IsNullOrEmpty(tempTableName))
            {
                throw new DAQQueryGenParameterException(nameof(GetIdsAndDropIdTempTable), nameof(tempTableName));
            }

            var sql = $"""
                SELECT * FROM {tempTableName}; 
                DROP TABLE {tempTableName};
                """;

            return sql;
        }

        public string Insert(string tableName, string asNoKeyFieldSet, string keyField, string asNoKeyArgSet)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(Insert), nameof(tableName));
            }

            if (string.IsNullOrEmpty(asNoKeyFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(Insert), nameof(asNoKeyFieldSet));
            }

            if (string.IsNullOrEmpty(keyField))
            {
                throw new DAQQueryGenParameterException(nameof(Insert), nameof(keyField));
            }

            if (string.IsNullOrEmpty(asNoKeyArgSet))
            {
                throw new DAQQueryGenParameterException(nameof(Insert), nameof(asNoKeyArgSet));
            }

            var sql = $"""
                INSERT INTO {tableName} 
                (
                    {asNoKeyFieldSet} 
                )
                OUTPUT INSERTED.{keyField} 
                VALUES
                (
                    {asNoKeyArgSet} 
                )
                """;


            return sql;
        }

        public string Select(string asFullFieldSet, string tableName)
        {
            if (string.IsNullOrEmpty(asFullFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(Select), nameof(asFullFieldSet));
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(Select), nameof(tableName));
            }

            var sql = $"""
                SELECT 
                    {asFullFieldSet} 
                FROM {tableName} 
                """;


            return sql;
        }
         
        public string Select(string asFullFieldSet, string tableName, string? whereClause)
        {
            if (string.IsNullOrEmpty(asFullFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(Select), nameof(asFullFieldSet));
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(Select), nameof(tableName));
            }

            var sql = $"""
                SELECT 
                    {asFullFieldSet} 
                FROM {tableName}  
                {whereClause} 
                """;


            return sql;
        }

        public string SelectOneById(string asFullFieldSet, string tableName, string keyEquality)
        {
            if (string.IsNullOrEmpty(asFullFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(SelectOneById), nameof(asFullFieldSet));
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(SelectOneById), nameof(tableName));
            }

            if (string.IsNullOrEmpty(keyEquality))
            {
                throw new DAQQueryGenParameterException(nameof(SelectOneById), nameof(keyEquality));
            }

            var sql = $"""
                SELECT 
                    {asFullFieldSet} 
                FROM {tableName} 
                WHERE 
                    {keyEquality}
                """;


            return sql;
        }

        public string Update(string tableName, string asNoKeyFieldAssignmentsSet, string keyField, string keyArg)

        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(Update), nameof(tableName));
            }

            if (string.IsNullOrEmpty(asNoKeyFieldAssignmentsSet))
            {
                throw new DAQQueryGenParameterException(nameof(Update), nameof(asNoKeyFieldAssignmentsSet));
            }

            if (string.IsNullOrEmpty(keyField))
            {
                throw new DAQQueryGenParameterException(nameof(Update), nameof(keyField));
            }

            if (string.IsNullOrEmpty(keyArg))
            {
                throw new DAQQueryGenParameterException(nameof(Update), nameof(keyArg));
            }

            var sql = $"""
                UPDATE {tableName} 
                SET
                    {asNoKeyFieldAssignmentsSet} 
                WHERE
                    {keyField} = {keyArg} 
                """;


            return sql;
        }
        public string UpdateBatch(string tableName, string tempTableName, string asNoKeyFieldAssignmentsSet, string keyField, string keyArg)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(UpdateBatch), nameof(tableName));
            }
            
            if (string.IsNullOrEmpty(tempTableName))
            {
                throw new DAQQueryGenParameterException(nameof(UpdateBatch), nameof(tempTableName));
            }

            if (string.IsNullOrEmpty(asNoKeyFieldAssignmentsSet))
            {
                throw new DAQQueryGenParameterException(nameof(UpdateBatch), nameof(asNoKeyFieldAssignmentsSet));
            }

            if (string.IsNullOrEmpty(keyField))
            {
                throw new DAQQueryGenParameterException(nameof(UpdateBatch), nameof(keyField));
            }

            if (string.IsNullOrEmpty(keyArg))
            {
                throw new DAQQueryGenParameterException(nameof(UpdateBatch), nameof(keyArg));
            }

            var sql = $"""
                SELECT TOP 0 * 
                FROM {tableName} 
                INTO {tempTableName}; 

                UPDATE {tableName} 
                SET 
                    {asNoKeyFieldAssignmentsSet} 
                FROM {tableName} 
                JOIN {tempTableName} ON {tempTableName}.{keyField} = {tableName}.{keyField}; 

                DROP TABLE {tempTableName}; 
                """;

            return sql;
        }

        public string SelectByIdList(string asFullFieldSet, string tableName, string keyInSet)
        {
            if (string.IsNullOrEmpty(asFullFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(SelectByIdList), nameof(asFullFieldSet));
            }

            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(SelectByIdList), nameof(tableName));
            }

            if (string.IsNullOrEmpty(keyInSet))
            {
                throw new DAQQueryGenParameterException(nameof(SelectByIdList), nameof(keyInSet));
            }

            var sql = $"""
                SELECT 
                    {asFullFieldSet} 
                FROM {tableName} 
                WHERE 
                    {keyInSet}
                """;


            return sql;
        }

        public string TruncateBatchTable(string tempTableName)
        {
            if (string.IsNullOrWhiteSpace(tempTableName))
            {
                throw new DAQQueryGenParameterException(nameof(TruncateBatchTable), nameof(tempTableName));
            }

            var sql = $"""
                DELETE FROM {tempTableName}
                """;


            return sql;
        }
        public string InsertBatch(string tableName, string asNoKeyFieldSet, string tempTableName, string asNoKeyArgSet)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                throw new DAQQueryGenParameterException(nameof(InsertBatch), nameof(tableName));
            }

            if (string.IsNullOrEmpty(asNoKeyFieldSet))
            {
                throw new DAQQueryGenParameterException(nameof(InsertBatch), nameof(asNoKeyFieldSet));
            }

            if (string.IsNullOrEmpty(tempTableName))
            {
                throw new DAQQueryGenParameterException(nameof(InsertBatch), nameof(tempTableName));
            }

            if (string.IsNullOrEmpty(asNoKeyArgSet))
            {
                throw new DAQQueryGenParameterException(nameof(InsertBatch), nameof(asNoKeyArgSet));
            }

            var sql = $"""
                INSERT INTO {tableName} 
                (
                    {asNoKeyFieldSet} 
                )
                OUTPUT INSERTED.Id INTO {tempTableName}
                VALUES
                (
                    {asNoKeyArgSet} 
                )
                """;


            return sql;
        }

    }
}