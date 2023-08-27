using Dapper.AutoQuery.TestModels;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace Dapper.AutoQuery.Lib.Tests
{
    
    public class DapperExtensionsTests
    {
        private SqlConnection _db;

        public DapperExtensionsTests(ITestOutputHelper testOutput)
        {
            var connString = "Data Source=ZTECBOOK\\SQLEXPRESS;Initial Catalog=TheWood.Dev;Integrated Security=True;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False";
            _db = new SqlConnection(connString);
            DAQDefaults
                .CreateBuilder()
                .SetNotMappedAttributeType(typeof(NotMappedAttribute))
                .SetKeyAttributeType(typeof(KeyAttribute))
                .SetTableAttributeType(typeof(TableAttribute))
                .SetColumnAttributeType(typeof(ColumnAttribute))
                .SetLayoutPolicy(FieldLayoutPolicy.StartCommaMultilineOneTab)
                .SetVarPrefix("@")
                .SetUpLogAction(testOutput.WriteLine)
                .Build();

            EntityManager.WarmUp<Product>();
        }
        [Fact()]
        public async Task DapperExtensions_SelectByIdAsync_OkAsync()
        {
            var item = await _db.SelectByIdAsync<Product, int>(36);
            Assert.NotNull(item);
        }

        [Fact()]
        public async Task DapperExtensions_SelectByIdListAsync_Ok()
        {
            var items = await _db.SelectByIdListAsync<Product, int>(new[] { 36, 37, 38 });
            Assert.NotEmpty(items);
        }
         
        [Fact()]
        public async Task DapperExtensions_SelectAsyncWithWhere_Ok()
        { 
            var items = await _db.SelectAsync(new SelectProductsWhereArgs
            {
                Search = "доска"
            });
            Assert.NotEmpty(items);
        }
        
        [Fact()]
        public async Task DapperExtensions_SelectAsync_Ok()
        {
            var search = new Product
            {
                Name = "доска",
            };
            var items = await _db.SelectAsync<Product, Product>((x, y) => x.Name.Contains(y.Name), search);
            Assert.NotEmpty(items);
        }

        [Fact()]
        public async Task DapperExtensions_DeleteAsync_Ok()
        {
            await _db.DeleteAsync<Product, int>(new[] { 1, 2, 3 });
        }

        [Fact()]
        public async Task DapperExtensions_DeleteSingleAsync_Ok()
        {
            await _db.DeleteSingleAsync(new Product { Id = 33 });
        }

        [Fact()]
        public async Task DapperExtensions_UpdateAsync_Ok()
        {
            var items = await _db.SelectAsync<Product, int>();

            foreach(var item in items)
            {
                item.DateCreated = DateTime.Now.AddDays(1);
            }

            await _db.UpdateAsync(items.ToArray());
        }

        [Fact()]
        public async Task DapperExtensions_InsertAsync_Ok()
        {
            var product = new Product
            {
                Name = "Доска еловая 100*4000",
                CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                DateCreated = DateTime.UtcNow,
            };
            
            var product2 = new Product
            {
                Name = "Доска еловая 100*2500",
                CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                DateCreated = DateTime.UtcNow,
            };
            
            var product3 = new Product
            {
                Name = "Доска сосновая 200*4000",
                CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                DateCreated = DateTime.UtcNow,
            };

            await _db.InsertAsync<Product, int>(new[] { product, product2, product3 }, (x, id) => x.Id = id);
        }
    }
}