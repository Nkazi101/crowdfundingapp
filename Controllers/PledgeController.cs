using System;
using System.Security.Claims;
using Crowdfunding.Config;
using Crowdfunding.Data;
using Crowdfunding.DTO;
using Crowdfunding.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe;

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


        [HttpPost]
        public async Task<IActionResult> ProcessPayment(PaymentViewModel model, string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                return BadRequest("Payment intent id is missing");
            }

            try
            {
                // 1. Check if the PaymentIntent exists on Stripe's side (Optional but recommended)
                //var stripeService = new PaymentIntentService();
                //var paymentIntent = await stripeService.GetAsync(paymentIntentId);

                //if (paymentIntent.Status != "succeeded")
                //{
                //    return BadRequest($"Payment was not successful. Stripe status: {paymentIntent.Status}");
                //}

                // 2. Create a new Pledge entity
                var pledge = new Pledge
                {
                    ProjectID = model.ProjectID,
                    PledgeAmount = model.Amount,
                    RewardID = model.RewardID, // Can be null if no reward was selected
                    BackerID = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
                    PaymentIntentID = paymentIntentId, // Store the Stripe PaymentIntent ID
                    PledgeDate = DateTime.UtcNow
                };

                // 3. Add and save the Pledge to the database
                _context.Pledges.Add(pledge);
                await _context.SaveChangesAsync();

                // 4. update the Project's total pledged amount
                //var project = await _context.Projects.FindAsync(model.ProjectID);
                //if (project != null)
                //{
                //    project.AmountRaised += model.Amount;
                //    await _context.SaveChangesAsync();
                //}

                return RedirectToAction("PaymentSuccess", "Pledge", new { paymentIntentId = paymentIntentId });
            }
            catch (StripeException ex)
            {
                
                ModelState.AddModelError("", $"Stripe error: {ex.Message}");
                return View("Error"); 
            }
            catch (DbUpdateException ex)
            {
                
                ModelState.AddModelError("", $"Database error: {ex.Message}");
                return View("Error");
            }
            catch (Exception ex)
            {
                
                ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                return View("Error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentIntent(PaymentViewModel model)
        {

            //create a payment intent
            var options = new PaymentIntentCreateOptions
            {

                Amount = (long)(model.Amount * 100), //convert to cents
                Currency = "usd",
                Description = $"Pledge for Project {model.ProjectID}",
                Metadata = new Dictionary<string, string>
                {
                    {"ProjectID", model.ProjectID.ToString() },
                    {"RewardID", model.RewardID?.ToString() },
                    {"UserID", User.FindFirstValue(ClaimTypes.NameIdentifier) }

                }

            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            //pass the secret key to the view
            model.SecretKey = paymentIntent.ClientSecret;

            return View(model);

        }

        [HttpGet]
        [Authorize]
        public IActionResult PaymentSuccess(string paymentIntentId)
        {
            if (string.IsNullOrEmpty(paymentIntentId))
            {
                return BadRequest("Payment Intent ID is missing."); 
            }

          
            //try
            //{
            //    var service = new PaymentIntentService();
            //    var paymentIntent = service.Get(paymentIntentId); 

                
            //    ViewBag.Amount = paymentIntent.AmountReceived / 100M;
            //    ViewBag.Currency = paymentIntent.Currency;
            //    ViewBag.PaymentIntentId = paymentIntentId;
               
            //}
            //catch (StripeException ex)
            //{
            //    // Handle Stripe errors (logging, error display)
            //    ModelState.AddModelError("", $"Stripe Error: {ex.Message}");
            //    return View("Error");
            //}

            return View(); 
        }

    }
}

