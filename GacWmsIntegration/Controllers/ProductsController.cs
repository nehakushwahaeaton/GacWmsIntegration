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
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: api/products
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving products");
            }
        }

        // GET: api/products/ABC123
        [HttpGet("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> GetProduct(string code)
        {
            try
            {
                var product = await _productService.GetProductByCodeAsync(code);
                return Ok(product);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with code: {ProductCode}", code);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving product");
            }
        }

        // POST: api/products
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                if (product == null)
                {
                    return BadRequest("Product data is null");
                }

                var createdProduct = await _productService.CreateProductAsync(product);
                return CreatedAtAction(nameof(GetProduct), new { code = createdProduct.ProductCode }, createdProduct);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating product");
            }
        }

        // PUT: api/products/ABC123
        [HttpPut("{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateProduct(string code, Product product)
        {
            try
            {
                if (product == null || code != product.ProductCode)
                {
                    return BadRequest("Invalid product data or code mismatch");
                }

                var updatedProduct = await _productService.UpdateProductAsync(product);
                return Ok(updatedProduct);
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
                _logger.LogError(ex, "Error updating product with code: {ProductCode}", code);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating product");
            }
        }

        // DELETE: api/products/ABC123
        [HttpDelete("{code}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct(string code)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(code);
                if (!result)
                {
                    return NotFound($"Product with code {code} not found");
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
                _logger.LogError(ex, "Error deleting product with code: {ProductCode}", code);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting product");
            }
        }

    }
}
