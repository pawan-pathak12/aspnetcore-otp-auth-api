using UserAuthWithOTP.API.Fixtures;

namespace UserAuthWithOTP.API.Controller
{
    [TestClass]
    public class UserControllerTest : IntegrationTestBase
    {
        //Authorizattion is used so sent token  to pass 

        #region Postive Path

        [TestMethod]
        public async Task CreateUser_When_Return()
        {
            //Arrange 

            //Act 

            //Assert
        }

        [TestMethod]
        public async Task GetUserById_When_Return()
        {
            //Arrange 

            //Act 

            //Assert
        }

        [TestMethod]
        public async Task GetAllUsers_When_Return()
        {
            //Arrange 

            //Act 

            //Assert
        }

        [TestMethod]
        public async Task UpdateUser_When_Return()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task DeleteUser_When_Return()
        {
            //Arrange 

            //Act 

            //Assert
        }

        #endregion

        #region Negative Path

        [TestMethod]
        public async Task CreateUser_WhenInvalid_Return400()
        {
            //Arrange 

            //Act 

            //Assert
        }

        [TestMethod]
        public async Task GetUserById_WhenUserNotFound_Return400()
        {
            //Arrange 

            //Act 

            //Assert
        }


        [TestMethod]
        public async Task UpdateUser_WhenInvalid_Return400()
        {
            //Arrange 

            //Act 

            //Assert

        }

        [TestMethod]
        public async Task DeleteUser_WhenUserNotFound_Return400()
        {
            //Arrange 

            //Act 

            //Assert
        }

        #endregion

    }
}
