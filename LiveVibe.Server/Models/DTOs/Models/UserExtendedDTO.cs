using LiveVibe.Server.Models.DTOs.Models;

namespace LiveVibe.Server.Models.DTOs.ModelDTOs;

public class UserExtendedDTO : UserDTO
{
    public List<OrderDTO> Orders { get; set; }        
}