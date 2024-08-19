using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models;
using TodoListApp.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.Routing;
using Firebase.Database.Query;

namespace TodoListApp.Controllers;

public class HomeController : Controller
{
    Database database = new Database();

    Users user;
    List<TodoList> todoLists = new List<TodoList>();
    List<ScoreBoard> scoreBoards = new List<ScoreBoard>();


    private readonly ILogger<HomeController> _logger;

    public async Task fillUserTable(string id)
    {
        try
        {
            var response = await database.client.Child("users").OnceAsync<Users>();
            if (response != null)
            {
                var users = response.Select(item => item.Object).ToList();
                user = users.FirstOrDefault(u => u.UserId == id);
            }
        }
        catch (Exception) { }

        string userJson = JsonConvert.SerializeObject(user);
        HttpContext.Session.SetString("UserObject", userJson);
    }

    public async Task fillTodoListTable(string id)
    {
        try
        {
            var response = await database.client.Child("todoList").OnceAsync<TodoList>();
            if (response != null)
            {
                var collection = response.Select(item => item.Object).ToList();
                foreach (var item in collection)
                {
                    if (item.UserId == id)
                    {
                        todoLists.Add(item);
                    }
                }
            }
        }
        catch (Exception) { }

        string todoListsJson = JsonConvert.SerializeObject(todoLists);
        HttpContext.Session.SetString("TodoListObject", todoListsJson);
    }

    public async Task fillScoreBoardTable()
    {
        try
        {
            var response = await database.client.Child("scoreBoard").OnceAsync<ScoreBoard>();
            if (response != null)
            {
                scoreBoards = response.Select(item => item.Object).ToList();
            }
        }
        catch (Exception) { }

        string scoreBoardsJson = JsonConvert.SerializeObject(scoreBoards);
        HttpContext.Session.SetString("ScoreBoardObject", scoreBoardsJson);
    }

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public async Task fill(string id)
    {
        todoLists = new List<TodoList>();
        scoreBoards = new List<ScoreBoard>();
        await fillUserTable(id);
        await fillTodoListTable(id);
        await fillScoreBoardTable();
    }

    public async Task<IActionResult> Index()
    {
        string? userIdNullable = HttpContext.Session.GetString("UserId");

        if (!string.IsNullOrEmpty(userIdNullable))
        {
            await fill(userIdNullable);
            foreach (var item in scoreBoards)
            {
                if (item.UserId.Equals(userIdNullable))
                {
                    ViewBag.score = item.Points;
                }
            }
        }
        else
        {
            return Redirect(Url.Action(new UrlActionContext { Action = "Login", Controller = "Login" }));
        }

        return View(todoLists);
    }

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] TodoList data)
    {
        try
        {
            string? userIdNullable = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdNullable))
            {
                if (data != null && !string.IsNullOrEmpty(data.Label))
                {
                    TodoList item = new TodoList(data.Label, data.Checked);
                    item.UserId = userIdNullable;
                    var addedItem = await database.client.Child("todoList").PostAsync(item);
                    return Json(new { success = true, id = addedItem.Key });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = ex.Message });
        }

        return Json(new { success = false, message = "Invalid data" });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem([FromBody] TodoList data)
    {
        try
        {
            if (data != null && !string.IsNullOrEmpty(data.UserId.ToString()))
            {
                await database.client.Child("todoList").Child(data.UserId).DeleteAsync();
                return Json(new { success = true });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = ex.Message });
        }

        return Json(new { success = false, message = "Invalid data" });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateItem([FromBody] TodoList data)
    {
        try
        {
            string? userIdNullable = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdNullable))
            {
                if (data != null && !string.IsNullOrEmpty(data.Label))
                {
                    TodoList item = new TodoList(data.Label, data.Checked);
                    item.UserId = userIdNullable;
                    await database.client.Child("todoList").Child(data.UserId).PutAsync(item);
                    return Json(new { success = true });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return Json(new { success = false, message = ex.Message });
        }

        return Json(new { success = false, message = "Invalid data" });
    }

    [HttpPost]
    public async Task<IActionResult> CompletedUpdate([FromBody] ScoreBoard data)
    {
        try
        {
            string? userIdNullable = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdNullable))
            {
                var allScoreBoards = await database.client.Child("scoreBoard").OnceAsync<ScoreBoard>();
                var userScoreBoard = allScoreBoards.FirstOrDefault(sb => sb.Object.UserId == userIdNullable);

                if (userScoreBoard != null)
                {
                    if (data.Completed == 1)
                    {
                        userScoreBoard.Object.Completed = userScoreBoard.Object.Completed + 1;
                    }
                    else
                    {
                        if (userScoreBoard.Object.Completed > 0)
                        {
                            userScoreBoard.Object.Completed = userScoreBoard.Object.Completed - 1;
                        }
                    }

                    await database.client.Child("scoreBoard").Child(userScoreBoard.Key).PutAsync(userScoreBoard.Object);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Json(new { success = false, message = "Invalid data" });
    }

    [HttpPost]
    public async Task<IActionResult> ScoreUpdate([FromBody] ScoreBoard data)
    {
        try
        {
            string? userIdNullable = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userIdNullable))
            {
                var allScoreBoards = await database.client.Child("scoreBoard").OnceAsync<ScoreBoard>();
                var userScoreBoard = allScoreBoards.FirstOrDefault(sb => sb.Object.UserId == userIdNullable);

                if (userScoreBoard != null && data.Points > 0)
                {
                    userScoreBoard.Object.Points = data.Points;
                    if (userScoreBoard.Object.Points > 1000)
                    {
                        userScoreBoard.Object.Awards = userScoreBoard.Object.Points % 1000;
                    }
                    await database.client.Child("scoreBoard").Child(userScoreBoard.Key).PutAsync(userScoreBoard.Object);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return Json(new { success = false, message = "Invalid data" });
    }

    public async Task PositionUpdate()
    {
        try
        {
            var response = await database.client.Child("scoreBoard").OnceAsync<ScoreBoard>();
            if (response != null)
            {
                var scoreBoards = response.Select(item => new
                {
                    Key = item.Key,
                    ScoreBoard = item.Object
                }).ToList();
                scoreBoards = scoreBoards.OrderByDescending(p => p.ScoreBoard.Points).ToList();

                for (int i = 0; i < scoreBoards.Count; i++)
                {
                    scoreBoards[i].ScoreBoard.Position = i + 1;
                }

                foreach (var scoreBoard in scoreBoards)
                {
                    await database.client.Child("scoreBoard").Child(scoreBoard.Key).PutAsync(scoreBoard.ScoreBoard);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task<IActionResult> Score()
    {
        List<ScoreBoard> tempList = new List<ScoreBoard>();
        string? userIdNullable = HttpContext.Session.GetString("UserId");
        if (!string.IsNullOrEmpty(userIdNullable))
        {
            await fill(userIdNullable);
            await PositionUpdate();
            string scoreBoardNullable = HttpContext.Session.GetString("ScoreBoardObject");
            if (!string.IsNullOrEmpty(scoreBoardNullable))
            {
                scoreBoards = JsonConvert.DeserializeObject<List<ScoreBoard>>(scoreBoardNullable);
            }

            foreach (var scoreBoard in scoreBoards)
            {
                tempList.Add(scoreBoard);
                if (scoreBoard.UserId.Equals(userIdNullable))
                {
                    ViewBag.name = scoreBoard.UserName;
                    ViewBag.awards = scoreBoard.Awards;
                    ViewBag.completed = scoreBoard.Completed;
                    ViewBag.points = scoreBoard.Points;
                    ViewBag.position = scoreBoard.Position;
                }
            }
            tempList = tempList.OrderByDescending(p => p.Points).ToList();
            tempList = tempList.Take(10).ToList();
        }
        else
        {
            return Redirect(Url.Action(new UrlActionContext { Action = "Login", Controller = "Login" }));
        }

        return View(tempList);
    }

    public async Task<IActionResult> Profile()
    {
        string? userIdNullable = HttpContext.Session.GetString("UserId");

        if (!string.IsNullOrEmpty(userIdNullable))
        {
            await fill(userIdNullable);
            string scoreBoardNullable = HttpContext.Session.GetString("ScoreBoardObject");
            if (!string.IsNullOrEmpty(scoreBoardNullable))
            {
                scoreBoards = JsonConvert.DeserializeObject<List<ScoreBoard>>(scoreBoardNullable);
            }

            foreach (var item in scoreBoards)
            {
                if (item.UserId.Equals(userIdNullable))
                {
                    ViewBag.name = item.UserName;
                    ViewBag.awards = item.Awards;
                    ViewBag.completed = item.Completed;
                    ViewBag.points = item.Points;
                    ViewBag.position = item.Position;
                    ViewBag.level = levelCal(item.Points);
                }
            }
        }
        else
        {
            return Redirect(Url.Action(new UrlActionContext { Action = "Login", Controller = "Login" }));
        }

        return View();
    }

    public int levelCal(int point)
    {
        int level = 1;
        while ((point - (level * 100)) >= 0)
        {
            point -= level * 100;
            level += 1;
        }
        return level;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
