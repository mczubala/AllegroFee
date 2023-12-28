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
}