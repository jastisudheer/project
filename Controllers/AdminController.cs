using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Persol_HMS.Data;
using Persol_HMS.Models;

[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private int GetDepartmentId()
    {
        var user = _context.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
        if(user == null)
        {
            return 5;
        }
        return user.DepartmentId;
    }

    private IActionResult RedirectToHome()
    {
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Index()
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }
        var users = _context.Users.Include(u => u.Department).ToList();
        return View(users);
    }

    public IActionResult Dashboard()
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }

        var viewModel = new AdminDashboardViewModel(){
            DailyDatas = _context.DailyDatas.Where(dd => dd.Date.Month == DateTime.Now.Month).ToList(),
            Records = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Records").ToList(),
            Nurses = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Nursing").ToList(),
            Doctors = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Doctor").ToList(),
            LabPersonnels = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Lab").ToList(),
            Pharmacists = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Pharmacy").ToList(),
            Cashiers = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Cashier").ToList(),
            Admins = _context.Users.Include(u => u.Department).Where(u => u.Department.DepartmentName == "Admin").ToList(),
            Patients = _context.Patients.ToList(),
            TotalUsers = _context.Users.ToList().Count
        };

        return View(viewModel);
    }

    public IActionResult Details(string id)
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }
        if (id == null)
        {
            return NotFound();
        }

        var user = _context.Users
            .Include(u => u.Department)
            .FirstOrDefault(u => u.Id == id);

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    public IActionResult Edit(string id)
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }
        if (id == null)
        {
            return NotFound();
        }

        var user = _context.Users.Find(id);
        if (user == null)
        {
            return NotFound();
        }

        ViewBag.Departments = _context.Departments.ToList();

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string id, User editedUser)
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }
        if (id != editedUser.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Retrieve the existing user from the database
                var userToUpdate = _context.Users
                                        .Include(u => u.Department)
                                        .FirstOrDefault(u => u.Id == id);

                if (userToUpdate == null)
                {
                    return NotFound();
                }

                // Update user properties
                userToUpdate.FirstName = editedUser.FirstName;
                userToUpdate.MiddleName = editedUser.MiddleName;
                userToUpdate.LastName = editedUser.LastName;
                userToUpdate.DateOfBirth = editedUser.DateOfBirth;
                userToUpdate.LockEnabled = editedUser.LockEnabled;
                userToUpdate.Attempts = editedUser.Attempts;
                userToUpdate.LockEnd = editedUser.LockEnd;

                // Update the Department
                userToUpdate.DepartmentId = editedUser.DepartmentId;

                _context.Update(userToUpdate);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(editedUser.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // If ModelState is not valid, reload departments for the dropdown
        ViewBag.Departments = _context.Departments.ToList();

        return View(editedUser);
    }

    public IActionResult Delete(string id)
    {
        ViewBag.deptId = GetDepartmentId();
        if (ViewBag.deptId != 5)
        {
            return RedirectToHome();
        }
        var user = _context.Users.Find(id);
        _context.Users.Remove(user);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    private bool UserExists(string id)
    {
        return _context.Users.Any(e => e.Id == id);
    }
}
