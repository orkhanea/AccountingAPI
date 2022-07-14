using Accounting.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Accounting.ViewModel;

namespace Accounting.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions option) : base(option)
        {

        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Position> Positions { get; set; }
        public DbSet<Tax> Taxes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Company>().HasMany(p => p.Transactions)
                .WithOne(x => x.SenderCompany)
                .HasForeignKey(g=>g.SenderCompanyId);

            builder.Entity<Company>().HasMany(p => p.ReceiverTransactions)
                .WithOne(x => x.RecieverCompany)
                .HasForeignKey(g=>g.RecieverCompanyId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
