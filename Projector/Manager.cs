using System.Windows.Forms;

using Projector;
using Projector.Controllers;

using Timer = System.Windows.Forms.Timer;

namespace JamCast
{
    public class Manager
    {
        private IController[] _controllers;

        public PubSubController _pubSubController;

        private ClientListController _clientListController;

        private ClientSelectionController _clientSelectionController;

        private StreamController _streamController;

        public FfmpegProcessController _ffmpegProcessController;

        private TwitterRetrieveController _twitterRetrieveController;

        private TwitterProcessingController _twitterProcessingController;

        private Broadcast m_Broadcast;

        private Timer m_RefreshTimer;

        /// <summary>
        /// Starts the manager cycle.
        /// </summary>
        public void Run()
        {
            // Set up form.
            this.InitalizeBroadcast();

            // Set up controllers.
            this._controllers = new IController[] {
                this._pubSubController = new PubSubController(),
                this._clientListController = new ClientListController(),
                this._clientSelectionController = new ClientSelectionController(),
                this._streamController = new StreamController(),
                this._ffmpegProcessController = new FfmpegProcessController(this.m_Broadcast),
                this._twitterRetrieveController = new TwitterRetrieveController(), 
                this._twitterProcessingController = new TwitterProcessingController(), 
            };

            // Start timers.
            this.InitalizeTimers();
            
            // Start the application message loop.
            Application.Run();
        }
        
        /// <summary>
        /// Initalizes and shows the broadcast form.
        /// </summary>
        private void InitalizeBroadcast()
        {
            this.m_Broadcast = new Broadcast(this);
            this.m_Broadcast.Show();
            this.m_Broadcast.FormClosed += (sender, e) =>
                {
                    // Shutdown all network connections and close the app.
                    Application.Exit();
                };
        }

        /// <summary>
        /// Initalizes the refresh timer, which is used to repaint
        /// the form at 60 FPS.
        /// </summary>
        private void InitalizeTimers()
        {
            // Set up the refresh timer.
			#if PLATFORM_MACOS
			var thread = new Thread(new ThreadStart(() =>
				{
					while (true) {
						this.m_Broadcast.Invalidate();
						Thread.Sleep(1000 / 60);
					}
				}));
			thread.IsBackground = true;
			thread.Start();
			#else
            this.m_RefreshTimer = new Timer();
            this.m_RefreshTimer.Interval = 1000 / 60;
            this.m_RefreshTimer.Tick += (sender, e) =>
                {
                    foreach (var i in this._controllers)
                    {
                        i.Update(
                            _pubSubController,
                            _twitterRetrieveController,
                            _twitterProcessingController,
                            _clientListController,
                            _clientSelectionController,
                            _streamController,
                            _ffmpegProcessController);
                    }

                    this.m_Broadcast.Invalidate();
                };
            this.m_RefreshTimer.Start();
		    #endif
        }
    }
}
