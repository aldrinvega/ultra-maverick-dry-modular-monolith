namespace Ultramaverick.Api.Authorization
{
    public static class Policies
    {
        public const string IdentityAdmin = "RequireIdentityAdmin";
        public static string RequireModule(string module) => $"RequireModule:{module}";
    }
}
