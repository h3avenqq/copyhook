using KeyDownTester.Keys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hooks
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            HotkeysManager.SetupSystemHook();

            HotkeysManager.AddHotkey(new GlobalHotkey(ModifierKeys.Control, Key.C, () => { AddToList(); }));

            HotkeysManager.AddHotkey(ModifierKeys.Control, Key.X, () => { AddToList(); });

            Closing += MainWindow_Closing;
        }

        public void AddToList()
        {
            if (Clipboard.ContainsText() == true)
            {
                string someText = Clipboard.GetText();
                hotkeysFired.Items.Add(someText);
            }     
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            HotkeysManager.ShutdownSystemHook();
        }
    }
}

