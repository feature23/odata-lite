using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;

namespace F23.ODataLite.Tests.Fakes
{
    public class TestPersonDbContext : DbContext
    {
        private static readonly Lazy<DbContextOptions<TestPersonDbContext>> _lazyOptions =
            new Lazy<DbContextOptions<TestPersonDbContext>>(CreateOptions);

        public TestPersonDbContext() : base(_lazyOptions.Value)
        {
        }

        public DbSet<TestPerson> TestPersons { get; set; } = null!;

        private static DbContextOptions<TestPersonDbContext> CreateOptions()
        {
            var sqliteConnection = new SqliteConnection("DataSource=:memory:");
            sqliteConnection.Open();

            return new DbContextOptionsBuilder<TestPersonDbContext>()
                //.UseLoggerFactory(LoggerFactory.Create(log => log.AddDebug()))
                .UseSqlite(sqliteConnection)
                .Options;
        }
    }
}
