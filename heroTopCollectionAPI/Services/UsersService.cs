using heroTopCollectionAPI.Data.Context;
using heroTopCollectionAPI.Models;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Mvc;
using heroTopCollectionAPI.Utilities;
using static Google.Cloud.Firestore.V1.StructuredQuery.Types;


namespace heroTopCollectionAPI.Services
{

    // Clase que maneja a los usuarios pero en este caso los clientes que no crear productos
    public class UsersService
    {
        private readonly FirestoreDb _firestoreDb;
        private readonly AuthenticationContext _identityContext;

        public UsersService(FirestoreContext context, AuthenticationContext identityContext)
        {
            _firestoreDb = context.FirestoreDb;
            _identityContext = identityContext;
        }

        //Codigo para logear al usuario
        public async Task<IActionResult> AuthenticateUser(string email, string plainPassword)
        {
            try
            {
                
                var docRef = _firestoreDb.Collection("users").Document(email);
                var snapshot = await docRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                {
                    return new BadRequestObjectResult(new { Message = "User not found" });
                }

                
                string storedSalt = snapshot.GetValue<string>("Salt");
                string storedHashedPassword = snapshot.GetValue<string>("HashedPassword");

               
                var hashPasswordUtil = new HashPassword();
                bool isPasswordValid = hashPasswordUtil.VerifyPassword(plainPassword, storedHashedPassword, storedSalt);

                if (!isPasswordValid)
                {
                    return new BadRequestObjectResult(new { Message = "Invalid password" });
                }

               
                var userToken = snapshot.GetValue<string>("UserToken");
                return new OkObjectResult(new { Token = userToken, Message = "Authentication successful" });
            }
            catch (Exception ex)
            {
                
                return new BadRequestObjectResult(new { Message = "Error authenticating user", Error = ex.Message });
            }
        }


        // Funcion para registar un nuevo usuario
        public async Task<IActionResult> PostUser(string email, string hashedPassword, string userToken, string salt)
        {
            try
            {
               
                string userId = Guid.NewGuid().ToString();

             
                var user = new FinalUser
                {
                    Email = email,
                    HashedPassword = hashedPassword,
                    UserToken = userToken,
                    Salt = salt
                };

                Console.WriteLine(user);
                Console.Write(user.Email);
               
                var docRef = _firestoreDb.Collection("users").Document(user.Email);


                await docRef.SetAsync(user);

               
                return new OkObjectResult(new { Token = userToken, Message = "User created successfully" });
            }
            catch (Exception ex)
            {
                
                
                return new BadRequestObjectResult(new { Message = "Error creating user", Error = ex.Message });

            }
        }

        public async Task<bool> IsTokenValid(string token)
        {
            try
            {
               
                var usersCollection = _firestoreDb.Collection("users");

               
                var query = usersCollection.WhereEqualTo("UserToken", token);
                var querySnapshot = await query.GetSnapshotAsync();

               
                return querySnapshot.Documents.Any();
            }
            catch (Exception ex)
            {
              
                Console.WriteLine($"Error verifying token: {ex.Message}");
                return false;
            }
        }


        public async Task<IActionResult> AddOrdersToUser(string token, ProductOrder[] orders)
        {
            try
            {
               
                var usersCollection = _firestoreDb.Collection("users");
                var query = usersCollection.WhereEqualTo("UserToken", token);
                var querySnapshot = await query.GetSnapshotAsync();

                if (querySnapshot.Documents.Count == 0)
                {
                    return new NotFoundObjectResult(new { Message = "User not found" });
                }

               
                var userDocument = querySnapshot.Documents.First();
                string userEmail = userDocument.Id; 

                
                string orderId = Guid.NewGuid().ToString();

                
                var ordersCollection = usersCollection.Document(userEmail).Collection("orders");

                var orderDocument = new
                {
                    OrderId = orderId,
                    Products = orders, 
                    OrderDate = DateTime.UtcNow
                };

                
                await ordersCollection.Document(orderId).SetAsync(orderDocument);

                return new OkObjectResult(new { Message = "Order added successfully", OrderId = orderId });
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error: {ex.Message}");
                return new BadRequestObjectResult(new { Message = "Error adding order", Error = ex.Message });
            }
        }





        private async Task<string> GetEmailFromToken(string token)
        {
            var usersRef = _firestoreDb.Collection("users");
            var query = usersRef.WhereEqualTo("UserToken", token);  

            var snapshot = await query.GetSnapshotAsync();
            if (snapshot.Documents.Count == 0)
            {
                throw new Exception("Token no válido.");
            }

         
            var doc = snapshot.Documents[0];
            return doc.GetValue<string>("Email");
            
        }


        public async Task<IActionResult> GetOrdersByToken(string token)
        {
            try
            {
               
                string email = await GetEmailFromToken(token);

                
                var userRef = _firestoreDb.Collection("users").Document(email); 

               
                var ordersRef = userRef.Collection("orders");
                var snapshot = await ordersRef.GetSnapshotAsync();

             
                var orders = new List<ProductOrder>();
                foreach (var doc in snapshot.Documents)
                {
                    orders.Add(doc.ConvertTo<ProductOrder>());
                }

           
                return new OkObjectResult(orders);
            }
            catch (Exception ex)
            {
               
                return new BadRequestObjectResult(new { message = "Error fetching orders", error = ex.Message });
            }
        }


        




    }
}

