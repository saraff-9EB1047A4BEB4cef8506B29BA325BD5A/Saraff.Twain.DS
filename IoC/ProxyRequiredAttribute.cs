using System;
using System.Collections.Generic;
using System.Text;

namespace Saraff.Twain.DS.IoC {

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ProxyRequiredAttribute : Attribute {
    }
}
