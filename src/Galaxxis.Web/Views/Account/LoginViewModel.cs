using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Web.Views.Account
{
    public class LoginViewModel
    {
        [DataType(DataType.Url)]
        public string ReturnUrl { get; set; }

        [DataType(DataType.Text)]
        public string User { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
