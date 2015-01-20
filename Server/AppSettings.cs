using System;
using System.IO;
using System.Windows.Forms;

namespace JamCast
{
    public static class AppSettings
    {
        public static string TwitterConsumerKey;
        public static string TwitterConsumerSecret;
        public static string TwitterOAuthToken;
        public static string TwitterOAuthSecret;
        public static string TwitterSearchQuery;

        public static string[] SlackChannels;
        public static string ProjectorName;
        public static string SlackToken;

        public static bool EnableChat = true;
        public static bool IsPrimary;
        
        public readonly static DateTime EndTime = new DateTime(2015, 01, 25, 15, 0, 0, DateTimeKind.Local);

        static AppSettings()
        {
            if (!File.Exists("appsettings.txt"))
            {
                MessageBox.Show("The appsettings.txt file is missing!");
                Application.Exit();
            }

            try
            {
                using (var reader = new StreamReader("appsettings.txt"))
                {
                    TwitterConsumerKey = reader.ReadLine();
                    TwitterConsumerSecret = reader.ReadLine();
                    TwitterOAuthToken = reader.ReadLine();
                    TwitterOAuthSecret = reader.ReadLine();
                    TwitterSearchQuery = reader.ReadLine();
                    ProjectorName = reader.ReadLine();
                    SlackChannels = reader.ReadLine().Split(',');
                    SlackToken = reader.ReadLine();
                    IsPrimary = bool.Parse(reader.ReadLine());
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The appsettings.txt file contains incorrect configuration!");
                Application.Exit();
            }
        }
    }
}
