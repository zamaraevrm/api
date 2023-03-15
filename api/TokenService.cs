﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Model;
using Microsoft.IdentityModel.Tokens;

namespace api;

public class TokenService
{
    private readonly string _issuer;
    private readonly string _audience;
    private readonly byte[] _accessTokenSecret;
    private readonly byte[] _refreshTokenSecret;


    public TokenService(IConfiguration config)
    {
        _issuer = config.GetValue<string>("Jwt:Issuer");
        _audience = config.GetValue<string>("Jwt:Audience");
        _accessTokenSecret = Encoding.ASCII.GetBytes(config.GetValue<string>("Jwt:Key"));
        _refreshTokenSecret = Encoding.ASCII.GetBytes(config.GetValue<string>("Jwt:Key"));
    }
    
    public string GenerateAccessToken(User user)
    {
    	var tokenDescriptor = new SecurityTokenDescriptor
    	{
    		Subject = new ClaimsIdentity(
    			new[]
    			{
    				new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
    				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
    				new Claim(ClaimTypes.Name, user.Name),
    				new Claim(ClaimTypes.Email, user.Email),
    				new Claim(ClaimTypes.Role, user.Role),
    			}
    		),
    		Expires = DateTime.UtcNow.AddMinutes(15),
    		SigningCredentials = new SigningCredentials(
    			new SymmetricSecurityKey(_accessTokenSecret),
    			SecurityAlgorithms.HmacSha256Signature
    		),
    		Issuer = _issuer,
    		Audience = _audience,
    	};

    	var tokenHandler = new JwtSecurityTokenHandler();
    	var token = tokenHandler.CreateToken(tokenDescriptor);
    	return tokenHandler.WriteToken(token);
    }
    
    public (Guid, string) GenerateRefreshToken()
    {
    	var tokenId = Guid.NewGuid();
    	var tokenHandler = new JwtSecurityTokenHandler();
    	var tokenDescriptor = new SecurityTokenDescriptor
    	{
    		Subject = new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Jti, tokenId.ToString()), }),
    		Expires = DateTime.UtcNow.AddDays(1),
    		SigningCredentials = new SigningCredentials(
    			new SymmetricSecurityKey(_refreshTokenSecret),
    			SecurityAlgorithms.HmacSha256Signature
    		),
    		Issuer = _issuer,
    		Audience = _audience,
    	};

    	var token = tokenHandler.CreateToken(tokenDescriptor);
    	return (tokenId, tokenHandler.WriteToken(token));
    }
}