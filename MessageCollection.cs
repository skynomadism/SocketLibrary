using System;

namespace SocketLibrary
{
    public class MessageCollection : System.Collections.CollectionBase
    {
        public MessageCollection()
        {

        }
        public void Add(Message value)
        {
            List.Add(value);
        }
        public Message this[int index]
        {
            get
            {
                return List[index] as Message;
            }
            set
            {
                List[index] = value;
            }
        }
        public MessageCollection this[Connection connection]
        {
            get
            {
                MessageCollection collection = new MessageCollection();
                foreach (Message message in List)
                {
                    if (message.SendToOrReceivedFrom == connection)
                        collection.Add(message);
                }
                return collection;
            }
        }
    }
}
