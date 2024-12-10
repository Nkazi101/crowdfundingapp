using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crowdfunding.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Crowdfunding.DTO
{
	//pledge view model for encapsulating data needed for pledging
	public class PledgeViewModel
	{
		public PledgeViewModel()
		{
		}

        [Required]
        public Guid ProjectID { get; set; }

		public string ProjectTitle { get; set; }


        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, Double.MaxValue, ErrorMessage = "Pledge amount must be greater than zero.")]
        public decimal Amount { get; set; }

		public Guid? RewardID { get; set; }

		public List<Reward> Rewards { get; set; }
	}
}


//separations of concerns: have strict boundary between domain model and the data needed by views 
