using Microsoft.AspNetCore.Mvc;
using Packt.Shared;
using SampleAPI.Repositories;

namespace SampleAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerRepository repo;

        // constructor injects repository registered in Startup
        public CustomerController(ICustomerRepository repo)
        {
            this.repo = repo;
        }

        // GET: api/customers
        // GET: api/customers/?country=[country]
        // this will always return a list of customers (but it might be empty)
        [HttpGet]
        [ProducesResponseType(200, Type = typeof(IEnumerable<Customer>))]
        public async Task<IEnumerable<Customer>> GetCustomers(string? country)
        {
            if (string.IsNullOrWhiteSpace(country))
            {
                return await repo.RetrieveAllAsync();
            }
            else
            {
                return (await repo.RetrieveAllAsync()).Where(customer => customer.Country == country);
            }
        }

        // GET: api/customers/[id]
        [HttpGet("{id}", Name = nameof(GetCustomer))] // named route
        [ProducesResponseType(200, Type = typeof(Customer))]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetCustomer(string id)
        {
            Customer? c = await repo.RetrieveAsync(id);
            if (c == null) { return NotFound(); } // 404 Resource not found

            return Ok(c); // 200 OK with customer in body
        }

        // POST: api/customers
        // BODY: Customer (JSON, XML)
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(Customer))]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] Customer c)
        {
            if (c == null) { return BadRequest(); } // 400 Bad request

            Customer? addedCustomer = await repo.CreateAsync(c);

            if (addedCustomer == null) { return BadRequest("Repository failed to create customer."); }

            return CreatedAtRoute(
                routeName: nameof(GetCustomer), 
                routeValues: new { id = addedCustomer.CustomerId.ToLower() }, 
                value: addedCustomer);
        }

        // PUT: api/customers/[id]
        // BODY: Customer (JSON, XML)
        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Update(string id, [FromBody] Customer c)
        {
            id = id.ToUpper();
            c.CustomerId = c.CustomerId.ToUpper();

            if (c == null || c.CustomerId != id) { return BadRequest(); } // 400 Bad request

            Customer? existing = await repo.RetrieveAsync(id);
            if (existing == null) { return NotFound(); } // 404 Resource not found

            await repo.UpdateAsync(id, c);

            return new NoContentResult(); // 204 No content
        }

        // DELETE: api/customers/[id]
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Delete(string id)
        {
            Customer? existing = await repo.RetrieveAsync(id);
            if (existing == null) { return NotFound(); } // 404 Resource not found

            bool? deleted = await repo.DeleteAsync(id);

            // short circuit AND
            if (deleted.HasValue && deleted.Value) { return new NoContentResult(); } // 204 No content

            return BadRequest($"Customer {id} was found but failed to delete."); // 400 Bad request
        }
    }
}
