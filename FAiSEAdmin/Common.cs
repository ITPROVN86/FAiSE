namespace FAiSEAdmin
{
    public class Common
    {
        /// <summary>
        /// Get Ngày giờ server
        /// Output: Ngày giờ được lấy tại Server
        /// </summary>
        public static DateTime GetServerDateTime()
        {
            return DateTime.Now.ToUniversalTime().AddHours(7);
        }
    }
}
