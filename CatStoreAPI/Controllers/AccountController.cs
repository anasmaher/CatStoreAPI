using AutoMapper;
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
        private readonly IMapper autoMapper;
        private readonly APIResponse response;

        public AccountController(UserManager<AppUser> userManager, IUserService userService, ITokenService tokenService, IEmailService emailService, IMapper autoMapper)
        {
            this.userManager = userManager;
            this.userService = userService;
            this.tokenService = tokenService;
            this.emailService = emailService;
            this.autoMapper = autoMapper;
            response = new APIResponse();
        }

        /// <summary>
        /// Registers a new user in the system.
        /// </summary>
        /// <param name="registerDTO">An object containing the registration details of the user.</param>
        /// <returns>An IActionResult indicating the result of the registration operation.</returns>
        /// <response code="200">User registered successfully.</response>
        /// <response code="400">Registration failed due to validation errors or duplicate email.</response>
        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(AuthRegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await userManager.FindByEmailAsync(registerDTO.email);
                if (existingUser is not null)
                {
                    response.IsSuccess = false;
                    response.StatusCode = HttpStatusCode.BadRequest;
                    response.Errors.Add("User with this email already exists.");
                    return BadRequest(response);
                }

                var res = await userService.CreateUserAsync(registerDTO.firstName, registerDTO.lastName, registerDTO.email, registerDTO.password);

                if (res.Succeeded)
                {
                    response.IsSuccess = true;
                    response.StatusCode = HttpStatusCode.Created;
                    return Ok(response);
                }

                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                foreach (var err in res.Errors) response.Errors.Add(err.Description);
                return BadRequest(response);
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        /// <summary>
        /// Logs in an existing user using email and password.
        /// </summary>
        /// <param name="loginDTO">An object containing the user's login credentials.</param>
        /// <returns>An IActionResult containing the access token and refresh token on successful authentication.</returns>
        /// <response code="200">User logged in successfully.</response>
        /// <response code="400">Entered data is not valid.</response>
        /// <response code="404">User was not found.</response>
        [HttpPost("Login")]
        [AllowAnonymous]
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

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            response.Errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(response);
        }

        /// <summary>
        /// Deletes the authenticated user's account.
        /// </summary>
        /// <param name="userDetails">An object containing the user's email and password.</param>
        /// <returns>An IActionResult indicating the result of the delete operation.</returns>
        /// <response code="200">Successfully deleted.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpDelete]
        [Authorize]
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
                return Unauthorized(response);
            }
        }

        /// <summary>
        /// Initiates the Google OAuth authentication flow.
        /// </summary>
        /// <returns>A ChallengeResult that redirects the user to Google for authentication.</returns>
        [HttpGet("signin-google")]
        [AllowAnonymous]
        public IActionResult LoginGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Handles the response from Google after user authentication.
        /// </summary>
        /// <returns>An IActionResult containing the generated tokens on successful authentication.</returns>
        /// <response code="200">User authenticated successfully and tokens issued.</response>
        /// <response code="400">Authentication failed or email claim not received from Google.</response>
        [HttpGet("GoogleResponse")]
        [AllowAnonymous]
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
            response.Result = new { token };
            return Ok(response);
        }

        /// <summary>
        /// Initiates the password reset process for the user by sending a reset link to the provided email (DOES NOT REALLY SEND THE EMAIL DUE TO THE LACK OF A SENDER EMAIL).
        /// </summary>
        /// <param name="forgotPasswordDTO">An object containing the email of the user who wants to reset the password.</param>
        /// <returns>An IActionResult indicating the result of sending the password reset email.</returns>
        /// <response code="200">Password reset email sent successfully.</response>
        /// <response code="400">Bad request due to validation errors or email sending failure.</response>
        /// <response code="404">User is not found.</response>
        [HttpPost("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDTO forgotPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(forgotPasswordDTO.Email);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User is not found");
                return NotFound(response);
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var resetUrl = Url.Action("ResetPassword", "Account", new { token, email = forgotPasswordDTO.Email }, Request.Scheme);

            try
            {
                EmailMetadata emailMetadata = new(
                    forgotPasswordDTO.Email,
                    "Reset password",
                    $"Reset your password using this link: {resetUrl}"
                );

                emailService.SendEmailAsync(emailMetadata);

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

        /// <summary>
        /// Resets the user's password using the provided reset token.
        /// </summary>
        /// <param name="resetPasswordDTO">An object containing the email, reset token, and new password.</param>
        /// <returns>An IActionResult indicating the result of the password reset operation.</returns>
        /// <response code="200">Password reset successfully.</response>
        /// <response code="400">Bad request due to validation errors or invalid token.</response>
        /// <response code="404">User is not found.</response>
        [HttpPost("ResetPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.FindByEmailAsync(resetPasswordDTO.Email);
            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User is not found");
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
            foreach (var err in result.Errors) response.Errors.Add(err.Description);
            return BadRequest(response);
        }

        /// <summary>
        /// Changes the password of the authenticated user.
        /// </summary>
        /// <param name="changePasswordDTO">An object containing the current password and new password.</param>
        /// <returns>An IActionResult indicating the result of the password change operation.</returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Bad request due to validation errors or incorrect current password.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return Unauthorized(response);
            }

            var res = await userManager.ChangePasswordAsync(user, changePasswordDTO.CurrentPassword, changePasswordDTO.NewPassword);

            if (!res.Succeeded)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = new List<string>();
                foreach (var err in res.Errors) response.Errors.Add(err.Description);
                return BadRequest(response);
            }

            user.TokenVersion++;
            await userManager.UpdateAsync(user);

            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Ok(response);
        }

        /// <summary>
        /// Logs out the user from all devices by invalidating all active tokens.
        /// </summary>
        /// <returns>An IActionResult indicating the result of the logout operation.</returns>
        /// <response code="200">User logged out from all devices successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpPost("LogOutAll")]
        [Authorize]
        public async Task<IActionResult> LogOutAll()
        {
            var user = await userManager.GetUserAsync(User);

            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not authenticated");
                return Unauthorized(response);
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

        /// <summary>
        /// Logs out the user from the current device by invalidating the current token.
        /// </summary>
        /// <returns>An IActionResult indicating the result of the logout operation.</returns>
        /// <response code="200">User logged out from the current device successfully.</response>
        /// <response code="401">User is not authenticated or token is invalid.</response>
        [HttpPost("LogOutSingle")]
        [Authorize]
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

        /// <summary>
        /// Generates a new access token and refresh token using the provided refresh token.
        /// </summary>
        /// <param name="refreshTokenDTO">An object containing the current access token and refresh token.</param>
        /// <returns>An IActionResult containing the new tokens.</returns>
        /// <response code="200">Tokens refreshed successfully.</response>
        /// <response code="400">Invalid request due to missing data.</response>
        /// <response code="401">Invalid refresh token.</response>
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

        /// <summary>
        /// Updates the authenticated user's profile information.
        /// </summary>
        /// <param name="editInfoDTO">An object containing the updated user information.</param>
        /// <returns>An IActionResult indicating the result of the update operation.</returns>
        /// <response code="200">User information updated successfully.</response>
        /// <response code="400">Bad request due to validation errors.</response>
        /// <response code="401">User is not authenticated.</response>
        /// <response code="404">User not found.</response>
        [HttpPost("EditInfo")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo(EditInfoDTO editInfoDTO)
        {
            if (!ModelState.IsValid)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(response);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.Unauthorized;
                response.Errors.Add("User is not Authenticated.");
                return Unauthorized(response);
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user is null)
            {
                response.IsSuccess = false;
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add("User not found.");
                return NotFound(response);
            }

            autoMapper.Map(editInfoDTO, user);

            var result = await userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.OK;
                return Ok(response);
            }

            response.IsSuccess = false;
            response.StatusCode = HttpStatusCode.BadRequest;
            foreach (var error in result.Errors) response.Errors.Add(error.Description);
            return BadRequest(response);
        }
    }
}