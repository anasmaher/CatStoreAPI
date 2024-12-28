using CatStoreAPI.DTO.AuthDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using NuGet.Protocol;
using System.Net;

namespace CatStoreAPI.Controllers
{
    [Route("api/Account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> userManager;
        private readonly IUserService userService;
        private readonly ITokenService tokenService;
        private readonly IEmailService emailService;
        private readonly APIResponse response;

        public AccountController(UserManager<AppUser> userManager, IUserService userService, ITokenService tokenService, IEmailService emailService)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.tokenService = tokenService;
            this.emailService = emailService;
            response = new APIResponse();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(AuthRegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                var res = await userService.CreateUserAsync(registerDTO.firstName, registerDTO.lastName, registerDTO.email, registerDTO.password);

                if (res.Succeeded)
                {
                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.OK;
                    return Ok(response);
                }

                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                foreach (var err in res.Errors)
                {
                    response.Errors.Add(err.Description);
                }                
                
                return BadRequest(response);
            }
            else 
                return BadRequest(ModelState);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthLoginDTO loginDTO)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userService.FindUserAsync(loginDTO.Email, loginDTO.Password);

                if (user is not null)
                {
                    var token = await tokenService.GenerateTokenAsync(user);

                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.OK;
                    response.Result = new { token };
                    return Ok(response);
                }
                return Unauthorized();
            }
            return BadRequest(ModelState);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount(AuthLoginDTO userDetails)
        {
            try
            {
                await userService.DeleteUserAsync(userDetails.Email, userDetails.Password);

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                response.Result = userDetails.Email;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add(ex.Message);
                return NotFound(response);
            }
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);

            if(user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return NotFound(response);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl = Url.Action("ResetPassword", "Account", new { token, email = forgotPasswordDTO.Email }, Request.Scheme);

            await emailService.SendEmailAsync(forgotPasswordDTO.Email, "Password reset", $"Reset your password using this link: {resetUrl}");

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return NotFound(response);
            }

            // Use the token to reset the password
            var result = await userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);

            if (result.Succeeded)
            {
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }

            // If the token is invalid or expired, return an error
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = new List<string>();

            foreach(var err in result.Errors)
            {
                response.Errors.Add($"{err}");
            }
            return BadRequest(response);
        }
    }
}
