using heroTopCollectionAPI.Attributes;
using heroTopCollectionAPI.Models;
using heroTopCollectionAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace heroTopCollectionAPI.Controllers
{
    

    [ApiController]
    [Route("api")]
    public class ProductsController : Controller
    {
        private readonly ProductsService _productService;

        public ProductsController(ProductsService productService)
        {
            _productService = productService;
        }

        // Acción para mostrar la vista
        [EndpointRule("", requiresAuthorization: false)]
        [Route("products")] 
        public IActionResult ProductView()
        {
            return View();
        }

        // Acción para recibir los datos del formulario y crear el producto
        [HttpPost]
        [Route("products")]
        [EndpointRule("multipart/form-data", requiresAuthorization: true)]
        public async Task<IActionResult> Create(Products model)
        {

            if (ModelState.IsValid)
            {
                // Subir la imagen a Google Cloud Storage y obtener la URL
                var imageUrl = await _productService.UploadImageToCloud(model.Image);

                // Crear el producto con sus características y la URL de la imagen

                await _productService.AddProductAsync(model, imageUrl);

                return RedirectToAction("Index", "Home");  // Redirigir a una página de éxito o lista
            }

            // Si el modelo no es válido, redirigir a la vista de creación de producto
            return RedirectToAction("ProductView");
        }

        [HttpGet]
        [Route("products/all")]
        [EndpointRule("", requiresAuthorization: false)]
        public async Task<IActionResult> GetAllProducts()
        {
            return await _productService.GetAllProducts();
        }

        // Acción para obtener los productos por CompanyName
        [HttpGet]
        [Route("products/company/{companyName}")]
        [EndpointRule("", requiresAuthorization: false)]
        public async Task<IActionResult> GetProductsByCompanyName(string companyName)
        {
            return await _productService.GetProductsByCompanyName(companyName);
        }

        // Acción para obtener los 5 productos más caros
        [HttpGet]
        [Route("products/top-expensive")]
        [EndpointRule("", requiresAuthorization: false)]
        public async Task<IActionResult> GetTop5ExpensiveProducts()
        {
            return await _productService.GetTop5ExpensiveProducts();
        }

        [HttpGet]
        [Route("products/{id}")]
        [EndpointRule("", requiresAuthorization: false)]
        public async Task<IActionResult> GetProductById(string id)
        {
            // Verifica si el id no es nulo o vacío
            Console.WriteLine(id);
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { message = "El ID del producto no puede ser nulo o vacío." });
            }

            // Llama al servicio para obtener el producto por id
            var product = await _productService.GetProductById(id);

            // Si no se encuentra el producto, devuelve un error 404
            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }

            // Devuelve el producto encontrado
            return Ok(product);
        }

    }
}
