using F23.ODataLite.Tests.Fakes;
using System.Collections.Generic;

namespace F23.ODataLite.Tests
{
    public class EnumerableResultTestContext
    {
        public readonly IEnumerable<TestPerson> FakePersons = FakeFactory.CreateEnumerable();
    }
}
