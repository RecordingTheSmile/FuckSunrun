using System;
namespace FuckSunrun.Exceptions
{
    public class BusinessException:Exception
    {
        public int Code { get; private set; }

        public BusinessException(string message):base(message)
        {
            Code = 503;
        }

        public BusinessException(string message,int code) : base(message)
        {
            Code = code;
        }
    }
}

