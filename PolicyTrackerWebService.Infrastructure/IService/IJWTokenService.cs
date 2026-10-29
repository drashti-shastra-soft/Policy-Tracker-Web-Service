namespace PolicyTrackerWebService.Infrastructure.IService
{
    public interface IJWTokenService
    {
        string GenerateJwtToken(Domain.User mUser);
    }
}
