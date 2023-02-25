using CrowdKnowledgeContribution.Data;
using CrowdKnowledgeContribution.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Drawing.Printing;

namespace CrowdKnowledgeContribution.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UsersController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        public ActionResult Index()
        {
            if (TempData.ContainsKey("message"))
            {
                ViewBag.message = TempData["message"].ToString();
            }

            var users = from user in db.Users
                        orderby user.UserName
                        select user;
            ViewBag.Users = users;
            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roleNames = await _userManager.GetRolesAsync(user);

            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Name)
                                              .First();
            SetBlockUnBlock(currentUserRole);
            return View(user);
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }

        public ActionResult New()
        {
            ApplicationUser user = new ApplicationUser();

            user.AllRoles = GetAllRoles();

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> New(ApplicationUser user, [FromForm] string role)
        {
            if (ModelState.IsValid)
            {
                user.NormalizedUserName = user.UserName.ToUpper();
                user.NormalizedEmail = user.Email.ToUpper();
                var hasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = hasher.HashPassword(null, user.PasswordHash);
                user.EmailConfirmed = true;
                db.Users.Add(user);
                db.SaveChanges();

                var roleName = await _roleManager.FindByIdAsync(role);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                TempData["message"] = "Utilizatorul a fost adaugat.";
                return RedirectToAction("Index");
            }

            else
            {
                user.AllRoles = GetAllRoles();
                return View(user);
            }
        }
        public async Task<ActionResult> Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();

            var roleNames = await _userManager.GetRolesAsync(user);

            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Id)
                                              .First();
            ViewBag.UserRole = currentUserRole;

            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(string id, ApplicationUser newData, [FromForm] string newRole)
        {
            ApplicationUser user = db.Users.Find(id);

            user.AllRoles = GetAllRoles();


            if (ModelState.IsValid)
            {
                user.UserName = newData.UserName;
                user.Email = newData.Email;
                user.FirstName = newData.FirstName;
                user.LastName = newData.LastName;
                user.PhoneNumber = newData.PhoneNumber;

                var roles = db.Roles.ToList();

                foreach (var role in roles)
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
         
                var roleName = await _roleManager.FindByIdAsync(newRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                db.SaveChanges();
                TempData["message"] = "Au fost modificate datele utilizatorului.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            db.Users.Remove(user);
            TempData["message"] = "Utilizatorul a fost eliminat.";
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void SetBlockUnBlock(string role)
        {
            ViewBag.AfisareBlock = false;
            ViewBag.AfisareUnBlock = false;
            if (role == "Editor")
            {
                ViewBag.AfisareBlock = true;
            }
            else if (role == "Editor interzis")
            {
                ViewBag.AfisareUnBlock = true;
            }
        }

        [HttpPost]
        public async Task<ActionResult> Block(string id)
        {

            ApplicationUser user = db.Users.Find(id);
            var roleNames = await _userManager.GetRolesAsync(user);

            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Name)
                                              .First();
            
            if (currentUserRole.ToString() == "Editor")
            {
                TempData["message"] = "Utilizatorul a fost blocat.";
                await _userManager.RemoveFromRoleAsync(user, "Editor");
                await _userManager.AddToRoleAsync(user, "Editor interzis");
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<ActionResult> UnBlock(string id)
        {

            ApplicationUser user = db.Users.Find(id);
            var roleNames = await _userManager.GetRolesAsync(user);

            var currentUserRole = _roleManager.Roles
                                              .Where(r => roleNames.Contains(r.Name))
                                              .Select(r => r.Name)
                                              .First();
            if (currentUserRole.ToString() == "Editor interzis")
            {
                TempData["message"] = "Utilizatorul a fost deblocat.";
                await _userManager.RemoveFromRoleAsync(user, "Editor interzis");
                await _userManager.AddToRoleAsync(user, "Editor");
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}
