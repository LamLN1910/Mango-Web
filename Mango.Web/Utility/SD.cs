namespace Mango.Web.Utility
{
    public class SD
    {
        public static String CouponAPIBase { get; set; }
        public static String AuthAPIBase { get; set; }
        public static String ProductAPIBase { get; set; }
        public static String ShoppingCartAPIBase { get; set; }
        public static String OrderAPIBase { get; set; }
        public const string RoleAdmin = "ADMIN";
        public const string RoleCustomer = "CUSTOMER";
        public const string TokenCookie = "JWTToken";
        public enum ApiType
        {
            GET,
            POST,
            DELETE,
            PUT
        }
    }
}
