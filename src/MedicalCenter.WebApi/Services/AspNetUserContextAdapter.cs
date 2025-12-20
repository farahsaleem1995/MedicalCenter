using System.Security.Claims;
using MedicalCenter.Core.Services;
using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.WebApi.Services
{
    public class AspNetUserContextAdapter(IHttpContextAccessor httpContextAccessor) : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        public bool IsAuthenticated => GetUserId() != null;

        public Guid UserId
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException("Cannot access UserId: User is not authenticated.");
                }

                string? userId = GetUserId();
                if (string.IsNullOrWhiteSpace(userId))
                {
                    throw new InvalidOperationException("Cannot access UserId: User identifier claim (NameIdentifier) is missing from the authentication token.");
                }

                if (!Guid.TryParse(userId, out Guid parsedUserId))
                {
                    throw new InvalidOperationException($"Cannot access UserId: User identifier claim value '{userId}' is not a valid GUID.");
                }

                return parsedUserId;
            }
        }


        public string Email
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException("Cannot access Email: User is not authenticated.");
                }

                string? email = _httpContextAccessor.HttpContext!.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;

                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new InvalidOperationException("Cannot access Email: Email claim is missing from the authentication token.");
                }

                return email;
            }
        }

        public UserRole Role
        {
            get
            {
                if (!IsAuthenticated)
                {
                    throw new InvalidOperationException("Cannot access Role: User is not authenticated.");
                }

                string? roleStr = _httpContextAccessor.HttpContext!.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                if (string.IsNullOrWhiteSpace(roleStr))
                {
                    throw new InvalidOperationException("Cannot access Role: Role claim is missing from the authentication token.");
                }

                if (!Enum.TryParse<UserRole>(roleStr, ignoreCase: true, out UserRole role))
                {
                    throw new InvalidOperationException($"Cannot access Role: Role claim value '{roleStr}' is not a valid UserRole. Valid values are: {string.Join(", ", Enum.GetNames<UserRole>())}.");
                }

                return role;
            }
        }



        private string? GetUserId()
        {
            return _httpContextAccessor.HttpContext!.User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }
    }
}
