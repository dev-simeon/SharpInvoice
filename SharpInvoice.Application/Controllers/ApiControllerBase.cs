using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Net.Mime;

namespace SharpInvoice.Application.Controllers;

/// <summary>
/// Base controller for API endpoints with standard configurations.
/// </summary>
[ApiController]
[Authorize]
[Produces(MediaTypeNames.Application.Json)]
[EnableRateLimiting("api")]
public abstract class ApiControllerBase : ControllerBase
{
    // Base functionality if needed
}