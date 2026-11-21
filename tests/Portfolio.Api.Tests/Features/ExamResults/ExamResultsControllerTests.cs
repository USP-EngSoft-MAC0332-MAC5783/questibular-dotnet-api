namespace Portfolio.Api.Tests.Features.ExamResults;

using Portfolio.Api.Features.ExamResults;
using Portfolio.ApiBase.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Claims;
using Xunit;

public sealed class ExamResultsControllerTests
{
    /*[Fact]
    public async Task GetById_InvalidLimit_ReturnsPortugueseProblemDetails()
    {
        var service = new Mock<IExamResultService>();
        var controller = new ExamResultsController(
            service.Object,
            NullLogger<ExamResultsController>.Instance,
            Mock.Of<IApplicationContextService>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("sub", Guid.NewGuid().ToString("N")),
                        new Claim(ClaimTypes.Role, "student")
                    ], "test"))
                }
            }
        };

        var response = await controller.GetById(Guid.NewGuid().ToString("N"), CancellationToken.None, limit: 0);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Title.Should().Be("Parâmetro inválido.");
        problem.Detail.Should().Be("O parâmetro limit deve estar entre 1 e 100.");
        service.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetById_NegativeOffset_ReturnsPortugueseProblemDetails()
    {
        var controller = new ExamResultsController(
            Mock.Of<IExamResultService>(),
            NullLogger<ExamResultsController>.Instance,
            Mock.Of<IApplicationContextService>())
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                    [
                        new Claim("sub", Guid.NewGuid().ToString("N")),
                        new Claim(ClaimTypes.Role, "student")
                    ], "test"))
                }
            }
        };

        var response = await controller.GetById(Guid.NewGuid().ToString("N"), CancellationToken.None, offset: -1);

        var objectResult = Assert.IsType<ObjectResult>(response.Result);
        var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
        problem.Status.Should().Be(StatusCodes.Status400BadRequest);
        problem.Detail.Should().Be("O parâmetro offset não pode ser negativo.");
    }*/
}
