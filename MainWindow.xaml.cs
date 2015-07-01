using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml;

namespace LanTribune
{
    public class VisualMessage
    {
        public string Message { get; set; }
        public string Color { get; set; }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private ClassNetwork _oNet;
        private ClassTribune _oTrib;
        private object _lock;
        private Dictionary<Guid, string> _lstColor; 

        public MainWindow()
        {
            
            _oTrib=new ClassTribune();
            _oNet=new ClassNetwork(_oTrib, this);
            _lock = new object();
            _lstColor=new Dictionary<Guid, string>();

            InitializeComponent();

            Closing += OnWindowClosing;

            _oNet.LaunchServer();
        }

        public void RefreshChatBox()
        {
            lock (_lock)
            {
                Application.Current.Dispatcher.Invoke(FillChatBox);
            }
        }

        private void FillChatBox()
        {
            List<VisualMessage> items= new List<VisualMessage>();

            foreach (ClassMessage msg in _oTrib)
            {
                items.Add(new VisualMessage() { Message = msg.TimeStamp.ToLongTimeString() + " " + msg.Message, Color = (CbShowColor.IsChecked != null && CbShowColor.IsChecked.Value)?GetBgColor(msg.SenderIdentity):"white" });
            }
            if(items.Count>0)
                ChatBox.ItemsSource = items;
        }

        private string GetBgColor(Guid id)
        {
             if (_lstColor.ContainsKey(id))
            {
                return _lstColor[id];
            }
            
            Type colors = typeof(Colors);
            PropertyInfo[] colorInfo = colors.GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach ( PropertyInfo info in colorInfo)
            {
               if (!_lstColor.ContainsValue(info.Name))
               {
                  _lstColor.Add(id, info.Name);
                  return info.Name;
               }
           }

            return "white";

        }

        private void BtnSend_OnClick(object sender, RoutedEventArgs e)
        {
            AddMsg();
        }

        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            _oNet.Run = false;
        }

        private void AddMsg()
        {
            XmlDocument doc;
            doc=_oTrib.Add(TextSend.Text);

            RefreshChatBox();
            TextSend.Clear();

            _oNet.Send(doc.InnerXml);
        }

        private void TextSend_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                AddMsg();
            }
        }

        private void BtnSendAll_OnClick(object sender, RoutedEventArgs e)
        {
            XmlDocument doc;
            doc = _oTrib.ToXml();
            _oNet.Send(doc.InnerXml);
        }

        private void CbShowColor_OnCheckChange(object sender, RoutedEventArgs e)
        {
            FillChatBox();
        }

    }
}
