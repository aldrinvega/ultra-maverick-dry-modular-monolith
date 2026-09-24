using Ultramaverick.Identity.Domain.Events;
using Ultramaverick.Identity.Domain.ValueObjects;
using Ultramaverick.SharedKernel;

namespace Ultramaverick.Identity.Domain.Entities
{
    public sealed class User : Entity
    {
        public int Id { get; private set; }
        public int? LegacyId { get; private set; }
        public string FullName { get; private set; } = null!;
        public string UserName { get; private set; } = null!;
        public PasswordHash Password { get; private set; } = null!;
        public int RoleId { get; private set; }
        public int DepartmentId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; private set; }
        public DateTime? ModifiedAtUtc { get; private set; }
        public int? ModifiedByUserId { get; private set; }

        private User() { }

        public static User Create(
            string fullName, 
            string userName, 
            PasswordHash password, 
            int roleId, 
            int departmentId,
            int? createdByUserId)
        {
            fullName = (fullName ?? string.Empty).Trim();
            userName = (userName ?? string.Empty).Trim();

            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);
            ArgumentException.ThrowIfNullOrWhiteSpace(password?.Value);

            var user = new User
            {
                FullName = fullName,
                UserName = userName,
                Password = password,
                RoleId = roleId,
                DepartmentId = departmentId,
                CreatedByUserId = createdByUserId,
                IsActive = true
            };

            user.RaiseDomainEvent(new UserChanged(UserChangeTypes.Created, DateTime.UtcNow));

            return user;
        }

        public void ChangePassword(PasswordHash newPassword, int modifiedByUserId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(newPassword?.Value);
            Password = newPassword;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;

            RaiseDomainEvent(new UserChanged(UserChangeTypes.PasswordChanged, DateTime.UtcNow));
        }

        public void UpdateProfile(string fullName, string userName, int roleId, int departmentId, int modifiedByUserId)
        {
            fullName = (fullName ?? string.Empty).Trim();
            userName = (userName ?? string.Empty).Trim();

            ArgumentException.ThrowIfNullOrWhiteSpace(fullName);
            ArgumentException.ThrowIfNullOrWhiteSpace(userName);

            FullName = fullName;
            UserName = userName;
            RoleId = roleId;
            DepartmentId = departmentId;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;

            RaiseDomainEvent(new UserChanged(UserChangeTypes.Updated, DateTime.UtcNow));
        }

        public void Deactivate(int modifiedByUserId)
        {
            if(!IsActive) return;
            IsActive = false;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;

            RaiseDomainEvent(new UserChanged(UserChangeTypes.Deactivated, DateTime.UtcNow));
        }

        public void Activate(int modifiedByUserId)
        {
            if(IsActive) return;
            IsActive = true;
            ModifiedAtUtc = DateTime.UtcNow;
            ModifiedByUserId = modifiedByUserId;

            RaiseDomainEvent(new UserChanged(UserChangeTypes.Activated, DateTime.UtcNow));
        }
    }
}
