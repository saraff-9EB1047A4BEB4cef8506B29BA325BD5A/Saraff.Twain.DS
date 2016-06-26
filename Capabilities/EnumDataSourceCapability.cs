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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Saraff.Twain.DS.Extensions;

namespace Saraff.Twain.DS.Capabilities {

    public abstract class EnumDataSourceCapability<TValue>:DataSourceCapability where TValue:IComparable {
        private int? _currentIndex=null;
        private int? _defaultIndex=null;
        private Collection<TValue> _values=null;

        #region DataSourceCapability

        protected override object[] GetCore() {
            return this.CoreValues.CastToArray();
        }

        protected override object[] GetValueCore(int index) {
            return new object[] { this.CoreValues[index] };
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
            this.SetCore(value[currentIndex]);
        }

        protected override void ResetCore() {
            this.CurrentIndexCore=this.DefaultIndexCore;
        }

        protected override int CurrentIndexCore {
            get {
                if(this._currentIndex==null) {
                    this._Fill(this.CoreValues);
                }
                return this._currentIndex??0;
            }
            set {
                this._currentIndex=value;
                this.OnCapabilityChanged();
            }
        }

        protected override int DefaultIndexCore {
            get {
                if(this._defaultIndex==null) {
                    this._Fill(this.CoreValues);
                }
                return this._defaultIndex??0;
            }
            set {
                this._currentIndex=this._defaultIndex=value;
                this.OnCapabilityChanged();
            }
        }

        public override object Value {
            get {
                if(this.CoreValues==null) {
                    return null;
                }
                return this.CoreValues[this.CurrentIndexCore];
            }
            set {
                for(var _val=value as IEnumerable<TValue>; _val!=null; ) {
                    this._Fill(_val);
                    return;
                }
                if(value is TValue) {
                    this.CurrentIndexCore=this.CoreValues.Find(val => val.CompareTo(value)==0);
                    return;
                }
                if(value is DefaultValue<TValue>) {
                    this.DefaultIndexCore=this.CoreValues.Find(val => val.CompareTo((TValue)(DefaultValue<TValue>)value)==0);
                    return;
                }
                throw new InvalidOperationException();
            }
        }

        #endregion

        protected virtual Collection<TValue> CoreValues {
            get {
                this.OnCapabilityValueNeeded();
                return this._values;
            }
            set {
                this._values=value;
            }
        }

        private void _Fill(IEnumerable<TValue> values) {
            var _vals=new Collection<TValue>();
            foreach(TValue _val in values) {
                if(!_vals.Contains(_val)) {
                    _vals.Add(_val);
                }
            }
            this.CoreValues=_vals;
        }
    }
}
