public interface IAccessTokenProvider
{
    Task<string> GetAccessForApplicationTokenAsync();
    Task<string> GetAccessForUserTokenAsync();
}