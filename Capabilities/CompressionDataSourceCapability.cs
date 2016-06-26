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
using System.Diagnostics;

namespace Saraff.Twain.DS.Capabilities {

    //ICAP_COMPRESSION All MSG_GET* operations required
    [DataSourceCapability(TwCap.ICompression, TwType.UInt16, SupportedOperations=TwQC.Get|TwQC.GetCurrent|TwQC.GetDefault|TwQC.Set|TwQC.Reset, Get=TwOn.Enum)]
    internal sealed class CompressionDataSourceCapability:EnumDataSourceCapability<TwCompression> {

        #region DataSourceCapability

        protected override object[] GetValueCore(int index) {
            if(((TwSX)this.DS[TwCap.IXferMech].Value)==TwSX.Native) {
                return new object[] { TwCompression.None };
            }
            return base.GetValueCore(index);
        }

        protected override int CurrentIndexCore {
            get {
                if(((TwSX)this.DS[TwCap.IXferMech].Value)==TwSX.Native) {
                    return 0;
                }
                return base.CurrentIndexCore;
            }
            set {
                base.CurrentIndexCore=value;
            }
        }

        protected override int DefaultIndexCore {
            get {
                if(((TwSX)this.DS[TwCap.IXferMech].Value)==TwSX.Native) {
                    return 0;
                }
                return base.DefaultIndexCore;
            }
            set {
                base.DefaultIndexCore=value;
            }
        }

        protected override TwQC SupportedOperationsCore {
            get {
                if(((TwSX)this.DS[TwCap.IXferMech].Value)==TwSX.Native) {
                    return TwQC.Get|TwQC.GetCurrent|TwQC.GetDefault;
                }
                return base.SupportedOperationsCore;
            }
        }

        #endregion

        #region EnumDataSourceCapability

        protected override Collection<TwCompression> CoreValues {
            get {
                if(((TwSX)this.DS[TwCap.IXferMech].Value)==TwSX.Native) {
                    return new Collection<TwCompression> { TwCompression.None };
                }
                var _result=base.CoreValues;
                if(_result==null) {
                    foreach(CompressionAttribute _attr in this.DS.GetType().GetCustomAttributes(typeof(CompressionAttribute), false)) {
                        _result=_attr.Compression.ToCollection();
                        break;
                    }
                    this.CoreValues=_result=_result??new Collection<TwCompression> { TwCompression.None };
                }
                return _result;
            }
            set {
                if(!value.Contains(TwCompression.None)) {
                    value.Add(TwCompression.None);
                }
                base.CoreValues=value;
                this.Value=(DefaultValue<TwCompression>)TwCompression.None;
            }
        }

        #endregion
    }
}
