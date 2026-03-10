using Microsoft.Extensions.DependencyInjection;
using System.Transactions;

namespace UserAuthWithOTP.API.Fixtures
{
    [TestClass]
    public abstract class IntegrationTestBase
    {
        protected static CustomWebApplicationFactory Factory = null!;
        protected HttpClient _client = null!;
        protected TransactionScope _scope = null!;
        protected TestDataBuilder testDataBuilder = null!;
        protected IServiceProvider Services = null!;

        [ClassInitialize(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void ClassInit(TestContext context)
        {
            Factory = new CustomWebApplicationFactory();
        }

        [ClassCleanup(InheritanceBehavior.BeforeEachDerivedClass)]
        public static void ClassCleanUp()
        {
            Factory.Dispose();
        }

        [TestInitialize]
        public void TestInit()
        {
            _scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            _client = Factory.CreateClient();

            var scope = Factory.Services.CreateScope();
            Services = scope.ServiceProvider;
            testDataBuilder = new TestDataBuilder(Services);

        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _scope.Dispose();
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
