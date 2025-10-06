
namespace Device.FingerPrint.Gateway.Services
{
    public class RequestCounterService
    {
        private static readonly object _lock = new object();
        private static RequestCounterService? _instance;
        private long _requestCount;
        
        public long RequestCount 
        { 
            get 
            { 
                lock (_lock) 
                { 
                    return _requestCount; 
                } 
            } 
        }
        
        private RequestCounterService()
        {
            _requestCount = 0;
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
                _requestCount++;
            }
        }
        
        public void Reset()
        {
            lock (_lock)
            {
                _requestCount = 0;
            }
        }
    }
}
