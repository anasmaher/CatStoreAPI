using CatStoreAPI.DTO.AuthDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;

namespace CatStoreAPI.Controllers
{
    [AllowAnonymous]
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

        [AllowAnonymous]
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
                foreach (var err in res.Errors) response.Errors.Add(err.Description);             
                
                return BadRequest(response);
            }
            
            return BadRequest(ModelState);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(AuthLoginDTO loginDTO)
        {
            if (ModelState.IsValid)
            {
                AppUser user = await userService.FindUserAsync(loginDTO.Email, loginDTO.Password);

                if (user is not null)
                {
                    var tokens = await tokenService.GenerateTokenAsync(user);

                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.OK;
                    response.Result = new { tokens };
                    return Ok(response);
                }
                
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found");
                return NotFound(response);
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
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return NotFound(response);
            }
        }

        [AllowAnonymous]
        [HttpGet("signin-google")]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        [HttpGet("GoogleResponse")]
        public async Task<IActionResult> GoogleResponse()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Google authentication failed.");
                return BadRequest(response);
            }

            // Extract user information from the claims
            var claims = authenticateResult.Principal.Identities.FirstOrDefault()
                                 ?.Claims.Select(claim => new
                                 {
                                     claim.Type,
                                     claim.Value
                                 });

            var emailClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Email);
            var nameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Name);
            var givenNameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.GivenName);
            var surnameClaim = authenticateResult.Principal.FindFirst(ClaimTypes.Surname);

            var email = emailClaim.Value;
            var firstName = givenNameClaim?.Value;
            var lastName = surnameClaim?.Value;
            var fullName = nameClaim?.Value;

            if (emailClaim is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Email claim not received from Google.");
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(emailClaim.Value);

            if (user is null)
            {
                user = new AppUser
                {
                    Email = email,
                    UserName = email,
                    EmailConfirmed = true,
                    FirstName = firstName,
                    LastName = lastName
                };

                if (!string.IsNullOrEmpty(fullName))
                {
                    var names = fullName.Split(' ');
                    firstName = names.FirstOrDefault();
                    lastName = names.Skip(1).FirstOrDefault();
                    user.FirstName = firstName;
                    user.LastName = lastName;
                }

                if (string.IsNullOrEmpty(firstName)) user.FirstName = "First";
                if (string.IsNullOrEmpty(lastName)) user.LastName = "Last";
                
                var res = await userManager.CreateAsync(user);

                if (!res.Succeeded)
                {
                    response.Result = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors.Add("Could not create user.");
                }
            }

            var token = await tokenService.GenerateTokenAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = new {token};
            return Ok(response);
        }

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            if(!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);

            if(user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return NotFound(response);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl = Url.Action("ResetPassword", "Account", new { token, email = forgotPasswordDTO.Email }, Request.Scheme);

            try
            {
                await emailService.SendEmailAsync(forgotPasswordDTO.Email, "Password reset", $"Reset your password using this link: {resetUrl}");

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(ex.Message);
                return BadRequest(response);
            }
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return NotFound(response);
            }

            // Use the token to reset the password
            var result = await userManager.ResetPasswordAsync(user, resetPasswordDTO.Token, resetPasswordDTO.NewPassword);

            if (result.Succeeded)
            {
                user.TokenVersion++;
                await userManager.UpdateAsync(user);

                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }

            // If the token is invalid or expired, return an error
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = new List<string>();

            foreach(var err in result.Errors) response.Errors.Add($"{err}");
            
            return BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return NotFound(response);
            }

            var res = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

            if (!res.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                foreach (var err in res.Errors) response.Errors.Add($"{err}");
                return BadRequest(response);
            }
           
            user.TokenVersion++;
            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost("LogOutAll")]
        public async Task<IActionResult> LogOutAll()
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return NotFound(response);
            }

            user.TokenVersion++; // invalidate tokenVersion

            var res = await userManager.UpdateAsync(user);

            if (res.Succeeded)
            {
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }
            
            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = new List<string>();
            foreach (var err in res.Errors) response.Errors.Add($"{err}");
            return BadRequest(response);
            
        }

        [Authorize]
        [HttpPost("LogOutSingle")]
        public async Task<IActionResult> LogOutSingle()
        {
            // Check if User is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return Unauthorized(response);
            }

            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrEmpty(jti))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("Token ID (jti) is missing");
                return Unauthorized(response);
            }

            await tokenService.RevokeTokenAsync(jti);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User ID is missing in the token");
                return Unauthorized(response);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User not found");
                return Unauthorized(response);
            }

            // Revoke the refresh token
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDTO refreshTokenDTO)
        {
            if (refreshTokenDTO is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add("Invalid request");
                return BadRequest(response);
            }

            string accessToken = refreshTokenDTO.Token;
            string refreshToken = refreshTokenDTO.RefreshToken;

            var principal = tokenService.GetPrincipalFromExpiredToken(accessToken);
            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

            var user = await userManager.FindByIdAsync(userId);

            if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("Invalid refresh token.");
                return Unauthorized(response);
            }

            var tokens = await tokenService.GenerateTokenAsync(user, false);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            response.Result = tokens;
            return Ok(response);
        }
    }
}
