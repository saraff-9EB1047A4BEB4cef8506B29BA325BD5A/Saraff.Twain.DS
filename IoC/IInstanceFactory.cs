using System;
using System.Collections.Generic;
using System.Text;

namespace Saraff.Twain.DS.IoC {

    public interface IInstanceFactory {

        object CreateInstance(Type type, params CtorCallback[] args);

        T CreateInstance<T>(params CtorCallback[] args) where T : class;
    }

    public delegate void CtorCallback(CtorCallbackCore callback);

    public delegate void CtorCallbackCore(string name, object val);
}
