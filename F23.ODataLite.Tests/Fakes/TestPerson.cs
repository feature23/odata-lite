using System;
using System.ComponentModel.DataAnnotations;

namespace F23.ODataLite.Tests.Fakes
{
    public class TestPerson
    {
        [Key]
        public int Id { get; set; }

        public Guid Uuid { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Email { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime? EmploymentStartDate { get; set; }

        public double Weight { get; set; }

        public decimal AllTheMoney { get; set; }

        public long FavoriteBigNumber { get; set; }

        public long? SecondFavoriteBigNumber { get; set; }

        public TestPets? Pet { get; set; }
    }
}
