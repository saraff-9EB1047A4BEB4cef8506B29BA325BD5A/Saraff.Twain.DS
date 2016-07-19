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

    /// <summary>
    /// Provide basic functionality for a capabilities classes with a single value.
    /// </summary>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <seealso cref="Saraff.Twain.DS.DataSourceCapability" />
    public abstract class OneDataSourceCapability<TValue>:DataSourceCapability {
        private object _defaultValue=null;
        private TValue _value;

        #region DataSourceCapability

        /// <summary>
        /// Returns the Source’s Available Values for a specified capability.
        /// </summary>
        /// <returns>
        /// Available Values.
        /// </returns>
        protected override object[] GetCore() {
            return new object[] { this.Value };
        }

        /// <summary>
        /// Returns the Source’s Available Value(s) at a specified index for a specified capability.
        /// </summary>
        /// <param name="index"></param>
        /// <returns>
        /// Available Value(s).
        /// </returns>
        /// <exception cref="System.ArgumentException"></exception>
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

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The value.</param>
        protected override void SetCore(object value) {
            for(var _type=typeof(TValue); _type.IsEnum; ) {
                this.Value=Enum.ToObject(_type, value);
                return;
            }
            this.Value=(TValue)value;
        }

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="step">The step.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override void SetCore(object minValue, object maxValue, object step, object defaultValue, object currentValue) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The values.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override void SetCore(object[] value) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The values.</param>
        /// <param name="defaultIndex">The default index.</param>
        /// <param name="currentIndex">Index of the current.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override void SetCore(object[] value, int defaultIndex, int currentIndex) {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Change the Current Value of the specified capability back to its power-on value and return the
        /// new Current Value.
        /// </summary>
        protected override void ResetCore() {
            if(this._defaultValue!=null) {
                this.Value=(TValue)this._defaultValue;
            }
        }

        /// <summary>
        /// Gets or sets index of current value.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override int CurrentIndexCore {
            get {
                return 1;
            }
            set {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets index of default value.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        /// <exception cref="System.NotSupportedException"></exception>
        protected override int DefaultIndexCore {
            get {
                return 0;
            }
            set {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        /// <exception cref="System.InvalidOperationException"></exception>
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

        /// <summary>
        /// Gets or sets the internal value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
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
