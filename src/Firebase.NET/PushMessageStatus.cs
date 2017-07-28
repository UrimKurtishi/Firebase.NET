using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Firebase.NET
{
    public enum PushMessageStatus
    {
        Success = 1,
        SuccessWithNewClientAppRegistrationId,
        FailedWithPartialRetries,
        FailedWithMaxRetries,
        InvalidSender,
        NotregisteredClientApp,
        Error,
        NULL
    }
}
