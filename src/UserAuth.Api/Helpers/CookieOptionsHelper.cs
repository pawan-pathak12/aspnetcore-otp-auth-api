namespace UserAuth.Api.Helpers
{
    public static class CookieOptionsHelper
    {
        public static CookieOptions RefreshTokenCookie(DateTime expiresAt)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = true, //goes through https only
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt
            };
        }
    }
}