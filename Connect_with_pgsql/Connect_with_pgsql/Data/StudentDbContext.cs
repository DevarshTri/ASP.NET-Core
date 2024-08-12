
using Connect_with_pgsql.Models;
using Microsoft.EntityFrameworkCore;

namespace Connect_with_pgsql.Data
{
    public class StudentDbContext : DbContext
    {
        public StudentDbContext(DbContextOptions<StudentDbContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
    }
}
