using System;

namespace LanTribune
{
    public class ClassMessage
    {
        public Guid Identity { get; private set; }
        public string Message { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public Guid SenderIdentity { get; private set; }
        
        public ClassMessage(string mess, DateTime ts, Guid id, Guid sender)
        {
            Identity = id;
            Message = mess;
            TimeStamp = ts;
            SenderIdentity = sender;
        }


        public ClassMessage(string mess, Guid sender)
        {
            Identity = Guid.NewGuid();
            Message = mess;
            TimeStamp = DateTime.Now;
            SenderIdentity = sender;
        }
    }
}
