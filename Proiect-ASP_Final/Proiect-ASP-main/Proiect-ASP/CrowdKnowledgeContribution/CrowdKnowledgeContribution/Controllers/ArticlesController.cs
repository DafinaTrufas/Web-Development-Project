using CrowdKnowledgeContribution.Data;
using CrowdKnowledgeContribution.Models;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CrowdKnowledgeContribution.Controllers
{
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ArticlesController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [Authorize(Roles = "Editor interzis,Editor,Admin")]
        public IActionResult Index()
        {
            var articles = db.Articles.Include("Category").Include("User").OrderBy(a => a.Date);

            var search = "";

            if (Convert.ToString(HttpContext.Request.Query["search"]) != null)
            {
                search = Convert.ToString(HttpContext.Request.Query["search"]).Trim();

                List<int> articleIds = db.Articles.Where
                                        (
                                        at => at.Title.Contains(search)
                                        || at.Content.Contains(search)
                                        ).Select(a => a.Id).ToList();

                List<int> articleIdsOfCommentsWithSearchString = db.Comments.Where
                                        (
                                        c => c.Content.Contains(search)
                                        ).Select(c => (int)c.ArticleId).ToList();

                List<int> mergedIds = articleIds.Union(articleIdsOfCommentsWithSearchString).ToList();

                articles = db.Articles.Where(article => mergedIds.Contains(article.Id))
                            .Include("Category")
                            .Include("User")
                            .OrderBy(a => a.Date);
            }

            ViewBag.SearchString = search;

            ViewBag.Articles = articles;

            int _perPage = 4;

            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            int totalItems = articles.Count();

            var currentPage = Convert.ToInt32(HttpContext.Request.Query["page"]);

            var offset = 0;

            if (!currentPage.Equals(0))
            {
                offset = (currentPage - 1) * _perPage;
            }

            var paginatedArticles = articles.Skip(offset).Take(_perPage);

            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)_perPage);

            ViewBag.Articles = paginatedArticles;

            if (search != "")
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?search=" + search + "&page";
            }
            else
            {
                ViewBag.PaginationBaseUrl = "/Articles/Index/?page";
            }

            return View();
        }

        private void SetAccessRights()
        {
            ViewBag.AfisareButoane = false;

            if (User.IsInRole("Editor"))
            {
                ViewBag.AfisareButoane = true;
            }

            ViewBag.UserCurent = _userManager.GetUserId(User);

            ViewBag.EsteAdmin = User.IsInRole("Admin");
        }

        private void SetAccessForArticleHistory()
        {
            ViewBag.VersionControl = false;

            if(User.IsInRole("Admin"))
            {
                ViewBag.VersionControl = true;
            }
        }

        [Authorize(Roles = "Editor interzis,Editor,Admin")]
        public IActionResult Show(int id)
        {
            Article article = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == id)
                                         .First();

            SetAccessRights();

            SetAccessForArticleHistory();

            SetProtectUnProtect(id);

            return View(article);
        }

        [HttpPost]
        public IActionResult Show([FromForm] Comment comment)
        {
            comment.Date = DateTime.Now;
            comment.UserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
                return Redirect("/Articles/Show/" + comment.ArticleId);
            }

            else
            {
                Article art = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == comment.ArticleId)
                                         .First();

                SetAccessRights();

                SetAccessForArticleHistory();

                return View(art);
            }
        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New()
        {
            Article article = new Article();
            article.Categ = GetAllCategories();

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult New(Article article)
        {
            var sanitizer = new HtmlSanitizer();

            
            article.Date = DateTime.Now;
            article.UserId = _userManager.GetUserId(User);

           

            if (ModelState.IsValid)
            {
                article.Content = sanitizer.Sanitize(article.Content);
                db.Articles.Add(article);
                db.SaveChanges();

                ArticleHistory articleHistory = new ArticleHistory();

                articleHistory.Id = article.Id;
                articleHistory.Content = article.Content;
                articleHistory.Title = article.Title;
                articleHistory.Date = DateTime.Now;
                articleHistory.UserId = _userManager.GetUserId(User);
                articleHistory.CategoryId = article.CategoryId;

                db.ArticleHistories.Add(articleHistory);
                db.SaveChanges();

                TempData["message"] = "Articolul a fost adaugat.";
                return RedirectToAction("Index");

            }
            else
            {
                article.Categ = GetAllCategories();
                return View(article);
            }

        }

        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id)
        {

            SetProtectUnProtect(id);



            Article article = db.Articles.Include("Category")
                                        .Where(art => art.Id == id)
                                        .First();

            article.Categ = GetAllCategories();

            if ((article.UserId == _userManager.GetUserId(User) && ViewBag.AfisareProtect == true) || User.IsInRole("Admin"))
            {
                return View(article);
            }
            else if (article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine.";
            else if (ViewBag.AfisareProtect == false)
                TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care este arhivat.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public IActionResult Edit(int id, Article requestArticle)
        {
            var sanitizer = new HtmlSanitizer();
            SetProtectUnProtect(id);

            Article article = db.Articles.Find(id);

            if (ModelState.IsValid)
            {
                if ((article.UserId == _userManager.GetUserId(User) && ViewBag.AfisareProtect == true) || User.IsInRole("Admin"))
                {
                    article.Title = requestArticle.Title;
                    requestArticle.Content = sanitizer.Sanitize(requestArticle.Content);
                    article.Content = requestArticle.Content;
                    article.CategoryId = requestArticle.CategoryId;
                    article.Id = requestArticle.Id;
                    article.UserId = _userManager.GetUserId(User);


                    ArticleHistory articleHistory = new ArticleHistory();

                    articleHistory.Title = article.Title;
                    articleHistory.Id = article.Id;
                    articleHistory.Date = DateTime.Now;
                    articleHistory.UserId = _userManager.GetUserId(User);
                    articleHistory.CategoryId = article.CategoryId;
                    articleHistory.Content = article.Content;


                    db.ArticleHistories.Add(articleHistory);


                    TempData["message"] = "Articolul a fost modificat.";
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else if (article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care nu va apartine.";
                else if (ViewBag.AfisareProtect == false)
                    TempData["message"] = "Nu aveti dreptul sa faceti modificari asupra unui articol care este arhivat.";
                return RedirectToAction("Index");
            }
            else
            {
                requestArticle.Categ = GetAllCategories();
                return View(requestArticle);
            }

        }

        [HttpPost]
        [Authorize(Roles = "Editor,Admin")]
        public ActionResult Delete(int id)
        {
            Article article = db.Articles.Include("Comments")
                                         .Where(art => art.Id == id)
                                         .First();

            List<ArticleHistory> article_versions = new List<ArticleHistory>();
            article_versions = db.ArticleHistories.Include("Comments").Where(art => art.Id == id).ToList();

           // List<ArticleHistory> article_versions =  db.ArticleHistories.Include("Comments").Where(art => art.Id == id).ToList();

            if ((article.UserId == _userManager.GetUserId(User) && ViewBag.AfisareProtect == true) || User.IsInRole("Admin"))
            {
                db.Articles.Remove(article);
                db.SaveChanges();

                foreach (ArticleHistory art in article_versions)
                    db.ArticleHistories.Remove(art);

                db.SaveChanges();
                TempData["message"] = "Articolul a fost sters.";
                return RedirectToAction("Index");
            }
            else if (article.UserId != _userManager.GetUserId(User) && !User.IsInRole("Admin"))
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care nu va apartine.";
            else if (ViewBag.AfisareProtect == false)
                TempData["message"] = "Nu aveti dreptul sa stergeti un articol care este arhivat.";
            return RedirectToAction("Index");
        }

        private void SetProtectUnProtect(int id)
        {
            ViewBag.AfisareProtect = false;
            ViewBag.AfisareUnProtect = false;
            if (db.ProtectedArticles.Find(id) == null)
            {
                ViewBag.AfisareProtect = true;
            }
            else
            {
                ViewBag.AfisareUnProtect = true;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Protect(int id)
        {
            if (db.ProtectedArticles.Find(id) == null)
            {
                Article article = db.Articles.Find(id);
                ProtectedArticle protectedArticle = new ProtectedArticle();
                protectedArticle.ProtectedArticleId = article.Id;
                protectedArticle.Date = DateTime.Now;
                protectedArticle.UserId = _userManager.GetUserId(User);
                db.ProtectedArticles.Add(protectedArticle);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost arhivat.";
            }
            else TempData["message"] = "Articolul este deja arhivat.";
            return RedirectToAction("Index");

        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UnProtect(int id)
        {
            if (db.ProtectedArticles.Find(id) != null)
            {
                ProtectedArticle article = db.ProtectedArticles.Find(id);
                db.ProtectedArticles.Remove(article);
                db.SaveChanges();
                TempData["message"] = "Articolul a fost dezarhivat.";
            }

            else TempData["message"] = "Articolul nu este arhivat.";
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.CategoryName.ToString()
                });
            }

            return selectList;
        }

        public IActionResult IndexNou()
        {
            return View();
        }
    }
}
