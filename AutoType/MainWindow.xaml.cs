using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using System.Windows.Forms;
using System.Diagnostics;
using AutoType.Model;
using System.Drawing;

namespace AutoType
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Dictionary<string, AutoMessage> messages = new Dictionary<string, AutoMessage>();
        private static string tempInput="";
        private static string[] validKeys = {"D0", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "D9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "NumPad0", "NumPad1", "NumPad2", "NumPad3", "NumPad4", "NumPad5", "NumPad6", "NumPad7", "NumPad8", "NumPad9" };
        private NotifyIcon oNotifyIcon;
        private System.Windows.Forms.ContextMenu trayIconMenu;
        private AutoMessageFactory factory;

        public MainWindow()
        {            
            _hookID = SetHook(_proc);
            InitializeComponent();
            this.factory = new AutoMessageFactory();
            messages = factory.loadAutoMessages();            
            this.ShowInTaskbar = false;
            this.oNotifyIcon = new NotifyIcon();
            this.oNotifyIcon.Text = "AutoType";            
            this.trayIconMenu = new System.Windows.Forms.ContextMenu();
            trayIconMenu.MenuItems.Add("Edit Messages", onClick_editWindow);
            trayIconMenu.MenuItems.Add("Exit", onClick_exitWindow);
            this.oNotifyIcon.ContextMenu = trayIconMenu;
            this.oNotifyIcon.Icon = new System.Drawing.Icon(SystemIcons.Application, 40, 40);
            this.oNotifyIcon.Visible = true;
            this.Hide();
            this.WindowState = WindowState.Minimized;
            this.loadCodesToListBox();

        }

        private void onClick_editWindow(object sender, EventArgs e) 
        {
            this.Show();
            this.WindowState = WindowState.Normal;
            this.Activate();
        }

        private void onClick_exitWindow(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            try
            {
                if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    string currentKey = ((Keys)vkCode).ToString();
                    if (currentKey.CompareTo("Space") == 0)
                    {
                        if (messages.ContainsKey(tempInput.ToLower()))
                        {
                            string strmessage = (messages[tempInput.ToLower()]).ToString();
                            string oClipData = System.Windows.Clipboard.GetText();
                            System.Windows.Clipboard.Clear();
                            System.Windows.Clipboard.SetText(strmessage);
                            string deleteEntryCommand = "";
                            for (int i = 0; i < tempInput.Length; i++)
                            {
                                deleteEntryCommand += "{BKSP}";
                            }
                            SendKeys.Flush();
                            SendKeys.SendWait(deleteEntryCommand);
                            SendKeys.Flush();    
                            SendKeys.SendWait("^v");                             
                            SendKeys.Flush();
                            
                            System.Windows.Clipboard.SetText(oClipData);
                        }
                        tempInput = "";
                    }
                    else if (currentKey.CompareTo("Back") == 0)
                    {
                        if (tempInput.Length > 0)
                            tempInput = tempInput.Remove(tempInput.Length - 1);
                    }
                    else if (!validKeys.Contains<string>(currentKey))
                    {
                        tempInput = "";
                    }

                    else
                    {
                        tempInput += currentKey[currentKey.Length - 1];
                    }
                }
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
            catch (Exception e)
            {
                return CallNextHookEx(_hookID, nCode, wParam, lParam);
            }
        }

        private void loadCodesToListBox()
        {
            this.tx_codes.ItemsSource = messages.Keys;            
        }


           

        private void tx_codes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(tx_codes.SelectedItem != null)
            {
                string selectedCode = ((System.Windows.Controls.ListBox)sender).SelectedItem.ToString();
                this.tx_message.Text = messages[selectedCode].Content;
            }            
        }

        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            if (this.tx_codes.SelectedItem != null)
            {
                string tempCode = this.tx_codes.SelectedItem.ToString();
                if (this.factory.saveMessage(messages[tempCode], this.tx_message.Text))
                {
                    messages[tempCode].Content = this.tx_message.Text;
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem trying to save the Message");
                }
            }
        }

        private void btnDeleteMessage_Click(object sender, RoutedEventArgs e)
        {
            if (this.tx_codes.SelectedItem != null)
            {
                string tempCode = this.tx_codes.SelectedItem.ToString();
                if (this.factory.deleteCode(tempCode))
                {
                    messages.Remove(tempCode);
                    tx_codes.Items.Refresh();
                    tx_message.Clear();
                }
                else
                {
                    System.Windows.MessageBox.Show("There was a problem trying to delete the Message");
                }
            }
        } 

        private void btnNewCode_Click(object sender, RoutedEventArgs e)
        {
            if (this.tx_newCode.Text.Trim().Length != 0) 
            {
                AutoMessage tempMessage = new AutoMessage(this.tx_newCode.Text,"");
                if (this.factory.addNewCode(tempMessage.Code))
                {
                    messages.Add(tempMessage.Code, tempMessage);
                    this.tx_codes.Items.Refresh();
                    this.tx_codes.SelectedIndex = this.tx_codes.Items.Count - 1;
                }
                else 
                {
                    System.Windows.MessageBox.Show("There was a problem trying to add the new code.");

                }
                this.tx_newCode.Text = "";
            }
        }  

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
            
       
    }
}
