using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;

namespace MFC.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController : ControllerBase
{
    [HttpGet("login")]
    public ChallengeResult LoginWithGoogle()
    {
        string redirectUri = "http://localhost:5029/signin-google";
        //var properties = new AuthenticationProperties { RedirectUri = redirectUri };
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") }; 
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        // await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme, new AuthenticationProperties()
        // {
        //     RedirectUri = Url.Action("GoogleResponse")
        // });
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var claims = result.Principal.Identities.FirstOrDefault().Claims.Select(claim => new
        {
            claim.Issuer,
            claim.OriginalIssuer,
            claim.Type,
            claim.Value
        });
        return Ok(claims);
    }
}
