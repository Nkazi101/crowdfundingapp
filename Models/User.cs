using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Crowdfunding.Models;

namespace Crowdfunding.Models
{
    public class User
    {
        [Key]
        public Guid UserID { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string Role { get; set; } // Possible values: "Backer", "ProjectCreator", "Admin"

        [Url]
        public string ProfilePictureUrl { get; set; }

        [MaxLength(500)]
        public string Bio { get; set; }

        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<Project> Projects { get; set; } // Projects created by the user

        public ICollection<Pledge> Pledges { get; set; } // Pledges made by the user

        public ICollection<Message> SentMessages { get; set; } // Messages sent by the user

        public ICollection<Message> ReceivedMessages { get; set; } // Messages received by the user
    }
}