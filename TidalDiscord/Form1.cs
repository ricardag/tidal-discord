using DarkUI.Forms;
using DiscordRPC;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace TidalDiscord
    {
    public partial class Form1 : DarkForm
        {
        #region Dark Theme Window Caption
        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private static bool IsWindows10OrGreater(int build = -1)
            {
            return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
            }

        private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
            {
            if (IsWindows10OrGreater(17763))
                {
                var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
                if (IsWindows10OrGreater(18985))
                    {
                    attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
                    }

                int useImmersiveDarkMode = enabled ? 1 : 0;
                return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
                }

            return false;
            }
        #endregion

        public DiscordRpcClient? client = null;
        public string? discordUser = null;
        public string? previousMainWndTitle = null;
        public RichPresence? currentPresence = null;

        public const string MY_GUID = "8BF610C1-30F3-4E63-B5DA-F50A7EC1B66A";
        public const int UDP_PORT = 11000;
        public static IPAddress MULTICAST_IP = IPAddress.Parse("127.0.0.1");
        public static IPEndPoint ENDPOINT = new IPEndPoint(MULTICAST_IP, UDP_PORT);

        public Mutex? mutex = null;
        public Socket? socket = null;
        public static UdpClient? listener = null;
        public static Form1? Instance { get; private set; }

        private static void ReceiveCallback(IAsyncResult ar)
            {
            var e = ENDPOINT;
            byte[] receivedBytes = listener.EndReceive(ar, ref e);
            string receivedString = Encoding.ASCII.GetString(receivedBytes);

            Instance?.BroadcastMessageReceived(receivedString);

            listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);
            }

        public Form1()
            {
            Instance = this;

            // Create a socket to broadcast message (this is because hidden windows have no handle)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            listener = new UdpClient();
            listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            listener.Client.Bind(ENDPOINT);
            listener.BeginReceive(new AsyncCallback(ReceiveCallback), null);

            bool onlyInstance = false;
            mutex = new Mutex(true, $"MUTEX_{MY_GUID}", out onlyInstance);
            if (!onlyInstance)
                {
                var myself = Process.GetCurrentProcess();
                byte[] message = Encoding.UTF8.GetBytes($"{MY_GUID}-{myself.Handle}");
                socket.SendTo(message, ENDPOINT);

                Debug.WriteLine("Already running");
                Environment.Exit(1);
                }

            // Now can initialize components etc
            InitializeComponent();

            timer1.Interval = 1000;
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Enabled = true;

            try
                {
                client = new DiscordRpcClient("964216766660235294");
                currentPresence = new RichPresence()
                    {
                    Timestamps = new Timestamps()
                    };

                //Subscribe to events
                client.OnReady += (sender, e) =>
                {
                    Debug.WriteLine($"Received Ready from user '{client.CurrentUser.Username}'");
                    discordUser = client.CurrentUser.Username;
                    previousMainWndTitle = "";
                    currentPresence = null;
                };

                client.OnConnectionFailed += (sender, e) =>
                {
                    Debug.WriteLine($"Received Connection Failed from Discord {e.ToString()}");
                    discordUser = null;
                    currentPresence = null;
                };

                //Connect to the RPC
                client.Initialize();
                }
            catch (Exception ex)
                {
                Debug.WriteLine($"Could not create Discord RPC Client: {ex.Message}");
                DarkMessageBox.ShowError(ex.Message, "Could not create Discord RPC Client");
                Environment.Exit(1);
                }

            }

        private void Timer1_Tick(object? Sender, EventArgs e)
            {
            Process[] localByName = Process.GetProcessesByName("TIDAL");
            var currentMainWndTitle = localByName
                        .Where(p => p.MainWindowHandle.ToInt64() != 0)
                        .Select(p => p.MainWindowTitle)
                        .Distinct()
                        .FirstOrDefault();

            if (currentMainWndTitle == null)
                {
                lblNowPlaying.Text = "TIDAL is not running";

                if (discordUser != null)
                    {
                    if (currentPresence == null)
                        {
                        currentPresence = new RichPresence();
                        currentPresence.Timestamps = new Timestamps();
                        }

                    if (currentPresence.State != "Not running")
                        {
                        currentPresence.Timestamps.Start = null;
                        currentPresence.Details = lblNowPlaying.Text.Truncate(120);
                        currentPresence.State = "Not running";
                        client?.SetPresence(currentPresence);

                        Debug.WriteLine(lblNowPlaying.Text);
                        }
                    }
                }
            else if (previousMainWndTitle != currentMainWndTitle)
                {
                if (currentMainWndTitle != "TIDAL")
                    {
                    lblNowPlaying.Text = $"TIDAL now playing \"{currentMainWndTitle}\"";

                    if (discordUser != null)
                        {
                        if (currentPresence == null)
                            {
                            currentPresence = new RichPresence();
                            currentPresence.Timestamps = new Timestamps();
                            }

                        if (currentPresence.State != "Playing" || lblNowPlaying.Text != currentPresence.Details)
                            {
                            currentPresence.Timestamps.Start = DateTime.UtcNow;
                            currentPresence.Details = lblNowPlaying.Text.Truncate(120);
                            currentPresence.State = "Playing";
                            client?.SetPresence(currentPresence);

                            Debug.WriteLine(lblNowPlaying.Text);
                            }
                        }
                    }
                else
                    {
                    lblNowPlaying.Text = "TIDAL is idle";

                    if (discordUser != null)
                        {
                        if (currentPresence == null)
                            {
                            currentPresence = new RichPresence();
                            currentPresence.Timestamps = new Timestamps();
                            }

                        if (currentPresence.State != "Idle")
                            {
                            currentPresence.Timestamps.Start = null;
                            currentPresence.Details = lblNowPlaying.Text.Truncate(120);
                            currentPresence.State = "Idle";
                            client?.SetPresence(currentPresence);

                            Debug.WriteLine(lblNowPlaying.Text);
                            }
                        }
                    }
                }

            previousMainWndTitle = currentMainWndTitle;
            if (discordUser != null)
                lblDiscordStatus.Text = $"DISCORD user '{discordUser}'";
            else
                lblDiscordStatus.Text = "No DISCORD user detected";
            }

        private void Form1_Load(object sender, EventArgs e)
            {
            notifyIcon1.Text = "TIDAL Rich Presence";
            notifyIcon1.Visible = true;

            notifyIcon1.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon1.BalloonTipText = "TIDAL Rich Presence is running in background.";
            notifyIcon1.BalloonTipTitle = "TIDAL Rich Presence";
            notifyIcon1.ShowBalloonTip(1500);

            WindowState = FormWindowState.Minimized;
            this.Visible = false;
            this.Hide();
            }

        private void Form1_Resize(object sender, EventArgs e)
            {
            switch (this.WindowState)
                {
                case FormWindowState.Minimized:
                    this.Hide();
                    this.ShowInTaskbar = false;
                    break;
                case FormWindowState.Normal:
                    this.Show();
                    this.ShowInTaskbar = true;
                    UseImmersiveDarkMode(this.Handle, true);
                    break;
                }
            }

        private void IconClick(object sender, EventArgs e)
            {
            switch (this.WindowState)
                {
                case FormWindowState.Minimized:
                    this.Visible = true;
                    this.ShowInTaskbar = true;
                    this.WindowState = FormWindowState.Normal;
                    break;
                case FormWindowState.Normal:
                    this.Visible = false;
                    this.ShowInTaskbar = false;
                    this.WindowState = FormWindowState.Minimized;
                    break;
                }
            }

        private void Form1_Closing(object sender, FormClosingEventArgs e)
            {
            if (e.CloseReason != CloseReason.WindowsShutDown)
                {
                if (DarkMessageBox.ShowInformation("Do you really want to close the application?", "Close TIDAL Rich Presence on Discord", DarkDialogButton.YesNo) == DialogResult.No)
                    e.Cancel = true;
                else
                    mutex?.ReleaseMutex();
                }
            }

        public void BroadcastMessageReceived(string message)
            {
            var myself = Process.GetCurrentProcess();
            if (message != null && message.StartsWith(MY_GUID) && message != $"{MY_GUID}-{myself.Handle}")
                {
                // Received a broadcast message and it's not from myself
                if (this.InvokeRequired)
                    {
                    this.Invoke(delegate { BroadcastMessageReceived(message); });
                    }
                else
                    {
                    this.Visible = true;
                    this.ShowInTaskbar = true;
                    this.WindowState = FormWindowState.Normal;
                    }
                }
            }

        }
    }
