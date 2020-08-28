using Auth_basics.Entities;
using Auth_basics.Helpers;
using Auth_basics.Services;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Util.Store;
using Google_Auth_Practice.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Text;

namespace Auth_basics.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private AppSettings _appSettings;

        public IEnumerable<string> Scopes { get; private set; }

        public UsersController(IUserService userService, IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]User userParam)
        {
            var user = _userService.Authenticate(userParam.Username, userParam.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpGet("checktoken")]
        public IActionResult CheckTokenIsValid([FromHeader]string authorization)
        {
            var token = authorization.Replace("Bearer ", "");
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var validationParameters = new TokenValidationParameters()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero

            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validatedToken = null;
            try
            {
                tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
            }
            catch (SecurityTokenException)
            {
                return StatusCode(401, new { TokenIsVlaid = "false" });
            }
            catch (Exception e)
            {

                return StatusCode(111, new { message = e.ToString() });
            }

            //... manual validations return false if anything untoward is discovered
            return Ok(validatedToken);
        }
        /// <summary>
        /// Api for listing users emails.
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet("getmessages")]

        public IActionResult GetMessages(string user)
        {
            {
                string[] Scopes = { "https://www.googleapis.com/auth/gmail.readonly", "profile", "email" };
                string ApplicationName = "Google Gmail Test Proj";
                UserCredential credential;

                using (var stream =
                    new FileStream("credentials2.json", FileMode.Open, FileAccess.Read))
                {
                    // The file token.json stores the user's access and refresh tokens, and is created
                    // automatically when the authorization flow completes for the first time.
                    string credPath = "token.json";
                    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                        GoogleClientSecrets.Load(stream).Secrets,
                        Scopes,
                        user,
                        System.Threading.CancellationToken.None,
                        new FileDataStore(credPath, true)).Result;
                    Console.WriteLine("Credential file saved to: " + credPath);
                }


                GmailService service = new GmailService();
                string userId = user;
                string query = string.Empty;

                List<Message> result = new List<Message>();
                UsersResource.MessagesResource.ListRequest request = service.Users.Messages.List(userId);
                string credentialsString = new StreamReader(".\\token.json\\Google.Apis.Auth.OAuth2.Responses.TokenResponse-"+user).ReadToEnd();
                var tokenString = JsonConvert.DeserializeObject<AccessToken>(credentialsString);
                request.OauthToken =tokenString.access_token;
                //request.Q ="from:laszlo.molnar25@gmail.com";
                do
                {
                    try
                    {
                        ListMessagesResponse response = request.Execute();
                        result.AddRange(response.Messages);
                        request.PageToken = response.NextPageToken;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("An error occurred: " + e.Message);
                    }
                } while (!String.IsNullOrEmpty(request.PageToken));

                int numberOfMessages = result.Count;
                return StatusCode(212, result);

            }
        }
    }
}