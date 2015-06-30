using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace LanTribune
{
    public class ClassTribune:IEnumerable<ClassMessage>
    {
        
        private Dictionary<Guid,ClassMessage> _messages;
        public Guid Identity { get; private set; }

        private object _lock; //Trhead safe object

        public ClassTribune()
        {
            Identity = Guid.NewGuid();
            _messages=new Dictionary<Guid, ClassMessage>();
            _lock = new object();
        }


        public XmlDocument Add(ClassMessage msg)
        {
            _messages.Add(msg.Identity, msg);
            return MessageToXml(msg);
        }

        public XmlDocument Add(String msg)
        {
            ClassMessage oMsg = new ClassMessage(msg, Identity);
            _messages.Add(oMsg.Identity,oMsg);
            return MessageToXml(oMsg);
        }

        private XmlDocument MessageToXml(ClassMessage msg)
        {
            XmlDocument oXml = new XmlDocument();

            XmlDeclaration oDec = oXml.CreateXmlDeclaration("1.0", null, null);
            oXml.AppendChild(oDec);

            XmlElement oRoot = oXml.CreateElement("tribune");

            XmlElement oMsg = oXml.CreateElement("message");
            oMsg.SetAttribute("timestamp", XmlConvert.ToString(msg.TimeStamp, XmlDateTimeSerializationMode.Utc));
            oMsg.SetAttribute("identity", msg.Identity.ToString());
            oMsg.SetAttribute("senderidentity", msg.SenderIdentity.ToString());
            oMsg.InnerText = msg.Message;
            oRoot.AppendChild(oMsg);

            oXml.AppendChild(oRoot);

            return oXml;
        }

        public IEnumerator<ClassMessage> GetEnumerator()
        {
            return _messages.Values.OrderByDescending((x => x.TimeStamp)).GetEnumerator();
        }

        public override string ToString()
        {
            return _messages.OrderByDescending(x=>x.Value.TimeStamp).Aggregate("", (current, msg) => current + (msg.Value.TimeStamp.ToLongTimeString() + " " + msg.Value.Message + "\r\n"));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public XmlDocument ToXml()
        {
            XmlDocument oXml = new XmlDocument();

            XmlDeclaration oDec = oXml.CreateXmlDeclaration("1.0", null, null);
            oXml.AppendChild(oDec);

            XmlElement oRoot = oXml.CreateElement("tribune");

            foreach (ClassMessage msg in _messages.Values)
            {
                XmlElement oMsg = oXml.CreateElement("message");
                oMsg.SetAttribute("timestamp", XmlConvert.ToString(msg.TimeStamp, XmlDateTimeSerializationMode.Utc));
                oMsg.SetAttribute("identity", msg.Identity.ToString());
                oMsg.SetAttribute("senderidentity", msg.SenderIdentity.ToString());
                oMsg.InnerText = msg.Message;
                oRoot.AppendChild(oMsg);
            }

            oXml.AppendChild(oRoot);

            return oXml;
        }

        public void FromXml(XmlDocument xml)
        {
            XmlNode root = xml.SelectSingleNode("/tribune");
            if(root==null)throw new Exception("No tribune node");

            lock (_lock) //Thread safety
            {
                XmlNodeList messages = xml.SelectNodes("/tribune/message");

                if (messages != null)
                    foreach (XmlNode message in messages)
                        if (message.Attributes != null && !_messages.ContainsKey(new Guid(message.Attributes["identity"].InnerText)))
                        {
                            if (root.Attributes != null)
                                _messages.Add(new Guid(message.Attributes["identity"].InnerText),
                                    new ClassMessage(message.InnerText,
                                        XmlConvert.ToDateTime(message.Attributes["timestamp"].InnerText,
                                            XmlDateTimeSerializationMode.Local),
                                        new Guid(message.Attributes["identity"].InnerText),
                                        new Guid(message.Attributes["senderidentity"].InnerText)));
                        }
            }
        }
    }
}
