using heroTopCollectionAPI.Data.Context;
using heroTopCollectionAPI.Models;
using Google.Cloud.Storage.V1;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using Firebase.Storage;
using heroTopCollectionAPI.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace heroTopCollectionAPI.Services
{
    // Clase que maneja los productos
    public class ProductsService
    {
        private readonly StorageClient _storageClient;
        private readonly FirestoreDb _firestoreDb;
        private const string _bucketName = "herotopcollection.firebasestorage.app";

        public ProductsService(StorageContext storageContext, FirestoreContext firestoreDb)
        {
            _storageClient = storageContext.Client;
            _firestoreDb = firestoreDb.FirestoreDb;
        }

        // Función para subir la imagen a Google Cloud Storage
        public async Task<string> UploadImageToCloud(IFormFile file)
        {

            Stream image;

            if (file.Length > 0)
            {
                var fileName = Path.GetFileName(file.FileName);
                

               
                using (image = file.OpenReadStream())
                {
                    var objectName = fileName; 
                    string token = "";

                    var cancellationToken = new CancellationToken();

                    var task = new FirebaseStorage(
                        "herotopcollection.firebasestorage.app",
                        new FirebaseStorageOptions
                        {
                            AuthTokenAsyncFactory = () => Task.FromResult(token),
                            ThrowOnCancel = true
                        }
                        ).Child("Collection")
                        .Child(fileName)
                        .PutAsync(image, cancellationToken);

                    var downloadURL = await task;
                    return downloadURL;
                }

            }

            return null;
        }

        // Función para agregar un producto
        public async Task AddProductAsync(Products product, string imageUrl)
        {
            string guid = Guid.NewGuid().ToString();

            
            var finalProduct = new ProductsFinal(
                product.Name,        
                product.Description, 
                guid,                
                product.Features,   
                product.Price,      
                imageUrl,             
                product.CompanyName   
            );

            
            var collection = _firestoreDb.Collection("products");

            
            await collection.AddAsync(finalProduct);
        }

        public async Task<IActionResult> GetAllProducts()
        {
            try
            {
                var collection = _firestoreDb.Collection("products");
                var snapshot = await collection.GetSnapshotAsync();

                var products = new List<ProductsFinal>();
                foreach (var doc in snapshot.Documents)
                {
                    var product = doc.ConvertTo<ProductsFinal>();
                    products.Add(product);
                }

                return new OkObjectResult(products);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new { Message = "Error fetching products", Error = ex.Message });
            }
        }

        // Obtener los productos por la compañia que los crea
        public async Task<IActionResult> GetProductsByCompanyName(string companyName)
        {
            try
            {
                
                var productsCollection = _firestoreDb.Collection("products");

                
                var query = productsCollection.WhereEqualTo("CompanyName", companyName);

                
                var querySnapshot = await query.GetSnapshotAsync();

               
                var products = new List<ProductsFinal>();

                
                foreach (var document in querySnapshot.Documents)
                {
                    
                    var product = document.ConvertTo<ProductsFinal>();
                    products.Add(product);
                }

                
                if (products.Count > 0)
                {
                    return new OkObjectResult(products);
                }
                else
                {
                    return new NotFoundObjectResult(new { Message = "No products found for this company." });
                }
            }
            catch (Exception ex)
            {
                
                return new BadRequestObjectResult(new { Message = "Error fetching products", Error = ex.Message });
            }
        }


        // Funcion para obtener los 5 productos más caros, para ponerlos en un sitio especifico para capta la atencion
        public async Task<IActionResult> GetTop5ExpensiveProducts()
        {
            try
            {

                var productsCollection = _firestoreDb.Collection("products");

                
                var query = productsCollection.OrderByDescending("Price").Limit(5);


                var querySnapshot = await query.GetSnapshotAsync();


                var products = new List<ProductsFinal>();


                foreach (var document in querySnapshot.Documents)
                {

                    var product = document.ConvertTo<ProductsFinal>();
                    products.Add(product);
                }

                if (products.Count >= 0)
                {
                    return new OkObjectResult(products);
                }
                else
                {
                    return new NotFoundObjectResult(new { Message = "No products found." });
                }
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return new BadRequestObjectResult(new { Message = "Error fetching products", Error = ex.Message });
            }
        }

        // Funcioon que obtiene productos por su ID
        public async Task<ProductsFinal?> GetProductById(string productId)
        {
            var collection = _firestoreDb.Collection("products");
            var query = collection.WhereEqualTo("ProductId", productId);
            var querySnapshot = await query.GetSnapshotAsync();

           
            if (querySnapshot.Count == 0)
            {
                return null;
            }

            var documentSnapshot = querySnapshot.Documents.First();

            var product = new ProductsFinal
            {
                Name = documentSnapshot.GetValue<string>("Name"),
                Description = documentSnapshot.GetValue<string>("Description"),
                ProductId = documentSnapshot.GetValue<string>("ProductId"),
                Features = documentSnapshot.GetValue<string>("Features"),
                Price = documentSnapshot.GetValue<double>("Price"),
                ImageUrl = documentSnapshot.GetValue<string>("ImageUrl"),
                CompanyName = documentSnapshot.GetValue<string>("CompanyName")
            };

            return product;
        }





    }
}
