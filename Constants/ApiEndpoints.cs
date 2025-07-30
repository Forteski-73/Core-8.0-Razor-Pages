namespace OXF.Constants
{
    public static class ApiEndpoints
    {
        public static class User
        {
            public const string Login       = "/v1/User/login";
            public const string Register    = "/v1/User/register";

        }

        public static class Product
        {
            public const string Products        = "/v1/Product";
            public const string Search          = "/v1/Product/Search";
            public const string Invent          = "/v1/Invent";
            public const string InventDim       = "/v1/Inventdim";
            public const string Oxford          = "/v1/Oxford";
            public const string TaxInformation  = "/v1/TaxInformation";
            public const string Sync            = "/v1/Product/Sync";
        }

    }
}
