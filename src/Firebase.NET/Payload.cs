using Firebase.NET.Contracts;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Firebase.NET
{
    /// <summary>
    /// CloudPayload inherits NameValueCollection.
    /// </summary>
    public class Payload : Dictionary<string, string>, IPayload
    {

    }
}
