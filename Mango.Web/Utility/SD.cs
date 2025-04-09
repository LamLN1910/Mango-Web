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

        public const string Status_Approved = "Approved";
        public const string Status_Pending = "Pending";
        public const string Status_ReadyForPickup = "ReadyForPickup";
        public const string Status_Completed = "Completed";
        public const string Status_Refunded = "Refunded";
        public const string Status_Cancelled = "Cancelled";
    }
}
