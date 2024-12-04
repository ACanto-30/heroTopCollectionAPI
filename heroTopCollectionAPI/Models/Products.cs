using Google.Cloud.Firestore;

namespace heroTopCollectionAPI.Models
{
    public class Products
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public IFormFile Image { get; set; }
        public string Features { get; set; }  // Ahora es un string
        public string CompanyName { get; set; }  // Nueva propiedad
    }
}
