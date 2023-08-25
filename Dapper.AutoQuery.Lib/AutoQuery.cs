using System;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;

namespace Dapper.AutoQuery.Lib
{
    public interface IWhereClauseArgs
    {
        string ToWhereClause();
    }
    public class AutoQuery
    {
        private static Action<string> _logAction;
        private static List<Type> _typesWithColumnAttribute = new List<Type>();
        private static CustomQueryTranslator _translator = new();
        private static Dictionary<string, string> _expressionCache = new Dictionary<string, string>();
        private static List<Type> GetTypesWithColumnAttribute()
        {
            return Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(x => x.IsClass
                && x.GetProperties().Any(x => x.GetCustomAttribute(DAQDefaults.Configuration.ColumnAttributeType, true) is not null))
            .ToList();
        }

        public static void SetUpCustomColumnAttributeSupport()
        {
            _typesWithColumnAttribute = GetTypesWithColumnAttribute();
            foreach (var type in _typesWithColumnAttribute)
            {
                SqlMapper.SetTypeMap(type, new CustomPropertyTypeMap(type, (type, columnName) =>
                {
                    return type
                        .GetProperties()
                        .FirstOrDefault(prop => //true
                        {
                            var attr = prop.GetCustomAttribute(DAQDefaults.Configuration.ColumnAttributeType, false);
                            var columnAttributeNamePropertyValue = (string?)DAQDefaults.Configuration.ColumnAttributeType
                                .GetProperty("Name")?.GetValue(attr);
                            return columnAttributeNamePropertyValue == columnName;
                        });
                }));
            }
        }
        public static void SetUpLogAction(Action<string> action) => _logAction = action;
        public static IEntityManagerConfigurationBuilder CreateBuilder() => DAQDefaults.CreateBuilder();

        static EntityManager<T> GetManager<T>() where T : class => EntityManager.GetInstance<T>();
        public static string? DeleteByIdList<T>()
            where T : class
        {
            var sql = $"""
                DELETE FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyInSet} 
                """;

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string? DeleteSingle<T>()
            where T : class
        {
            var sql = $"""
                DELETE FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyEquality} 
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string CreateIdTempTable(string tempTableName, string sqlIdType)
        {
            var sql = $"CREATE TABLE {tempTableName}(Id {sqlIdType})";

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string GetIdsAndDropIdTempTable(string tempTableName)
        {
            var sql = $"""
                SELECT * FROM {tempTableName}; 
                DROP TABLE {tempTableName};
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Insert<T>()
            where T : class
        {
            var sql = $"""
                INSERT INTO {GetManager<T>().TableName} 
                (
                    {GetManager<T>().Members.AsNoKeyFieldSet} 
                )
                VALUES
                (
                    {GetManager<T>().Members.AsNoKeyArgSet} 
                )
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Select<T>()
            where T : class
        {
            var sql = $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string SelectOneById<T, TId>()
            where T : class
        {
            var sql = $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyEquality}
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string Update<T>()
            where T : class
        {
            var sql = $"""
                UPDATE {GetManager<T>().TableName} 
                SET
                    {GetManager<T>().Members.AsNoKeyFieldAssignmentsSet} 
                WHERE
                    {GetManager<T>().Members.KeyField} = {GetManager<T>().Members.KeyArg} 
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string SelectByIdList<T, TId>()
            where T : class
        {
            var sql = $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyInSet}
                """;

            _logAction?.Invoke(sql);

            return sql;
        }

        public static string TruncateBatchTable<T>(string tempTableName) where T : class
        {
            var sql = $"""
                DELETE FROM {tempTableName}
                """;

            _logAction?.Invoke(sql);

            return sql;
        }
        public static string InsertBatch<T>(string tempTableName) where T : class
        {
            var sql = $"""
                INSERT INTO {GetManager<T>().TableName} 
                (
                    {GetManager<T>().Members.AsNoKeyFieldSet} 
                )
                OUTPUT INSERTED.Id INTO {tempTableName}
                VALUES
                (
                    {GetManager<T>().Members.AsNoKeyArgSet} 
                )
                """;

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

            var sql = $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet}
                FROM {GetManager<T>().TableName}
                {whereClause}
                """;

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