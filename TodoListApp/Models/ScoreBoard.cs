namespace TodoListApp.Models
{
    public class ScoreBoard
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public int Awards { get; set; }
        public int Completed { get; set; }
        public int Position { get; set; }
        public int Points { get; set; }

        public ScoreBoard() { }

        public ScoreBoard(int awards, int completed, int position, int points)
        {
            Awards = awards;
            Completed = completed;
            Position = position;
            Points = points;
        }
    }
}