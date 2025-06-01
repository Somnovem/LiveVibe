using LiveVibe.Server.Models.Tables;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs
{
    public class UserDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public UserDTO(User user) {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Phone = user.Phone;
            Email = user.Email!;
        }
    }
}
