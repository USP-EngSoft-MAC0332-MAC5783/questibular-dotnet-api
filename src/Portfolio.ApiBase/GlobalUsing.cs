global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Cors.Infrastructure;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.AspNetCore.Server.Kestrel.Core;
global using Microsoft.AspNetCore.Server.Kestrel.Https;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.OpenApi;

global using Portfolio.ApiBase.Configuration;
global using Portfolio.ApiBase.Services;
global using Portfolio.ApiBase.Settings;

global using Swashbuckle.AspNetCore.SwaggerGen;
global using Swashbuckle.AspNetCore.SwaggerUI;

global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.Security.Authentication;
global using System.Security.Claims;
global using System.Security.Cryptography.X509Certificates;
global using System.Text.Json.Serialization;
global using System.Threading.Tasks;