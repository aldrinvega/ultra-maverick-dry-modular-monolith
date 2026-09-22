namespace Ultramaverick.Identity.Application.Models
{
    public sealed record AuthenticateRequest(string UserName, string Password);
}