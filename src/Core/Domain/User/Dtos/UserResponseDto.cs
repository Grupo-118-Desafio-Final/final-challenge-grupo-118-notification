namespace Domain.User.Dtos;

public class UserResponseDto
{ 
    public string Id { get; set; }
    public string Email { get; set; }
    public string? Name { get; set; }
    public string LastName { get; set; }

    public UserResponseDto()
    {
    }
}