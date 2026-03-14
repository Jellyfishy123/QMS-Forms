using Microsoft.EntityFrameworkCore;
using QMSForms.Models;

namespace QMSForms.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<IMSAssessment> IMSAssessments { get; set; }
        public DbSet<QMRow> QMRows { get; set; }  // <--- Add this
        public DbSet<User> Users { get; set; }
        public DbSet<Request> Requests { get; set; }
    }
}