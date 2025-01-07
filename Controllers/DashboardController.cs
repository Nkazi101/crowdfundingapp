using System;
using System.Security.Claims;
using Crowdfunding.Data;
using Crowdfunding.DTO;
using Crowdfunding.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Hosting;
using NuGet.Protocol.Plugins;
using Stripe;

namespace Crowdfunding.Controllers
{
	public class DashboardController : Controller
	{

		private readonly CrowdFundingDBContext _context;



		public DashboardController(CrowdFundingDBContext context)
        {
            _context = context;
        }


		public async Task<IActionResult> Backer()
		{

            //View pledged projects, rewards, and transaction history.
            //ClaimTypes.NameIdentifier specifies identity of the user as object perspective.
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

			var pledges = await _context.Pledges
                .Include(p => p.Project)
                .Include(p => p.Reward)
                .Where(p => p.BackerID == userId)
				.ToListAsync();

            return View(pledges);
        }



        public async Task<IActionResult> Creator()
        {
            //Monitor funding progress, manage backers, and post updates.
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var projects = await _context.Projects
                .Include(p => p.Pledges)
                .Include(p => p.Updates)
                .Where(p => p.CreatorID == userId)
                .ToListAsync();


            return View(projects);
        }

        [HttpPost]
        public async Task<IActionResult> PostUpdate(Guid projectID, string content)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var project = await _context.Projects.FindAsync(projectID);

            if (project == null || project.CreatorID != userId)
            {
                return Unauthorized();
            }

            var update = new ProjectUpdate
            {
                ProjectID = projectID,
                Content = content
            };

            _context.Add(update);
            await _context.SaveChangesAsync();

            return RedirectToAction("Creator");
        }


    }
}

