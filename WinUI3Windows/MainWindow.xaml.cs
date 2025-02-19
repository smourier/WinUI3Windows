using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace WinUI3Windows
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {

            Title = "On thread " + Environment.CurrentManagedThreadId;
            var thread = new Thread(state =>
            {
                // create a DispatcherQueue on this new thread
                var dq = DispatcherQueueController.CreateOnCurrentThread();

                // initialize xaml in it
                WindowsXamlManager.InitializeForCurrentThread();

                // create a new window
                var myOtherWindow = new MyOtherWindow(); // some other Xaml window
                myOtherWindow.AppWindow.Show(true);
                myOtherWindow.Closed += (s, e) => myOtherWindow = null;

                // send some message to the second window
                Task.Run(async () =>
                {
                    for (var i = 0; i < 10; i++)
                    {
                        await Task.Delay(1000);
                        if (myOtherWindow == null)
                            return;

                        myOtherWindow.DispatcherQueue.TryEnqueue(() =>
                        {
                            try
                            {
                                if (myOtherWindow == null)
                                    return;

                                myOtherWindow.Title = "#" + i + " on thread " + Environment.CurrentManagedThreadId;
                            }
                            catch (COMException e)
                            {
                                const int ERROR_INVALID_OPERATION = unchecked((int)0x800710DD);
                                if (e.HResult != ERROR_INVALID_OPERATION) // race condition if window was closed
                                    throw;
                            }
                        });
                    }
                });

                // run message pump
                dq.DispatcherQueue.RunEventLoop();
            })
            { IsBackground = true }; // will be destroyed when main is closed, can be changed            
            thread.Start();

        }
    }
}
