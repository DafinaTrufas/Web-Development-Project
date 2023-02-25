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
    [Authorize(Roles="Admin")]
    public class ArticleHistoryController : Controller
    {
        

        private readonly ApplicationDbContext db;
        
        public ArticleHistoryController(ApplicationDbContext context)
        {
            db = context;
        }

        public IActionResult Index(int id)
        {
            List<ArticleHistory> article_versions = db.ArticleHistories.Include("Category")
                                                                      .Include("User")
                                                                      .Include("Comments")
                                                                      .Include("Comments.User")
                                                                      .Where(art => art.Id == id)
                                                                      .OrderByDescending(art => art.Date)
                                                                      .ToList();
            ViewBag.Istoric = article_versions;
            return View();
        }

        public IActionResult UpdateArticle(int id, DateTime date)
        {
            List<ArticleHistory> article_versions = db.ArticleHistories.Include("Category")
                                                                .Include("User")
                                                                .Include("Comments")
                                                                .Include("Comments.User")
                                                                .Where(art => art.Id == id)
                                                                .ToList();

            

            Article article = db.Articles.Include("Category")
                                         .Include("User")
                                         .Include("Comments")
                                         .Include("Comments.User")
                                         .Where(art => art.Id == id)
                                         .First();



            ///{1/7/2023 1:44:51 PM}
            ///{1/7/2023 4:19:51 PM}
            ///{1/7/2023 10:54:21 PM}

            int nr = 0;

            foreach (ArticleHistory art in article_versions)
            {
                nr++;

                var ok = art.Date.Equals(date);

                if(date.Date == art.Date.Date && date.Hour == art.Date.Hour && date.Minute == art.Date.Minute && date.Second == art.Date.Second)
                {
                    
                    article.Title = art.Title;
                    article.Content = art.Content;
                    article.Date = date;
                    article.CategoryId = art.CategoryId;
                    article.Comments = art.Comments;
                    article.User = art.User;
                    article.UserId = art.UserId;
                    article.Categ = art.Categ;

                    db.SaveChanges();
                }
                
            }

            return View(article);

        }
    }
}
