using BenchmarkDotNet.Attributes;
using Dapper.AutoQuery.Lib;
using Dapper.AutoQuery.TestModels;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dapper.AutoQuery.ExtensionsBenchmark
{
    [MemoryDiagnoser]
    public class Benchmark
    {
        private SqlConnection _db;
        private Product[] _insertableItems;
        private Product[] _updatableItems;
        private Product _productToDelete;

        public Benchmark()
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
                .Build();

            _productToDelete = new Product { Id = 33 };

            _insertableItems = new[] 
            { 
                new Product
                {
                    Name = "Доска еловая 100*4000",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                }, new Product
                {
                    Name = "Доска еловая 100*2500",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                }, new Product
                {
                    Name = "Доска сосновая 200*4000",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                } 
            };
            _updatableItems = new[] 
            { 
                new Product
                {
                    Id = 34,
                    Name = "Доска еловая 100*4000",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                }, new Product
                {
                    Id = 35,
                    Name = "Доска еловая 100*2500",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                }, new Product
                {
                    Id = 36,
                    Name = "Доска сосновая 200*4000",
                    CreatedById = "3c012057-47d2-429e-af52-dc5db23813e7",
                    CustomerId = "3c012057-47d2-429e-af52-dc5db23813e7",
                    DateCreated = DateTime.UtcNow,
                } 
            };
        }
        [Benchmark]
        public async Task<Product?> DapperExtensions_SelectByIdAsync()
        {
            return await _db.SelectByIdAsync<Product, int>(36);
        }
        
        [Benchmark]
        public async Task<Product?> Dapper_SelectByIdAsync()
        {
            return await _db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = 36 });
        }

        [Benchmark]
        public async Task<List<Product>> DapperExtensions_SelectByIdListAsync()
        {
            return (await _db.SelectByIdListAsync<Product, int>(new[] { 36, 37, 38 })).ToList();
        }
        
        [Benchmark]
        public async Task<List<Product>> Dapper_SelectByIdListAsync()
        {
            return (await _db.QueryAsync<Product>("SELECT * FORM Products WHERE Id IN @Ids", 
                new { Ids = new[] { 36, 37, 38 } })).ToList();
        }

        [Benchmark]
        public async Task<List<Product>> DapperExtensions_SelectAsyncWithWhere()
        {
            return (await _db.SelectAsync(new SelectProductsWhereArgs
            {
                Search = "доска",
            })).ToList();
        }

        [Benchmark]
        public async Task<List<Product>> DapperExtensions_SelectAsync()
        {
            var search = new Product
            {
                Name = "доска",
            };
            return (await _db.SelectAsync<Product, Product>((x, y) => x.Name.Contains(y.Name), search)).ToList();
        }
        
        [Benchmark]
        public async Task<List<Product>> Dapper_SelectAsync()
        {
            return (await _db.QueryAsync<Product>(
                "SELECT TOP(50) * FROM Products WHERE Name LIKE '%' + @Search + '%'", 
                new { Search = "доска" })).ToList();
        }

        [Benchmark]
        public async Task DapperExtensions_DeleteAsync()
        {
            await _db.DeleteAsync<Product, int>(new[] { 1, 2, 3 });
        }
        
        [Benchmark]
        public async Task Dapper_DeleteAsync()
        {
            await _db.ExecuteAsync("DELETE FROM Products WHERE Id IN @Ids", 
                new { Ids = new[] { 1, 2, 3 } });
        }

        [Benchmark]
        public async Task DapperExtensions_DeleteSingleAsync()
        {
            await _db.DeleteSingleAsync(_productToDelete);
        }


        [Benchmark]
        public async Task Dapper_DeleteSingleAsync()
        {
            await _db.ExecuteAsync("DELETE FROM Products WHERE Id = @Id", _productToDelete);
        }

        [Benchmark]
        public async Task DapperExtensions_UpdateAsync()
        {
            await _db.UpdateAsync(_updatableItems.ToArray());
        }
        
        [Benchmark]
        public async Task Dapper_UpdateAsync()
        {
            await _db.ExecuteAsync("""
                UPDATE Products 
                SET 
                     Name = @Name 
                    ,DateCreated = @DateCreated 
                    ,CreatedById = @CreatedById 
                    ,CustomerId = @CustomerId 
                    ,OrderId = @OrderId 
                WHERE 
                    Id = @Id
                """, _updatableItems);
        }

        [Benchmark]
        public async Task DapperExtensions_InsertAsync()
        {
            await _db.InsertAsync<Product, int>(_insertableItems, static (x, id) => x.Id = id);
        }
        
        [Benchmark]
        public async Task Dapper_InsertAsync()
        {
            foreach (var item in _insertableItems)
            {
                item.Id = await _db.QueryFirstOrDefaultAsync<int>("""
                    INSERT INTO Products
                    (
                        Name 
                        ,DateCreated 
                        ,CreatedById 
                        ,CustomerId 
                        ,OrderId 
                    ) 
                    OUTPUT INSERTED.Id 
                    VALUES 
                    (
                        @Name 
                        ,@DateCreated 
                        ,@CreatedById 
                        ,@CustomerId 
                        ,@OrderId 
                    )
                    """, item);
            }
        }
    }
}
