using MedicalCenter.Core.SharedKernel;

namespace MedicalCenter.Core.Services
{
    public interface IUserContext
    {
        public bool IsAuthenticated { get; }

        public Guid UserId { get; }

        public string UserName { get; }

        public string Email { get; }

        public UserRole Role { get; }
    }
}
