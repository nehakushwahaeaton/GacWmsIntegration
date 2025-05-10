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
    public class PurchaseOrdersController : ControllerBase
    {
        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly ILogger<PurchaseOrdersController> _logger;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService, ILogger<PurchaseOrdersController> logger)
        {
            _purchaseOrderService = purchaseOrderService ?? throw new ArgumentNullException(nameof(purchaseOrderService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/purchaseorders
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetPurchaseOrders()
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetAllPurchaseOrdersAsync();
                return Ok(purchaseOrders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all purchase orders");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving purchase orders");
            }
        }

        // GET: api/purchaseorders/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PurchaseOrder>> GetPurchaseOrder(int id)
        {
            try
            {
                var purchaseOrder = await _purchaseOrderService.GetPurchaseOrderByIdAsync(id);
                return Ok(purchaseOrder);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving purchase order");
            }
        }

        // GET: api/purchaseorders/customer/5
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PurchaseOrder>>> GetPurchaseOrdersByCustomer(int customerId)
        {
            try
            {
                var purchaseOrders = await _purchaseOrderService.GetPurchaseOrdersByCustomerAsync(customerId);
                return Ok(purchaseOrders);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving purchase orders for customer ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving purchase orders");
            }
        }

        // POST: api/purchaseorders
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PurchaseOrder>> CreatePurchaseOrder(PurchaseOrder purchaseOrder)
        {
            try
            {
                if (purchaseOrder == null)
                {
                    return BadRequest("Purchase order data is null");
                }

                var createdPurchaseOrder = await _purchaseOrderService.CreatePurchaseOrderAsync(purchaseOrder);
                return CreatedAtAction(nameof(GetPurchaseOrder), new { id = createdPurchaseOrder.OrderID }, createdPurchaseOrder);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating purchase order");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating purchase order");
            }
        }

        // PUT: api/purchaseorders/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdatePurchaseOrder(int id, PurchaseOrder purchaseOrder)
        {
            try
            {
                if (purchaseOrder == null || id != purchaseOrder.OrderID)
                {
                    return BadRequest("Invalid purchase order data or ID mismatch");
                }

                var updatedPurchaseOrder = await _purchaseOrderService.UpdatePurchaseOrderAsync(purchaseOrder);
                return Ok(updatedPurchaseOrder);
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
                _logger.LogError(ex, "Error updating purchase order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating purchase order");
            }
        }

        // DELETE: api/purchaseorders/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeletePurchaseOrder(int id)
        {
            try
            {
                var result = await _purchaseOrderService.DeletePurchaseOrderAsync(id);
                if (!result)
                {
                    return NotFound($"Purchase order with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting purchase order with ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting purchase order");
            }
        }

        // POST: api/purchaseorders/5/sync

        // GET: api/purchaseorders/5/items
        [HttpGet("{id}/items")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<PurchaseOrderDetails>>> GetOrderItems(int id)
        {
            try
            {
                var orderItems = await _purchaseOrderService.GetOrderItemsAsync(id);
                return Ok(orderItems);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving order items for purchase order ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving order items");
            }
        }

        // POST: api/purchaseorders/5/items
        [HttpPost("{id}/items")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PurchaseOrderDetails>> AddOrderItem(int id, PurchaseOrderDetails item)
        {
            try
            {
                if (item == null)
                {
                    return BadRequest("Order item data is null");
                }

                var addedItem = await _purchaseOrderService.AddOrderItemAsync(id, item);
                return CreatedAtAction(nameof(GetOrderItems), new { id = id }, addedItem);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding order item to purchase order ID: {OrderId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding order item");
            }
        }

        // DELETE: api/purchaseorders/items/5
        [HttpDelete("items/{itemId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RemoveOrderItem(int itemId)
        {
            try
            {
                var result = await _purchaseOrderService.RemoveOrderItemAsync(itemId);
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
