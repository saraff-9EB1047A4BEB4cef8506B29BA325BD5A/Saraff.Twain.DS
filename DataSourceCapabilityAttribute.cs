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

namespace Saraff.Twain.DS {

    /// <summary>
    /// Define property of a capability.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class,AllowMultiple=false,Inherited=false)]
    public sealed class DataSourceCapabilityAttribute:Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceCapabilityAttribute"/> class.
        /// </summary>
        /// <param name="capability">The capability.</param>
        /// <param name="type">The type.</param>
        public DataSourceCapabilityAttribute(TwCap capability, TwType type) {
            this.Capability=capability;
            this.Type=type;
            this.Get=TwOn.Enum;
            this.GetCurrent=this.GetDefault=TwOn.One;
            this.SupportedOperations=TwQC.Get|TwQC.GetCurrent|TwQC.GetDefault|TwQC.Set|TwQC.Reset;
        }

        /// <summary>
        /// Gets the capability constant.
        /// </summary>
        /// <value>
        /// The capability constant.
        /// </value>
        public TwCap Capability {
            get;
            private set;
        }

        /// <summary>
        /// Gets the type of capability value.
        /// </summary>
        /// <value>
        /// The data type.
        /// </value>
        public TwType Type {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a container type of capability values.
        /// </summary>
        /// <value>
        /// The container type.
        /// </value>
        public TwOn Get {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a container type of current value.
        /// </summary>
        /// <value>
        /// The container type.
        /// </value>
        public TwOn GetCurrent {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a container type of default value.
        /// </summary>
        /// <value>
        /// The container type.
        /// </value>
        public TwOn GetDefault {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the supported operations.
        /// </summary>
        /// <value>
        /// The supported operations.
        /// </value>
        public TwQC SupportedOperations {
            get;
            set;
        }
    }
}
