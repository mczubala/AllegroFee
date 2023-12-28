using MFC.DataAccessLayer.Entities;

namespace MFC.DataAccessLayer.Repository;

public interface IMfcDbRepository
{
    Task<List<OfferFee>> GetOfferFeesByOfferIdAsync(string offerId);
    Task<bool> SaveChangesAsync();
    Task AddOfferFee(OfferFee newOfferFee);
}