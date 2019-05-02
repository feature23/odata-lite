using F23.Hateoas;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Example.Models
{
    public class Customer : HypermediaBase
    {
        public Customer()
        {
        }

        public Customer(int id, string firstName, string lastName, int age, bool isPreferred, int? memberNumber = null, CustomerLoyaltyProgramLevel? loyaltyLevel = null)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            IsPreferred = isPreferred;

            if (memberNumber.HasValue && loyaltyLevel.HasValue)
            {
                LoyaltyProgram = new CustomerLoyaltyProgram
                {
                    MemberNumber = memberNumber.Value,
                    Level = loyaltyLevel.Value
                };
            }
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public bool IsPreferred { get; set; }

        public CustomerLoyaltyProgram LoyaltyProgram { get; set; }
    }

    public class CustomerLoyaltyProgram
    {
        public int MemberNumber { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CustomerLoyaltyProgramLevel Level { get; set; }
    }

    public enum CustomerLoyaltyProgramLevel
    {
        Bronze,
        Silver,
        Gold
    }
}
