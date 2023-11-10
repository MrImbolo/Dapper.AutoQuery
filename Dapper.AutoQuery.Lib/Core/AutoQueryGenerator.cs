using Dapper.AutoQuery.Lib.Configuration;
using Dapper.AutoQuery.Lib.Queries;
using System.Linq.Expressions;
using System.Text;

namespace Dapper.AutoQuery.Lib.Core
{

    public static class AutoQueryGenerator
    {
        private static Action<string> _logAction = null!;
        private static CustomQueryTranslator _translator = new();
        private static Dictionary<string, string> _expressionCache = new();

        public static AutoQueryConfiguration Configuration { get; internal set; } = null!;

        public static IAutoQueryConfigurationBuilder CreateBuilder()
                => new SQLDependentConfigurationBuilder();

        static EntityManager<T> GetManager<T>() where T : class => EntityManager.GetInstance<T>();
        public static string? DeleteByIdList<T>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.DeleteByIdList(
                entityManager.TableName,
                entityManager.Members.KeyInSet);

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string? DeleteSingle<T>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.DeleteSingle(
                entityManager.TableName,
                entityManager.Members.KeyEquality
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string CreateIdTempTable(string tempTableName, string sqlIdType)
        {
            var sql = Configuration.QueryLibrary.CreateIdTempTable(tempTableName, sqlIdType);

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string GetIdsAndDropIdTempTable(string tempTableName)
        {
            var sql = Configuration.QueryLibrary.GetIdsAndDropIdTempTable(tempTableName);

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Insert<T>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Insert(
                    entityManager.TableName,
                    entityManager.Members.AsNoKeyFieldSet,
                    entityManager.Members.KeyField,
                    entityManager.Members.AsNoKeyArgSet
                );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Select<T>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Select(
                entityManager.Members.AsFullFieldSet,
                entityManager.TableName
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        /// <summary>
        /// Select query with where clause.
        /// </summary>
        /// <typeparam name="T">Entity to query</typeparam>
        /// <param name="whereClause">Filter statement starting with 'WHERE' including table field names and variable names from filter object eg. 
        /// <br /> WHERE Name LIKE @Name</param>
        /// <returns></returns>
        public static string Select<T>(string? whereClause)
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Select(
                entityManager.Members.AsFullFieldSet,
                entityManager.TableName,
                whereClause
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Select<T>(IWhereClauseArgs<T> args)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Select(
                entityManager.Members.AsFullFieldSet,
                entityManager.TableName,
                "WHERE " + args.ToWhereClause()
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string SelectOneById<T, TId>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.SelectOneById(
                entityManager.Members.AsFullFieldSet,
                entityManager.TableName,
                entityManager.Members.KeyEquality
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Update<T>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Update(
                entityManager.TableName,
                entityManager.Members.AsNoKeyFieldAssignmentsSet,
                entityManager.Members.KeyField,
                entityManager.Members.KeyArg
            );

            _logAction?.Invoke(sql);

            return sql;
        }
        
        public static string UpdateBatch<T>(string tempTableName)
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.UpdateBatch(
                entityManager.TableName,
                tempTableName,
                entityManager.Members.AsNoKeyFieldAssignmentsSet,
                entityManager.Members.KeyField,
                entityManager.Members.KeyArg
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string SelectByIdList<T, TId>()
            where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.SelectByIdList(
                entityManager.Members.AsFullFieldSet,
                entityManager.TableName,
                entityManager.Members.KeyInSet
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string TruncateBatchTable<T>(string tempTableName) where T : class
        {
            var sql = Configuration.QueryLibrary.TruncateBatchTable(tempTableName);

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string InsertBatch<T>(string tempTableName) where T : class
        {
            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.InsertBatch(
                entityManager.TableName,
                entityManager.Members.AsNoKeyFieldSet,
                tempTableName,
                entityManager.Members.AsNoKeyArgSet
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Select<T, TArgs>(Expression<Func<T, TArgs, bool>>? selector = null) where T : class
        {
            string? whereClause = null;
            if (selector is not null)
            {
                if (!_expressionCache.TryGetValue(selector.ToString(), out var expr))
                {
                    expr = _translator.Translate(selector);
                    _expressionCache.Add(selector.ToString(), expr);
                }

                whereClause = "WHERE " + expr;
            }

            EntityManager<T> entityManager = GetManager<T>();
            var sql = Configuration.QueryLibrary.Select(
                entityManager.Members.AsFullFieldSet,
                GetManager<T>().TableName,
                whereClause
            );

            _logAction?.Invoke(sql);

            return sql;
        }

        private static void SanitizeWhereParams(StringBuilder whereClauseBuilder, string param, string replacer = "")
        {
            whereClauseBuilder.Replace(param + ".", replacer);
        }
    }

    internal enum EContainsType
    {
        String,
        Collection,
        None
    }
}