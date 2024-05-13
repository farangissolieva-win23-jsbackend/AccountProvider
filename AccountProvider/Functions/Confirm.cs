using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;


namespace AccountProvider.Functions;

public class Confirm(ILogger<Confirm> logger, UserManager<UserAccount> userManager)
{
	private readonly ILogger<Confirm> _logger = logger;
	private readonly UserManager<UserAccount> _userManager = userManager;

	[Function("Confirm")]
	public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
	{
		string body = null!;
		try
		{
			body = await new StreamReader(req.Body).ReadToEndAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError($": Confirm.Run :: {ex.Message}");
			return new BadRequestResult();
		}

		if (body != null)
		{
			VerificationRequest vr = null!;
			try
			{
				vr = JsonConvert.DeserializeObject<VerificationRequest>(body)!;
			}
			catch (Exception ex)
			{
				_logger.LogError($": Confirm.DeserializeObject<VerificationRequest> :: {ex.Message}");
				return new BadRequestResult();
			}

			if (vr != null && !string.IsNullOrEmpty(vr.Email) && !string.IsNullOrEmpty(vr.VerificationCode))
			{
				try
				{
					using var http = new HttpClient();
					StringContent content = new(JsonConvert.SerializeObject(vr), Encoding.UTF8, "application/json");
					var response = await http.PostAsync("https://verificationprovider-silicon.azurewebsites.net/api/validate?code=vHmvVCO1u0-5fkAXQ7e3Xh-29PxsKGhNsZT9O9fsd8jhAzFuzHy0Gg==", content);

					if (response.IsSuccessStatusCode)
					{
						var userAccount = await _userManager.FindByEmailAsync(vr.Email);
						if (userAccount != null)
						{
							userAccount.EmailConfirmed = true;
							await _userManager.UpdateAsync(userAccount);
							if (await _userManager.IsEmailConfirmedAsync(userAccount))
							{
								return new OkResult();
							}
						}
					}
					else
					{
						_logger.LogError($": Confirm.VerifyCode :: Verification code validation failed");
						return new UnauthorizedResult();
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($": Confirm.VerifyCode :: {ex.Message}");
					return new StatusCodeResult(StatusCodes.Status500InternalServerError);
				}
			}
			return new BadRequestResult();
		}
		return new BadRequestResult();
	}
}
