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

    public abstract class ArrayDataSourceCapability<TValue>:DataSourceCapability where TValue:IComparable {
        private Collection<TValue> _values=null;
        private Collection<TValue> _default=null;

        #region DataSourceCapability

        protected override object[] GetCore() {
            return this.CoreValues.CastToArray();
        }

        protected override object[] GetValueCore(int index) {
            switch(index) {
                case 0:
                    return this.GetCore();
                case 1:
                    return this._default.CastToArray();
            }
            throw new InvalidOperationException();
        }

        protected override void SetCore(object value) {
            this.Value=this._Cast(value);
        }

        protected override void SetCore(object minValue, object maxValue, object step, object defaultValue, object currentValue) {
            throw new NotSupportedException();
        }

        protected override void SetCore(object[] value) {
            var _result=new TValue[value.Length];
            for(var i=0; i<value.Length; i++) {
                _result[i]=this._Cast(value[i]);
            }
            this.Value=_result;
        }

        protected override void SetCore(object[] value, int defaultIndex, int currentIndex) {
            throw new NotSupportedException();
        }

        protected override void ResetCore() {
            if(this.DefaultIndexCore!=0) {
                this.CoreValues=this._default;
            }
        }

        protected override int CurrentIndexCore {
            get {
                return 0;
            }
            set {
                throw new NotSupportedException();
            }
        }

        protected override int DefaultIndexCore {
            get {
                return this._default!=null&&this._default.Count>0?1:0;
            }
            set {
                throw new NotSupportedException();
            }
        }

        public override object Value {
            get {
                return this.CoreValues;
            }
            set {
                for(var _val=value as IEnumerable<TValue>; _val!=null; ) {
                    this._Fill(_val);
                    return;
                }
                if(value is TValue) {
                    this._Fill(new TValue[] { (TValue)value });
                    return;
                }
                if(value is DefaultValue<IEnumerable<TValue>>) {
                    this._default=((DefaultValue<IEnumerable<TValue>>)value).Value.ToCollection();
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
                this.OnCapabilityChanged();
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

        private TValue _Cast(object value) {
            for(var _type=typeof(TValue); _type.IsEnum; ) {
                return (TValue)Enum.ToObject(_type, value);
            }
            return (TValue)value;
        }

    }
}
