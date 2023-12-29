using MFC.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace MFC.DataAccessLayer.Repository;

public class MfcDbRepository : IMfcDbRepository
{
    private readonly MfcDbContext _context;
    public MfcDbRepository(MfcDbContext context)
    {
        _context = context;
    }

    public async Task<List<OfferFee>> GetOfferFeesByOfferIdAsync(string offerId)
    {
        return await _context.OfferFees.Where(offerFee => offerFee.OfferId == offerId).ToListAsync();
    }
    
    public async Task<bool> SaveChangesAsync()
    {
        return (await _context.SaveChangesAsync() >= 0);
    }
    
    public async Task AddOfferFee(OfferFee newOfferFee)
    {
        _context.OfferFees.Add(newOfferFee);
    }

    #region AllegroAccessToken

    public async Task<AllegroAccessToken> GetAllegroAccessTokenByClientIdAsync(string clientId)
    {
        return await _context.AllegroAccessTokens.FirstOrDefaultAsync(accessToken => accessToken.ClientId == clientId);
    }
    
    public Task AddAllegroAccessToken(AllegroAccessToken newAllegroAccessToken)
    {
        if(_context.AllegroAccessTokens.Any(accessToken => accessToken.AccessToken == newAllegroAccessToken.AccessToken))
        {
             _context.AllegroAccessTokens.Update(newAllegroAccessToken);
        }
        else
        {
            _context.AllegroAccessTokens.Add(newAllegroAccessToken);
        }

        return Task.CompletedTask;
    }
    
    public async Task UpdateAllegroAccessTokenAsync(AllegroAccessToken newAllegroAccessToken)
    {
        _context.AllegroAccessTokens.Update(newAllegroAccessToken);
    }

    #endregion
    
}