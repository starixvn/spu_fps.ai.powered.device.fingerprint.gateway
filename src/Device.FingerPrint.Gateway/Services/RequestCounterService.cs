
namespace Device.FingerPrint.Gateway.Services
{
    public class RequestCounterService
    {
        private static readonly Lock _lock = new();
        private static RequestCounterService? _instance;
        public Int64 RequestCount { get; private set; }
        private RequestCounterService()
        {
            RequestCount = 0;
        }
        public static RequestCounterService GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new RequestCounterService();
                }
            }
            return _instance;
        }
        public void Increment()
        {
            lock (_lock)
            {
                RequestCount++;
            }
        }
    }
}
