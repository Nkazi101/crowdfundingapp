using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Crowdfunding.Models
{
    public class Reward
    {
        [Key]
        public Guid RewardID { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ProjectID { get; set; }

        [ForeignKey("ProjectID")]
        public Project Project { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PledgeAmount { get; set; }

        public int? QuantityAvailable { get; set; } // Null if unlimited

        public int QuantityClaimed { get; set; } = 0;

        public DateTime? EstimatedDelivery { get; set; }

        public bool IsLimited { get; set; } = false;

        public ICollection<Pledge> Pledges { get; set; }
    }
}