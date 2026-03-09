using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace UserAuthWithOTP.API.Fixtures
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {

            builder.ConfigureAppConfiguration(services =>
            {

            });

            builder.ConfigureAppConfiguration(config =>
            {
                var settings = new Dictionary<string, string>
                {
                    {"Jwt:Issuer", "JwtAuthLearning" },
                    {"Jwt:Audience", "JwtAuthLearningUsers" },
                    {"Jwt:key", "THIS_IS_A_SUPER_SECRET_KEY_CHANGE_LATER_BYOWNER" }
                };
                config.AddInMemoryCollection(settings!);
            });

            builder.UseEnvironment("Test");
        }
    }
}
