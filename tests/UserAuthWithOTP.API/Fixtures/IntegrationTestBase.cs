using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Respawn;
using System.Net.Http.Headers;
using UserAuth.Api.Data;

namespace UserAuthWithOTP.API.Fixtures
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        protected static CustomWebApplicationFactory Factory = null!;
        protected HttpClient _client = null!;
        protected TestDataBuilder testDataBuilder = null!;

        protected IServiceProvider Services = null!;
        private static Respawner _respawner = null!;
        private static string _connectionString = null!;

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static async Task ClassInit(TestContext context)
        {
            Factory = new CustomWebApplicationFactory();

            // Get connection string from the factory's DbContext
            using var scope = Factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            _connectionString = dbContext.Database.GetConnectionString()!;

            //Ensure databse is created 

            await dbContext.Database.EnsureCreatedAsync();

            // initialize Respawner 
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
            {
                TablesToIgnore = new Respawn.Graph.Table[]
                {
                    "__EFMigrationsHistory"
                },
                DbAdapter = DbAdapter.SqlServer
            });
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void ClassCleanUp()
        {
            Factory.Dispose();
        }

        [TestInitialize]
        public async Task TestInit()
        {
            // Reset database to clean state BEFORE each test
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await _respawner.ResetAsync(connection);

            _client = Factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                HandleCookies = true,
                AllowAutoRedirect = false
            });


            var scope = Factory.Services.CreateScope();
            Services = scope.ServiceProvider;
            testDataBuilder = new TestDataBuilder(Services);

            var user = await testDataBuilder.CreateAndReturnUser();

            var token = JwtTestTokenGenerator.GenerateToken(user);

            _client.DefaultRequestHeaders.Authorization =
                 new AuthenticationHeaderValue("Bearer", token);

        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _client.Dispose();
        }

        public void SetRefreshTokenCookie(HttpResponseMessage loginResponse)
        {
            var cookies = loginResponse.Headers.GetValues("Set-Cookie")
                .Where(c => c.Contains("refreshToken="))
                .Select(c => c.Split(';')[0])
                .ToList();

            foreach (var cookie in cookies)
            {
                // rmeove existing cookie 
                if (_client.DefaultRequestHeaders.Contains("Cookie"))
                {
                    _client.DefaultRequestHeaders.Remove("Cookie");
                }
                _client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

        }
    }
}
