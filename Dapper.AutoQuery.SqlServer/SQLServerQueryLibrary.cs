using Dapper.AutoQuery.Lib.Queries;

namespace Dapper.AutoQuery
{
    public class SQLServerQueryLibrary : IQueryLibrary
    {
        public string? DeleteByIdList(string tableName, string keyInSet)
        {
            var sql = $"""
                DELETE FROM {tableName} 
                WHERE 
                    {keyInSet} 
                """;

            return sql;
        }

        public string? DeleteSingle(string tableName, string keyEquality)
        {
            var sql = $"""
                DELETE FROM {tableName} 
                WHERE 
                    {keyEquality} 
                """;

            return sql;
        }

        public string CreateIdTempTable(string tempTableName, string sqlIdType)
        {
            var sql = $"CREATE TABLE {tempTableName}(Id {sqlIdType})";

            return sql;
        }

        public string GetIdsAndDropIdTempTable(string tempTableName)
        {
            var sql = $"""
                SELECT * FROM {tempTableName}; 
                DROP TABLE {tempTableName};
                """;

            return sql;
        }

        public string Insert(string tableName, string asNoKeyFieldSet, string keyField, string asNoKeyArgSet)
        {
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
            var sql = $"""
                SELECT 
                    {asFullFieldSet} 
                FROM {tableName} 
                """;


            return sql;
        }

        /// <summary>
        /// Select query with where clause.
        /// </summary>
        /// <typeparam name="T">Entity to query</typeparam>
        /// <param name="whereClause">Filter statement starting with 'WHERE' including table field names and variable names from filter object eg. 
        /// <br /> WHERE Name LIKE @Name</param>
        /// <returns></returns>
        public string Select(string asFullFieldSet, string tableName, string? whereClause)

        {
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
            var sql = $"""
                UPDATE {tableName} 
                SET
                    {asNoKeyFieldAssignmentsSet} 
                WHERE
                    {keyField} = {keyArg} 
                """;


            return sql;
        }

        public string SelectByIdList(string asFullFieldSet, string tableName, string keyInSet)

        {
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
            var sql = $"""
                DELETE FROM {tempTableName}
                """;


            return sql;
        }
        public string InsertBatch(string tableName, string asNoKeyFieldSet, string tempTableName, string asNoKeyArgSet)
        {
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