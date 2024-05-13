using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AccountProvider.Services
{
	public class TokenValidator(string tokenValidationKey)
	{
		private readonly string _tokenValidationKey = tokenValidationKey;

		public bool ValidateToken(string token, string email)
		{
			try
			{
				var tokenHandler = new JwtSecurityTokenHandler();
				var key = Encoding.ASCII.GetBytes(_tokenValidationKey);

				var validationParameters = new TokenValidationParameters
				{
					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(key),
					ValidateIssuer = false,
					ValidateAudience = false,
					ClockSkew = TimeSpan.Zero
				};

				var claimsPrincipal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

				var emailClaim = claimsPrincipal.FindFirst("email");
				if (emailClaim == null)
				{
					return false; 
				}

				return emailClaim.Value.Equals(email, StringComparison.OrdinalIgnoreCase);
			}
			catch (Exception)
			{
				return false; 
			}
		}
	}
}
