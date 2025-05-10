using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace GacWmsIntegration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/customers
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllCustomersAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all customers");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customers");
            }
        }

        // GET: api/customers/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            try
            {
                var customer = await _customerService.GetCustomerByIdAsync(id);
                return Ok(customer);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer");
            }
        }

        // POST: api/customers
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Customer>> CreateCustomer(Customer customer)
        {
            try
            {
                if (customer == null)
                {
                    return BadRequest("Customer data is null");
                }

                var createdCustomer = await _customerService.CreateCustomerAsync(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.CustomerID }, createdCustomer);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating customer");
            }
        }

        // PUT: api/customers/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            try
            {
                if (customer == null || id != customer.CustomerID)
                {
                    return BadRequest("Invalid customer data or ID mismatch");
                }

                var updatedCustomer = await _customerService.UpdateCustomerAsync(customer);
                return Ok(updatedCustomer);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer");
            }
        }

        // DELETE: api/customers/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteCustomerAsync(id);
                if (!result)
                {
                    return NotFound($"Customer with ID {id} not found");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {CustomerId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting customer");
            }
        }
        
    }
}
