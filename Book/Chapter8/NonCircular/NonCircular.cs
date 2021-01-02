namespace Book.Chapter8.NonCircular
{
    public class CheckOutService
    {
        public void CheckOut(int orderId)
        {
            var service = new ReportGenerationService();
            Report report = service.GenerateReport(orderId);

            /* other work */
        }
    }

    public class ReportGenerationService
    {
        public Report GenerateReport(int orderId)
        {
            /* ... */

            return null;
        }
    }

    public class Report
    {
    }
}
