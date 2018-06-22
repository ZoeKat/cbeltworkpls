using Microsoft.EntityFrameworkCore;
 
namespace cbeltworkpls.Models
{
    public class cbeltworkplscontext : DbContext
    {
        // base() calls the parent class' constructor passing the "options" parameter along
        public cbeltworkplscontext(DbContextOptions<cbeltworkplscontext> options) : base(options) { }
        public DbSet<users> users {get;set;}
        public DbSet<activities> activities {get;set;}
        public DbSet<participants> participants {get;set;}
    }
}