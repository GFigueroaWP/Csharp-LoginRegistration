using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Login.Models;
using Microsoft.AspNetCore.Identity;

namespace Login.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private MyContext _context;

    public UserController(ILogger<HomeController> logger, MyContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpPost("users/create")]
    public IActionResult RegisterUser(User user)
    {
        if (ModelState.IsValid)
        {
            PasswordHasher<User> Hasher = new PasswordHasher<User>();
            // Updating our newUser's password to a hashed version
            user.Password = Hasher.HashPassword(user, user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UserId", user.UserId);
            return RedirectToAction("Success", "Home");
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }
    [HttpPost("users/login")]
    public IActionResult Login(LoginUser userSubmission)
{
    if(ModelState.IsValid)
    {
        // If initial ModelState is valid, query for a user with the provided email
        User? userInDb = _context.Users.FirstOrDefault(u => u.Email == userSubmission.Email);
        // If no user exists with the provided email
        if(userInDb == null)
        {
            // Add an error to ModelState and return to View!
            ModelState.AddModelError("Email", "Invalid Email/Password");
            return RedirectToAction("Index", "Home");
        }
        // Otherwise, we have a user, now we need to check their password
        // Initialize hasher object
        PasswordHasher<LoginUser> hasher = new PasswordHasher<LoginUser>();
        // Verify provided password against hash stored in db
        var result = hasher.VerifyHashedPassword(userSubmission, userInDb.Password, userSubmission.Password);                                    // Result can be compared to 0 for failure
        if(result == 0)
        {
            return RedirectToAction("Index", "Home");
        }
        // Handle success (this should route to an internal page)
        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
        return RedirectToAction("Success", "Home");
    } else {
        return RedirectToAction("Index", "Home");
    }
}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
