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
