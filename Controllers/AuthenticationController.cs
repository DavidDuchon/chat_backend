using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

using chat_backend.Data;
using chat_backend.Services;
using chat_backend.Models;

namespace chat_backend.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthenticationController: ControllerBase {
    private const string validUsername = @"^[a-zA-Z0-9@%()!]{4,100}$";
    private const string validPassword = @"^[a-zA-Z0-9@%()!&*_+ ]{4,100}$";
    private string _token = string.Empty;
    private string _refreshToken = string.Empty;
    private ulong _exp {get;set;}
    private readonly IJWTService _jwtService;
    private readonly DatabaseContext _context;
    private CookieOptions _cookieOptions{get;set;}
    public AuthenticationController(IJWTService jwtService,DatabaseContext context){
        _jwtService = jwtService;
        _context = context;
        _cookieOptions = new CookieOptions {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

    }

    private static bool ValidCredentials(User user){
        return !(!Regex.IsMatch(@""+user.Username,validUsername) || !Regex.IsMatch(@""+user.Password,validPassword));
    }

    private void CreateTokens(string Username){

        var tokenPayload = new AuthenticationTokenPayload{User = Username};
        _token = _jwtService.GetSignedToken(tokenPayload,IJWTService.TypeToken.AccessToken);

        var refreshTokenPayload = new RefreshTokenPayload{For = Username};
        _refreshToken = _jwtService.GetSignedToken(refreshTokenPayload,IJWTService.TypeToken.RefreshToken);

        _exp = _jwtService.GetPayloadFromToken<Expiration>(_token).exp;
    }

    [HttpPost]
    [Route("login")]
    public IActionResult Login(User user){
        if (!ValidCredentials(user))
            return BadRequest(new {error= "Invalid Credentials"});

        var alreadyRegistered = _context.Users.SingleOrDefault<User>(usr => usr.Username == user.Username && usr.Password == user.Password);


        if (alreadyRegistered is null)
            //user with given credentials not found
            return NotFound(); 

        CreateTokens(user.Username);

        HttpContext.Response.Cookies.Append("refresh",_refreshToken,_cookieOptions);
        return Ok(new {token = new {value = _token,exp = _exp}});
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(User user){
        if (!ValidCredentials(user))
            return BadRequest(new {error= "Invalid Credentials"});
        
        var alreadyRegistered = _context.Users.SingleOrDefault(usr => usr.Username == user.Username);

        if (alreadyRegistered is not null)
            return BadRequest(new {error = "Username already registered"}); 

        await _context.Users.AddAsync(user);

        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException){
            return BadRequest(new {error = "Username already registered"});
        }

        CreateTokens(user.Username);

        HttpContext.Response.Cookies.Append("refresh",_refreshToken,_cookieOptions);
        return CreatedAtAction(nameof(Register),new {token = new {value = _token,exp = _exp}});
    }

    [HttpPost]
    [Route("refresh")]
    [Authorize(AuthenticationSchemes="refresh",Policy ="RefreshToken")]
    public IActionResult Refresh(){
        try{

            var refreshToken = HttpContext.Request.Cookies["refresh"];

            var refTokenPayload = _jwtService.GetPayloadFromToken<RefreshTokenPayload>(refreshToken!);


            var alreadyRegistered = _context.Users.SingleOrDefault(usr => usr.Username == refTokenPayload.For);

            if (alreadyRegistered is null)
                return Unauthorized();

            CreateTokens(refTokenPayload.For);

            HttpContext.Response.Cookies.Append("refresh",_refreshToken,_cookieOptions);
            return CreatedAtAction(nameof(Refresh),new {token = new {value = _token,exp = _exp}});
        } catch {
            return BadRequest( new {error = "Bad input data"});
        }
    }

    [HttpPost]
    [Route("verify")]
    [Authorize(AuthenticationSchemes="default",Policy ="AccessToken")]
    public IActionResult Verify(){
        return Ok();
    }
}
