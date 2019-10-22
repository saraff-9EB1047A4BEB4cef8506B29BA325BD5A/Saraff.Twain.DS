using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using _IoC = Saraff.IoC;

namespace Saraff.Twain.DS.IoC {

    internal sealed class _Configuration : Component, _IoC.IConfiguration {

        public Type BindServiceAttributeType => typeof(BindServiceAttribute);

        public _IoC.BindServiceCallback BindServiceCallback => (x, callback) => {
            if(x is BindServiceAttribute _attr) {
                callback(_attr.Service, _attr.ObjectType);
            }
        };

        public Type ContextBinderType => typeof(IContextBinder<,>);

        public Type ServiceRequiredAttributeType => typeof(ServiceRequiredAttribute);

        public Type ProxyRequiredAttributeType => typeof(ProxyRequiredAttribute);

        public Type ListenerType => typeof(IListener);

        public _IoC.InvokingCallback InvokingCallback => (listener, method, instance, parameters) => (listener as IListener)?.OnInvoking(method, instance, parameters);

        public _IoC.InvokedCallback InvokedCallback => (listener, method, instance, result) => (listener as IListener)?.OnInvoked(method, instance, result);

        public _IoC.CatchCallback CatchCallback => (listener, method, instance, ex) => (listener as IListener)?.OnCatch(method, instance, ex);

        public Type LazyCallbackType => typeof(Lazy<>);
    }

    public delegate T Lazy<T>() where T : class;
}
