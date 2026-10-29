using PolicyTrackerWebService.Infrastructure.CustomException;
using PolicyTrackerWebService.Infrastructure.IRepository;
using PolicyTrackerWebService.Infrastructure.IService;
using PolicyTrackerWebService.Infrastructure.Utils;

namespace PolicyTrackerWebService.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string Failed = "Authentication failed: user identity could not be verified!";

        private readonly IUserRepository _userRepo;
        private readonly IJWTokenService _jwtService;

        public AuthenticationService(IUserRepository userRepo, IJWTokenService jwtService)
        {
            _userRepo = userRepo;
            _jwtService = jwtService;
        }

        public Domain.User Authenticate(Domain.Credential mCredential)
        {
            if (mCredential == null)
                throw new APIRequestFailedException("Object must have a value!");

            if (mCredential.EmailAddress.IsNullOrEmpty())
                throw new APIRequestFailedException("Email Address is required!");

            if (mCredential.Password.IsNullOrEmpty())
                throw new APIRequestFailedException("Password is required!");

            var mUser = _userRepo.GetBy(mCredential.EmailAddress);
            if (mUser == null || !PasswordHasher.Verify(mCredential.Password, mUser.Password))
                throw new APIRequestFailedException(Failed);

            if (!mUser.IsActive)
                throw new APIRequestFailedException("Account Suspended: Access has been disabled. Kindly reach out to your administrator.");

            mUser.Password = null;
            mUser.Token = _jwtService.GenerateJwtToken(mUser);
            return mUser;
        }
    }
}
