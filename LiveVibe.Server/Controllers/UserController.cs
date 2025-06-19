using LiveVibe.Server.Models.DTOs.Requests.Users;
using LiveVibe.Server.Models.DTOs.ModelDTOs;
using LiveVibe.Server.Models.Tables;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using LiveVibe.Server.HelperClasses;
using LiveVibe.Server.HelperClasses.Extensions;
using LiveVibe.Server.Models.DTOs.Shared;
using LiveVibe.Server.Models.DTOs.Responses;
using LiveVibe.Server.Models.DTOs.Models;

namespace LiveVibe.Server.Controllers
{
    [ApiController]
    [Route("/api/users")]
    public class UserController : Controller
    {
        private readonly IConfiguration _config;
        private readonly UserManager<User> _userManager;
        private readonly ApplicationContext _context;

        public UserController(UserManager<User> userManager, IConfiguration config, ApplicationContext context)
        {
            _userManager = userManager;
            _config = config;
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        [SwaggerOperation(Summary = "[Admin] Retrieve all users", Description = "Returns a list of all users in the database.")]
        [SwaggerResponse(200, "Success", typeof(PagedListDTO<User>))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        public async Task<ActionResult<PagedListDTO<User>>> GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 10;

            var users = await _userManager.Users
                .AsNoTracking()
                .ToPagedListAsync(pageNumber, pageSize);

            return Ok(users.ToDto());
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Get user by ID")]
        [SwaggerResponse(200, "User found", typeof(User))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "User not found.", typeof(ErrorDTO))]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new ErrorDTO("User not found."));

            return Ok(user);
        }

        [HttpPost("register")]
        [SwaggerOperation(Summary = "[Any] Register a new user", Description = "Create and authenticate a new user, and issue a JWT.")]
        [SwaggerResponse(201, "User registered successfully")]
        [SwaggerResponse(400, "Invalid input", typeof(ErrorDTO))]
        [SwaggerResponse(409, "User with the same email or phone already exists", typeof(ErrorDTO))]
        public async Task<IActionResult> RegisterUser([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _userManager.Users.AnyAsync(u => u.Email == request.Email))
                return Conflict(new ErrorDTO("A user with this email already exists."));

            if (await _userManager.Users.AnyAsync(u => u.Phone == request.Phone))
                return Conflict(new ErrorDTO("A user with this phone already exists."));

            var user = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Phone = request.Phone,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName!)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = JWTTokenGenerator.GenerateToken(_config, claims);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        token_type = "Bearer"
                    });
        }

        [HttpPost("login")]
        [SwaggerOperation(Summary = "[Any] Login", Description = "Authenticate user and issue a JWT.")]
        [SwaggerResponse(200, "Login successful, JWT returned", typeof(TokenDTO))]
        [SwaggerResponse(401, "Invalid email or password", typeof(ErrorDTO))]
        public async Task<ActionResult<TokenDTO>> Login([FromBody] LoginRequest model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return Unauthorized(new ErrorDTO("Invalid credentials"));

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email!),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.UserName!)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = JWTTokenGenerator.GenerateToken(_config, claims);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin") ||  await _userManager.IsInRoleAsync(user, "SuperAdmin");
            return Ok(new TokenDTO(token,"Bearer", isAdmin));
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(Summary = "[Any authorised] Get current user", Description = "Returns the currently authenticated user's information.")]
        [SwaggerResponse(200, "Current user info returned successfully.", typeof(UserDTO))]
        [SwaggerResponse(401, "Invalid or expired token, or user no longer exists.", typeof(ErrorDTO))]
        public async Task<ActionResult<UserDTO>> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user ID missing in token)."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user no longer exists)."));
            }

            return Ok(new UserDTO(user));
        }

        [Authorize(Roles = "User")]
        [HttpGet("my-tickets")]
        [SwaggerOperation(Summary = "[User] Get user's tickets.", Description = "Retrieves the tickets purchased by the logged-in user.")]
        [SwaggerResponse(200, "Tickets retrieved successfully", typeof(List<TicketDTO>))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as User.", typeof(ErrorDTO))]
        public async Task<ActionResult<List<TicketDTO>>> MyTickets()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user ID missing in token)."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user no longer exists)."));
            }

            var ticketDTOs = await _context.Orders
                .AsNoTracking()
                .Where(o => o.UserId == user.Id)
                .SelectMany(o => o.Tickets)
                .Include(t => t.Event)
                .Select(t => new TicketDTO
                {
                    Id = t.Id,
                    EventId = t.EventId,
                    EventName = t.Event.Title,
                    SeatTypeId = t.SeatingCategoryId,
                    Seat = t.Seat,
                    OrderId = t.OrderId,
                    UserId = user.Id,
                    QRCodeUrl = $"http://localhost:5000/api/tickets/{t.Id}/qrcode",
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            return Ok(ticketDTOs);
        }

        [Authorize(Roles = "User")]
        [HttpGet("my-orders")]
        [SwaggerOperation(Summary = "[User] Get user's orders.", Description = "Retrieves the orders purchased by the logged-in user.")]
        [SwaggerResponse(200, "Orders retrieved successfully", typeof(List<OrderDTO>))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as User.", typeof(ErrorDTO))]
        public async Task<ActionResult<List<OrderDTO>>> MyOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user ID missing in token)."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials (user no longer exists)."));
            }

            var orderDTOs = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Tickets)
                .ThenInclude(t => t.Event)
                .Where(o => o.UserId == user.Id)
                .Select(o => new OrderDTO()
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    Status = o.WasRefunded ? "Refunded" : "Paid",
                    CreatedAt = o.CreatedAt,
                    Firstname = user.FirstName,
                    Lastname = user.LastName,
                    Email = user.Email!,
                    Tickets = o.Tickets.Select(t => new TicketDTO
                    {
                        Id = t.Id,
                        EventId = t.EventId,
                        EventName = t.Event.Title,
                        SeatTypeId = t.SeatingCategoryId,
                        Seat = t.Seat,
                        OrderId = t.OrderId,
                        UserId = user.Id,
                        QRCodeUrl = $"http://localhost:5000/api/tickets/{t.Id}/qrcode",
                        CreatedAt = t.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orderDTOs);
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPut("update")]
        [SwaggerOperation(Summary = "[User,Admin] Update an existing user", Description = "Updates existing user details.")]
        [SwaggerResponse(200, "User updated successfully", typeof(UserDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as User or Admin.")]
        [SwaggerResponse(404, "User not found", typeof(ErrorDTO))]
        public async Task<ActionResult<UserDTO>> UpdateUser([FromBody] UpdateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(request.Id.ToString());

            if (user == null)
                return NotFound(new ErrorDTO($"No user found with ID {request.Id}"));

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Phone = request.Phone;
            user.Email = request.Email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new UserDTO(user));
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost("change-password")]
        [SwaggerOperation(Summary = "[User,Admin] Change a user's password", Description = "User must provide their current password to change it.")]
        [SwaggerResponse(200, "Password changed successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Invalid input")]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as User or Admin.")]
        [SwaggerResponse(404, "User not found", typeof(ErrorDTO))]
        [SwaggerResponse(403, "Current password is incorrect or new password is the same", typeof(ErrorDTO))]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
                return NotFound(new ErrorDTO("User not found."));

            if (!await _userManager.CheckPasswordAsync(user, request.CurrentPassword))
                return StatusCode(403, new ErrorDTO("Current password is incorrect."));

            if (request.CurrentPassword == request.NewPassword)
                return StatusCode(403, new ErrorDTO("New password must be different from the current password."));

            await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new SuccessDTO("Password changed successfully."));
        }

        [Authorize(Roles = "User")]
        [HttpDelete("delete")]
        [SwaggerOperation(Summary = "[User] Delete your account", Description = "Deletes the currently logged-in user's account.")]
        [SwaggerResponse(200, "User deleted successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Failed to delete user", typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized or invalid token", typeof(ErrorDTO))]
        [SwaggerResponse(404, "User not found", typeof(ErrorDTO))]
        public async Task<IActionResult> DeleteUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials: user ID missing from token."));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);
                return Unauthorized(new ErrorDTO("Invalid credentials: user no longer exists."));
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ErrorDTO("Failed to delete user."));

            await HttpContext.SignOutAsync(JwtBearerDefaults.AuthenticationScheme);

            return Ok(new SuccessDTO("User deleted successfully."));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id:guid}")]
        [SwaggerOperation(Summary = "[Admin] Delete a user", Description = "Deletes the user with the specified ID. Admin only.")]
        [SwaggerResponse(200, "User deleted successfully", typeof(SuccessDTO))]
        [SwaggerResponse(400, "Failed to delete user.", typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as Admin.")]
        [SwaggerResponse(404, "User not found", typeof(ErrorDTO))]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new ErrorDTO("User not found."));

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new ErrorDTO("Failed to delete user."));

            return Ok(new SuccessDTO("User deleted successfully."));
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("promote-to-admin/{userId:guid}")]
        [SwaggerOperation(Summary = "[SuperAdmin] Promote a user to admin", Description = "If provided with an Id of a regular user, promotes them to admin. SuperAdmin only.")]
        [SwaggerResponse(200, "User promoted to admin successfully.", typeof(SuccessDTO))]
        [SwaggerResponse(400, "User is already an admin.", typeof(ErrorDTO))]
        [SwaggerResponse(401, "Unauthorized: user must be authenticated as SuperAdmin.")]
        [SwaggerResponse(404, "User not found.", typeof(ErrorDTO))]
        public async Task<IActionResult> PromoteUserToAdmin(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new ErrorDTO("User not found."));

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Admin"))
                return BadRequest(new ErrorDTO("Already an admin."));

            if (roles.Contains("User"))
                await _userManager.RemoveFromRoleAsync(user, "User");

            var addResult = await _userManager.AddToRoleAsync(user, "Admin");
            if (!addResult.Succeeded)
                return BadRequest(new ErrorDTO("Failed to assign admin role."));

            return Ok(new SuccessDTO("User promoted to admin successfully."));
        }
    }
}