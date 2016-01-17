using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Controller.TreeNode;
using GooglePubSub;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Controller.Forms
{
    public partial class JamEditForm : Form
    {
        private bool m_Deploying = false;

        public JamTreeNode JamTreeNode { get; private set; }

        private const string NewtonsoftJsonRedirect = @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""Newtonsoft.Json"" publicKeyToken=""30AD4FE6B2A6AEED"" culture=""neutral""/>
        <bindingRedirect oldVersion=""0.0.0.0-6.0.0.0"" newVersion=""6.0.0.0""/>
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>";

        public JamEditForm(JamTreeNode jamTreeNode)
        {
            JamTreeNode = jamTreeNode;
            
            InitializeComponent();

            this.Text = JamTreeNode.Jam.Name;
            this._googleCloudProjectID.Text = JamTreeNode.Jam.GoogleCloudProjectID;
            this._projectorSlackChannels.Text = JamTreeNode.Jam.ProjectorSlackChannels;
            this._projectorSlackAPIToken.Text = JamTreeNode.Jam.ProjectorSlackAPIToken;
            this._googleCloudOAuthEndpointURL.Text = JamTreeNode.Jam.GoogleCloudOAuthEndpointURL;
            this._googleCloudStorageSecret.Text = JamTreeNode.Jam.GoogleCloudStorageSecret;

            this.RefreshConnectionStatus();
        }

        public void NameChanged()
        {
            this.Text = JamTreeNode.Jam.Name;
        }

        private void _googleCloudProjectID_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.GoogleCloudProjectID = this._googleCloudProjectID.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void _googleCloudOAuthEndpointURL_TextChanged(object sender, EventArgs e)
        {
            this.JamTreeNode.Jam.GoogleCloudOAuthEndpointURL = this._googleCloudOAuthEndpointURL.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void _googleCloudStorageSecret_TextChanged(object sender, EventArgs e)
        {
            this.JamTreeNode.Jam.GoogleCloudStorageSecret = this._googleCloudStorageSecret.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void _projectorSlackChannels_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ProjectorSlackChannels = this._projectorSlackChannels.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void _projectorSlackAPIToken_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ProjectorSlackAPIToken = this._projectorSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void JamEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.JamTreeNode.Jam.GoogleCloudOAuthEndpointURL = this._googleCloudOAuthEndpointURL.Text;
            this.JamTreeNode.Jam.GoogleCloudProjectID = this._googleCloudProjectID.Text;
            this.JamTreeNode.Jam.GoogleCloudStorageSecret = this._googleCloudStorageSecret.Text;
            this.JamTreeNode.Jam.ProjectorSlackChannels = this._projectorSlackChannels.Text;
            this.JamTreeNode.Jam.ProjectorSlackAPIToken = this._projectorSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void UpdateDeployButtons()
        {
            var enabled = true;

            if (this.m_Deploying)
            {
                enabled = false;
            }
            else
            {
                if (!string.IsNullOrEmpty(this.JamTreeNode.Jam.GoogleCloudOAuthEndpointURL) &&
                    !string.IsNullOrEmpty(this.JamTreeNode.Jam.GoogleCloudProjectID) &&
                    !string.IsNullOrEmpty(this.JamTreeNode.Jam.GoogleCloudStorageSecret) &&
                    this.m_SlackConnectionStatus.BackColor == Color.DarkSeaGreen)
                {
                    enabled = true;
                }
                else
                {
                    enabled = false;
                }
            }

            if (enabled)
            {
                this.c_PackageAndDeploy.Enabled = true;
                this.c_PackageAndDeployExternal.Enabled = true;
            }
            else
            {
                this.c_PackageAndDeploy.Enabled = false;
                this.c_PackageAndDeployExternal.Enabled = false;
            }
        }

        private void c_CreateBootstrap_Click(object sender, System.EventArgs e)
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.FileName = "Bootstrap.exe";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new FileStream(sfd.FileName, FileMode.Create))
                        {
                            var bytes = BootstrapCreator.CreateCustomBootstrap(
                                _googleCloudProjectID.Text,
                                _googleCloudOAuthEndpointURL.Text);
                            writer.Write(bytes, 0, bytes.Length);
                            writer.Flush();
                        }

                        //var path = new FileInfo(sfd.FileName).Directory.FullName;

                        //foreach (var file in new FileInfo(typeof (Program).Assembly.Location).Directory.GetFiles("*.dll"))
                        //{
                        //    file.CopyTo(Path.Combine(path, file.Name), true);
                        //}

                        //foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.pdb"))
                        //{
                        //    file.CopyTo(Path.Combine(path, file.Name), true);
                        //}

                        //foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.dll.config"))
                        //{
                        //    file.CopyTo(Path.Combine(path, file.Name), true);
                        //}

                        using (var writer = new StreamWriter(sfd.FileName + ".config", false))
                        {
                            writer.Write(NewtonsoftJsonRedirect);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Failed to write one or more files to output directory.");
                    }
                }
            }
        }

        public void RefreshConnectionStatus()
        {
            var mainForm = JamTreeNode.TreeView.FindForm() as MainForm;
            var status = mainForm.PubSubController.GetConnectionStatus(JamTreeNode.Jam.Guid);
            switch (status)
            {
                case PubSubConnectionStatus.Connected:
                    this.m_SlackConnectionStatus.Text = "Connected to Google Cloud Pub/Sub!";
                    this.m_SlackConnectionStatus.BackColor = Color.DarkSeaGreen;
                    break;
                case PubSubConnectionStatus.Connecting:
                    this.m_SlackConnectionStatus.Text = "Connecting to Google Cloud Pub/Sub...";
                    this.m_SlackConnectionStatus.BackColor = Color.LightYellow;
                    break;
                case PubSubConnectionStatus.Disconnected:
                    this.m_SlackConnectionStatus.Text = "Not connected to Google Cloud Pub/Sub!";
                    this.m_SlackConnectionStatus.BackColor = Color.LightCoral;
                    break;
            }

            this.UpdateDeployButtons();
        }

        private void c_PackageAndDeploy_Click(object sender, System.EventArgs e)
        {
            var path = new FileInfo(typeof (JamEditForm).Assembly.Location).Directory.FullName;
            this.DeployPackage(path, path);
        }

        private void c_PackageAndDeployExternal_Click(object sender, System.EventArgs e)
        {
        }

        private void DeployPackage(string clientPath, string projectorPath)
        {
            var token = this._googleCloudOAuthEndpointURL.Text;
            
            var mainForm = JamTreeNode.TreeView.FindForm() as MainForm;
            var jam = this.JamTreeNode.Jam;

            var form = new PackageDeployForm();
            form.MdiParent = this.MdiParent;
            form.Show();

            var thread = new Thread(() =>
            {
                Action<int> showProgress = p =>
                {
                    try
                    {
                        form.Invoke(new Action(() =>
                        {
                            form.Progress = p;
                        }));
                    }
                    catch
                    {
                    }
                };

                showProgress(0);

                var clientDirectory = new DirectoryInfo(clientPath);
                var projectorDirectory = new DirectoryInfo(projectorPath);

                var redirectBytes = Encoding.ASCII.GetBytes(NewtonsoftJsonRedirect);

                var clientMemory = new MemoryStream();
                var clientZip = new ZipOutputStream(clientMemory);
                clientZip.SetLevel(3);

                var files = clientDirectory.GetFiles(@"Client.exe")
                    .Concat(clientDirectory.GetFiles(@"*.dll"))
                    .Concat(clientDirectory.GetFiles(@"*.pdb"))
                    .Concat(clientDirectory.GetFiles(@"*.dll.config"));
                foreach (var file in files)
                {
                    var entry = new ZipEntry(file.Name);
                    entry.DateTime = DateTime.Now;
                    entry.Size = file.Length;
                    clientZip.PutNextEntry(entry);
                    using (var reader = file.OpenRead())
                    {
                        reader.CopyTo(clientZip);
                    }
                    clientZip.CloseEntry();
                }
                var entry2 = new ZipEntry("Client.exe.config");
                entry2.DateTime = DateTime.Now;
                entry2.Size = NewtonsoftJsonRedirect.Length;
                clientZip.PutNextEntry(entry2);
                clientZip.Write(redirectBytes, 0, redirectBytes.Length);
                clientZip.CloseEntry();
                clientZip.IsStreamOwner = false;
                clientZip.Close();

                showProgress(10);

                var projectorMemory = new MemoryStream();
                var projectorZip = new ZipOutputStream(projectorMemory);
                projectorZip.SetLevel(3);

                files = projectorDirectory.GetFiles(@"Projector.exe")
                    .Concat(projectorDirectory.GetFiles(@"*.dll"))
                    .Concat(projectorDirectory.GetFiles(@"*.pdb"))
                    .Concat(projectorDirectory.GetFiles(@"*.dll.config"));
                foreach (var file in files)
                {
                    var entry = new ZipEntry(file.Name);
                    entry.DateTime = DateTime.Now;
                    entry.Size = file.Length;
                    projectorZip.PutNextEntry(entry);
                    using (var reader = file.OpenRead())
                    {
                        reader.CopyTo(projectorZip);
                    }
                    projectorZip.CloseEntry();
                }
                entry2 = new ZipEntry("Projector.exe.config");
                entry2.DateTime = DateTime.Now;
                entry2.Size = NewtonsoftJsonRedirect.Length;
                projectorZip.PutNextEntry(entry2);
                projectorZip.Write(redirectBytes, 0, redirectBytes.Length);
                projectorZip.CloseEntry();
                projectorZip.IsStreamOwner = false;
                projectorZip.Close();

                showProgress(20);

                var clientData = new byte[clientMemory.Position];
                clientMemory.Seek(0, SeekOrigin.Begin);
                clientMemory.Read(clientData, 0, clientData.Length);

                var md5 = MD5.Create();
                md5.TransformFinalBlock(clientData, 0, clientData.Length);
                var clientHash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();

                var clientUrl = 
                    UploadFile("Client.zip", clientData, p =>
                    {
                        showProgress(20 + (int)(p * 40));
                    });

                var projectorData = new byte[projectorMemory.Position];
                projectorMemory.Seek(0, SeekOrigin.Begin);
                projectorMemory.Read(projectorData, 0, projectorData.Length);

                md5 = MD5.Create();
                md5.TransformFinalBlock(projectorData, 0, projectorData.Length);
                var projectorHash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();

                var projectorUrl = 
                    UploadFile("Projector.zip", projectorData, p =>
                    {
                        showProgress(60 + (int)(p * 40));
                    });

                mainForm.Invoke(new Action(() =>
                {
                    jam.AvailableClientFile = clientUrl;
                    jam.AvailableClientVersion = clientHash;
                    jam.AvailableProjectorFile = projectorUrl;
                    jam.AvailableProjectorVersion = projectorHash;
                    jam.Save();
                    this.m_Deploying = false;
                    this.UpdateDeployButtons();
                    form.Close();

                    this.JamTreeNode.MarkComputersAsWaitingForPing();
                    mainForm.PubSubController.ScanComputers(jam.Guid);
                }));
            });
            thread.IsBackground = true;
            this.m_Deploying = true;
            this.UpdateDeployButtons();
            thread.Start();
        }

        private static OAuthToken GetOAuthTokenFromEndpoint(string currentEndpoint, string controllerSecret)
        {
            var client = new WebClient();
            var jsonResult = client.DownloadString(currentEndpoint + "?controller_secret=" + controllerSecret);
            var json = JsonConvert.DeserializeObject<dynamic>(jsonResult);
            if (!(bool)json.has_error)
            {
                var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds((double)(json.result.created + json.result.expires_in));
                return new OAuthToken
                {
                    ExpiryUtc = dtDateTime,
                    AccessToken = json.result.access_token
                };
            }

            throw new Exception("Error when retrieving access token: " + json.error);
        }

        private string UploadFile(string filename, byte[] data, Action<double> progress)
        {
            var storage = new Storage(_googleCloudProjectID.Text,
                () => GetOAuthTokenFromEndpoint(_googleCloudOAuthEndpointURL.Text, _googleCloudStorageSecret.Text));
            var unixTimestamp = (long)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            var uploadName = Path.GetFileNameWithoutExtension(filename) + "." + unixTimestamp + Path.GetExtension(filename);
            return storage.Upload(data, uploadName, progress).Result;
        }
    }
}