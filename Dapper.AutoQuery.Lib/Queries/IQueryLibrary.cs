namespace Dapper.AutoQuery.Lib.Queries
{
    public interface IQueryLibrary
    {
        string CreateIdTempTable(string tempTableName, string sqlIdType);
        string? DeleteByIdList(string tableName, string keyInSet);
        string? DeleteSingle(string tableName, string keyEquality);
        string GetIdsAndDropIdTempTable(string tempTableName);
        string Insert(string tableName, string asNoKeyFieldSet, string keyField, string asNoKeyArgSet);
        string InsertBatch(string tableName, string asNoKeyFieldSet, string tempTableName, string asNoKeyArgSet);
        string Select(string asFullFieldSet, string tableName);
        string Select(string asFullFieldSet, string tableName, string? whereClause);
        string SelectByIdList(string asFullFieldSet, string tableName, string keyInSet);
        string SelectOneById(string asFullFieldSet, string tableName, string keyEquality);
        string TruncateBatchTable(string tempTableName);
        string Update(string tableName, string asNoKeyFieldAssignmentsSet, string keyField, string keyArg);
    }
}