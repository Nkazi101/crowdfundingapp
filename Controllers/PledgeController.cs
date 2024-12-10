using System;
using Crowdfunding.Config;
using Crowdfunding.Data;
using Crowdfunding.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Crowdfunding.Controllers
{
	public class PledgeController : Controller
	{

        private readonly CrowdFundingDBContext _context;
        private readonly StripeSettings _stripeSettings;

        public PledgeController(CrowdFundingDBContext context, IOptions<StripeSettings> stripeSettings)
        {
            _context = context;
            _stripeSettings = stripeSettings.Value;
        }

        //GET : Pledge/ProjectID

        //asynchronous allows a program to start a long-running task. and then continue executing other code while waiting for the task to complete
        public async Task<IActionResult> Index(Guid projectId)
        {

            var project = await _context.Projects
                .Include(p => p.Rewards)
                .FirstOrDefaultAsync(p => p.ProjectID == projectId);


            if (project == null)
            {
                return NotFound();
            }

            var model = new PledgeViewModel
            {
                ProjectID = projectId,
                ProjectTitle = project.Title,
                Rewards = project.Rewards.ToList()
        };

        return View(model);

        }


        [HttpPost]
        public async Task<IActionResult> Index(PledgeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                //stores the pledge data temporarily in the session
                TempData["PledgeData"] = JsonConvert.SerializeObject(model);

                return RedirectToAction("ProcessPayment");
            }

            //reload rewards if model is not valid
            var project = await _context.Projects
            .Include(p => p.Rewards)
            .FirstOrDefaultAsync(p => p.ProjectID == model.ProjectID);

            model.Rewards = project.Rewards.ToList();

            return View(model);
        }


        public async Task<IActionResult> ProcessPayment()
        {

            var pledgeDataJson = TempData["PledgeData"] as string;

            if (string.IsNullOrEmpty(pledgeDataJson))
            {
                return RedirectToAction("Index");
            }

            var pledgeData = JsonConvert.DeserializeObject<PledgeViewModel>(pledgeDataJson);

            var model = new PaymentViewModel
            {
                ProjectID = pledgeData.ProjectID,
                Amount = pledgeData.Amount,
                RewardID = pledgeData.RewardID,
                PublicKey = _stripeSettings.PublicKey
            };

            return View(model);

        }



    }
}

