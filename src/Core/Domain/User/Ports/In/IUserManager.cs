using System.Threading;
using System.Threading.Tasks;
using Domain.User.Dtos;

namespace Domain.User.Ports.In;

public interface IUserManager
{
    Task<UserResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
}