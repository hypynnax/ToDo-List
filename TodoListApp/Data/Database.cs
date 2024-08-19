using Firebase.Database;

namespace TodoListApp.Data
{
    public class Database
    {
        public FirebaseClient client;

        public Database()
        {
            client = new FirebaseClient("https://todoapp-9cbcd-default-rtdb.europe-west1.firebasedatabase.app/");
        }
    }
}
