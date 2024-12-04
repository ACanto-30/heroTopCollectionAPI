using Google.Cloud.Firestore;

namespace heroTopCollectionAPI.Models
{
    [FirestoreData]
    public class ProductOrder
    {
        [FirestoreProperty]
        public string Name { get; set; }
        [FirestoreProperty]
        public string Description { get; set; }
        [FirestoreProperty]
        public string ProductId { get; set; }
        [FirestoreProperty]
        public double Price { get; set; }
        [FirestoreProperty]
        public string Features { get; set; }
        [FirestoreProperty]
        public string ImageUrl { get; set; }
        [FirestoreProperty]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        [FirestoreProperty]
        public string CompanyName { get; set; }  // Nueva propiedad

        public ProductOrder() { }
    }
}
