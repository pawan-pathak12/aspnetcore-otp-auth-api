namespace UserAuthWithOTP.DataLayer.Fixtures
{
    public abstract class IntegrationTestBase
    {
        protected readonly DatabaseFixture _fixture;

        protected IntegrationTestBase(DatabaseFixture fixture)
        {
            _fixture = fixture;
        }
    }
}
