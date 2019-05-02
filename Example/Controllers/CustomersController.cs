using System.Collections.Generic;
using System.Linq;
using Example.Models;
using F23.Hateoas;
using F23.ODataLite;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers
{
    [Route("api/customers")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        // Names generated from http://random-name-generator.info/random/?n=10&g=1&st=2
        // Ages generated from https://www.random.org/integers/?num=10&min=18&max=100&col=5&base=10&format=html&rnd=new
        // Loyalty program member numbers generated from https://www.random.org/integers/?num=10&min=100000&max=1000000&col=5&base=10&format=html&rnd=new
        // Any similarity to persons real or fictional is coincidental!
        private static readonly Customer[] _customers =
        {
            new Customer(1, "Daryl", "Barber", 73, true, 832373, CustomerLoyaltyProgramLevel.Gold),
            new Customer(2, "Cheryl", "Jimenez", 54, true, 775751, CustomerLoyaltyProgramLevel.Silver),
            new Customer(3, "Caleb", "Mitchell", 29, false),
            new Customer(4, "Joanne", "Griffin", 61, true, 534424, CustomerLoyaltyProgramLevel.Bronze),
            new Customer(5, "Irma", "Holloway", 40, false),
            new Customer(6, "Arthur", "Sandoval", 64, true, 229658, CustomerLoyaltyProgramLevel.Gold),
            new Customer(7, "Tracy", "Frank", 45, true, 860876, CustomerLoyaltyProgramLevel.Silver),
            new Customer(8, "Sherman", "Drake", 89, false),
            new Customer(9, "Gwen", "Brown", 48, true, 373442, CustomerLoyaltyProgramLevel.Bronze),
            new Customer(10, "Melvin", "Woods", 54, false)
        };

        // GET api/customers
        [HttpGet]
        [ODataLite]
        public ActionResult<IEnumerable<Customer>> Get()
        {
            return _customers;
        }

        // GET api/customers/{id}
        [HttpGet("{id}")]
        public ActionResult<Customer> Get(int id)
        {
            var customer = _customers.FirstOrDefault(i => i.Id == id);

            if (customer == null)
                return NotFound();

            return customer;
        }

        // GET api/customers/hateoas
        [HttpGet("hateoas")]
        [ODataLite]
        public ActionResult<HypermediaResponse> GetHateoas()
        {
            return new HypermediaResponse(_customers);
        }
    }
}
