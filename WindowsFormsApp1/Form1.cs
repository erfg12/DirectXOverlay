using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using EasyHook;
using System.Runtime.Remoting.Channels.Ipc;
using System.Runtime.Remoting;
using System.Runtime.InteropServices;
using System.IO;
using Capture.Interface;
using Capture.Hook;
using Capture;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (_captureProcess == null)
                AttachProcess("DolphinWX"); //test game is Dolphin emulator WX executable
            else
            {
                HookManager.RemoveHookedProcess(_captureProcess.Process.Id);
                _captureProcess.CaptureInterface.Disconnect();
                _captureProcess = null;
            }

            _captureProcess.CaptureInterface.DrawOverlayInGame(new Capture.Hook.Common.Overlay
            {
                Elements = new List<Capture.Hook.Common.IOverlayElement>
                {
                    new Capture.Hook.Common.TextElement(new System.Drawing.Font("Arial", 16, FontStyle.Bold)) {
                            Location = new Point(25, 25),
                            Color = Color.Red,
                            AntiAliased = true,
                            Text = "This is some test text."
                        },
                },
                Hidden = false
            });
        }

        int processId = 0;
        Process _process;
        CaptureProcess _captureProcess;
        private void AttachProcess(string proc)
        {
            string exeName = Path.GetFileNameWithoutExtension(proc);

            Process[] processes = Process.GetProcessesByName(exeName);
            foreach (Process process in processes)
            {
                if (process.MainWindowHandle == IntPtr.Zero)
                    continue;
                
                if (HookManager.IsHooked(process.Id))
                    continue;

                Direct3DVersion direct3DVersion = Direct3DVersion.AutoDetect;

                CaptureConfig cc = new CaptureConfig()
                {
                    Direct3DVersion = direct3DVersion,
                    ShowOverlay = true
                };

                processId = process.Id;
                _process = process;

                var captureInterface = new CaptureInterface();
                //captureInterface.RemoteMessage += new MessageReceivedEvent(CaptureInterface_RemoteMessage); //DEBUG
                _captureProcess = new CaptureProcess(process, cc, captureInterface);

                break;
            }
            Thread.Sleep(10);

            if (_captureProcess == null)
                MessageBox.Show("No executable found matching: '" + exeName + "'");
        }

        void CaptureInterface_RemoteMessage(MessageReceivedEventArgs message)
        {
            MessageBox.Show(message.ToString());
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            HookManager.RemoveHookedProcess(_captureProcess.Process.Id);
            _captureProcess.CaptureInterface.Disconnect();
            _captureProcess = null;
        }
    }
}
