using Dapper.AutoQuery.Lib;
using Humanizer;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.AutoQuery.TestModels
{
    public class TestModel
    {
        public TestModel()
        {
            
        }

        public int Id { get; set; }
        public string Name { get; set; }

        [Column("CustomName")]
        public string FieldWithCustomName { get; set; }

    }
}