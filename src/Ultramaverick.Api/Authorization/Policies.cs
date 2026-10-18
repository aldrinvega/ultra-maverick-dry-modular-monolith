namespace Ultramaverick.Api.Authorization
{
    public static class Policies
    {
        public const string IdentityAdmin = "RequireIdentityAdmin";

        // Attributes need a constant; must stay in sync with RequireModule.
        public const string Catalog = "RequireModule:Catalog";
        public const string Procurement = "RequireModule:Procurement";
        public const string Quality = "RequireModule:Quality";
        public const string Laboratory = "RequireModule:Laboratory";
        public const string Warehouse = "RequireModule:Warehouse";

        public static string RequireModule(string module) => $"RequireModule:{module}";
    }
}
