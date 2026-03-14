namespace UserAuth.Api.Helpers
{
    public static class CookieOptionsHelper
    {
        public static CookieOptions RefreshTokenCookie(DateTime expiryDate)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = false,              // ✅ false for test environment
                SameSite = SameSiteMode.Lax, // ✅ Lax, not Strict
                Expires = expiryDate,
                Path = "/"
            };
        }
    }
}