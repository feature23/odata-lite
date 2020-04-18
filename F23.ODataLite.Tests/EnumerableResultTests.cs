using System;
using System.Threading.Tasks;
using Xunit;

namespace F23.ODataLite.Tests
{
    public class EnumerableResultTests : CommonTests, IClassFixture<EnumerableResultTestContext>
    {
        public EnumerableResultTests(EnumerableResultTestContext testContext)
            : base(() => (testContext.FakePersons, NullDisposer.Instance))
        {
        }

        [Fact]
        public async Task Enumerable_Filter_Id_42()
        {
            await Filter_Id_42();
        }

        [Fact]
        public async Task Enumerable_Filter_Uuid()
        {
            await Filter_Uuid();
        }

        [Fact]
        public async Task Enumerable_Filter_FirstName_Sarah()
        {
            await Filter_FirstName_Sarah();
        }

        [Fact]
        public async Task Enumerable_Filter_FirstName_Sarah_LastName_Corkery()
        {
            await Filter_FirstName_Sarah_LastName_Corkery();
        }

        [Fact]
        public async Task Enumerable_Filter_BirthDate_After_2005()
        {
            await Filter_BirthDate_After_2005();
        }

        [Fact]
        public async Task Enumerable_Filter_AllTheMoney_Equals_Number()
        {
            await Filter_AllTheMoney_Equals_Number();
        }

        [Fact]
        public async Task Enumerable_Filter_AllTheMoney_Equals_NumericString()
        {
            await Filter_AllTheMoney_Equals_NumericString();
        }

        [Fact]
        public async Task Enumerable_Filter_AllTheMoney_GreaterThan_Number()
        {
            await Filter_AllTheMoney_GreaterThan_Number();
        }
                
        [Fact]
        public async Task Enumerable_Filter_AllTheMoney_GreaterThan_NumericString()
        {
            await Filter_AllTheMoney_GreaterThan_NumericString();
        }

        [Fact]
        public async Task Enumerable_Filter_FavoriteBigNumber_GreaterThan_Integer()
        {
            await Filter_FavoriteBigNumber_GreaterThan_Integer();
        }

        [Fact]
        public async Task Enumerable_Filter_Weight_GreaterThan_Number()
        {
            await Filter_Weight_GreaterThan_Number();
        }

        [Fact]
        public async Task Enumerable_Filter_Weight_GreaterThan_NumericString()
        {
            await Filter_Weight_GreaterThan_NumericString();
        }

        [Fact]
        public async Task Enumerable_Filter_Pet_IsCat()
        {
            await Filter_Pet_IsCat();
        }

        [Fact]
        public async Task Enumerable_Filter_Pet_HasNoPet()
        {
            await Filter_Pet_HasNoPet();
        }

        [Fact]
        public async Task Enumerable_OrderBy()
        {
            await OrderBy();
        }

        [Fact]
        public async Task Enumerable_Select()
        {
            await Select();
        }

        [Fact]
        public async Task Enumerable_Skip()
        {
            await Skip();
        }

        [Fact]
        public async Task Enumerable_Top()
        {
            await Top();
        }

        [Fact]
        public async Task Enumerable_Count()
        {
            await Count();
        }

        private class NullDisposer : IAsyncDisposable
        {
            public static readonly NullDisposer Instance = new NullDisposer();

            private NullDisposer() { }

            public ValueTask DisposeAsync() => new ValueTask(Task.CompletedTask);
        }
    }
}
