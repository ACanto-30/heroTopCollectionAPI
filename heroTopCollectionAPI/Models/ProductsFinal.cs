using Google.Cloud.Firestore;

namespace heroTopCollectionAPI.Models
{
    [FirestoreData]
    public class ProductsFinal
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public string ProductId { get; set; }
        [FirestoreProperty]
        public string Features { get; set; }  // Ahora es un string
        [FirestoreProperty]
        public double Price { get; set; }
        [FirestoreProperty]
        public string ImageUrl { get; set; }
        [FirestoreProperty]
        public string CompanyName { get; set; }  // Nueva propiedad

        public ProductsFinal(string name, string description, string productId, string features, double price, string imageUrl, string companyName)
        {
            Name = name;
            Description = description;
            ProductId = productId;
            Features = features;
            Price = price;
            ImageUrl = imageUrl;
            CompanyName = companyName;
        }

        public ProductsFinal() { }
    }
}
