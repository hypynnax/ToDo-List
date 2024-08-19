using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TodoListApp.Models;
using TodoListApp.Data;
using Newtonsoft.Json;

namespace TodoListApp.Controllers;

public class LoginController : Controller
{
    Database database = new Database();

    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login()
    {
        HttpContext.Session.Remove("UserId");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email_login, string password_login)
    {
        try
        {
            var response = await database.client.Child("users").OnceAsync<Users>();

            if (response != null && response.Any())
            {
                var userSnapshot = response.FirstOrDefault(snapshot => snapshot.Object.Email == email_login && snapshot.Object.Password == password_login);

                if (userSnapshot != null)
                {
                    var userId = userSnapshot.Key;
                    HttpContext.Session.SetString("UserId", userId);
                    return RedirectToAction("Index", "Home");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return View("Login", new { errorMessage = "Invalid credentials" });
    }

    [HttpPost]
    public async Task<IActionResult> Register(string name_register, string email_register, string password_register)
    {
        try
        {
            Users user = new Users(name_register, email_register, password_register);
            ScoreBoard scoreBoard = new ScoreBoard(0, 0, 0, 0);

            var userJson = JsonConvert.SerializeObject(user);
            var userResponse = await database.client.Child("users").PostAsync(userJson);

            scoreBoard.UserId = userResponse.Key;
            scoreBoard.UserName = name_register;
            var scoreBoardJson = JsonConvert.SerializeObject(scoreBoard);
            await database.client.Child("scoreBoard").PostAsync(scoreBoardJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return View("Login");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}