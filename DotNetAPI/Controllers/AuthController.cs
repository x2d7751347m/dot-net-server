using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DotNetAPI.Data;
using DotNetAPI.Dtos;
using DotNetAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Microsoft.IdentityModel.Tokens;

namespace DotNetAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextDapper _dapper;
        private readonly AuthHelper _authHelper;

        public AuthController(IConfiguration config)
        {
            _dapper = new DataContextDapper(config);
            _authHelper = new AuthHelper(config);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                string sqlCheckUserExists = "SELECT Email FROM Auth WHERE Email = @Email";

                var parameters = new { Email = userForRegistration.Email };

                IEnumerable<string> existingUsers = _dapper.LoadData<string, dynamic>(sqlCheckUserExists, parameters);
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                        INSERT INTO Auth (Email, PasswordHash, PasswordSalt) 
                        VALUES (@Email, @PasswordHash, @PasswordSalt)";

                    var authParameters = new 
                    {
                        Email = userForRegistration.Email,
                        PasswordHash = passwordHash,
                        PasswordSalt = passwordSalt
                    };

                    if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, authParameters))
                    {
                        string sqlAddUser = @"
                            INSERT INTO Users(
                                FirstName,
                                LastName,
                                Email,
                                Gender,
                                Active
                            ) VALUES (
                                @FirstName, 
                                @LastName,
                                @Email,
                                @Gender,
                                1)";

                        var userParameters = new
                        {
                            FirstName = userForRegistration.FirstName,
                            LastName = userForRegistration.LastName,
                            Email = userForRegistration.Email,
                            Gender = userForRegistration.Gender
                        };

                        if (_dapper.ExecuteSqlWithParameters(sqlAddUser, userParameters))
                        {
                            return Ok();
                        }
                        throw new Exception("Failed to add user.");
                    }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
            throw new Exception("Passwords do not match!");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT 
                PasswordHash,
                PasswordSalt FROM Auth WHERE Email = @Email";

            var parameters = new { Email = userForLogin.Email };

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto, dynamic>(sqlForHashAndSalt, parameters);

            byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password!");
                }
            }

            string userIdSql = @"
                SELECT UserId FROM Users WHERE Email = @Email";

            int userId = _dapper.LoadDataSingle<int>(userIdSql, parameters);

            return Ok(new Dictionary<string, string> {
                {"token", _authHelper.CreateToken(userId)}
            });
        }

        [HttpGet("RefreshToken")]
        public string RefreshToken()
        {
            string userIdSql = @"
                SELECT UserId FROM Users WHERE UserId = @UserId";
            
            var parameters = new { UserId = User.FindFirst("userId")?.Value };

            int userId = _dapper.LoadDataSingle<int>(userIdSql, parameters);

            return _authHelper.CreateToken(userId);
        }
    }
}