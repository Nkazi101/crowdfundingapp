using System;
using System.Collections.Generic;
using System.Linq; // LINQ namespace
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore; // Entity Framework Core namespace
using Crowdfunding.Data;
using Crowdfunding.Models;

namespace Crowdfunding.Controllers
{
    public class ProjectController : Controller
    {
        private readonly CrowdFundingDBContext _context;

        public ProjectController(CrowdFundingDBContext context)
        {
            _context = context;
        }

        // GET: Project
        public async Task<IActionResult> Index(string category, string sortOrder, string fundingStatus, string searchString)
        {
            // Using ViewData to maintain the current filter and sort order.
            ViewData["CurrentCategory"] = category;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentFundingStatus"] = fundingStatus;
            ViewData["CurrentSearch"] = searchString;

            // Start with all projects, including their related Pledges.
            // AsQueryable allows for building the query step by step.
            var projects = _context.Projects
                .Include(p => p.Pledges) // Eagerly load the Pledges related to each Project.
                .AsQueryable(); // Convert to IQueryable to enable LINQ query building.

            if (!string.IsNullOrEmpty(category))
            {
                projects = projects.Where(p => p.Category == category);
            }


            //search by title or description
            if (!string.IsNullOrEmpty(searchString))
            {
                projects = projects.Where(p => p.Title.Contains(searchString) || p.Description.Contains(searchString));
            }


            // Filter projects based on the funding status if provided.
            if (!string.IsNullOrEmpty(fundingStatus))
            {
                if (fundingStatus == "Funded")
                {
                    //select * from project where currentfunding >= fundinggoal
                    // Use LINQ Where to filter projects where CurrentFunding >= FundingGoal.
                    projects = projects.Where(p => p.CurrentFunding >= p.FundingGoal);
                }
                else if (fundingStatus == "Unfunded")
                {
                    // Filter projects where CurrentFunding < FundingGoal.
                    projects = projects.Where(p => p.CurrentFunding < p.FundingGoal);
                }
            }

            // Apply sorting based on the sortOrder parameter.
            switch (sortOrder)
            {
                case "popular":
                    // Order projects by the number of Pledges in descending order.
                    projects = projects.OrderByDescending(p => p.Pledges.Count);
                    break;
                case "recent":
                    // Order projects by DateCreated in descending order.
                    projects = projects.OrderByDescending(p => p.DateCreated);
                    break;
                default:
                    // Default ordering by Title in ascending order.
                    projects = projects.OrderBy(p => p.Title);
                    break;
            }

            // Get distinct categories from the Projects for the dropdown in the view.
            var categories = await _context.Projects
                .Select(p => p.Category) // Select only the Category field.
                .Distinct() // Get unique categories.
                .ToListAsync(); // Execute the query asynchronously and get the list.

            //viewbag passes the categories to the view
            ViewBag.Categories = categories;

            // Execute the projects query asynchronously and pass the result to the view.
            return View(await projects.ToListAsync());
        }

        // GET: Project/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            // Find the project by ID and include the Creator, Pledges, navigation property.
            var project = await _context.Projects
                .Include(p => p.Creator) // Eagerly load the Creator of the project.
                .Include(p => p.Pledges)
                .ThenInclude(pl => pl.Backer)
                .FirstOrDefaultAsync(m => m.ProjectID == id); // Get the first matching project.

            if (project == null)
            {
                return NotFound();
            }

            //backer count
            int backerCount = project.Pledges.Select(pl => pl.BackerID).Distinct().Count();

            ViewBag.BackerCount = backerCount;

            return View(project);
        }

        // GET: Project/Create
        public IActionResult Create()
        {
            ViewData["CreatorID"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Project/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProjectID,CreatorID,Title,Description,Category,FundingGoal,CurrentFunding,StartDate,EndDate,Status,MediaUrls,DateCreated,LastUpdated")] Project project)
        {
            // Generate a new GUID for the ProjectID.
            project.ProjectID = Guid.NewGuid();
            _context.Add(project); // Add the new project to the context.
            await _context.SaveChangesAsync(); // Save changes to the database asynchronously.
            return RedirectToAction(nameof(Index));
        }

        // GET: Project/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            // Find the project by ID.
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            ViewData["CreatorID"] = new SelectList(_context.Users, "Id", "Id", project.CreatorID);
            return View(project);
        }

        // POST: Project/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ProjectID,CreatorID,Title,Description,Category,FundingGoal,CurrentFunding,StartDate,EndDate,Status,MediaUrls,DateCreated,LastUpdated")] Project project)
        {
            if (id != project.ProjectID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(project); // Update the project in the context.
                    await _context.SaveChangesAsync(); // Save changes to the database.
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.ProjectID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throw the exception if it's not due to the project not existing.
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CreatorID"] = new SelectList(_context.Users, "Id", "Id", project.CreatorID);
            return View(project);
        }

        // GET: Project/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            // Find the project by ID and include the Creator.
            var project = await _context.Projects
                .Include(p => p.Creator)
                .FirstOrDefaultAsync(m => m.ProjectID == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Project/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'CrowdFundingDBContext.Projects'  is null.");
            }

            // Find the project by ID.
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                _context.Projects.Remove(project); // Remove the project from the context.
            }

            await _context.SaveChangesAsync(); // Save changes to the database.
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(Guid id)
        {
            // Check if any project exists with the given ID using LINQ Any method.
            return (_context.Projects?.Any(e => e.ProjectID == id)).GetValueOrDefault();
        }
    }
}