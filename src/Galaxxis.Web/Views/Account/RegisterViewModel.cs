using System.ComponentModel.DataAnnotations;

namespace Galaxxis.Web.Views.Account
{
    public class RegisterViewModel
    {
        #region properties ------------------------------------------------------------------------

        [DataType(DataType.EmailAddress)]
        [Required]
        public string Email { get; set; } = null;

        [DataType(DataType.Text)]
        [Required]
        public string Username { get; set; } = null;

        [DataType(DataType.Text)]
        [Required]
        public string Firstname { get; set; } = null;

        [DataType(DataType.Text)]
        [Required]
        public string Lastname { get; set; } = null;

        [DataType(DataType.Password)]
        [Required]
        public string Password { get; set; } = null;

        [DataType(DataType.Password)]
        [Required]
        public string Password2 { get; set; } = null;

        #endregion

        public bool IsEmpty() 
        {
            return
                Email == null
                && Username == null
                && Firstname == null
                && Lastname == null
                && Password == null
                && Password2 == null;
        }

        public bool IsIncomplete()
        {
            return Email == null
            || Username == null
            || Firstname == null
            || Lastname == null
            || Password == null
            || Password2 == null;
        }

        public bool IsPasswordValid()
        {
            return Password.Equals(Password2);
        }

        public void Reset()
        {
            Email = null;
            Username = null;
            Firstname = null;
            Lastname = null;
            Password = null;
            Password2 = null;
        }
    }
}
