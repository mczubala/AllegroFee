using MFC.DataAccessLayer.Entities;
using Microsoft.EntityFrameworkCore;

namespace MFC.DataAccessLayer;

public class MfcDbContext: DbContext
{
    public MfcDbContext(DbContextOptions<MfcDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<OfferFee> OfferFees { get; set; }
    public DbSet<AllegroAccessToken> AllegroAccessTokens { get; set; }
}