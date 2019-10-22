using System;
using System.Collections.Generic;
using System.Text;

namespace Saraff.Twain.DS.IoC {

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class ServiceRequiredAttribute : Attribute {
    }
}
