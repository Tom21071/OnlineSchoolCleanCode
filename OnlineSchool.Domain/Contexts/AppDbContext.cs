using OnlineSchool.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace OnlineSchool.Domain.Contexts
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        private readonly DbContextOptions _options;
        public AppDbContext(DbContextOptions options) : base(options)
        {
            _options = options;
        }

        public DbSet<UserSubjectDateMark> UserSubjectDateMarks { get; set; }
        public DbSet<SubjectMessage> SubjectMessages { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<UserClass> UserClasses { get; set; }
        public DbSet<Subject> Subjects { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<UserNotification> UserNotifications { get; set; }
        public DbSet<SubjectDate> SubjectDates { get; set; }
        public DbSet<Logins> Logins { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<AppUser>().HasIndex(u => u.IDNP).IsUnique();
            modelBuilder.Entity<AppUser>(b =>
            {
                //Each User can have many entries in the UserRole join table
                b.HasMany(e => e.Roles)
                    .WithOne()
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });
            modelBuilder.Entity<UserClass>().HasKey(uc => new { uc.UserId, uc.ClassId });
            modelBuilder.Entity<UserNotification>().HasKey(uc => new { uc.RecieverId, uc.NotificationId });
            modelBuilder.Entity<Lesson>().HasKey(l => new { l.Start, l.End, l.SubjectId, l.ClassId, l.DayOfTheWeek });
            modelBuilder.Entity<UserSubjectDateMark>().HasKey(l => new { l.UserId, l.SubjectDateId});
        }
    }
}
