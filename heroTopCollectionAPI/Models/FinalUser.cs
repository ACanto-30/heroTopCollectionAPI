using Google.Cloud.Firestore;

namespace heroTopCollectionAPI.Models
{
    [FirestoreData]
    public class FinalUser
    {
        [FirestoreProperty]
        public string Email { get; set; }
        [FirestoreProperty]
        public string UserToken { get; set; }
        [FirestoreProperty]
        public string HashedPassword { get; set; }
        [FirestoreProperty]
        public string Salt { get; set; }
    }
}
