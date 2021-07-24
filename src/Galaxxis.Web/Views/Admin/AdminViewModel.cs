namespace Galaxxis.Web.Views.Admin
{
    public class AdminViewModel
    {
        public string TickerStatus { get; set; }

        public AdminViewModel()
        {
            this.TickerStatus = Galaxxis.Core.Engine.Ticker.Instance.Status;
        }
    }
}
