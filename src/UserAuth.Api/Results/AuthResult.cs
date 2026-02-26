namespace UserAuth.Api.Results
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }

        //token
        public string? AccessTokenhash { get; set; }
        public string? RefreshToken { get; set; }

        //expiry 
        public DateTime ExpiryDate { get; set; }

        public static AuthResult Success(string accessTokenHash, string refreshTokenHash, DateTime expiryDate)
        {
            return new AuthResult
            {
                IsSuccess = true,
                AccessTokenhash = accessTokenHash,
                RefreshToken = refreshTokenHash,
                ExpiryDate = expiryDate
            };
        }

        public static AuthResult Success(string message)
        {
            return new AuthResult
            {
                IsSuccess = true,
                Message = message
            };
        }
        public static AuthResult Failure(string error)
        {
            return new AuthResult
            {
                IsSuccess = false,
                Error = error
            };
        }
    }
}
