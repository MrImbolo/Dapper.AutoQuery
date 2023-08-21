using Xunit;
using Dapper.AutoQuery.Lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Dapper.AutoQuery.TestModels;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
                .SetGlobally();
        }

        [Fact()]
        public void BuildInsertTest()
        {
            var sql = $"""
                INSERT INTO TestModels 
                (
                    Name
                    ,CustomName 
                )
                VALUES
                (
                    @Name
                    ,@FieldWithCustomName 
                )
                """;
            Assert.Equal(sql, SqlQueryGenerator.Insert<TestModel>());
        }

        [Fact()]
        public void BuildSelectTest()
        {
            var sql = """
                SELECT 
                    Id
                    ,Name
                    ,CustomName 
                FROM TestModels 
                """;

            Assert.Equal(sql, SqlQueryGenerator.Select<TestModel>());
        }

        [Fact()]
        public void BuildUpdateTest()
        {
            var sql = """
                UPDATE TestModels 
                SET
                    Name = @Name
                    ,CustomName = @FieldWithCustomName 
                WHERE
                    Id = @Id 
                """;

            Assert.Equal(sql, SqlQueryGenerator.Update<TestModel>());
        }

        [Fact()]
        public void BuildDeleteTest()
        {
            var sql = """
                DELETE FROM TestModels 
                WHERE 
                    Id IN @Ids 
                """;

            Assert.Equal(sql, SqlQueryGenerator.DeleteByIdList<TestModel>());
        }

        [Fact()]
        public void DeleteByIdListTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void DeleteSingleTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void CreateIdTempTableTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void InsertTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void SelectTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void SelectOneByIdTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void UpdateTest()
        {
            Assert.True(false, "This test needs an implementation");
        }

        [Fact()]
        public void SelectByIdListTest()
        {
            Assert.True(false, "This test needs an implementation");
        }
    }
}