using Dapper.AutoQuery.Lib.Core;
using Dapper.AutoQuery.TestModels;
using Xunit.Abstractions;

namespace Dapper.AutoQuery.Lib.Tests
{
    public class SqlTesterTests
    {
        private readonly ITestOutputHelper _testOutput;


        public SqlTesterTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            AutoQueryGenerator
                .CreateBuilder()
                .SetUpSQLServer()
                .SetUpLogAction(Console.WriteLine)
                .Build();
        }
                 

        [Fact()]
        public void DeleteByIdListTest()
        {
            var sql = """
                DELETE FROM [TestModels] 
                WHERE 
                    [Id] IN @Ids 
                """;

            Assert.Equal(sql, AutoQueryGenerator.DeleteByIdList<TestModel>());
        }

        [Fact()]
        public void DeleteSingleTest()
        {
            var sql = """
                DELETE FROM [TestModels] 
                WHERE 
                    [Id] = @Id 
                """;

            Assert.Equal(sql, AutoQueryGenerator.DeleteSingle<TestModel>());
        }

        [Fact()]
        public void CreateIdTempTableTest()
        {
            var tempTableName = "##temp_1";
            var sql = $"""
                CREATE TABLE {tempTableName}(Id INT)
                """;

            Assert.Equal(sql, AutoQueryGenerator.CreateIdTempTable(tempTableName, "INT"));
        }

        [Fact()]
        public void InsertTest()
        {
            var sql = $"""
                INSERT INTO [TestModels] 
                (
                    [Name]
                    ,[Created]
                    ,[CustomName] 
                )
                OUTPUT INSERTED.[Id] 
                VALUES
                (
                    @Name
                    ,@Created
                    ,@FieldWithCustomName 
                )
                """;
            Assert.Equal(sql, AutoQueryGenerator.Insert<TestModel>());
        }

        [Fact()]
        public void SelectTest()
        {
            var sql = """
                SELECT 
                    [Id]
                    ,[Name]
                    ,[Created]
                    ,[CustomName] 
                FROM [TestModels] 
                """;

            Assert.Equal(sql, AutoQueryGenerator.Select<TestModel>());
        }

        [Fact()]
        public void SelectOneByIdTest()
        {
            var sql = """
                SELECT 
                    [Id]
                    ,[Name]
                    ,[Created]
                    ,[CustomName] 
                FROM [TestModels] 
                WHERE 
                    [Id] = @Id
                """;

            Assert.Equal(sql, AutoQueryGenerator.SelectOneById<TestModel, int>());
        }

        [Fact()]
        public void UpdateTest()
        {
            var sql = """
                UPDATE [TestModels] 
                SET
                    [Name] = @Name
                    ,[Created] = @Created
                    ,[CustomName] = @FieldWithCustomName 
                WHERE
                    [Id] = @Id 
                """;

            Assert.Equal(sql, AutoQueryGenerator.Update<TestModel>());
        }

        [Fact()]
        public void SelectByIdListTest()
        {
            var sql = """
                SELECT 
                    [Id]
                    ,[Name]
                    ,[Created]
                    ,[CustomName] 
                FROM [TestModels] 
                WHERE 
                    [Id] IN @Ids
                """;

            Assert.Equal(sql, AutoQueryGenerator.SelectByIdList<TestModel, int>());
        }
    }
}