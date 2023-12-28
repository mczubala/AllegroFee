using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MFC.DataAccessLayer.Entities;

public class OfferFee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; } 
    [Required]
    [MaxLength(40)]
    public string OfferId { get; set; }
    public decimal FeePercent { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public OfferFee(string offerId, decimal feePercent)
    {
        OfferId = offerId;
        FeePercent = feePercent;
    }
}