using Geo.Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Geo.Core
{
    public class GeoDbContext
    : IdentityDbContext<
        Employee, Permission, int,
        IdentityUserClaim<int>, EmployeePermission, IdentityUserLogin<int>,
        IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Brigade> Brigades { get; set; }

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        public GeoDbContext(DbContextOptions<GeoDbContext> options,
            IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>().ToTable("_Employees");
            modelBuilder.Entity<Permission>().ToTable("_Permissions");
            modelBuilder.Entity<EmployeePermission>().ToTable("_EmployeePermissions");
            modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("_EmployeeClaims");
            modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("_EmployeeLogins");
            modelBuilder.Entity<IdentityUserToken<int>>().ToTable("_EmployeeTokens");
            modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("_PermissionClaims");

            modelBuilder.Entity<Employee>(b =>
            {
                b.HasMany(e => e.Claims)
                    .WithOne()
                    .HasForeignKey(uc => uc.UserId)
                    .IsRequired();

                b.HasMany(e => e.Logins)
                    .WithOne()
                    .HasForeignKey(ul => ul.UserId)
                    .IsRequired();

                b.HasMany(e => e.Tokens)
                    .WithOne()
                    .HasForeignKey(ut => ut.UserId)
                    .IsRequired();

                b.HasMany(e => e.EmployeePermissions)
                    .WithOne(d => d.Employee)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Permission>(b =>
            {
                b.HasMany(e => e.EmployeePermissions)
                    .WithOne(e => e.Permission)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();
            });

            /*Фильтруем, каждая бригада видит только те заявки которые им назначены */
            var _brigadeFilter = new BrigadeFilter(_configuration, _httpContextAccessor);
            modelBuilder.Entity<Order>().HasQueryFilter(o =>
                o.BrigadeId == _brigadeFilter.GetBrigadeId());

            modelBuilder.Entity<Permission>().HasData(
            new Permission[]
            {
                new Permission{ Id=1, Name="Admin", NormalizedName="Администратор"},
                 new Permission{ Id=2, Name="Secretary", NormalizedName="Секретарь"},
                  new Permission{ Id=3, Name="Master", NormalizedName="Бригадир"},
                   new Permission{ Id=4, Name="Manager", NormalizedName="Директор"}
            });
        }
    }
}
