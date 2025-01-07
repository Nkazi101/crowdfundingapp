using System;
using Crowdfunding.Data;
using Crowdfunding.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Crowdfunding.Controllers
{
	public class MessagesController : Controller
	{

		private readonly CrowdFundingDBContext _context;
		private readonly UserManager<User> _userManager;


		public MessagesController(CrowdFundingDBContext context, UserManager<User> userManager)
		{
			_context = context;
			_userManager = userManager;
		}


		//messages/inbox

		public async Task<IActionResult> Inbox()
		{
			var currentUser = await _userManager.GetUserAsync(User);

			var messages = await _context.Messages
				.Include(m => m.Sender)
				.Where(m => m.ReceiverID == currentUser.Id)
				.OrderByDescending(m => m.SentDate)
				.ToListAsync();

			return View(messages);
							
					
		}

        //messages/outbox

        public async Task<IActionResult> Outbox()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            var messages = await _context.Messages
                .Include(m => m.Receiver)
                .Where(m => m.SenderID == currentUser.Id)
                .OrderByDescending(m => m.SentDate)
                .ToListAsync();

            return View(messages);


        }

        //messages/create(get)

		public async Task<IActionResult> Create(Guid? receiverId)
		{

			var currentUserId = _userManager.GetUserId(User);

			//loading our list of users(excluding signed in user)
			var receivers = _userManager.Users
				.Where(u => u.Id.ToString() != currentUserId) //excludes currently signed in user
				.Select(u => new { u.Id, u.UserName })
				.ToList();

			//pass the list to the view
			ViewBag.Receivers = receivers;

			//if a receiver is already specified
			var model = new Message();
			if (receiverId.HasValue)
			{
				model.ReceiverID = receiverId.Value;
			}

			return View(model);
        }


		//message/create(post)
		[HttpPost]
		public async Task<IActionResult> Create(Message message)
		{

            var currentUser = await _userManager.GetUserAsync(User);
			message.SenderID = currentUser.Id;
			message.SentDate = DateTime.UtcNow;

			_context.Messages.Add(message);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Outbox));


        }


		//messages/details/{id}

		public async Task<IActionResult> Details(Guid id)
		{

			var message = await _context.Messages
				.Include(m => m.Sender)
				.Include(m => m.Receiver)
				.FirstOrDefaultAsync(m => m.MessageID == id);

			if(message == null)
			{
				return NotFound();
			}

            var currentUser = await _userManager.GetUserAsync(User);
			if(message.ReceiverID == currentUser.Id && !message.IsRead)
			{

				message.IsRead = true;
				_context.Update(message);
				await _context.SaveChangesAsync();
			}


			return View(message);
        }

	}
}

