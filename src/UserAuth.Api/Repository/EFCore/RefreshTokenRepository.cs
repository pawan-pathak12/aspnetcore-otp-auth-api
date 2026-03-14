using Microsoft.EntityFrameworkCore;
using UserAuth.Api.Data;
using UserAuth.Api.Entities;
using UserAuth.Api.Interfaces.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> AddAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token.Id;
    }

    public async Task<RefreshToken?> GetByIdAsync(int id)
    {
        return await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == token && !rt.IsRevoked);
    }

    public async Task<IEnumerable<RefreshToken>> GetAllByUserIdAsync(int userId)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId)
            .ToListAsync();
    }

    public async Task<bool> RevokeAsync(int id)
    {
        var refreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
        if (refreshToken == null) return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var refreshToken = await _context.RefreshTokens.FindAsync(id);
        if (refreshToken == null) return false;

        _context.RefreshTokens.Remove(refreshToken);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(int id, RefreshToken refreshToken)
    {
        var refreshTokenData = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Id == id);
        if (refreshTokenData == null)
        {
            return false;
        }

        // Update relevant fields
        refreshTokenData.IsRevoked = refreshToken.IsRevoked;
        refreshTokenData.RevokedAt = refreshToken.RevokedAt;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<RefreshToken?> GetDataByTokenAsync(string token)
    {
        var existingData = await _context.RefreshTokens
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == token && !x.IsRevoked);

        return existingData;
    }
}