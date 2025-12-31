using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Transit.Domain.Models.MOT;

namespace Transit.Domain.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var options = new JsonSerializerOptions(); // Use default or customize if needed

            // Composite key: UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // MOT System Entity Configurations
            ConfigureMOTEntities(modelBuilder);

            base.OnModelCreating(modelBuilder);
        }

        private void ConfigureMOTEntities(ModelBuilder modelBuilder)
        {
            var service = modelBuilder.Entity<Service>();

            // You MUST set both to NoAction to satisfy SQL Server
            service.HasOne(s => s.AssignedCaseExecutor)
                .WithMany()
                .HasForeignKey(s => s.AssignedCaseExecutorId)
                .OnDelete(DeleteBehavior.NoAction);

            service.HasOne(s => s.AssignedAssessor)
                .WithMany()
                .HasForeignKey(s => s.AssignedAssessorId)
                .OnDelete(DeleteBehavior.NoAction);

            // Ensure Customer and CreatedBy also don't conflict
            service.HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.NoAction);

            // ServiceStageExecution configurations
            modelBuilder.Entity<ServiceStageExecution>()
                .HasOne(ss => ss.Service)
                .WithMany(s => s.Stages)
                .HasForeignKey(ss => ss.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceStageExecution>()
                .HasOne(ss => ss.UpdatedByUser)
                .WithMany()
                .HasForeignKey(ss => ss.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // 1. Fix the Service -> ServiceDocument relationship
            modelBuilder.Entity<ServiceDocument>()
                .HasOne(sd => sd.Service)
                .WithMany(s => s.Documents)
                .HasForeignKey(sd => sd.ServiceId)
                .OnDelete(DeleteBehavior.NoAction); // Change from Cascade to NoAction

            // 2. While we are here, fix the Stage -> Document relationship to be safe
            modelBuilder.Entity<ServiceDocument>()
                .HasOne(sd => sd.ServiceStage)
                .WithMany()
                .HasForeignKey(sd => sd.ServiceStageId)
                .OnDelete(DeleteBehavior.NoAction); // Change from SetNull to NoAction

            // 3. Ensure User relationships in this table are also NoAction
            modelBuilder.Entity<ServiceDocument>()
                .HasOne(sd => sd.UploadedByUser)
                .WithMany()
                .HasForeignKey(sd => sd.UploadedByUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ServiceDocument>()
                .HasOne(sd => sd.VerifiedByUser)
                .WithMany()
                .HasForeignKey(sd => sd.VerifiedByUserId)
                .OnDelete(DeleteBehavior.NoAction);
            // Customer configurations
            modelBuilder.Entity<Customer>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.CreatedByDataEncoder)
                .WithMany()
                .HasForeignKey(c => c.CreatedByDataEncoderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification configurations
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Service)
                .WithMany()
                .HasForeignKey(n => n.ServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // StageComment configurations
            modelBuilder.Entity<StageComment>()
                .HasOne(sc => sc.ServiceStage)
                .WithMany(ss => ss.StageComments)
                .HasForeignKey(sc => sc.ServiceStageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StageComment>()
                .HasOne(sc => sc.CommentedByUser)
                .WithMany()
                .HasForeignKey(sc => sc.CommentedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // StageDocument configurations
            modelBuilder.Entity<StageDocument>()
                .HasOne(sd => sd.ServiceStage)
                .WithMany(ss => ss.Documents)
                .HasForeignKey(sd => sd.ServiceStageId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StageDocument>()
                .HasOne(sd => sd.UploadedByUser)
                .WithMany()
                .HasForeignKey(sd => sd.UploadedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StageDocument>()
                .HasOne(sd => sd.VerifiedByUser)
                .WithMany()
                .HasForeignKey(sd => sd.VerifiedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // ServiceMessage configurations
            modelBuilder.Entity<ServiceMessage>()
                .HasOne(sm => sm.Service)
                .WithMany(s => s.Messages)
                .HasForeignKey(sm => sm.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ServiceMessage>()
                .HasOne(sm => sm.SenderUser)
                .WithMany()
                .HasForeignKey(sm => sm.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ServiceMessage>()
                .HasOne(sm => sm.RecipientUser)
                .WithMany()
                .HasForeignKey(sm => sm.RecipientUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }

        #region User
        public DbSet<Role> Role { get; set; }
        public DbSet<Privilege> Privilege { get; set; }
        public DbSet<RolePrivilege> RolePrivilege { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRole { get; set; }
        #endregion

        #region MOT System
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceStageExecution> ServiceStages { get; set; }
        public DbSet<ServiceDocument> ServiceDocuments { get; set; }
        public DbSet<ServiceMessage> ServiceMessages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<StageDocument> StageDocuments { get; set; }
        public DbSet<StageComment> StageComments { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<StageTransport> StageTransports { get; set; }
        public DbSet<CustomerDocument> CustomerDocuments { get; set; }
        #endregion

    }
}
