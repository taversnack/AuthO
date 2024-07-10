namespace STSL.SmartLocker.Utils.Kiosk.App.Wrapper.Handlers.HandlerFactory;

public partial class WebRequestHandlerFactory
{
    [System.Serializable]
    public class HandlerNotFoundException : System.Exception
    {
        public HandlerNotFoundException() { }
        public HandlerNotFoundException(string message) : base(message) { }
        public HandlerNotFoundException(string message, System.Exception inner) : base(message, inner) { }
        protected HandlerNotFoundException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
