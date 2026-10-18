using MediatR;
using Ultramaverick.Manufacturing.Application.Models;

namespace Ultramaverick.Manufacturing.Application.Commands.TransformationRequests;

/// <summary>Prepares the materials of an existing request. Only the id travels in the body;
/// the caller identity comes from the request principal.</summary>
public sealed record PrepareTransformationRequestCommand(int Id) : IRequest<Result>;
