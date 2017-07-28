using System.Collections.Generic;

namespace Firebase.NET.Contracts
{
    /// <summary>
    /// Request payload data specifies custom key-value pairs 
    /// </summary>
    public interface IPayload : IDictionary<string, string>
    {
    }
}
