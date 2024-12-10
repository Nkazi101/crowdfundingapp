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
    public class RewardController : Controller
    {
        private readonly CrowdFundingDBContext _context;

        public RewardController(CrowdFundingDBContext context)
        {
            _context = context;
        }

        // GET: Reward/ProjectID
        public async Task<IActionResult> Index(Guid? projectId)
        {
            if (projectId == null)
            {
                return BadRequest("Project is required");
            }

            // LINQ query to select rewards where ProjectID matches the provided projectId.
            var rewards = await _context.Rewards
                .Where(r => r.ProjectID == projectId) // Filters rewards by ProjectID.
                .Include(r => r.Project) // Eagerly loads the related Project entity.
                .ToListAsync(); // Executes the query asynchronously and returns a list.

            // Fetch the project to get its Title.
            var project = await _context.Projects.FindAsync(projectId);
            ViewBag.ProjectTitle = project?.Title ?? "Unknown Project";
            ViewBag.ProjectID = projectId;

            return View(rewards);
        }

        // GET: Reward/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Rewards == null)
            {
                return NotFound();
            }

            // LINQ query to find the reward by RewardID and include the related Project.
            var reward = await _context.Rewards
                .Include(r => r.Project) // Eagerly loads the Project related to the reward.
                .FirstOrDefaultAsync(m => m.RewardID == id); // Gets the first reward matching the ID.

            if (reward == null)
            {
                return NotFound();
            }

            return View(reward);
        }

        // GET: Reward/Create
        public IActionResult Create(Guid? projectId)
        {
            if (projectId == null)
            {
                return BadRequest("Project ID is required to add Rewards");
            }

            var reward = new Reward { ProjectID = projectId.Value };

            // Creates a SelectList for the Projects dropdown, if needed.
            ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category");
            return View();
        }

        // POST: Reward/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RewardID,ProjectID,Title,Description,PledgeAmount,QuantityAvailable,QuantityClaimed,EstimatedDelivery,IsLimited")] Reward reward)
        {
            // Generate a new GUID for the RewardID.
            reward.RewardID = Guid.NewGuid();
            _context.Add(reward); // Adds the new reward to the context.
            await _context.SaveChangesAsync(); // Saves changes to the database asynchronously.
            return RedirectToAction(nameof(Index), new { projectId = reward.ProjectID });
        }

        // GET: Reward/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Rewards == null)
            {
                return NotFound();
            }

            // Finds the reward by RewardID.
            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null)
            {
                return NotFound();
            }

            // Prepares the Projects SelectList for the view.
            ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category", reward.ProjectID);
            return View(reward);
        }

        // POST: Reward/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("RewardID,ProjectID,Title,Description,PledgeAmount,QuantityAvailable,QuantityClaimed,EstimatedDelivery,IsLimited")] Reward reward)
        {
            if (id != reward.RewardID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reward); // Updates the reward in the context.
                    await _context.SaveChangesAsync(); // Saves changes to the database.
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RewardExists(reward.RewardID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw; // Re-throws the exception if not due to reward not existing.
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category", reward.ProjectID);
            return View(reward);
        }

        // GET: Reward/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.Rewards == null)
            {
                return NotFound();
            }

            // LINQ query to find the reward and include the related Project.
            var reward = await _context.Rewards
                .Include(r => r.Project) // Includes the Project related to the reward.
                .FirstOrDefaultAsync(m => m.RewardID == id); // Gets the first reward matching the ID.

            if (reward == null)
            {
                return NotFound();
            }

            return View(reward);
        }

        // POST: Reward/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.Rewards == null)
            {
                return Problem("Entity set 'CrowdFundingDBContext.Rewards' is null.");
            }

            // Finds the reward by RewardID.
            var reward = await _context.Rewards.FindAsync(id);
            if (reward != null)
            {
                _context.Rewards.Remove(reward); // Removes the reward from the context.
            }

            await _context.SaveChangesAsync(); // Saves changes to the database.
            return RedirectToAction(nameof(Index));
        }

        private bool RewardExists(Guid id)
        {
            // Checks if any reward exists with the given ID using LINQ Any method.
            return (_context.Rewards?.Any(e => e.RewardID == id)).GetValueOrDefault();
        }
    }
}