namespace Dapper.AutoQuery.Lib
{
    public interface IWhereClauseArgs
    {
        string ToWhereClause();
    }
    public class SqlQueryGenerator
    {
        //private static EntityManagerV2<T> GetManager<T>();

        //public SqlTester(EntityManagerV2<T> entityManager)
        //{
        //    GetManager<T>() = entityManager;
        //}

        static EntityManager<T> GetManager<T>() where T : class => EntityManager.GetInstance<T>();
        public static string? DeleteByIdList<T>()
            where T : class
        {
            return $"""
                DELETE FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyInSet} 
                """;
        }
        public static string? DeleteSingle<T>()
            where T : class
        {
            return $"""
                DELETE FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyEquality} 
                """;
        }
        public record TempTableParams
        {
            public required string Name { get; init; }
            public required string Command { get; init; }
        }
        public static string CreateIdTempTable(string tempTableName, string sqlIdType)
        {
            return $"CREATE TABLE #temp_{tempTableName}(Id {sqlIdType})";
        }

        public static string Insert<T>()
            where T : class
        {
            return $"""
                INSERT INTO {GetManager<T>().TableName} 
                (
                    {GetManager<T>().Members.AsNoKeyFieldSet} 
                )
                VALUES
                (
                    {GetManager<T>().Members.AsNoKeyArgSet} 
                )
                """;
        }

        public static string Select<T>()
            where T : class
        {
            return $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                """;
        }
        
        public static string SelectOneById<T, TId>()
            where T : class
        {
            return $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyEquality}
                """;
        }

        public static string Update<T>()
            where T : class
        {
            //SqlMapper.AddTypeHandler(new DirectComparableWhereArgHandler<int>());
            return $"""
                UPDATE {GetManager<T>().TableName} 
                SET
                    {GetManager<T>().Members.AsNoKeyFieldAssignmentsSet} 
                WHERE
                    {GetManager<T>().Members.KeyField} = {GetManager<T>().Members.KeyArg} 
                """;
        }

        public static string SelectByIdList<T, TId>()
            where T : class
        {
            return $"""
                SELECT 
                    {GetManager<T>().Members.AsFullFieldSet} 
                FROM {GetManager<T>().TableName} 
                WHERE 
                    {GetManager<T>().Members.KeyInSet}
                """;
        }

        internal static string TruncateBatchTable<T>(string tempTableName) where T : class
        {
            return $"""
                DELETE FROM {tempTableName}
                """;
        }
        internal static string InsertBatch<T>(string tempTableName) where T : class
        {
            return $"""
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
        }
    }
    //public sealed class DirectComparableWhereArgHandler<T> : TypeHandler<DirectComparableWhereArg<T>>, ITypeHandler
    //    where T : IEquatable<T>
    //{
    //    public override DirectComparableWhereArg<T> Parse(object value)
    //    {
    //        if (value is T i)
    //            return new DirectComparableWhereArg<T> { Value = i };

    //        throw new InvalidCastException(
    //            $"Object '{value}' is not a string and is not convertable to {nameof(DirectComparableWhereArg<T>)}.");
    //    }

    //    public override void SetValue(IDbDataParameter parameter, DirectComparableWhereArg<T> value)
    //    {
    //        parameter.Value = value.Value;
    //    }
    //}
    //public class DirectComparableWhereArg<T>
    //    where T : IEquatable<T>
    //{
    //    public T Value { get; set; }

    //    public static implicit operator T(DirectComparableWhereArg<T> obj) => obj.Value;
    //    public static implicit operator DirectComparableWhereArg<T>(T obj) => new DirectComparableWhereArg<T> { Value = obj };
    //}
    //public class SearchWhereArg
    //{

    //}
    //public class WhereArg
    //{
    //    public DirectComparableWhereArg<int> Id { get; set; } // Id = @Id
    //    public string Search { get; set; } // Name LIKE '%' + @Search OR Email LIKE '%' + @Search + '%'

    //    public DateTime DateFrom { get; set; }
    //    public DateTime DateTo { get; set; }
    //    public string DateFilter { get; set; } 
    //    // CASE WHEN DateFilter = 'CreatedUTC' THEN CreatedUTC > DateFrom AND CreatedUTC < DateTo

    //    //public int[] Ids { get; set; } // Id IN @Ids

    //    public string Ids { get; set; } // Id IN (SELECT * FROM STRING_SPLIT(@Ids))


    //}
}
