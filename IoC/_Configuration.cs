/* Этот файл является частью библиотеки Saraff.Twain.DS
 * © SARAFF SOFTWARE (Кирножицкий Андрей), 2015.
 * Saraff.Twain.DS - свободная программа: вы можете перераспространять ее и/или
 * изменять ее на условиях Меньшей Стандартной общественной лицензии GNU в том виде,
 * в каком она была опубликована Фондом свободного программного обеспечения;
 * либо версии 3 лицензии, либо (по вашему выбору) любой более поздней
 * версии.
 * Saraff.Twain.DS распространяется в надежде, что она будет полезной,
 * но БЕЗО ВСЯКИХ ГАРАНТИЙ; даже без неявной гарантии ТОВАРНОГО ВИДА
 * или ПРИГОДНОСТИ ДЛЯ ОПРЕДЕЛЕННЫХ ЦЕЛЕЙ. Подробнее см. в Меньшей Стандартной
 * общественной лицензии GNU.
 * Вы должны были получить копию Меньшей Стандартной общественной лицензии GNU
 * вместе с этой программой. Если это не так, см.
 * <http://www.gnu.org/licenses/>.)
 * 
 * This file is part of Saraff.Twain.DS.
 * © SARAFF SOFTWARE (Kirnazhytski Andrei), 2015.
 * Saraff.Twain.DS is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * Saraff.Twain.DS is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * You should have received a copy of the GNU Lesser General Public License
 * along with Saraff.Twain.DS. If not, see <http://www.gnu.org/licenses/>.
 * 
 * PLEASE SEND EMAIL TO:  twain@saraff.ru.
 */
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
