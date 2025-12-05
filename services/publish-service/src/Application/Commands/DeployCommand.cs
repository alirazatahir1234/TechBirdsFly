using MediatR;
using PublishService.Application.DTOs;

namespace PublishService.Application.Commands;

/// <summary>
/// Command to deploy a website
/// </summary>
public record DeployCommand(DeployRequestDto Request) : IRequest<string>;
