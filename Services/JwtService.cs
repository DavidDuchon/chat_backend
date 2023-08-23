using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;

using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

using chat_backend.Options;

namespace chat_backend.Services;

public interface IJWTService {
    string GetSignedToken<PayloadType>(PayloadType Payload,IJWTService.TypeToken? Token);
    PayloadType GetPayloadFromToken<PayloadType>(string Token);

    enum TypeToken {
        RefreshToken,
        AccessToken
    }
}

public class JWTService : IJWTService {

    private JWTOptions _options; 
    public JWTService(IOptions<JWTOptions> options){
        _options = options.Value;
    }
    public string GetSignedToken<PayloadType>(PayloadType Payload,IJWTService.TypeToken? Token = null){
        var tokenHandler = new JsonWebTokenHandler();

        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_options.JWTKey));
        var signinCredentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);
        
        var currentTime = DateTime.UtcNow;

        TimeSpan timeSpanToAdd;

        switch (Token){
            case IJWTService.TypeToken.AccessToken:
                timeSpanToAdd = new TimeSpan(0,30,0);
                break; 
            case IJWTService.TypeToken.RefreshToken:
                timeSpanToAdd = new TimeSpan(1,30,0);
                break;
            default:
                timeSpanToAdd = new TimeSpan(0,30,0);
                break;
        }

        var totalTime = currentTime + timeSpanToAdd;

        var claims = Payload!.GetType().GetProperties().ToDictionary((property) => property.Name,(property) => property.GetValue(Payload));

        var securityTokenDescriptor = new SecurityTokenDescriptor{
            Expires = totalTime,
            NotBefore = currentTime,
            SigningCredentials = signinCredentials,
            IssuedAt = currentTime,
            Claims = claims
        };

        return tokenHandler.CreateToken(securityTokenDescriptor);
    }

    public PayloadType GetPayloadFromToken<PayloadType>(string Token) {
        JwtSecurityToken token = new JwtSecurityToken(Token);

        var payload = JsonSerializer.Deserialize<PayloadType>(token.Payload.SerializeToJson());

        return payload!;
    }

}
