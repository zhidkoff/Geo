using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Security.Claims;

namespace Geo.Core
{
    public interface IBrigadeFilter
    {
        int GetBrigadeId();
    }
    public class BrigadeFilter : IBrigadeFilter
    {
        private IHttpContextAccessor _accessor;
        private GeoDbContext _context;
        public BrigadeFilter(IConfiguration configuration, IHttpContextAccessor accessor)
        {
            var optionsBuilder = new DbContextOptionsBuilder<GeoDbContext>();
            optionsBuilder.UseSqlServer(configuration.GetConnectionString("GeoConnection"));
            _context = new GeoDbContext(optionsBuilder.Options, accessor, configuration);
            _accessor = accessor;
        }

        public int GetBrigadeId()
        {
            var identity = _accessor.HttpContext.User.Identity;
            if (identity.IsAuthenticated)
            {
                var userId = int.Parse(_accessor.HttpContext.User
                    .FindFirst(ClaimTypes.NameIdentifier).Value);
                var brigadeId = _context.Users.FirstOrDefault(d => d.Id == userId).BrigadeId;
                return brigadeId ?? 0;
            }
            else
            {
                return 0;
            }
        }
    }
}
