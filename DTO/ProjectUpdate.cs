using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crowdfunding.Models;

namespace Crowdfunding.DTO
{
	public class ProjectUpdate
	{
		public ProjectUpdate()
		{
		}
		[Key]
		public Guid UpdateID { get; set; } = Guid.NewGuid();

		[Required]
		public Guid ProjectID { get; set; }

		[ForeignKey("ProjectID")]
		public Project Project { get; set; }

		public string Content { get; set; }

		public DateTime DatePosted { get; set; } = DateTime.UtcNow;
	}
}

