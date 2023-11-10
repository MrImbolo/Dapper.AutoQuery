using Dapper.AutoQuery.Lib.Core;
using Dapper.AutoQuery.Lib.Queries;
using System.Data;
using System.Linq.Expressions;
using System.Numerics;
namespace Dapper.AutoQuery.Lib.Extensions;

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
            AutoQueryGenerator.SelectOneById<T, TKey>(),
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
            AutoQueryGenerator.SelectByIdList<T, TKey>(),
            new { Ids = ids },
            transaction);

        return items;
    }

    public static async Task<IEnumerable<T>> SelectAsync<T, TSelectArgs>(
        this IDbConnection db,
        IDbTransaction? transaction = null)
        where T : class
    {
        var items = await db.QueryAsync<T>(
            AutoQueryGenerator.Select<T, TSelectArgs>(),
            transaction);

        return items;
    }

    public static async Task<IEnumerable<T>> SelectAsync<T, TSelectArgs>(
        this IDbConnection db,
        Expression<Func<T, TSelectArgs, bool>> selector,
        TSelectArgs args,
        IDbTransaction? transaction = null)
        where T : class
    {
        var items = await db.QueryAsync<T>(
            AutoQueryGenerator.Select(selector),
            args,
            transaction);

        return items;
    }

    public static async Task<IEnumerable<T>> SelectAsync<T>(
        this IDbConnection db,
        IWhereClauseArgs<T> args,
        IDbTransaction? transaction = null)
        where T : class
    {
        var items = await db.QueryAsync<T>(
            AutoQueryGenerator.Select(args),
            args,
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
            AutoQueryGenerator.DeleteByIdList<T>(),
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
            AutoQueryGenerator.DeleteSingle<T>(),
            item,
            transaction);
    }

    public static async Task UpdateAsync<T>(
        this IDbConnection db,
        T[] items,
        IDbTransaction? transaction = null)
        where T : class
    {
        foreach(var chunk in items.Chunk(1000))
        {
            await db.ExecuteAsync(
                AutoQueryGenerator.Update<T>(),
                chunk,
                transaction);
        }
    }

    public static async Task InsertAsync<T, TKey>(
        this IDbConnection db,
        T[] items,
        Action<T, TKey> assignId,
        IDbTransaction? transaction = null)
        where T : class
    {
        var externalTransaction = transaction is not null;

        if (items.Length >= 100)
        {
            db.Open();
            transaction ??= db.BeginTransaction();

            var tempTableName = "##temp_" + Guid.NewGuid().ToString("N")[..8];
             
            // TODO: get sql db type for c# type. 

            var tempTableQuery = AutoQueryGenerator.CreateIdTempTable(tempTableName, "INT");

            await db.ExecuteAsync(tempTableQuery, transaction: transaction);

            foreach (var chunk in items.Chunk(500))
            {
                await db.ExecuteAsync(
                    AutoQueryGenerator.InsertBatch<T>(tempTableName),
                    chunk,
                    transaction);
            }

            tempTableQuery = AutoQueryGenerator.GetIdsAndDropIdTempTable(tempTableName);

            var ids = await db.QueryAsync<TKey>(
                tempTableQuery,
                transaction: transaction);

            foreach (var (item, id) in items.Zip(ids))
            {
                assignId(item, id);
            }

            if (!externalTransaction)
            {
                transaction.Commit();
            }
            return;
        }
        else
        {
            foreach (var item in items)
            {

                var id = await db.QueryFirstOrDefaultAsync<TKey>(
                    AutoQueryGenerator.Insert<T>(),
                    item,
                    transaction: transaction
                );

                assignId(item, id);
            }
        }
    }

}
