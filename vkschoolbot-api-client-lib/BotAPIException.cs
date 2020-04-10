using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolBotAPI
{
    class BotAPIException : Exception
    {
        public BotAPIException(string message) : base(message)
        { }
    }
}
