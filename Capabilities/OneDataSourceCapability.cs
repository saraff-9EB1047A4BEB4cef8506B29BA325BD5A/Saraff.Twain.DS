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
using System.Linq;
using System.Text;

namespace Saraff.Twain.DS.Capabilities {

    public abstract class OneDataSourceCapability<TValue>:DataSourceCapability {
        private object _defaultValue=null;
        private TValue _value;

        #region DataSourceCapability

        protected override object[] GetCore() {
            return new object[] { this.Value };
        }

        protected override object[] GetValueCore(int index) {
            switch(index) {
                case 0:
                    if(this._defaultValue!=null) {
                        return new object[] { this._defaultValue };
                    }
                    return this.GetCore();
                case 1:
                    return this.GetCore();
            }
            throw new ArgumentException();
        }

        protected override void SetCore(object value) {
            for(var _type=typeof(TValue); _type.IsEnum; ) {
                this.Value=Enum.ToObject(_type, value);
                return;
            }
            this.Value=(TValue)value;
        }

        protected override void SetCore(object minValue, object maxValue, object step, object defaultValue, object currentValue) {
            throw new NotSupportedException();
        }

        protected override void SetCore(object[] value) {
            throw new NotSupportedException();
        }

        protected override void SetCore(object[] value, int defaultIndex, int currentIndex) {
            throw new NotSupportedException();
        }

        protected override void ResetCore() {
            if(this._defaultValue!=null) {
                this.Value=(TValue)this._defaultValue;
            }
        }

        protected override int CurrentIndexCore {
            get {
                return 1;
            }
            set {
                throw new NotSupportedException();
            }
        }

        protected override int DefaultIndexCore {
            get {
                return 0;
            }
            set {
                throw new NotSupportedException();
            }
        }

        public override object Value {
            get {
                return this.CoreValue;
            }
            set {
                if(value is TValue) {
                    this.CoreValue=(TValue)value;
                    this.OnCapabilityChanged();
                    return;
                }
                if(value is DefaultValue<TValue>) {
                    this._defaultValue=(TValue)(DefaultValue<TValue>)value;
                    return;
                }
                throw new InvalidOperationException();
            }
        }

        #endregion

        protected virtual TValue CoreValue {
            get {
                this.OnCapabilityValueNeeded();
                return this._value;
            }
            set {
                this._value=value;
            }
        }

    }
}
