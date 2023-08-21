using System.Data;
using System.Numerics;

namespace Dapper.AutoQuery.Lib;

public static class DapperExtensions
{
    public static async Task<T?> SelectByIdAsync<T, TKey>(
        this IDbConnection db,
        TKey id,
        IDbTransaction? transaction = null)
        where T : class
        where TKey : INumber<TKey>
    {
        var item = await db.QueryFirstOrDefaultAsync<T>(
            SqlQueryGenerator.SelectOneById<T, TKey>(), 
            new { Id = id }, 
            transaction);

        return item;
    }
    public static async Task<IEnumerable<T>> SelectByIdListAsync<T, TKey>(
        this IDbConnection db,
        TKey[] ids,
        IDbTransaction? transaction = null)
        where T : class
    {
        var items = await db.QueryAsync<T>(
            SqlQueryGenerator.SelectByIdList<T, TKey>(), 
            new { Ids = ids }, 
            transaction);

        return items;
    }

    public static async Task DeleteAsync<T, TKey>(
        this IDbConnection db,
        TKey[] ids,
        IDbTransaction? transaction = null)
        where T : class
    {
        await db.ExecuteAsync(
            SqlQueryGenerator.DeleteByIdList<T>(), 
            new { Ids = ids }, 
            transaction);
    }
    
    public static async Task DeleteSingleAsync<T>(
        this IDbConnection db,
        T item,
        IDbTransaction? transaction = null)
        where T : class
    {
        await db.ExecuteAsync(
            SqlQueryGenerator.DeleteSingle<T>(), 
            item, 
            transaction);
    }
    
    public static async Task UpdateAsync<T>(
        this IDbConnection db,
        T[] items,
        IDbTransaction? transaction = null)
        where T : class
    {
        await db.ExecuteAsync(
            SqlQueryGenerator.Update<T>(),
            items, 
            transaction);
    }
    
    public static async Task InsertAsync<T, TKey>(
        this IDbConnection db,
        T[] items,
        Action<T, TKey> assignId,
        IDbTransaction? transaction = null)
        where T : class
    {
        var externalTransaction = transaction is not null;

        db.Open();
        transaction ??= db.BeginTransaction();

        var tempTableName = "temp_" + Guid.NewGuid().ToString("N");
        var tempTableCreateQuery = SqlQueryGenerator.CreateIdTempTable(tempTableName, "INT");

        await db.ExecuteAsync(tempTableCreateQuery, transaction: transaction);

        foreach (var chunk in items.Chunk(500))
        {
            await db.ExecuteAsync(
                SqlQueryGenerator.InsertBatch<T>(tempTableName),
                chunk, 
                transaction);
        }

        var ids = await db.QueryAsync<TKey>($"SELECT * FROM {tempTableName}", transaction: transaction);

        foreach (var (item, id) in items.Zip(ids))
        {
            assignId(item, id);
        }

        if (!externalTransaction)
        {
            transaction.Commit();
        }
    }

}
