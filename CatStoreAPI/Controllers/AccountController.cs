﻿using CatStoreAPI.DTO.AuthDTOs;
using Core.Interfaces;
using Core.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly APIResponse response;

        public AccountController(UserManager<AppUser> userManager, IUserService userService)
        {
            this.userManager = userManager;
            this.userService = userService;
            response = new APIResponse();
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register(AuthRegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                AppUser user = new AppUser();

                user.Email = registerDTO.email;
                user.FirstName = registerDTO.firstName;
                user.LastName = registerDTO.lastName;
                IdentityResult res = await userManager.CreateAsync(user, registerDTO.password);

                response.Result = res.Succeeded;
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

                }
                return Unauthorized();
            }
            return BadRequest(ModelState);

    }
}
