using GacWmsIntegration.Core.Interfaces;
using GacWmsIntegration.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GacWmsIntegration.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesOrdersController : ControllerBase
    {
        private readonly ISalesOrderService _salesOrderService;
        private readonly ILogger<SalesOrdersController> _logger;

        public SalesOrdersController(ISalesOrderService salesOrderService, ILogger<SalesOrdersController> logger)
        {
            _salesOrderService = salesOrderService ?? throw new ArgumentNullException(nameof(salesOrderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/salesorders
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesOrder>>> GetSalesOrders()
        {
            try
            {
                var salesOrders = await _salesOrderService.GetAllSalesOrdersAsync();
                return Ok(salesOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all sales orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving sales orders");
            }
        }

        // GET: api/salesorders/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOrder>> GetSalesOrder(int id)
        {
            try
            {
                var salesOrder = await _salesOrderService.GetSalesOrderByIdAsync(id);
                return Ok(salesOrder);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving sales order");
            }
        }

        // GET: api/salesorders/customer/5
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesOrder>>> GetSalesOrdersByCustomer(int customerId)
        {
            try
            {
                var salesOrders = await _salesOrderService.GetSalesOrdersByCustomerAsync(customerId);
                return Ok(salesOrders);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving sales orders for customer ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving sales orders");
            }
        }

        // POST: api/salesorders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOrder>> CreateSalesOrder(SalesOrder salesOrder)
        {
            try
            {
                if (salesOrder == null)
                {
                    return BadRequest("Sales order data is null");
                }

                var createdSalesOrder = await _salesOrderService.CreateSalesOrderAsync(salesOrder);
                return CreatedAtAction(nameof(GetSalesOrder), new { id = createdSalesOrder.OrderID }, createdSalesOrder);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating sales order");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating sales order");
            }
        }

        // PUT: api/salesorders/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateSalesOrder(int id, SalesOrder salesOrder)
        {
            try
            {
                if (salesOrder == null || id != salesOrder.OrderID)
                {
                    return BadRequest("Invalid sales order data or ID mismatch");
                }

                var updatedSalesOrder = await _salesOrderService.UpdateSalesOrderAsync(salesOrder);
                return Ok(updatedSalesOrder);
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
                _logger.LogError(ex, "Error updating sales order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating sales order");
            }
        }

        // DELETE: api/salesorders/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSalesOrder(int id)
        {
            try
            {
                var result = await _salesOrderService.DeleteSalesOrderAsync(id);
                if (!result)
                {
                    return NotFound($"Sales order with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting sales order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting sales order");
            }
        }

        // POST: api/salesorders/5/sync
        [HttpPost("{id}/sync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SyncSalesOrder(int id)
        {
            try
            {
                // Check if sales order exists
                if (!await _salesOrderService.SalesOrderExistsAsync(id))
                {
                    return NotFound($"Sales order with ID {id} not found");
                }

                var result = await _salesOrderService.SyncSalesOrderWithWmsAsync(id);
                if (result)
                {
                    return Ok(new { message = $"Sales order with ID {id} synchronized successfully with WMS" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to synchronize sales order with WMS");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error synchronizing sales order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error synchronizing sales order");
            }
        }

        // GET: api/salesorders/5/items
        [HttpGet("{id}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<SalesOrderDetails>>> GetOrderItems(int id)
        {
            try
            {
                var orderItems = await _salesOrderService.GetOrderItemsAsync(id);
                return Ok(orderItems);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for sales order ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving order items");
            }
        }

        // POST: api/salesorders/5/items
        [HttpPost("{id}/items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<SalesOrderDetails>> AddOrderItem(int id, SalesOrderDetails item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest("Order item data is null");
                }

                var addedItem = await _salesOrderService.AddOrderItemAsync(id, item);
                return CreatedAtAction(nameof(GetOrderItems), new { id = id }, addedItem);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order item to sales order ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding order item");
            }
        }

        // DELETE: api/salesorders/items/5
        [HttpDelete("items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveOrderItem(int itemId)
        {
            try
            {
                var result = await _salesOrderService.RemoveOrderItemAsync(itemId);
                if (!result)
                {
                    return NotFound($"Order item with ID {itemId} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing order item with ID: {ItemId}", itemId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error removing order item");
            }
        }
    }
}
