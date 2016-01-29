using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace Client
{
    public partial class AuthForm : Form
    {
        public AuthForm()
        {
            InitializeComponent();
        }

        private void _login_Click(object sender, System.EventArgs e)
        {
            _emailAddress.Enabled = false;
            _password.Enabled = false;
            _login.Enabled = false;
            _login.Text = "Logging in...";

            var emailAddress = _emailAddress.Text;
            var password = _password.Text;

            Task.Run(async () =>
            {
                try
                {
                    var authenticated = await Authenticate(emailAddress, password);
                    if (authenticated.IsValid)
                    {
                        AuthResult = authenticated;
                        this.Invoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                    }
                    else
                    {
                        this.Invoke(new Action(() =>
                        {
                            _emailAddress.Enabled = true;
                            _password.Enabled = true;
                            _login.Enabled = true;
                            _login.Text = "Login";
                            _error.Text = authenticated.Error;
                        }));
                    }
                }
                catch (Exception ex)
                {
                    this.Invoke(new Action(() =>
                    {
                        _error.Text = ex.Message;
                    }));
                }
                finally
                {
                    this.Invoke(new Action(() =>
                    {
                        _emailAddress.Enabled = true;
                        _password.Enabled = true;
                        _login.Enabled = true;
                        _login.Text = "Login";
                    }));
                }
            });
        }

        public AuthInfo AuthResult { get; private set; }

        private async Task<AuthInfo> Authenticate(string emailAddress, string password)
        {
            var url = @"http://melbggj16.info/jamcast/authenticate/api";
            var client = new WebClient();

            var result = await client.UploadValuesTaskAsync(url, "POST", new NameValueCollection
            {
                {"email", emailAddress},
                {"password", password}
            });

            var resultParsed = JsonConvert.DeserializeObject<dynamic>(Encoding.ASCII.GetString(result));
            if ((bool)resultParsed.has_error)
            {
                return new AuthInfo
                {
                    IsValid = false,
                    Error = (string) resultParsed.error
                };
            }
            else
            {
                return new AuthInfo
                {
                    IsValid = true,
                    FullName = (string) resultParsed.result.fullName,
                    EmailAddress = (string)resultParsed.result.email,
                };
            }
        }

        private void _emailAddress_KeyUp(object sender, KeyEventArgs e)
        {
            if (_emailAddress.Enabled)
            {
                UpdateLoginButton();
            }
        }

        private void _password_KeyUp(object sender, KeyEventArgs e)
        {
            if (_emailAddress.Enabled)
            {
                UpdateLoginButton();
            }
        }

        private void UpdateLoginButton()
        {
            _login.Enabled = (_emailAddress.Text.Length > 0 &&
                              _password.Text.Length > 0);
        }
    }
}