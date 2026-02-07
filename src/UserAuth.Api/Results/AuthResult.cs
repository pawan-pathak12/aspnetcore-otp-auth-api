namespace UserAuth.Api.Results
{
    public class AuthResult
    {
        public bool IsSuccess { get; set; }
        public string? Error { get; set; }

        //token
        public string? AccessTokenhash { get; set; }
        public string? RefreshTokenHash { get; set; }

        //expiry 
        public DateTime ExpiryDate { get; set; }

        public static AuthResult Success(string accessTokenHash, string refreshTokenHash, DateTime expiryDate)
        {
            return new AuthResult
            {
                IsSuccess = true,
                AccessTokenhash = accessTokenHash,
                RefreshTokenHash = refreshTokenHash,
                ExpiryDate = expiryDate
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
