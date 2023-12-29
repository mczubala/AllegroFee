using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MFC.DataAccessLayer.Entities;

public class AllegroAccessToken
{   
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    [Required]
    public string ClientId { get; set; }
    [Required]
    [MaxLength(2000)]
    public string AccessToken { get; set; }
    [MaxLength(2000)]
    public string RefreshToken { get; set; }
    public DateTime ExpiresIn { get; set; }
    public DateTime UpdatedDate { get; set; } = DateTime.UtcNow;
    
    public AllegroAccessToken(string clientId, string accessToken)
    {
        ClientId = clientId;
        AccessToken = accessToken;
    }
}