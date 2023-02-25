using CrowdKnowledgeContribution.Data;
using CrowdKnowledgeContribution.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrowdKnowledgeContribution.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext db;

        public HomeController(
            ILogger<HomeController> logger,
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context
            )
        {
            _logger = logger;
            _roleManager = roleManager;
            _userManager = userManager;
            db = context;
        }

        public IActionResult Index()
        {
            List<Category> categories = db.Categories.ToList();
            List<Article> articles = db.Articles.ToList();
            List<Article> new_articles = new List<Article>();

            foreach (var category in categories)
            {
                if (articles.Where(a => a.CategoryId == category.Id).Count() != 0)
                {
                    Article article = articles.OrderByDescending(d => d.Date).First(a => a.CategoryId == category.Id);
                    new_articles.Add(article);
                }

            }

            ViewBag.Categories = categories;
            ViewBag.Articles = new_articles;

            return View();
        }

        public IActionResult Show(int id, string sortingOrder)
        {
            Category category = db.Categories.First(c => c.Id == id);
            List<Article> articles = db.Articles.Where(a => a.CategoryId == id).ToList();

            switch (sortingOrder)
            {
                case "Alfabetic":
                    articles = db.Articles.Include("Category").Include("User").Where(a => a.CategoryId == id).OrderBy(a => a.Title).ToList();
                    break;

                case "Cronologic":
                    articles = db.Articles.Include("Category").Include("User").Where(a => a.CategoryId == id).OrderByDescending(d => d.Date).ToList();
                    break;

                case "Autor":
                    articles = db.Articles.Include("Category").Include("User").Where(a => a.CategoryId == id).OrderByDescending(u => u.User.UserName).ToList();
                    break;

                default:
                    articles = db.Articles.Include("Category").Include("User").Where(a => a.CategoryId == id).OrderByDescending(a => a.Title).ToList();
                    break;

            }

            ViewBag.Articles = articles;
            ViewBag.Category = category;

            return View();
        }

        public IActionResult Contact()
        {
            ViewBag.Message = "This is your contact page for your application";

            return View();
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}