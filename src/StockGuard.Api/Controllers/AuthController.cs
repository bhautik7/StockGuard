using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockGuard.Application.DTOs;
using StockGuard.Infrastructure.Identity;
using StockGuard.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace StockGuard.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly JwtTokenGenerator _tokenGenerator;
    private readonly AppDbContext _context;   

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, JwtTokenGenerator tokenGenerator,AppDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenGenerator = tokenGenerator;
        _context=context;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = request.Email,
            Email = request.Email,
            FullName = request.FullName
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        var roleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!roleResult.Succeeded)
            return BadRequest(roleResult.Errors.Select(e => e.Description));
        
        var token = _tokenGenerator.GenerateToken(user, new List<string> { request.Role });
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse(user.Id, user.Email!, user.FullName, request.Role, token, refreshToken.Token));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
            return Unauthorized("Invalid email or password.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password.");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenGenerator.GenerateToken(user, roles);
        var refreshToken = _tokenGenerator.GenerateRefreshToken(user.Id);
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse(user.Id, user.Email!, user.FullName, roles.FirstOrDefault() ?? "", token, refreshToken.Token));
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh([FromBody] string refreshToken)
    {
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshToken);
        if (stored is null || stored.IsRevoked || stored.ExpiresAtUtc < DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token.");

        var user = await _userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null) return Unauthorized();

        var roles = await _userManager.GetRolesAsync(user);
        var newToken = _tokenGenerator.GenerateToken(user, roles);

        stored.IsRevoked = true; // old refresh token can't be reused
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken(user.Id);
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync();

        return Ok(new AuthResponse(user.Id, user.Email!, user.FullName, roles.FirstOrDefault() ?? "", newToken, newRefreshToken.Token));
    }
}