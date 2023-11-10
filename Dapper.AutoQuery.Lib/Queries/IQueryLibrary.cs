using Dapper.AutoQuery.Lib.Core;

namespace Dapper.AutoQuery.Lib.Queries
{
    /// <summary>
    /// Provides a set of methods required by <see cref="AutoQueryGenerator"/> to generate SQL dialect-specific queries.
    /// <br/> Should be implemented in dialect-specific query libraries and set up to <see cref="IAutoQueryConfigurationBuilder"/> 
    /// before using <see cref="Extensions.DapperExtensions"/>
    /// </summary>
    public interface IQueryLibrary
    {
        /// <summary>
        /// Generates Dapper query creating temporary table of identificators of type provided in <paramref name="sqlIdType"/>. 
        /// Default ddentificator column name is "Id"
        /// </summary>
        /// <param name="tempTableName">Name of temporary table to create</param>
        /// <param name="sqlIdType">Type of identificator</param>
        /// <param name="sqlIdName">Name of identificator column</param>
        /// <returns>Dapper SQL string creating a temporary table. SQL dialect depends on specific implementation. </returns>
        string CreateIdTempTable(string tempTableName, string sqlIdType, string sqlIdName = "Id");

        /// <summary>
        /// Generates Dapper query deleting records from <paramref name="tableName"/> by list-based filtering 
        /// </summary>
        /// <param name="tableName">Name of table to delete records from</param>
        /// <param name="keyInSet">Stores a sql template of key matching (eg Id IN @ids)</param>
        /// <returns>Dapper SQL string deleting records from the table using WHERE XX IN YY query.</returns>
        string? DeleteByIdList(string tableName, string keyInSet);

        /// <summary>
        /// Generates Dapper query deleting specific record using direct key equality.
        /// </summary>
        /// <param name="tableName">Table to delete record from.</param>
        /// <param name="keyEquality">Key comparison statement (eg Id = @Id)</param>
        /// <returns>Dapper SQL string deleting specific record.</returns>
        string? DeleteSingle(string tableName, string keyEquality);

        /// <summary>
        /// Generates Dapper query to obtain values from temporary table <paramref name="tempTableName"/> and drop it.
        /// </summary>
        /// <param name="tempTableName">Temporary table to get values from.</param>
        /// <returns>Dapper SQL query getting values from temp table and dropping it.</returns>
        string GetIdsAndDropIdTempTable(string tempTableName);
        /// <summary>
        /// Generates Dapper query to insert single instance of an object into table <paramref name="tableName"/>.
        /// Key field pointed in <paramref name="keyField"/> will be returned by OUTPUT statement or similar
        /// </summary>
        /// <param name="tableName">Table to insert values to</param>
        /// <param name="asNoKeyFieldSet">Fields to insert values to (eg Name, CreatedDate, CreatedBy, etc  )</param>
        /// <param name="keyField">Key field to output (eg. Id)</param>
        /// <param name="asNoKeyArgSet">Argument set to be replaced by Dapper (eg @Name, @CreatedDate, @CreatedBy, etc  )</param>
        /// <returns>Dapper SQL query for inserting single object instance values</returns>
        string Insert(string tableName, string asNoKeyFieldSet, string keyField, string asNoKeyArgSet);

        /// <summary>
        /// Generates Dapper query to batch insert multiple instances of an object into table <paramref name="tableName"/>.
        /// </summary>
        /// <param name="tableName">Table to insert values to</param>
        /// <param name="asNoKeyFieldSet">Fields to insert values to (eg Name, CreatedDate, CreatedBy, etc  )</param>
        /// <param name="tempTableName">Temp table to insert inserted.ids into.</param>
        /// <param name="asNoKeyArgSet">Argument set to be replaced by Dapper (eg @Name, @CreatedDate, @CreatedBy, etc  )</param>
        /// <returns>Dapper SQL query for inserting multiple object instances in batch</returns>
        string InsertBatch(string tableName, string asNoKeyFieldSet, string tempTableName, string asNoKeyArgSet);
        /// <summary>
        /// Generates Dapper query selecting ALL fields provided in <paramref name="asFullFieldSet"/> 
        /// of records from <paramref name="tableName"/>. Be carefull with it.
        /// </summary>
        /// <param name="asFullFieldSet">List of field to select</param>
        /// <param name="tableName">Table from where to select</param>
        /// <returns>Dapper SQL query selecting ALL values from table</returns>
        string Select(string asFullFieldSet, string tableName);

        /// <summary>
        /// Generates Dapper query selecting <paramref name="asFullFieldSet"/> fields of <paramref name="tableName"/> table records 
        /// using filter <paramref name="whereClause"/>
        /// </summary>
        /// <param name="asFullFieldSet">List of fields to select</param>
        /// <param name="tableName">Table to select from</param>
        /// <param name="whereClause">Filter statement starting with 'WHERE' including table field names and variable names from filter object eg.
        /// <br/> WHERE Name LIKE @Name</param>
        /// <returns>Dapper SQL query to select by filter</returns>
        string Select(string asFullFieldSet, string tableName, string? whereClause);

        /// <summary>
        /// Generates Dapper query selecting <paramref name="asFullFieldSet"/> fields of <paramref name="tableName"/> table  
        /// using list-based filtering.
        /// </summary>
        /// <param name="asFullFieldSet">List of fields to select</param>
        /// <param name="tableName">Table to select from</param>
        /// <param name="keyInSet">List-based filtering statement (eg Id IN @Ids)</param>
        /// <returns>Dapper SQL select query with list-based filtering</returns>
        string SelectByIdList(string asFullFieldSet, string tableName, string keyInSet);

        /// <summary>
        /// Generates Dapper query selecting <paramref name="asFullFieldSet"/> fields of <paramref name="tableName"/> table 
        /// using direct key equality
        /// </summary>
        /// <param name="asFullFieldSet">List of fields to select</param>
        /// <param name="tableName">Table to select from</param>
        /// <param name="keyEquality">Key equality statement (eg. Id = @Id)</param>
        /// <returns>Dapper SQL select query with direct key equality</returns>
        string SelectOneById(string asFullFieldSet, string tableName, string keyEquality);

        /// <summary>
        /// Generates dapper query deleting or truncating all records in <paramref name="tempTableName"/>
        /// </summary>
        /// <param name="tempTableName">Table to clear</param>
        /// <returns>Dapper SQL query to clear the table</returns>
        /// 
        string TruncateBatchTable(string tempTableName);
        /// <summary>
        /// Generates Dapper query updating single entity in table <paramref name="tableName"/> defined using direct key equality. 
        /// </summary>
        /// <param name="tableName">Table to update entity from</param>
        /// <param name="asNoKeyFieldAssignmentsSet">Field assignment string without a key (eg. Name = @Name, Value = @Value etc)</param>
        /// <param name="keyField">Key field to find updatable entity</param>
        /// <param name="keyArg">Key arg to find updatable entity</param>
        /// <returns>Dapper SQL update query by id</returns>
        string Update(string tableName, string asNoKeyFieldAssignmentsSet, string keyField, string keyArg);

        /// <summary>
        /// Generates Dapper query performing batch update of entities from table <paramref name="tableName"/> 
        /// using set-based approach and join with temporary table <paramref name="tempTableName"/>.
        /// </summary>
        /// <param name="tableName">Table of entities to update</param>
        /// <param name="tempTableName">Table of inserted values to be used in joined batch update</param>
        /// <param name="asNoKeyFieldAssignmentsSet">Field assignment string without a key (eg. Name = @Name, Value = @Value etc)</param>
        /// <param name="keyField">Key field to find updatable entity</param>
        /// <param name="keyArg">Key arg to find updatable entity</param>
        /// <returns>Dapper SQL query performing batch update</returns>
        string UpdateBatch(string tableName, string tempTableName, string asNoKeyFieldAssignmentsSet, string keyField, string keyArg);
    }
}