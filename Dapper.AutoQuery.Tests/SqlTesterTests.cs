using Dapper.AutoQuery.TestModels;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Xunit.Abstractions;

namespace Dapper.AutoQuery.Lib.Tests
{
    public class SqlTesterTests
    {
        private readonly ITestOutputHelper _testOutput;


        public SqlTesterTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
            DAQDefaults
                .CreateBuilder()
                .SetNotMappedAttributeType(typeof(NotMappedAttribute))
                .SetKeyAttributeType(typeof(KeyAttribute))
                .SetTableAttributeType(typeof(TableAttribute))
                .SetColumnAttributeType(typeof(ColumnAttribute))
                .SetLayoutPolicy(FieldLayoutPolicy.StartCommaMultilineOneTab)
                .SetVarPrefix("@")
                .SetUpLogAction(Console.WriteLine)
                .Build();
        }
                 

        [Fact()]
        public void DeleteByIdListTest()
        {
            var sql = """
                DELETE FROM TestModels 
                WHERE 
                    Id IN @Ids 
                """;

            Assert.Equal(sql, AutoQuery.DeleteByIdList<TestModel>());
        }

        [Fact()]
        public void DeleteSingleTest()
        {
            var sql = """
                DELETE FROM TestModels 
                WHERE 
                    Id = @Id 
                """;

            Assert.Equal(sql, AutoQuery.DeleteSingle<TestModel>());
        }

        [Fact()]
        public void CreateIdTempTableTest()
        {
            var tempTableName = "##temp_1";
            var sql = $"""
                CREATE TABLE {tempTableName}(Id INT)
                """;

            Assert.Equal(sql, AutoQuery.CreateIdTempTable(tempTableName, "INT"));
        }

        [Fact()]
        public void InsertTest()
        {
            var sql = $"""
                INSERT INTO TestModels 
                (
                    Name
                    ,Created
                    ,CustomName 
                )
                VALUES
                (
                    @Name
                    ,@Created
                    ,@FieldWithCustomName 
                )
                """;
            Assert.Equal(sql, AutoQuery.Insert<TestModel>());
        }

        [Fact()]
        public void SelectTest()
        {
            var sql = """
                SELECT 
                    Id
                    ,Name
                    ,Created
                    ,CustomName 
                FROM TestModels 
                """;

            Assert.Equal(sql, AutoQuery.Select<TestModel>());
        }

        [Fact()]
        public void SelectOneByIdTest()
        {
            var sql = """
                SELECT 
                    Id
                    ,Name
                    ,Created
                    ,CustomName 
                FROM TestModels 
                WHERE 
                    Id = @Id
                """;

            Assert.Equal(sql, AutoQuery.SelectOneById<TestModel, int>());
        }

        [Fact()]
        public void UpdateTest()
        {
            var sql = """
                UPDATE TestModels 
                SET
                    Name = @Name
                    ,Created = @Created
                    ,CustomName = @FieldWithCustomName 
                WHERE
                    Id = @Id 
                """;

            Assert.Equal(sql, AutoQuery.Update<TestModel>());
        }

        [Fact()]
        public void SelectByIdListTest()
        {
            var sql = """
                SELECT 
                    Id
                    ,Name
                    ,Created
                    ,CustomName 
                FROM TestModels 
                WHERE 
                    Id IN @Ids
                """;

            Assert.Equal(sql, AutoQuery.SelectByIdList<TestModel, int>());
        }
    }
}