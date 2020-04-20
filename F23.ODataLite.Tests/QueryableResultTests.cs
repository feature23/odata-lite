using F23.ODataLite.Tests.Fakes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace F23.ODataLite.Tests
{
    public class QueryableResultTests : CommonTests, IClassFixture<QueryableResultTestContext>
    {
        private readonly QueryableResultTestContext _testContext;

        public QueryableResultTests(QueryableResultTestContext testContext)
            : base(() => GetQueryablePersons(testContext.DbContext))
        {
            _testContext = testContext;
        }

        private static (IEnumerable<TestPerson>, IAsyncDisposable) GetQueryablePersons(
            Func<TestPersonDbContext> getDbContext)
        {
            var dbContext = getDbContext();

            return (dbContext.TestPersons, dbContext);
        }

        [Fact]
        public async Task Queryable_Filter_Id_42()
        {
            await Filter_Id_42();
        }

        [Fact]
        public async Task Queryable_Filter_Uuid()
        {
            await Filter_Uuid();
        }

        [Fact]
        public async Task Queryable_Filter_FirstName_Sarah()
        {
            await Filter_FirstName_Sarah();
        }

        [Fact]
        public async Task Queryable_Filter_FirstName_Sarah_LastName_Corkery()
        {
            await Filter_FirstName_Sarah_LastName_Corkery();
        }

        [Fact]
        public async Task Queryable_Filter_BirthDate_After_2005()
        {
            await Filter_BirthDate_After_2005();
        }

        [Fact]
        public async Task Queryable_Filter_AllTheMoney_Equals_Number()
        {
            await Filter_AllTheMoney_Equals_Number();
        }

        [Fact]
        public async Task Queryable_Filter_AllTheMoney_Equals_NumericString()
        {
            await Filter_AllTheMoney_Equals_NumericString();
        }

        [Fact]
        public async Task Queryable_Filter_AllTheMoney_GreaterThan_Number()
        {
            await Filter_AllTheMoney_GreaterThan_Number();
        }

        [Fact]
        public async Task Queryable_Filter_AllTheMoney_GreaterThan_NumericString()
        {
            await Filter_AllTheMoney_GreaterThan_NumericString();
        }

        [Fact]
        public async Task Queryable_Filter_FavoriteBigNumber_GreaterThan_Integer()
        {
            await Filter_FavoriteBigNumber_GreaterThan_Integer();
        }

        [Fact]
        public async Task Queryable_Filter_Weight_GreaterThan_Number()
        {
            await Filter_Weight_GreaterThan_Number();
        }

        [Fact]
        public async Task Queryable_Filter_Weight_GreaterThan_NumericString()
        {
            await Filter_Weight_GreaterThan_NumericString();
        }

        [Fact]
        public async Task Queryable_Filter_Pet_IsCat()
        {
            await Filter_Pet_IsCat();
        }

        [Fact]
        public async Task Queryable_Filter_Pet_HasNoPet()
        {
            await Filter_Pet_HasNoPet();
        }

        [Fact]
        public async Task Queryable_OrderBy()
        {
            await OrderBy();
        }

        [Fact]
        public async Task Queryable_Select()
        {
            await Select();
        }

        [Fact]
        public async Task Queryable_Skip()
        {
            await Skip();
        }

        [Fact]
        public async Task Queryable_Top()
        {
            await Top();
        }

        [Fact]
        public async Task Queryable_Count()
        {
            await Count();
        }
    }
}
