using Microsoft.UI.Xaml;

namespace WinUI3Windows
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            if (m_window != null)
                return;

            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
