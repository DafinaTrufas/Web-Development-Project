using CrowdKnowledgeContribution.Data;
using CrowdKnowledgeContribution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace CrowdKnowledgeContribution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext db;
        private IWebHostEnvironment _env;

        public CategoriesController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            db = context;
            _env = env;
        }
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var categories = from category in db.Categories
                             orderby category.CategoryName
                             select category;
            ViewBag.Categories = categories;
            return View();
        }

        public ActionResult Show(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(Category category, IFormFile Icon)
        {
            if (category.CategoryName != null && Icon != null)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "images", Icon.FileName);
                var databaseFileName = "/images/" + Icon.FileName;

                using (var fileStream = new FileStream(storagePath, FileMode.Create))
                {
                    await Icon.CopyToAsync(fileStream);
                }

                category.Icon = databaseFileName;
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["message"] = "Categoria a fost adaugata";
                return RedirectToAction("Index");
            }

            else
            {
                if (category.CategoryName == null)
                    ViewBag.Message = "Numele categoriei este obligatoriu.";

                if (Icon == null)
                    ViewBag.Message += "\nIcon-ul categoriei este obligatoriu.";

                return View(category);
            }

            return await Task.Run<IActionResult>(() =>
            {
                if (true)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(category);
                }
            });
        }

        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, IFormFile Icon, Category requestCategory)
        {

            Category categ = db.Categories.Find(id);

            if (Icon != null)
            {
                var storagePath = Path.Combine(_env.WebRootPath, "images", Icon.FileName);
                var databaseFileName = "/images/" + Icon.FileName;

                bool fileExists = new FileInfo(databaseFileName).Exists;

                if (fileExists)
                    using (var fileStream = new FileStream(storagePath, FileMode.Open))
                    {
                        await Icon.CopyToAsync(fileStream);
                    }

                else
                    using (var fileStream = new FileStream(storagePath, FileMode.Create))
                    {
                        await Icon.CopyToAsync(fileStream);
                    }

                categ.CategoryName = requestCategory.CategoryName;
                categ.Icon = databaseFileName;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata";
            }

            else
            {
                if (Icon == null)
                    ViewBag.Message = "Icon-ul categoriei este obligatoriu.";

                return View(categ);
            }

            return await Task.Run<IActionResult>(() =>
            {
                if (true)
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(categ);
                }
            });
        }

        /*[HttpPost]
        public ActionResult Edit(int id, Category requestCategory)
        {
            Category category = db.Categories.Find(id);

            if (ModelState.IsValid)
            {

                category.CategoryName = requestCategory.CategoryName;
                category.Icon = requestCategory.Icon;
                db.SaveChanges();
                TempData["message"] = "Categoria a fost modificata.";
                return RedirectToAction("Index");
            }
            else
            {
                return View(requestCategory);
            }
        }*/

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            TempData["message"] = "Categoria a fost stearsa.";
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
