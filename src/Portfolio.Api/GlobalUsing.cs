global using AspNetHasher = Microsoft.AspNetCore.Identity.IPasswordHasher<object>;
global using AspNetResult = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

global using Asp.Versioning;

global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.DataProtection;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.Extensions.Options;
global using Microsoft.IdentityModel.Tokens;

global using OtpNet;

global using Portfolio.Api;
global using Portfolio.Api.Composition.Extensions;
global using Portfolio.Api.Features.Auth;
global using Portfolio.Api.Features.Auth.Contracts;
global using Portfolio.Api.Features.Auth.Handlers;
global using Portfolio.Api.Features.Exams;
global using Portfolio.Api.Features.Exams.Contracts;
global using Portfolio.Api.Features.Users;
global using Portfolio.Api.Features.Users.Contracts;
global using Portfolio.Api.Infrastructure;
global using Portfolio.Api.Infrastructure.EnemApi;
global using Portfolio.Api.Infrastructure.Events;
global using Portfolio.Api.Infrastructure.Security;

global using Portfolio.ApiBase.Builders;
global using Portfolio.ApiBase.Controllers;
global using Portfolio.ApiBase.Services;

global using Portfolio.DataAccess.Extensions;
global using Portfolio.DataAccess.Mapping;

global using Portfolio.Domain.Entities;
global using Portfolio.Domain.Enums;
global using Portfolio.Domain.Events;
global using Portfolio.Domain.Events.AuthEvents;
global using Portfolio.Domain.Notifications;
global using Portfolio.Domain.Repositories;
global using Portfolio.Domain.Security;

global using System.ComponentModel.DataAnnotations;
global using System.IdentityModel.Tokens.Jwt;
global using System.Net;
global using System.Security.Claims;
global using System.Security.Cryptography;
global using System.Text;
global using System.Text.Json;
global using System.Text.Json.Serialization;
global using System.Threading.Channels;