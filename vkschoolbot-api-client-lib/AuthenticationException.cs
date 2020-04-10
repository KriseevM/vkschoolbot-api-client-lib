using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolBotAPI
{
    class AuthenticationException : BotAPIException
    {
        public AuthenticationException(string message) : base(message)
        { }
    }
}
