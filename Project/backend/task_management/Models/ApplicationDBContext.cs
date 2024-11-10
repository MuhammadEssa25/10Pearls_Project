using Microsoft.EntityFrameworkCore;
using task_management.Models;

namespace task_management.Models
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }
        public  DbSet<User> User { get; set; }
        public  DbSet<Task> Task { get; set; }
    }
}