﻿global using System.Dynamic;
global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.CookiePolicy;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;
global using NSwag;
global using Serilog;
global using UserManagement.Common;
global using UserManagement.Common.DI;
global using UserManagement.Common.Entities;
global using UserManagement.Common.PaginationParameters;
global using UserManagement.UserManagementAPI.DataAccess;
global using UserManagement.UserManagementAPI.Models;
global using UserManagement.UserManagementAPI.Services;