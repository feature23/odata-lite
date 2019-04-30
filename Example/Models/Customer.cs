using F23.Hateoas;

namespace Example.Models
{
    public class Customer : HypermediaBase
    {
        public Customer()
        {
        }

        public Customer(int id, string firstName, string lastName, int age, bool isPreferred)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Age = age;
            IsPreferred = isPreferred;
        }

        public int Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int Age { get; set; }

        public bool IsPreferred { get; set; }
    }
}
