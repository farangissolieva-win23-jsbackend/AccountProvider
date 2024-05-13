
namespace AccountProvider.Models;

public class TokenVerificationRequest
{
	public string Email { get; set; } = null!;
	public string Token { get; set; } = null!;
}
