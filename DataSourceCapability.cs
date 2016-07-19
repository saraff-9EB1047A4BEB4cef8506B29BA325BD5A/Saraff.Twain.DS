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
using System.Diagnostics;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Provide basic functionality for a capabilities classes.
    /// </summary>
    [DebuggerDisplay("{this.CapabilityInfo.Capability}; SupportedOperationsCore = {this.SupportedOperationsCore}; Get = {this.CapabilityInfo.Get}; GetCurrent = {this.CapabilityInfo.GetCurrent}; GetDefault = {this.CapabilityInfo.GetDefault};")]
    public abstract class DataSourceCapability {
        private DataSourceCapabilityAttribute _info;
        private bool _suppressEvents=false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceCapability"/> class.
        /// </summary>
        protected DataSourceCapability() {
        }

        #region Internal

        internal static DataSourceCapability Create(Type type,DataSource ds) {
            for(var _instance=Activator.CreateInstance(type, true) as DataSourceCapability; _instance!=null; ) {
                _instance.DS=ds;
                return _instance;
            }
            return null;
        }

        internal TwQC SupportedOperations {
            get {
                return this.SupportedOperationsCore;
            }
        }

        internal TwCapability Get() {
            return this._ToTwCapability(this.GetCore(), this.CapabilityInfo.Get);
        }

        internal TwCapability GetCurrent() {
            if(this.CapabilityInfo.GetCurrent==TwOn.One||this.CapabilityInfo.GetCurrent==TwOn.Array) {
                return this._ToTwCapability(this.GetValueCore(this.CurrentIndex), this.CapabilityInfo.GetCurrent);
            }
            throw new InvalidOperationException("Недопустимый тип контейнера.");
        }

        internal TwCapability GetDefault() {
            if(this.CapabilityInfo.GetDefault==TwOn.One||this.CapabilityInfo.GetDefault==TwOn.Array) {
                return this._ToTwCapability(this.GetValueCore(this.DefaultIndex), this.CapabilityInfo.GetDefault);
            }
            throw new InvalidOperationException("Недопустимый тип контейнера.");
        }

        internal void Set(TwCapability cap) {
            try {
                switch(cap.ConType) {
                    case TwOn.One:
                        var _genericValue=cap.GetValue();
                        var _oneValue=_genericValue as TwOneValue;
                        if(_oneValue!=null) {
                            this.SetCore(TwTypeHelper.CastToCommon(_oneValue.ItemType, TwTypeHelper.ValueToTw<uint>(_oneValue.ItemType, _oneValue.Item)));
                        } else {
                            this.SetCore(_genericValue.ToString());
                        }
                        break;
                    case TwOn.Range:
                        for(var _range=cap.GetValue() as TwRange; _range!=null; _range=null) {
                            this.SetCore(
                                TwTypeHelper.CastToCommon(_range.ItemType, TwTypeHelper.ValueToTw<uint>(_range.ItemType, _range.MinValue)),
                                TwTypeHelper.CastToCommon(_range.ItemType, TwTypeHelper.ValueToTw<uint>(_range.ItemType, _range.MaxValue)),
                                TwTypeHelper.CastToCommon(_range.ItemType, TwTypeHelper.ValueToTw<uint>(_range.ItemType, _range.StepSize)),
                                TwTypeHelper.CastToCommon(_range.ItemType, TwTypeHelper.ValueToTw<uint>(_range.ItemType, _range.DefaultValue)),
                                TwTypeHelper.CastToCommon(_range.ItemType, TwTypeHelper.ValueToTw<uint>(_range.ItemType, _range.CurrentValue)));
                        }
                        break;
                    case TwOn.Array:
                        for(var _array=cap.GetValue() as __ITwArray; _array!=null; _array=null) {
                            this.SetCore(_array.Items);
                        }
                        break;
                    case TwOn.Enum:
                        for(var _enum=cap.GetValue() as __ITwEnumeration; _enum!=null; _enum=null) {
                            this.SetCore(_enum.Items, _enum.DefaultIndex, _enum.CurrentIndex);
                        }
                        break;
                    default:
                        throw new DataSourceException(TwRC.Failure, TwCC.BadValue);
                }
            } catch(Exception ex) {
                throw new DataSourceException(TwRC.Failure, TwCC.BadValue, ex.Message);
            }
        }

        internal void Reset() {
            this.ResetCore();
        }

        internal int CurrentIndex {
            get {
                return this.CurrentIndexCore;
            }
        }

        internal int DefaultIndex {
            get {
                return this.DefaultIndexCore;
            }
        }

        #endregion

        /// <summary>
        /// Gets the capability information.
        /// </summary>
        /// <value>
        /// The capability information.
        /// </value>
        /// <exception cref="System.InvalidOperationException"></exception>
        public DataSourceCapabilityAttribute CapabilityInfo {
            get {
                if(this._info==null) {
                    var _attrs=this.GetType().GetCustomAttributes(typeof(DataSourceCapabilityAttribute), false);
                    if(_attrs.Length>0) {
                        this._info=_attrs[0] as DataSourceCapabilityAttribute;
                    } else {
                        throw new InvalidOperationException(string.Format("Тип \"{0}\" не отмечен атрибутом \"{1}\".", this.GetType().FullName, typeof(DataSourceCapabilityAttribute).FullName));
                    }
                }
                return this._info;
            }
        }

        /// <summary>
        /// Occurs when a capability value changed.
        /// </summary>
        public event EventHandler<CapabilityEventArgs> CapabilityChanged;

        /// <summary>
        /// Occurs when a capability value needed.
        /// </summary>
        public event EventHandler<CapabilityEventArgs> CapabilityValueNeeded;

        #region Core

        /// <summary>
        /// Get related instance of <see cref="DataSource"/>.
        /// </summary>
        /// <value>
        /// The Data Source.
        /// </value>
        protected DataSource DS {
            get;
            private set;
        }

        /// <summary>
        /// Returns the Source’s Available Values for a specified capability.
        /// </summary>
        /// <returns>Available Values.</returns>
        protected abstract object[] GetCore();

        /// <summary>
        /// Returns the Source’s Available Value(s) at a specified index for a specified capability.
        /// </summary>
        /// <returns>Available Value(s).</returns>
        protected abstract object[] GetValueCore(int index);

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The value.</param>
        protected abstract void SetCore(object value);

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="step">The step.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="currentValue">The current value.</param>
        protected abstract void SetCore(object minValue, object maxValue, object step, object defaultValue, object currentValue);

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The values.</param>
        protected abstract void SetCore(object[] value);

        /// <summary>
        /// Changes the Current Value of the capability to that specified by the application.
        /// </summary>
        /// <param name="value">The values.</param>
        /// <param name="defaultIndex">The default index.</param>
        /// <param name="currentIndex">Index of the current.</param>
        protected abstract void SetCore(object[] value, int defaultIndex, int currentIndex);

        /// <summary>
        /// Change the Current Value of the specified capability back to its power-on value and return the
        /// new Current Value.
        /// </summary>
        protected abstract void ResetCore();

        /// <summary>
        /// Returns the Source’s support status of this capability.
        /// </summary>
        /// <value>
        /// The supported operations.
        /// </value>
        protected virtual TwQC SupportedOperationsCore {
            get {
                return this.CapabilityInfo.SupportedOperations;
            }
        }

        /// <summary>
        /// Gets or sets index of current value.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        protected abstract int CurrentIndexCore {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets index of default value.
        /// </summary>
        /// <value>
        /// The index.
        /// </value>
        protected abstract int DefaultIndexCore {
            get;
            set;
        }

        #endregion

        private TwCapability _ToTwCapability(object[] data, TwOn container) {
            switch(container) {
                case TwOn.One:
                    if(data[0] is string) {
                        return new TwCapability(this.CapabilityInfo.Capability, data[0] as string, this.CapabilityInfo.Type);
                    } else {
                        return new TwCapability(this.CapabilityInfo.Capability, TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[0])), this.CapabilityInfo.Type);
                    }
                case TwOn.Range: //object[] {<MinValue>,<StepSize>,<DefaultValue>,<CurrentValue>,<MaxValue>}
                    return new TwCapability(
                        this.CapabilityInfo.Capability,
                        new TwRange {
                            ItemType=this.CapabilityInfo.Type,
                            DefaultValue=TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[2])),
                            CurrentValue=TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[3])),
                            MinValue=TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[0])),
                            MaxValue=TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[data.Length-1])),
                            StepSize=TwTypeHelper.ValueFromTw<uint>(TwTypeHelper.CastToTw(this.CapabilityInfo.Type, data[1]))
                        });
                case TwOn.Array:
                    return new TwCapability(
                        this.CapabilityInfo.Capability,
                        new TwArray {
                            ItemType=this.CapabilityInfo.Type,
                            NumItems=(uint)data.Length
                        },
                        data);
                case TwOn.Enum:
                    return new TwCapability(
                        this.CapabilityInfo.Capability,
                        new TwEnumeration {
                            ItemType=this.CapabilityInfo.Type,
                            NumItems=(uint)data.Length,
                            DefaultIndex=(uint)this.DefaultIndex,
                            CurrentIndex=(uint)this.CurrentIndex
                        },
                        data);
            }
            throw new DataSourceException(TwRC.Failure, TwCC.CapSeqError);
        }

        /// <summary>
        /// Pack values to the range array.
        /// </summary>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="step">The step.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="currentValue">The current value.</param>
        /// <returns>Array of values.</returns>
        protected object[] ToRange(object minValue, object maxValue, object step, object defaultValue, object currentValue) {
            return new object[] { minValue, step, defaultValue, currentValue, maxValue };
        }

        /// <summary>
        /// Invoked when the capability value changed.
        /// </summary>
        protected void OnCapabilityChanged() {
            if(this.CapabilityChanged!=null&&!this._suppressEvents) {
                this.CapabilityChanged(this, new CapabilityEventArgs(this));
            }
        }

        /// <summary>
        /// Invoked when the capability value needed.
        /// </summary>
        protected void OnCapabilityValueNeeded() {
            if(!this._suppressEvents) {
                this._suppressEvents=true;
                try {
                    if(this.CapabilityValueNeeded!=null) {
                        this.CapabilityValueNeeded(this, new CapabilityEventArgs(this));
                    }
                } finally {
                    this._suppressEvents=false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public abstract object Value {
            get;
            set;
        }
    }
}
