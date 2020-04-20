using Bogus;
using Bogus.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace F23.ODataLite.Tests.Fakes
{
    internal static class FakeFactory
    {
        public const int TestPersonCount = 100;

        private static readonly TestPets[] Pets = new[] { TestPets.Cat, TestPets.Dog };

        private static readonly Lazy<List<TestPerson>> _resourcePersons = 
            new Lazy<List<TestPerson>>(LoadFromJson);
        private static readonly Lazy<List<TestPerson>> _bogusPersons =
            new Lazy<List<TestPerson>>(GenerateWithBogus);

        public static IEnumerable<TestPerson> CreateEnumerable(bool loadFromResource = true) =>
            (loadFromResource ? _resourcePersons.Value : _bogusPersons.Value).AsReadOnly();

        private static List<TestPerson> LoadFromJson()
        {
            var asm = Assembly.GetExecutingAssembly();
            
            using var res = asm.GetManifestResourceStream($"{asm.GetName().Name}.person-data.json");
            using var sr = new StreamReader(res);

            var json = sr.ReadToEnd();

            var persons = JsonConvert.DeserializeObject<List<TestPerson>>(json);

            if (TestPersonCount > persons.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(TestPersonCount), "Specified value exceeds test data in resource file.");
            }

            return persons.Take(TestPersonCount).ToList();
        }

        private static List<TestPerson> GenerateWithBogus()
        {
            // Set the randomizer seed for repeatable data set.
            Randomizer.Seed = new Random(8675309);
            var now = DateTime.UtcNow;

            var ids = 0;

            var generator = new Faker<TestPerson>()
                .CustomInstantiator(f => new TestPerson { Id = ++ids })
                .RuleFor(p => p.Uuid, (f, p) => f.Random.Guid())
                .RuleFor(p => p.FirstName, (f, p) => f.Name.FirstName())
                .RuleFor(p => p.LastName, (f, p) => f.Name.LastName())
                .RuleFor(p => p.PhoneNumber, (f, p) => f.Phone.PhoneNumber())
                .RuleFor(p => p.Email, (f, p) => f.Internet.Email(p.FirstName, p.LastName))
                .RuleFor(p => p.BirthDate, (f, p) => f.Date.Past(30, now))
                .RuleFor(p => p.EmploymentStartDate, (f, p) => f.Date.Past(5, now).OrNull(f))
                .RuleFor(p => p.Weight, (f, p) => Math.Truncate(100d * f.Random.Double(150, 250)) / 100d)
                .RuleFor(p => p.AllTheMoney, (f, p) => Math.Truncate(100m * f.Random.Decimal(5, 1_000_000)) / 100m)
                .RuleFor(p => p.FavoriteBigNumber, (f, p) => f.Random.Long(10_000_000_000))
                .RuleFor(p => p.SecondFavoriteBigNumber, (f, p) => f.Random.Bool() 
                    ? f.Random.Long(10_000_000_000) 
                    : (long?)null)
                .RuleFor(p => p.Pet, (f, p) => f.Random.Bool() 
                    ? f.PickRandom(Pets) 
                    : (TestPets?)null);

            return generator.GenerateLazy(TestPersonCount).ToList();
        }
    }
}
