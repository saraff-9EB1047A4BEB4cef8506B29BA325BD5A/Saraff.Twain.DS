using System;
using System.Collections.Generic;
using System.Text;

namespace Saraff.Twain.DS.IoC {

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
    public sealed class BindServiceAttribute : Attribute {

        public BindServiceAttribute(Type service, Type obj) {
            this.Service = service;
            this.ObjectType = obj;
        }

        public Type Service {
            get;
            private set;
        }

        public Type ObjectType {
            get;
            private set;
        }
    }
}
