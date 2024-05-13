using AccountProvider.Models;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace AccountProvider.Functions;

public class SignUp(ILogger<SignUp> logger, UserManager<UserAccount> userManager)
{
    private readonly ILogger<SignUp> _logger = logger;
    private readonly UserManager<UserAccount> _userManager = userManager;

    [Function("SignUp")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
    {
        string body = null!;
        try
        {
            body = await new StreamReader(req.Body).ReadToEndAsync();
            if (body != null)
            {
                var userRegReq = JsonConvert.DeserializeObject<UserRegistrationRequest>(body);
                if (userRegReq != null && !string.IsNullOrEmpty(userRegReq.Email) && !string.IsNullOrEmpty(userRegReq.Password)) 
                {
                    if (!await _userManager.Users.AnyAsync(x => x.Email == userRegReq.Email))
                    {
                        var userAccout = new UserAccount
                        {
                            FirstName = userRegReq.FirstName,
                            LastName = userRegReq.LastName,
                            Email = userRegReq.Email,
                            UserName = userRegReq.Email
                        };

                        var result = await _userManager.CreateAsync(userAccout, userRegReq.Password);
                        if (result.Succeeded) 
                        {
                            try
                            {
								// verification code
								using var http = new HttpClient();
								StringContent content = new StringContent(JsonConvert.SerializeObject(new { Email = userAccout.Email }), Encoding.UTF8, "applicatiom.json");
								var response = await http.PostAsync("", content);
								return new OkResult();
							}
                            catch (Exception ex)
                            {
								_logger.LogError($": SignUp.http.PostAsync :: {ex.Message}");
							}
                           
                        }
                    }
                    else
                    {
                        return new ConflictResult();
                    }
                        
                }
            }
            
        }
        catch (Exception ex)
        {
            _logger.LogError($": SignUp.Run :: {ex.Message}");
        }
        return new BadRequestResult();
    }
}
