using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crowdfunding.Models;

namespace Crowdfunding.Models
{
    public class Project
    {
        [Key]
        public Guid ProjectID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid CreatorID { get; set; }

        [ForeignKey("CreatorID")]
        public User Creator { get; set; }

        [Required]
        [MaxLength(150)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        [MaxLength(50)]
        public string Category { get; set; } // E.g., Technology, Art

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal FundingGoal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentFunding { get; set; } = 0;

        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime EndDate { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending Approval"; // Possible values: "Pending Approval", "Active", "Completed", "Cancelled"

        public string MediaUrls { get; set; } // Can be a JSON array or a delimited string

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<Reward> Rewards { get; set; }

        public ICollection<Pledge> Pledges { get; set; }
    }
}