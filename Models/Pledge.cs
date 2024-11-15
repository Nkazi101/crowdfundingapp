using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Crowdfunding.Models
{
    public class Pledge
    {
        [Key]
        public Guid PledgeID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public Project Project { get; set; }


        [Required]
        public Guid BackerID { get; set; }

        [ForeignKey("BackerID")]
        public User Backer { get; set; }

        public Guid? RewardID { get; set; } // Nullable if no reward selected

        [ForeignKey("RewardID")]
        public Reward Reward { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, Double.MaxValue, ErrorMessage = "Pledge amount must be greater than zero.")]
        public decimal PledgeAmount { get; set; }

        public DateTime PledgeDate { get; set; } = DateTime.UtcNow;

        public bool IsAnonymous { get; set; } = false;

        // Navigation property
        public Transaction Transaction { get; set; }
    }
}