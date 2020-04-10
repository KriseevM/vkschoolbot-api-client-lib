using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolBotAPI
{
    class ServerException : BotAPIException
    {
        public ServerException(string message) : base(message)
        {}
    }
}
