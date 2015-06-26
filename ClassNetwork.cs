using System.Xml;

namespace LanTribune
{
    class ClassNetwork : ClassNetworkServerBroadcast 
    {
        private ClassTribune _oTribune;

        private MainWindow _oWin;

        public ClassNetwork(ClassTribune trib, MainWindow win)
        {
            _oTribune = trib;
            _oWin = win;
        }

        protected override void ServerAction(string data)
        {
            XmlDocument doc=new XmlDocument();
            doc.LoadXml(data);
            _oTribune.FromXml(doc);
            _oWin.RefreshChatBox();
        }
    }
}
