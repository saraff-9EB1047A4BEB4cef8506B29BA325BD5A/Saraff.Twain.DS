using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using _IoC = Saraff.IoC;

namespace Saraff.Twain.DS.IoC {

    internal sealed class _Binder:Component, IBinder {
        private _IoC.ServiceContainer _container;

        [IoC.ServiceRequired]
        public _Binder(_IoC.ServiceContainer container) => this._container = container;

        #region IBinder

        public void Bind(Type service, Type obj) => this._container.Bind(service, obj);

        public void Bind(Type service, object obj) => this._container.Bind(service, obj);

        public void Bind(Type service, object obj, bool addToContainer) {
            this._container.Bind(service,obj);
            if(addToContainer) {
                this._container.Add(obj as IComponent);
            }
        }

        public void Bind<TService>(object obj) => this._container.Bind<TService>(obj);

        public void Bind<TService>(object obj, bool addToContainer) {
            this._container.Bind<TService>(obj);
            if(addToContainer) {
                this._container.Add(obj as IComponent);
            }
        }

        public void Bind<TService, T>() => this._container.Bind<TService, T>();

        public void Load(Assembly assembly) => this._container.Load(assembly);

        #endregion
    }
}
