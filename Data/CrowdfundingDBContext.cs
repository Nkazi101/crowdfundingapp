using Crowdfunding.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Crowdfunding.Data
{
    public class CrowdFundingDBContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        // DbSet properties represent tables in the database for each model class
        //public DbSet<User> Users { get; set; } already includes this DBSet
        public DbSet<Project> Projects { get; set; }
        public DbSet<Reward> Rewards { get; set; }
        public DbSet<Pledge> Pledges { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Message> Messages { get; set; }

        public CrowdFundingDBContext(DbContextOptions<CrowdFundingDBContext> options)
            : base(options)
        {
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base class implementation
            base.OnModelCreating(modelBuilder);


            // Change Identity table names
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable(name: "Users");

                // Configure properties specific to User
                entity.Property(e => e.Bio)
                    .HasMaxLength(500)
                    .IsRequired(false); // Makes the Bio property optional

                entity.Property(e => e.ProfilePictureUrl)
                    .HasMaxLength(500)
                    .IsRequired(false);

                entity.Property(u => u.Role)
                    .HasConversion<string>()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<IdentityRole<Guid>>(entity =>
            {
                entity.ToTable(name: "Roles");
            });

            modelBuilder.Entity<IdentityUserRole<Guid>>(entity =>
            {
                entity.ToTable("UserRoles");
            });

            modelBuilder.Entity<IdentityUserClaim<Guid>>(entity =>
            {
                entity.ToTable("UserClaims");
            });

            modelBuilder.Entity<IdentityUserLogin<Guid>>(entity =>
            {
                entity.ToTable("UserLogins");
            });

            modelBuilder.Entity<IdentityRoleClaim<Guid>>(entity =>
            {
                entity.ToTable("RoleClaims");
            });

            modelBuilder.Entity<IdentityUserToken<Guid>>(entity =>
            {
                entity.ToTable("UserTokens");
            });



            // ---------------------
            // Unique Constraints
            // ---------------------

            // Configure a unique index on the Email property of the User entity.
            // This ensures that no two users can have the same email address.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)       // Specify the property to index (User.Email)
                .IsUnique();                  // Enforce uniqueness on the Email property

            // Configure a unique index on the Username property of the User entity.
            // This ensures that each username is unique across all users.
            modelBuilder.Entity<User>()
                .HasIndex(u => u.UserName)    // Specify the property to index (User.Username)
                .IsUnique();                  // Enforce uniqueness on the Username property

            // ---------------------
            // One-to-Many Relationships
            // ---------------------

            // Define the one-to-many relationship between Project and Reward.
            // A Project can have many Rewards, but a Reward belongs to one Project.
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Rewards)        // Navigation property in Project (Project.Rewards)
                .WithOne(r => r.Project)        // Navigation property in Reward (Reward.Project)
                .HasForeignKey(r => r.ProjectID);// Foreign key in Reward pointing to Project

            // Define the one-to-many relationship between Project and Pledge.
            // A Project can have many Pledges, but a Pledge belongs to one Project.
            modelBuilder.Entity<Project>()
                .HasMany(p => p.Pledges)        // Navigation property in Project (Project.Pledges)
                .WithOne(pl => pl.Project)      // Navigation property in Pledge (Pledge.Project)
                .HasForeignKey(pl => pl.ProjectID); // Foreign key in Pledge pointing to Project

            // Define the one-to-many relationship between User and Project.
            // A User (Creator) can have many Projects, but a Project has one Creator.
            modelBuilder.Entity<User>()
                .HasMany(u => u.Projects)       // Navigation property in User (User.Projects)
                .WithOne(p => p.Creator)        // Navigation property in Project (Project.Creator)
                .HasForeignKey(p => p.CreatorID); // Foreign key in Project pointing to User

            // Define the one-to-many relationship between User and Pledge.
            // A User (Backer) can have many Pledges, but a Pledge has one Backer.
            modelBuilder.Entity<User>()
                .HasMany(u => u.Pledges)        // Navigation property in User (User.Pledges)
                .WithOne(pl => pl.Backer)       // Navigation property in Pledge (Pledge.Backer)
                .HasForeignKey(pl => pl.BackerID) // Foreign key in Pledge pointing to User
                .OnDelete(DeleteBehavior.Restrict);

            // Define the one-to-many relationship between Reward and Pledge.
            // A Reward can be associated with many Pledges, but a Pledge has one Reward.
            modelBuilder.Entity<Reward>()
                .HasMany(r => r.Pledges)        // Navigation property in Reward (Reward.Pledges)
                .WithOne(pl => pl.Reward)       // Navigation property in Pledge (Pledge.Reward)
                .HasForeignKey(pl => pl.RewardID) // Foreign key in Pledge pointing to Reward
                .OnDelete(DeleteBehavior.Restrict);

            // ---------------------
            // One-to-One Relationship
            // ---------------------

            // Define the one-to-one relationship between Pledge and Transaction.
            // Each Pledge has one Transaction, and each Transaction is associated with one Pledge.
            modelBuilder.Entity<Pledge>()
                .HasOne(pl => pl.Transaction)       // Navigation property in Pledge (Pledge.Transaction)
                .WithOne(t => t.Pledge)             // Navigation property in Transaction (Transaction.Pledge)
                .HasForeignKey<Transaction>(t => t.PledgeID); // Foreign key in Transaction pointing to Pledge

            // ---------------------
            // Self-Referencing Relationships for Messages
            // ---------------------

            // Define the self-referencing relationship for Messages (Sender).
            // A Message has one Sender (User), and a User can send many Messages.
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)              // Navigation property in Message (Message.Sender)
                .WithMany(u => u.SentMessages)      // Navigation property in User (User.SentMessages)
                .HasForeignKey(m => m.SenderID)     // Foreign key in Message pointing to User (SenderID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid deleting Users when Messages are deleted

            // Define the self-referencing relationship for Messages (Receiver).
            // A Message has one Receiver (User), and a User can receive many Messages.
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)            // Navigation property in Message (Message.Receiver)
                .WithMany(u => u.ReceivedMessages)  // Navigation property in User (User.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverID)   // Foreign key in Message pointing to User (ReceiverID)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid deleting Users when Messages are deleted

            // ---------------------
            // Check Constraints
            // ---------------------

            // Add a check constraint on the Pledge entity to ensure PledgeAmount is greater than 0.
            // This enforces data integrity at the database level.
            modelBuilder.Entity<Pledge>(entity =>
            {
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_Pledge_PledgeAmount", "[PledgeAmount] > 0");
                });
            });
            // SQL check constraint

            // Note: Default values and additional configurations can be set here if needed.
        }
    }
}