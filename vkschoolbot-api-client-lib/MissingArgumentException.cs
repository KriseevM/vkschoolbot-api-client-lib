using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolBotAPI
{
    class MissingArgumentException : BotAPIException
    {
        public MissingArgumentException(string message) : base(message)
        { }
    }
}
