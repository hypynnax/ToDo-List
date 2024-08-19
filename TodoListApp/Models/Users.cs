namespace TodoListApp.Models
{
    public class Users
    {
        public string UserId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public Users() { }

        public Users(string name, string email, string password)
        {
            Name = name;
            Email = email;
            Password = password;
        }
    }
}