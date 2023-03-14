public interface IAccessTokenProvider
{
    Task<string> GetAccessTokenAsync();
}