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

    [DataSourceCapability(TwCap.XferCount, TwType.Int16, Get=TwOn.Range)]
    internal sealed class XferCountDataSourceCapability:DataSourceCapability {
        private short[] _value=new short[2];

        public XferCountDataSourceCapability() {
            this.Current=this.Default=-1;
            this.DefaultIndexCore=0;
            this.CurrentIndexCore=1;
        }

        public short Default {
            get {
                return this._value[0];
            }
            private set {
                this._value[0]=value;
            }
        }

        public short Current {
            get {
                return this._value[1];
            }
            private set {
                if(this._value[1]!=value) {
                    this._value[1]=value;
                    this.OnCapabilityChanged();
                }
            }
        }

        #region DataSourceCapability

        protected override object[] GetCore() {
            return this.ToRange((short)-1, short.MaxValue, (short)1, this.Default, this.Current);
        }

        protected override object[] GetValueCore(int index) {
            if(index==this.CurrentIndexCore) {
                return new object[] { this.Current };
            }
            if(index==this.DefaultIndexCore) {
                return new object[] { this.Default };
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadValue);
        }

        protected override void SetCore(object value) {
            var _val=(short)value;
            if(_val>=-1) {
                if(_val==0) {
                    this.Current=-1;
                    throw new DataSourceException(TwRC.CheckStatus, TwCC.BadValue);
                }
                this.Current=_val;
            } else {
                throw new DataSourceException(TwRC.Failure, TwCC.BadValue);
            }
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
            this.Current=this.Default;
        }

        protected override int CurrentIndexCore {
            get;
            set;
        }

        protected override int DefaultIndexCore {
            get;
            set;
        }

        public override object Value {
            get {
                return this.Current;
            }
            set {
                if(value is short) {
                    this.Current=(short)value;
                    return;
                }
                throw new InvalidOperationException();
            }
        }

        #endregion

    }
}
