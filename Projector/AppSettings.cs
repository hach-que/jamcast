using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

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
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "JamCast",
                "projector-settings.json");

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "projector-settings.json has not been set.  Configure " +
                    "this projector in the controller software.");
                Application.Exit();
            }

            try
            {
                using (var reader = new StreamReader(path))
                {
                    var settings = JsonConvert.DeserializeObject<dynamic>(reader.ReadToEnd());
                    TwitterConsumerKey = (string) settings.TwitterConsumerKey;
                    TwitterConsumerSecret = (string) settings.TwitterConsumerSecret;
                    TwitterOAuthToken = (string) settings.TwitterOAuthToken;
                    TwitterOAuthSecret = (string) settings.TwitterOAuthSecret;
                    TwitterSearchQuery = (string) settings.TwitterSearchQuery;
                    ProjectorName = (string) settings.ProjectorName;
                    SlackChannels = ((string) settings.SlackChannels).Split(',');
                    SlackToken = (string) settings.SlackToken;
                    IsPrimary = (bool) settings.IsPrimary;
                }
            }
            catch (Exception)
            {
                MessageBox.Show(
                    "projector-settings.json is not valid.  Configure " +
                    "this projector in the controller software.");
                Application.Exit();
            }
        }
    }
}
