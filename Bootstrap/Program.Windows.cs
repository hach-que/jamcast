using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace Bootstrap
{
    public static partial class Program
    {
        private static NotifyIcon _trayIcon;

        private static Icon _trayIconNormal;

        private static Icon _trayIconNoContact;

        private static Icon _trayIconDownloading;

        private static MenuItem _lastContactMenuItem;
        private static MenuItem _roleMenuItem;
        private static MenuItem _statusMenuItem;
        private static MenuItem _bootstrapVersionMenuItem;
        private static MenuItem _clientVersionMenuItem;
        private static MenuItem _projectorVersionMenuItem;
        private static MenuItem _bootstrapAvailableVersionMenuItem;
        private static MenuItem _clientAvailableVersionMenuItem;
        private static MenuItem _projectorAvailableVersionMenuItem;

        public static void PlatformTraySetup()
        {
            _trayIconNormal = ConvertEmbeddedResourceToIcon("Bootstrap.satellite-16.png");
            _trayIconNoContact = ConvertEmbeddedResourceToIcon("Bootstrap.satellite-16-nocontact.png");
            _trayIconDownloading = ConvertEmbeddedResourceToIcon("Bootstrap.satellite-16-downloading.png");

            new Thread(() =>
            {
                // Create the menu.
                var context = new ContextMenu();
                context.MenuItems.AddRange(new MenuItem[]
                {
                    new MenuItem("JamCast Bootstrap") { Enabled = false },
                    new MenuItem("-"),
					_lastContactMenuItem = new MenuItem("Last Contact: " + (LastContact == null ? "Never" : LastContact.ToString())) { Enabled = false },
					_roleMenuItem = new MenuItem("Role: " + Role) { Enabled = false },
                    _statusMenuItem = new MenuItem("Status: " + Status) { Enabled = false },
                    new MenuItem("-"),
                    _bootstrapVersionMenuItem = new MenuItem("Bootstrap Version: " + (Bootstrap == null ? "..." : Bootstrap.Version)) { Enabled = false },
                    _clientVersionMenuItem = new MenuItem("Client Version: " + (Client == null ? "..." : Client.Version)) { Enabled = false },
                    _projectorVersionMenuItem = new MenuItem("Projector Version: " + (Projector == null ? "..." : Projector.Version)) { Enabled = false },
                    new MenuItem("-"),
                    _bootstrapAvailableVersionMenuItem = new MenuItem("Bootstrap Available Version: " + (Bootstrap == null ? "..." : Bootstrap.AvailableVersion)) { Enabled = false },
                    _clientAvailableVersionMenuItem = new MenuItem("Client Available Version: " + (Client == null ? "..." : Client.AvailableVersion)) { Enabled = false },
                    _projectorAvailableVersionMenuItem = new MenuItem("Projector Available Version: " + (Projector == null ? "..." : Projector.AvailableVersion)) { Enabled = false },
                });

                _trayIcon = new NotifyIcon();
                _trayIcon.Text = "JamCast Bootstrap";
                _trayIcon.Icon = _trayIconNormal;
                _trayIcon.ContextMenu = context;
                _trayIcon.Visible = true;

                new Thread(() =>
                {
                    while (Thread.CurrentThread.IsAlive)
                    {
                        _lastContactMenuItem.Text = "Last Contact: " +
                                                    (LastContact == null ? "Never" : LastContact.ToString())
                            ;
                        _roleMenuItem.Text = "Role: " + Role;
                        _statusMenuItem.Text = "Status: " + Status;
                        _bootstrapVersionMenuItem.Text = "Bootstrap Version: " +
                                                         (Bootstrap == null ? "..." : Bootstrap.Version)
                            ;
                        _clientVersionMenuItem.Text = "Client Version: " + (Client == null ? "..." : Client.Version);
                        _projectorVersionMenuItem.Text = "Projector Version: " +
                                                         (Projector == null ? "..." : Projector.Version)
                            ;
                        _bootstrapAvailableVersionMenuItem.Text = "Bootstrap Available Version: " +
                                                         (Bootstrap == null ? "..." : Bootstrap.AvailableVersion)
                            ;
                        _clientAvailableVersionMenuItem.Text = "Client Available Version: " + (Client == null ? "..." : Client.AvailableVersion);
                        _projectorAvailableVersionMenuItem.Text = "Projector Available Version: " +
                                                         (Projector == null ? "..." : Projector.AvailableVersion)
                            ;

                        if (LastContact == null || LastContact < DateTime.Now.AddSeconds(-60))
                        {
                            _trayIcon.Icon = _trayIconNoContact;
                        }
                        else
                        {
                            if (Status.StartsWith("Updating"))
                            {
                                if (_trayIcon.Icon == _trayIconNormal)
                                {
                                    _trayIcon.Icon = _trayIconDownloading;
                                }
                                else
                                {
                                    _trayIcon.Icon = _trayIconNormal;
                                }
                            }
                            else
                            {
                                _trayIcon.Icon = _trayIconNormal;
                            }
                        }

						Thread.Sleep(500);
                    }
                })
                { IsBackground = true }.Start();

                Application.Run();
            }) { IsBackground = true }.Start();
        }

        private static Icon ConvertEmbeddedResourceToIcon(string name)
        {
            var er = Assembly.GetEntryAssembly().GetManifestResourceStream(name);
			var ms = new MemoryStream();
            ConvertToIcon(er, ms);
            ms.Seek(0, SeekOrigin.Begin);
			return new Icon(ms);
        }

        /// <summary>
        /// Converts a PNG image to a icon (ico)
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="output">The output stream</param>
        /// <param name="size">The size (16x16 px by default)</param>
        /// <param name="preserveAspectRatio">Preserve the aspect ratio</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        private static bool ConvertToIcon(Stream input, Stream output, int size = 16, bool preserveAspectRatio = false)
        {
            Bitmap inputBitmap = (Bitmap)Bitmap.FromStream(input);
            if (inputBitmap != null)
            {
                int width, height;
                if (preserveAspectRatio)
                {
                    width = size;
                    height = inputBitmap.Height / inputBitmap.Width * size;
                }
                else
                {
                    width = height = size;
                }
                Bitmap newBitmap = new Bitmap(inputBitmap, new Size(width, height));
                if (newBitmap != null)
                {
                    // save the resized png into a memory stream for future use
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        newBitmap.Save(memoryStream, ImageFormat.Png);

                        BinaryWriter iconWriter = new BinaryWriter(output);
                        if (output != null && iconWriter != null)
                        {
                            // 0-1 reserved, 0
                            iconWriter.Write((byte)0);
                            iconWriter.Write((byte)0);

                            // 2-3 image type, 1 = icon, 2 = cursor
                            iconWriter.Write((short)1);

                            // 4-5 number of images
                            iconWriter.Write((short)1);

                            // image entry 1
                            // 0 image width
                            iconWriter.Write((byte)width);
                            // 1 image height
                            iconWriter.Write((byte)height);

                            // 2 number of colors
                            iconWriter.Write((byte)0);

                            // 3 reserved
                            iconWriter.Write((byte)0);

                            // 4-5 color planes
                            iconWriter.Write((short)0);

                            // 6-7 bits per pixel
                            iconWriter.Write((short)32);

                            // 8-11 size of image data
                            iconWriter.Write((int)memoryStream.Length);

                            // 12-15 offset of image data
                            iconWriter.Write((int)(6 + 16));

                            // write image data
                            // png data must contain the whole png data file
                            iconWriter.Write(memoryStream.ToArray());

                            iconWriter.Flush();

                            return true;
                        }
                    }
                }
                return false;
            }
            return false;
        }
    }
}
