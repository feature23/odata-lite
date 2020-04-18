using F23.ODataLite.Internal;
using F23.ODataLite.Tests.Fakes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace F23.ODataLite.Tests
{
    public abstract class CommonTests
    {
        private readonly Func<(IEnumerable<TestPerson>, IAsyncDisposable)> _fakeDataFactory;

        public CommonTests(Func<(IEnumerable<TestPerson>, IAsyncDisposable)> fakeDataFactory)
        {
            _fakeDataFactory = fakeDataFactory;
        }


        public async Task Filter_Id_42()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Id)} eq 42"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.Equal(42, p.Id));
            }
        }

        public async Task Filter_Uuid()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Uuid)} eq '1037cfdd-ed0b-5b6a-108d-b948551068d9'"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                var guid = Guid.Parse("1037cfdd-ed0b-5b6a-108d-b948551068d9");
                Assert.All(persons, p => Assert.Equal(guid, p.Uuid));
            }
        }

        public async Task Filter_FirstName_Sarah()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.FirstName)} eq Sarah"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.Equal("Sarah", p.FirstName));
            }
        }

        public async Task Filter_FirstName_Sarah_LastName_Corkery()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.FirstName)} eq Sarah and {nameof(TestPerson.LastName)} eq Corkery"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p =>
                {
                    Assert.Equal("Sarah", p.FirstName);
                    Assert.Equal("Corkery", p.LastName);
                });
            }
        }

        public async Task Filter_BirthDate_After_2005()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.BirthDate)} ge '1/1/2005'"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                var date = new DateTime(2005, 1, 1);
                Assert.All(persons, p => Assert.True(p.BirthDate >= date));
            }
        }

        public async Task Filter_AllTheMoney_Equals_Number()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.AllTheMoney)} eq 95090.03"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.Equal(95090.03m, p.AllTheMoney));
            }
        }

        public async Task Filter_AllTheMoney_Equals_NumericString()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.AllTheMoney)} eq '95090.03'"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.Equal(95090.03m, p.AllTheMoney));
            }
        }

        public async Task Filter_AllTheMoney_GreaterThan_Number()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.AllTheMoney)} gt 95090.03"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.AllTheMoney > 95090.03m));
            }
        }

        public async Task Filter_AllTheMoney_GreaterThan_NumericString()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.AllTheMoney)} gt '95090.03'"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.AllTheMoney > 95090.03m));
            }
        }

        public async Task Filter_FavoriteBigNumber_GreaterThan_Integer()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.FavoriteBigNumber)} gt 100"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.FavoriteBigNumber > 100));
            }
        }

        public async Task Filter_Weight_GreaterThan_Number()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Weight)} gt 200.5"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.Weight > 200.5d));
            }
        }

        public async Task Filter_Weight_GreaterThan_NumericString()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Weight)} gt '200.5'"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.Weight > 200.5d));
            }
        }

        public async Task Filter_Pet_IsCat()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Pet)} eq Cat"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.Pet == TestPets.Cat));
            }
        }

        public async Task Filter_Pet_HasNoPet()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Filter] = $"{nameof(TestPerson.Pet)} eq null"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.All(persons, p => Assert.True(p.Pet == null));
            }
        }

        public async Task OrderBy()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.OrderBy] = $"{nameof(TestPerson.FirstName)}, {nameof(TestPerson.LastName)}"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>().ToList();

                var expectedPersons = data
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .ToList();

                int index = 0;
                Assert.All(persons, r =>
                {
                    var expected = expectedPersons[index++];
                    Assert.Equal(expected.Id, r.Id);
                });
            }
        }

        public async Task Select()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Select] = $"{nameof(TestPerson.FirstName)}, {nameof(TestPerson.LastName)}"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var results = ((IEnumerable<object>)value).ToList();

                var type = results.First().GetType();
                Assert.True(type.IsAnonymousType(requireCompilerGenerated: false));

                var firstNameField = type.GetField(nameof(TestPerson.FirstName));
                var lastNameField = type.GetField(nameof(TestPerson.LastName));

                var fakeDataList = data.ToList();

                int index = 0;
                Assert.All(results, r =>
                {
                    var expected = fakeDataList[index++];

                    Assert.Equal(expected.FirstName, firstNameField.GetValue(r));
                    Assert.Equal(expected.LastName, lastNameField.GetValue(r));
                });
            }
        }

        public async Task Skip()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Skip] = $"{FakeFactory.TestPersonCount - 1}"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.Single(persons);
            }
        }

        public async Task Top()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Top] = "1"
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                var persons = ((IEnumerable<object>)value).Cast<TestPerson>();

                Assert.Single(persons);
            }
        }

        public async Task Count()
        {
            // Arrange
            var (data, disposer) = _fakeDataFactory();

            await using (disposer)
            {
                var actionResult = new OkObjectResult(data);
                var query = new QueryCollection(new Dictionary<string, StringValues>
                {
                    [ODataQueryParameters.Count] = string.Empty
                });

                // Act
                var oDataResult = await ODataLiteActionResultHandler.HandleResult(actionResult, query);

                // Assert
                Assert.NotNull(oDataResult);
                Assert.IsType<OkObjectResult>(oDataResult);

                var value = ((OkObjectResult)oDataResult).Value;
                Assert.IsType<int>(value);

                Assert.Equal(FakeFactory.TestPersonCount, (int)value);
            }
        }
    }
}
