using AccountProvider.Models;
using AccountProvider.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;


namespace AccountProvider.Functions;

public class VerifyToken(ILogger<VerifyToken> logger, TokenValidator tokenValidator)
{
	private readonly ILogger<VerifyToken> _logger = logger;
	private readonly TokenValidator _tokenValidator = tokenValidator;

	[Function("VerifyToken")]
	public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
	{
		string email = null!;
		string token = null!;

		try
		{
			string body = await new StreamReader(req.Body).ReadToEndAsync();

			if (string.IsNullOrEmpty(body))
			{
				return new BadRequestObjectResult("Request body is empty");
			}

			var tokenVerificationRequest = JsonConvert.DeserializeObject<TokenVerificationRequest>(body);

			email = tokenVerificationRequest!.Email;
			token = tokenVerificationRequest.Token;
		}
		catch (Exception ex)
		{
			_logger.LogError($": VerifyToken.Run :: {ex.Message}");
			return new BadRequestObjectResult("Failed to process request");
		}

		if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
		{
			return new BadRequestObjectResult("Email or token is missing");
		}

		bool isValid = _tokenValidator.ValidateToken(token, email);

		if (isValid)
		{
			return new OkResult();
		}
		else
		{
			return new UnauthorizedResult();
		}
	}
}