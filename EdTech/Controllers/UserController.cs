using Dapper;
using EdTech.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using EdTech.Entities;
using Org.BouncyCastle.Asn1.Pkcs;

namespace EdTech.Controllers
{
  [Route("api/[controller]")]
   [ApiController]
   public class UserController : ControllerBase
   {
        private readonly IConnectionProvider _connectionProvider;
        private readonly ITools _tools;
        private readonly IBCryptHelper _bCryptHelper;

        public UserController(IConnectionProvider connectionProvider, ITools tools, IBCryptHelper bCryptHelper)
        {
            _connectionProvider = connectionProvider;
            _tools = tools;
            _bCryptHelper = bCryptHelper;
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RegisterAccount")]
        public async Task<IActionResult> RegisterAccount(User entity)
        {
            // ApiResponse<string> response = new ApiResponse<string>();
            ApiResponse<User>   response = new ApiResponse<User>();

            try
            {
                if (string.IsNullOrEmpty(entity.UserName) || string.IsNullOrEmpty(entity.UserEmail))
                {
                    response.ErrorMessage = "Name and email are required.";
                    response.Code = 400;
                    return BadRequest(response);
                }

                // Hash the password and assign it back to the entity
                entity.UserPassword = _bCryptHelper.HashPassword(entity.UserPassword);
                //entity.UserToken = _tools.GenerateToken(entity.UserToken);


                using (var context = _connectionProvider.GetConnection())
                {
                    // Use MakeHtmlNewUser method to create a customized HTML email
                    string body = _tools.MakeHtmlNewUser(entity);

                    // Check if the HTML body was generated successfully
                    if (body == "Error")
                    {
                        response.ErrorMessage = "Error creating HTML email.";
                        response.Code = 500;
                        return BadRequest(response);
                    }


                    string recipient = entity.UserEmail;

                    bool emailIsSend = _tools.SendEmail(recipient, "New Account", body);
                    if (emailIsSend)
                    {
              
                        var data = await context.QueryFirstOrDefaultAsync<User>("RegisterAccount",
                        new { entity.UserName, entity.UserEmail, entity.UserPassword, entity.UserType, entity.UserState },
                        commandType: CommandType.StoredProcedure);
                        Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(data));

                        if (data != null)
                        {
                         
                            response.Success = true;
                            response.Code = 200;
                            response.Data = data;
                            response.Data.UserToken = _tools.GenerateToken(data.UserId.ToString(), data.UserType.ToString());
                            return Ok(response);
                        }
                        else
                        {
                            response.ErrorMessage = "Error saving new user";
                            response.Code = 500;
                            return BadRequest(response);
                        }
                    }
                    else
                    {
                        response.ErrorMessage = "Error  email";
                        response.Code = 500;
                        return BadRequest(response);
                    }
                }
            }
            catch (SqlException ex)
            {
                response.ErrorMessage = "Unexpected Error: " + ex.Message;
                response.Code = 500;
                return BadRequest(response);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("Login")]
        public async Task<IActionResult> Login(User entity)
        {
            ApiResponse<User> response = new ApiResponse<User>();

            try
            {
                if (string.IsNullOrEmpty(entity.UserEmail) || string.IsNullOrEmpty(entity.UserPassword))
                {
                    response.ErrorMessage = "Email and password are required";
                    response.Code = 400;
                    return BadRequest(response);
                }

                using (var connection = _connectionProvider.GetConnection())
                {
                    var data = await connection.QueryFirstOrDefaultAsync<User>("Login",
                        new { entity.UserEmail },
                        commandType: CommandType.StoredProcedure);

                    //Check password
                    bool validPassword = _bCryptHelper.CheckPassword(entity.UserPassword, data.UserPassword);

                    if (data == null || !validPassword)
                    {
                        response.ErrorMessage = "Incorrect email or password";
                        response.Code = 404;
                        return NotFound(response);
                    }

                    response.Success = true;
                    response.Code = 200;
                    response.Data = data;
                    response.Data.UserToken = _tools.GenerateToken(data.UserId.ToString(), data.UserType.ToString());
                    return Ok(response);
                }
            }
            catch (SqlException ex)
            {
                response.ErrorMessage = "Unexpected Error: " + ex.Message;
                return BadRequest(response);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("PwdRecovery")]
        public async Task<IActionResult> PwdRecovery(User entity)
        {
            ApiResponse<User> response = new ApiResponse<User>();

            try
            {
                if (string.IsNullOrEmpty(entity.UserEmail))
                {
                    response.ErrorMessage = "Email is required";
                    response.Code = 400;
                    return BadRequest(response);
                }

                var randomPassword = _tools.GenerateRandomCode(8);
                var hashedPassword = _bCryptHelper.HashPassword(randomPassword);

                using (var connection = _connectionProvider.GetConnection())
                {
                    // Execute the combined stored procedure
                    var result = await connection.ExecuteAsync("PwdRecovery",
                        new { entity.UserEmail, NewPassword = randomPassword },
                        commandType: CommandType.StoredProcedure);

                    // Check result for success message or error
                    if (result > 0)
                    {
                        bool emailIsSend = _tools.SendEmail(entity.UserEmail, "Password Recovery",
                            $"Your temporary password is: {randomPassword}");

                        if (emailIsSend)
                        {
                            response.Success = true;
                            response.Code = 200;
                            return Ok(response);
                        }
                        else
                        {
                            response.ErrorMessage = "Error sending the email";
                            response.Code = 500;
                            return BadRequest(response);
                        }
                    }
                    else
                    {
                        response.ErrorMessage = "Error updating password";
                        response.Code = 500;
                        return BadRequest(response);
                    }
                }
            }
            catch (SqlException ex)
            {
                response.ErrorMessage = "Unexpected Error: " + ex.Message;
                return BadRequest(response);
            }
        }


        [HttpGet]
        [AllowAnonymous]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            ApiResponse<List<User>> response = new ApiResponse<List<User>>();

            try
            {

                using (var context = _connectionProvider.GetConnection())
                {
                    var usersData = await context.QueryAsync<User>("GetAllUsers", commandType: CommandType.StoredProcedure);

                    response.Success = true;
                    response.Data = usersData.ToList();
                    // Add the ngrok skip browser warning header
                    Response.Headers.Add("ngrok-skip-browser-warning", "1");

                    return Ok(response);
                }
            }
            catch (SqlException ex)
            {
                response.ErrorMessage = "Unexpected Error: " + ex.Message;
                response.Code = 500;
                Response.Headers.Add("ngrok-skip-browser-warning", "1");
                return BadRequest(response);
            }
        }


    }

}
   
