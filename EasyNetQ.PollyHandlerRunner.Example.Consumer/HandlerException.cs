namespace EasyNetQ.PollyHandlerRunner.Example.Consumer {
    using System;
    
    class HandlerException : Exception {
        public HandlerException(Int32 messageIndex) {
            MessageIndex = messageIndex;
        }

        public Int32 MessageIndex { get; }
    }
}
