using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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

            //select * from reward where id = "projectid"
            var rewards = await _context.Rewards
                .Where(r => r.ProjectID == projectId)
                .Include(r => r.Project)
                .ToListAsync();

            var project = await _context.Projects.FindAsync(projectId);
            ViewBag.ProjectTitle = project?.Title ?? "Unknown Project";
            ViewBag.ProjectID = projectId;

            return View(rewards);

            //var crowdFundingDBContext = _context.Rewards.Include(r => r.ProjectID);
            //return View(await crowdFundingDBContext.ToListAsync());
        }

        // GET: Reward/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Rewards == null)
            {
                return NotFound();
            }

            var reward = await _context.Rewards
                .Include(r => r.Project)
                .FirstOrDefaultAsync(m => m.RewardID == id);
            if (reward == null)
            {
                return NotFound();
            }

            return View(reward);
        }

        // GET: Reward/Create
        public IActionResult Create(Guid? projectId)
        {

            if(projectId == null)
            {

                return BadRequest("Project ID is required to add Rewards");
            }

            var reward = new Reward { ProjectID = projectId.Value};

            ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category");
            return View();
        }

        // POST: Reward/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RewardID,ProjectID,Title,Description,PledgeAmount,QuantityAvailable,QuantityClaimed,EstimatedDelivery,IsLimited")] Reward reward)
        {
            //if (ModelState.IsValid)
            //{


            //Inserting into the reward table 
                reward.RewardID = Guid.NewGuid();
                _context.Add(reward);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new {projectId = reward.ProjectID});
        }
        //    ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category", reward.ProjectID);
        //    return View(reward);
        //}

        // GET: Reward/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null || _context.Rewards == null)
            {
                return NotFound();
            }

            var reward = await _context.Rewards.FindAsync(id);
            if (reward == null)
            {
                return NotFound();
            }
            ViewData["ProjectID"] = new SelectList(_context.Projects, "ProjectID", "Category", reward.ProjectID);
            return View(reward);
        }

        // POST: Reward/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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
                    _context.Update(reward);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RewardExists(reward.RewardID))
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

            var reward = await _context.Rewards
                .Include(r => r.Project)
                .FirstOrDefaultAsync(m => m.RewardID == id);
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
                return Problem("Entity set 'CrowdFundingDBContext.Rewards'  is null.");
            }
            var reward = await _context.Rewards.FindAsync(id);
            if (reward != null)
            {
                _context.Rewards.Remove(reward);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RewardExists(Guid id)
        {
          return (_context.Rewards?.Any(e => e.RewardID == id)).GetValueOrDefault();
        }
    }
}
