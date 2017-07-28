using System.Threading.Tasks;

namespace Firebase.NET.Contracts
{
    public interface IPushService
    {
        Task<IMessage> PushMessage(IMessage requestMessage);
    }
}
