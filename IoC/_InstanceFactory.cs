using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using _IoC = Saraff.IoC;

namespace Saraff.Twain.DS.IoC {

    internal sealed class _InstanceFactory:Component, IInstanceFactory {
        private _IoC.ServiceContainer _container;

        [IoC.ServiceRequired]
        public _InstanceFactory(_IoC.ServiceContainer container) {
            this._container = container;
        }

        #region IInstanceFactory

        public object CreateInstance(Type type, params CtorCallback[] args) {
            var _args = new _IoC.ServiceContainer.CtorCallback[args.Length];
            for(var i = 0; i < args.Length; i++) {
                var _index = i;
                _args[i] = x => args[_index](Delegate.CreateDelegate(typeof(CtorCallbackCore), x.Target, x.Method) as CtorCallbackCore);
            }
            return this._container.CreateInstance(type, _args);
        }

        public T CreateInstance<T>(params CtorCallback[] args) where T : class {
            return this.CreateInstance(typeof(T), args) as T;
        }

        #endregion
    }
}
