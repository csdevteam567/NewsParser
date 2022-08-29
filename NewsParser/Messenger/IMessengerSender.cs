using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsParser.Messenger
{
    public interface IMessengerSender
    {
        void SendMessage(Message message);
    }
}
