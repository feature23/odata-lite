using F23.ODataLite.Tests.Fakes;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F23.ODataLite.Tests
{
    public class QueryableResultTestContext : IAsyncLifetime
    {
        public TestPersonDbContext DbContext() => new TestPersonDbContext();

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public async Task InitializeAsync()
        {
            await using var dbContext = new TestPersonDbContext();

            await dbContext.Database.EnsureCreatedAsync();

            var fakePeople = FakeFactory.CreateEnumerable().ToList();

            dbContext.TestPersons.AddRange(fakePeople);

            await dbContext.SaveChangesAsync();
        }
    }
}
