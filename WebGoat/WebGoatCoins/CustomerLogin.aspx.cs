using System;
using System.Web.Security;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using OWASP.WebGoat.NET.App_Code.DB;
using OWASP.WebGoat.NET.App_Code;
using log4net;
using System.Reflection;
using System.Security;

namespace OWASP.WebGoat.NET.WebGoatCoins
{
    public partial class CustomerLogin : System.Web.UI.Page
    {
        private IDbProvider du = Settings.CurrentDbProvider;
        ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        protected void Page_Load(object sender, EventArgs e)
        {
            PanelError.Visible = false;

            string returnUrl = Request.QueryString["ReturnUrl"];
            if (returnUrl != null)
            {
                PanelError.Visible = true;
            }
        }

        protected void ButtonLogOn_Click(object sender, EventArgs e)
        {
            string email = HttpUtility.HtmlEncode(txtUserName.Text);
            SecureString pwd = ConvertToSecureString(HttpUtility.HtmlEncode(txtPassword.Text));

            log.Info("User " + email + " attempted to log in with password XXXXX");

            if (!du.IsValidCustomerLogin(email, pwd))
            {
                labelError.Text = "Incorrect username/password"; 
                PanelError.Visible = true;
                return;
            }
            // put ticket into the cookie
            FormsAuthenticationTicket ticket =
                        new FormsAuthenticationTicket(
                            1, //version 
                            email, //name 
                            DateTime.Now, //issueDate
                            DateTime.Now.AddDays(14), //expireDate 
                            true, //isPersistent
                            "customer", //userData (customer role)
                            FormsAuthentication.FormsCookiePath //cookiePath
            );

            string encrypted_ticket = FormsAuthentication.Encrypt(ticket); //encrypt the ticket

            // put ticket into the cookie
            HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encrypted_ticket);

            //set expiration date
            if (ticket.IsPersistent)
                cookie.Expires = ticket.Expiration;
                
            Response.Cookies.Add(cookie);
            
            string returnUrl = Request.QueryString["ReturnUrl"];
            
            if (returnUrl == null) 
                returnUrl = "/WebGoatCoins/MainPage.aspx";
                string sanitizedUrl = SanitizeUrlRedirect(returnUrl);
                Response.Redirect(sanitizedUrl);        
        }
        private static string SanitizeUrlRedirect(string url)
        {
            const string VALID_URL1="/WebGoatCoins/CustomerSettings.aspx";
            const string VALID_URL2="/WebGoatCoins/Catalog.aspx";
            const string VALID_URL3="/WebGoatCoins/ChangePassword.aspx";
            const string VALID_URL4="/WebGoatCoins/ForgotPassword.aspx";
            
            switch(url)
            {
                case VALID_URL1:
                return VALID_URL1;
                case VALID_URL2:
                return VALID_URL2;
                case VALID_URL3:
                return VALID_URL3;
                case VALID_URL4:
                return VALID_URL4;
            }
            return "/WebGoatCoins/MainPage.aspx";
        }
        private static SecureString ConvertToSecureString(string password)
        {
            if (password == null)
            throw new ArgumentNullException("password");

            unsafe
            {
                fixed (char* passwordChars = password)
                {
                    var securePassword = new SecureString(passwordChars, password.Length);
                    securePassword.MakeReadOnly();
                    return securePassword;
                }
            }
        }
    }
}