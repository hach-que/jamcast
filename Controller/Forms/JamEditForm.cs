using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Controller.TreeNode;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;

namespace Controller.Forms
{
    public partial class JamEditForm : Form
    {
        private bool m_Deploying = false;

        public JamTreeNode JamTreeNode { get; private set; }

        public JamEditForm(JamTreeNode jamTreeNode)
        {
            JamTreeNode = jamTreeNode;
            
            InitializeComponent();

            this.Text = JamTreeNode.Jam.Name;
            this.c_ControllerSlackAPIToken.Text = JamTreeNode.Jam.ControllerSlackToken;
            this.c_ProjectorSlackChannels.Text = JamTreeNode.Jam.ProjectorSlackChannels;
            this.c_ClientSlackAPIToken.Text = JamTreeNode.Jam.ClientSlackToken;
            this.c_ControllerStorageAPIToken.Text = JamTreeNode.Jam.ControllerSlackStorageToken;

            this.RefreshConnectionStatus();
        }

        public void NameChanged()
        {
            this.Text = JamTreeNode.Jam.Name;
        }

        private void c_SlackAPIToken_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ControllerSlackToken = this.c_ControllerSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_ControllerStorageAPIToken_TextChanged(object sender, EventArgs e)
        {
            this.JamTreeNode.Jam.ControllerSlackStorageToken = this.c_ControllerStorageAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_ProjectorSlackChannels_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ProjectorSlackChannels = this.c_ProjectorSlackChannels.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void c_ClientSlackAPIToken_TextChanged(object sender, System.EventArgs e)
        {
            this.JamTreeNode.Jam.ClientSlackToken = this.c_ClientSlackAPIToken.Text;
            this.JamTreeNode.Jam.Save();
        }

        private void JamEditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.JamTreeNode.Jam.ControllerSlackStorageToken = this.c_ControllerStorageAPIToken.Text;
            this.JamTreeNode.Jam.ControllerSlackToken = this.c_ControllerSlackAPIToken.Text;
            this.JamTreeNode.Jam.ProjectorSlackChannels = this.c_ProjectorSlackChannels.Text;
            this.JamTreeNode.Jam.ClientSlackToken = this.c_ClientSlackAPIToken.Text;
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
                if (!string.IsNullOrEmpty(this.JamTreeNode.Jam.ControllerSlackStorageToken) &&
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
                    using (var writer = new FileStream(sfd.FileName, FileMode.Create))
                    {
                        var bytes = BootstrapCreator.CreateCustomBootstrap(this.c_ClientSlackAPIToken.Text);
                        writer.Write(bytes, 0, bytes.Length);
                        writer.Flush();
                    }

                    var path = new FileInfo(sfd.FileName).Directory.FullName;

                    foreach (var file in new FileInfo(typeof (Program).Assembly.Location).Directory.GetFiles("*.dll"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.pdb"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    foreach (var file in new FileInfo(typeof(Program).Assembly.Location).Directory.GetFiles("*.dll.config"))
                    {
                        file.CopyTo(Path.Combine(path, file.Name), true);
                    }

                    using (var writer = new StreamWriter(sfd.FileName + ".config", false))
                    {
                        writer.Write(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<configuration>
  <runtime>
    <assemblyBinding xmlns=""urn:schemas-microsoft-com:asm.v1"">
      <dependentAssembly>
        <assemblyIdentity name=""Newtonsoft.Json"" publicKeyToken=""30AD4FE6B2A6AEED"" culture=""neutral""/>
        <bindingRedirect oldVersion=""0.0.0.0-6.0.0.0"" newVersion=""6.0.0.0""/>
      </dependentAssembly>
    </assemblyBinding>
  </runtime>
</configuration>");
                    }
                }
            }
        }

        public void RefreshConnectionStatus()
        {
            var mainForm = JamTreeNode.TreeView.FindForm() as MainForm;
            var status = mainForm.SlackController.GetConnectionStatus(JamTreeNode.Jam.Guid);
            switch (status)
            {
                case SlackConnectionStatus.Connected:
                    this.m_SlackConnectionStatus.Text = "Connected to Slack!";
                    this.m_SlackConnectionStatus.BackColor = Color.DarkSeaGreen;
                    break;
                case SlackConnectionStatus.Connecting:
                    this.m_SlackConnectionStatus.Text = "Connecting to Slack...";
                    this.m_SlackConnectionStatus.BackColor = Color.LightYellow;
                    break;
                case SlackConnectionStatus.Disconnected:
                    this.m_SlackConnectionStatus.Text = "Not connected to Slack!";
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
            var token = this.c_ControllerStorageAPIToken.Text;
            
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
                clientZip.IsStreamOwner = false;
                clientZip.Close();

                showProgress(10);

                var projectorMemory = new MemoryStream();
                var projectorZip = new ZipOutputStream(projectorMemory);
                projectorZip.SetLevel(3);

                files = clientDirectory.GetFiles(@"Projector.exe")
                    .Concat(clientDirectory.GetFiles(@"*.dll"))
                    .Concat(clientDirectory.GetFiles(@"*.pdb"))
                    .Concat(clientDirectory.GetFiles(@"*.dll.config"));
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
                projectorZip.IsStreamOwner = false;
                projectorZip.Close();

                showProgress(20);

                var clientData = new byte[clientMemory.Position];
                clientMemory.Seek(0, SeekOrigin.Begin);
                clientMemory.Read(clientData, 0, clientData.Length);

                var md5 = MD5.Create();
                md5.TransformFinalBlock(clientData, 0, clientData.Length);
                var clientHash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();

                var clientResponse = JsonConvert.DeserializeObject<dynamic>(
                    UploadFile("https://slack.com/api/files.upload", token, "Client.zip", clientData, p =>
                    {
                        showProgress(20 + (int)(p * 40));
                    }));

                var projectorData = new byte[projectorMemory.Position];
                projectorMemory.Seek(0, SeekOrigin.Begin);
                projectorMemory.Read(projectorData, 0, projectorData.Length);

                md5 = MD5.Create();
                md5.TransformFinalBlock(projectorData, 0, projectorData.Length);
                var projectorHash = BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();

                var projectorResponse = JsonConvert.DeserializeObject<dynamic>(
                    UploadFile("https://slack.com/api/files.upload", token, "Projector.zip", projectorData, p =>
                    {
                        showProgress(60 + (int)(p * 40));
                    }));

                mainForm.Invoke(new Action(() =>
                {
                    jam.AvailableClientFile = clientResponse.file.url_download;
                    jam.AvailableClientVersion = clientHash;
                    jam.AvailableProjectorFile = projectorResponse.file.url_download;
                    jam.AvailableProjectorVersion = projectorHash;
                    jam.Save();
                    this.m_Deploying = false;
                    this.UpdateDeployButtons();
                    form.Close();

                    this.JamTreeNode.MarkComputersAsWaitingForPing();
                    mainForm.SlackController.ScanComputers(jam.Guid);
                }));
            });
            thread.IsBackground = true;
            this.m_Deploying = true;
            this.UpdateDeployButtons();
            thread.Start();
        }

        private void c_StorageTokenLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://api.slack.com/web");
        }

        private string UploadFile(string url, string token, string filename, byte[] data, Action<double> progress)
        {
            var boundary = "----------------------------" +
                           DateTime.Now.Ticks.ToString("x");
            var httpRequest = (HttpWebRequest) WebRequest.Create(url);
            httpRequest.ContentType = @"multipart/form-data; boundary=" + boundary;
            httpRequest.Method = @"POST";
            httpRequest.KeepAlive = true;
            httpRequest.AllowWriteStreamBuffering = false;

            httpRequest.Credentials = CredentialCache.DefaultCredentials;

            var memory = new MemoryStream();

            var boundarybytes = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "\r\n");
            memory.Write(boundarybytes, 0, boundarybytes.Length);

            var headerbytes = System.Text.Encoding.UTF8.GetBytes(string.Format(
                "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
                "token",
                token));
            memory.Write(headerbytes, 0, headerbytes.Length);

            boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            memory.Write(boundarybytes, 0, boundarybytes.Length);

            headerbytes = System.Text.Encoding.UTF8.GetBytes(string.Format(
                "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: application/octet-stream\r\n\r\n",
                "file",
                filename));
            memory.Write(headerbytes, 0, headerbytes.Length);
            memory.Write(data, 0, data.Length);

            boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            memory.Write(boundarybytes, 0, boundarybytes.Length);

            httpRequest.ContentLength = memory.Length;

            var requestStream = httpRequest.GetRequestStream();

            var copy = new byte[memory.Position];
            memory.Position = 0;
            memory.Read(copy, 0, copy.Length);
            var datacopy = Encoding.ASCII.GetString(copy);

            memory.Position = 0;

            int bytesRead = 0;
            long bytesSoFar = 0;
            byte[] buffer = new byte[10240];
            while ((bytesRead = memory.Read(buffer, 0, buffer.Length)) != 0)
            {
                bytesSoFar += bytesRead;
                requestStream.Write(buffer, 0, bytesRead);
                progress(bytesSoFar/(double)memory.Length);
            }

            requestStream.Close();
            var webResponse2 = httpRequest.GetResponse();

            var responseStream = webResponse2.GetResponseStream();
            var responseReader = new StreamReader(responseStream);
            var result = responseReader.ReadToEnd();

            webResponse2.Close();
            return result;
        }
    }
}