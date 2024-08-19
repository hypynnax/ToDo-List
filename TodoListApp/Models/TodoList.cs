namespace TodoListApp.Models
{
    public class TodoList
    {
        public string UserId { get; set; }
        public string Label { get; set; }
        public bool Checked { get; set; }

        public TodoList() { }

        public TodoList(string label, bool checkedd)
        {
            Label = label;
            Checked = checkedd;
        }
    }
}