using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Clasess
{
    public class SubDbContextFactory : IDesignTimeDbContextFactory<SubDbContext>
    {
        public SubDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SubDbContext>();

            optionsBuilder.UseSqlite(
                "Data Source=app.db"
            );

            return new SubDbContext(optionsBuilder.Options);
        }
    }
}
