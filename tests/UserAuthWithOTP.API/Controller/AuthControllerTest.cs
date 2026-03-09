using UserAuthWithOTP.API.Fixtures;

namespace UserAuthWithOTP.API.Controller
{
    [TestClass]
    public class AuthControllerTest : IntegrationTestBase
    {

        #region Postive Path 
        [TestMethod]
        public async Task RegisterAsync_WhenValid_Return200()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task VerifyAndRegisterAsync_WhenValid_Return200()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task LoginAsync_WhenValid_Return200WithAccessTokenAndSetRefreshCookie()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task Refresh_WhenTokenExists_Return200WithNewAccesTokenAndSetRefreshCookie()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task Logout_WhenLoginIn_Return200AndClearsRefreshCookie()
        {
            //Arrange 

            //Act 

            //Assert

        }

        #endregion

        #region Negative Path
        public async Task RegisterAsync_WhenInValid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task VerifyAndRegisterAsync_WhenInValid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task LoginAsync_WhenInvalid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task Refresh_WhenInvalid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task Logout_WhenInvalid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        #endregion
    }
}
