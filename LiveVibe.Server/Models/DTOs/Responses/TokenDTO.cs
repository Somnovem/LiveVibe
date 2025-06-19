using System.IdentityModel.Tokens.Jwt;

namespace LiveVibe.Server.Models.DTOs.Responses
{
    public class TokenDTO
    {
        public string Token { get; set; } = string.Empty;
        public string TokenType { get; set; } = "Bearer";
        public bool IsAdmin { get; set; }

        public TokenDTO(JwtSecurityToken token, string type, bool isAdmin)
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token);
            TokenType = type;
            IsAdmin = isAdmin;
        }
    }
}
