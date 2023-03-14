public interface IAccessTokenProvider
{
    Task<string> GetAccessForApplicationTokenAsync();
    // Task<string> GetAccessTokenForUserAsync();
    // Task<string> GetAccessTokenUsingDeviceFlowAsync();
    // Task<string> GetAccessTokenAsync2();
    Task<string> GetAccessForUserTokenAsync();
}