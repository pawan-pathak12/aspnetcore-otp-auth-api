using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UserAuth.Api.Data;

namespace UserAuthWithOTP.DataLayer.Fixtures
{
    public class DatabaseFixture : IDisposable
    {
        public IConfiguration Configuration { get; }
        public AppDbContext? DbContext { get; set; }

        public DatabaseFixture()
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Test.json", optional: false)
                .Build();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                 .UseSqlServer(Configuration.GetConnectionString("DefaultConnection")) // or your test connection string
                 .Options;
            DbContext = new AppDbContext(options);

        }
        public void Dispose()
        {
        }
    }
}
