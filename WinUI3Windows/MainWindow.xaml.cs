using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;

namespace WinUI3Windows
{
    public sealed partial class MainWindow : Window
    {
        private MyOtherWindow _myOtherWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void myButton_Click(object sender, RoutedEventArgs e)
        {
            if (_myOtherWindow != null)
                return;

            Title = "On thread " + Environment.CurrentManagedThreadId;
            var thread = new Thread(state =>
            {
                // create a DispatcherQueue on this new thread
                var dq = DispatcherQueueController.CreateOnCurrentThread();

                // initialize xaml in it
                WindowsXamlManager.InitializeForCurrentThread();

                // create a new window
                _myOtherWindow = new MyOtherWindow(); // some other Xaml window
                _myOtherWindow.AppWindow.Show(true);

                // run message pump
                dq.DispatcherQueue.RunEventLoop();
            });
            thread.IsBackground = true; // will be destroyed when main is closed, can be changed
            thread.Start();

            // send some message to the second window
            Task.Run(async () =>
            {
                for (var i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    _myOtherWindow.DispatcherQueue.TryEnqueue(() =>
                    {
                        _myOtherWindow.Title = "#" + i + " on thread " + Environment.CurrentManagedThreadId;
                    });
                }
            });
        }
    }
}
