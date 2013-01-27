using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace JamCast
{
    public static class AppSettings
    {
        public static string ConsumerKey = "";
        public static string ConsumerSecret = "";
        public static string OAuthToken = "";
        public static string OAuthSecret = "";

        public static string StreamUsername = "";
        public static string StreamPassword = "";

        public static string MessagingUser = "";

        public static bool EnableChat = false;

        public readonly static DateTime EndTime = new DateTime(2013, 01, 27, 15, 0, 0, DateTimeKind.Local);

        static AppSettings()
        {
            try
            {
                if (File.Exists("appsettings.txt"))
                {
                    using (var reader = new StreamReader("appsettings.txt"))
                    {
                        ConsumerKey = reader.ReadLine();
                        ConsumerSecret = reader.ReadLine();
                        OAuthToken = reader.ReadLine();
                        OAuthSecret = reader.ReadLine();
                        StreamUsername = reader.ReadLine();
                        StreamPassword = reader.ReadLine();
                        MessagingUser = reader.ReadLine();
                        EnableChat = Convert.ToBoolean(reader.ReadLine());
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The appsettings.txt file contains incorrect configuration!");
            }
        }
    }
}
