#if PLATFORM_WINDOWS

using System;
using System.ComponentModel;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using Client.Properties;
using NetCast.Messages;

namespace Client
{
    public class TrayIcon
    {
        private NotifyIcon m_NotifyIcon = null;
        private Manager m_Manager = null;

        public TrayIcon(Manager manager)
        {
            this.m_Manager = manager;

            // Create the container.
            IContainer container = new Container();

            // Create the exit menu item.
            MenuItem item = new MenuItem();
            item.Index = 0;
            item.Text = "E&xit";
            item.Click += new EventHandler(item_Click);

            // Create the menu.
            ContextMenu context = new ContextMenu();
            context.MenuItems.AddRange(new MenuItem[] { item });

            // Create the notify icon.
            this.m_NotifyIcon = new NotifyIcon(container);
            this.m_NotifyIcon.Icon = Resources.TrayOff;
            this.m_NotifyIcon.ContextMenu = context;
            this.m_NotifyIcon.Text = "JamCast Client - " + manager.User;
            this.m_NotifyIcon.Visible = true;
            this.m_NotifyIcon.DoubleClick += new EventHandler(item_Click);
        }

        public Icon Icon
        {
            get { return this.m_NotifyIcon.Icon; }
            set { this.m_NotifyIcon.Icon = value; }
        }

        void item_Click(object sender, EventArgs e)
        {
            ClientServiceStoppingMessage cssm = new ClientServiceStoppingMessage(this.m_Manager.NetCast.TcpSelf);
            cssm.SendUDP(new IPEndPoint(IPAddress.Broadcast, 13000));
            this.m_NotifyIcon.Visible = false;
            Application.Exit();
        }
    }
}

#endif