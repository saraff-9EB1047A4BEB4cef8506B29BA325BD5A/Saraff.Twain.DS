using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _IoC = Saraff.IoC;

namespace Saraff.Twain.DS.IoC {

    internal sealed class _ContainerFactory : Component, IContainerFactory {
        private _IoC.ServiceContainer _container;

        public _ContainerFactory(_IoC.ServiceContainer container) => this._container = container;

        #region IContainerFactory

        public IServiceProvider Create() {
            var _nested = this._container.CreateNestedContainer();

            _nested.Bind<IoC.IInstanceFactory>(_nested.CreateInstance<IoC._InstanceFactory>(i => i("container", _nested)));
            _nested.Bind<IoC.IBinder>(_nested.CreateInstance<IoC._Binder>(i => i("container", _nested)));
            _nested.Bind<IoC.IContainerFactory>(_nested.CreateInstance<IoC._ContainerFactory>(i => i("container", _nested)));
            _nested.Bind<IServiceProvider>(_nested);

            return _nested;
        }

        #endregion
    }
}
