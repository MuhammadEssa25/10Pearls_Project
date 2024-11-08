using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using task_management.Models;

namespace task_management.Models
{
 public class ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : DbContext(options)
    {
        public DbSet<User> User { get; set; }
        public DbSet<Task> Task { get; set; }
    }
}
