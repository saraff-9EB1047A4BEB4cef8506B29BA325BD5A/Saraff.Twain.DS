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
using System.Drawing;

namespace Saraff.Twain.DS.Capabilities {

    /// <summary>
    /// Define device properties.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public sealed class DevicePropertiesAttribute:Attribute {
        private static Dictionary<uint, DevicePropertiesAttribute> _current=new Dictionary<uint, DevicePropertiesAttribute>();

        /// <summary>
        /// Initializes a new instance of the <see cref="DevicePropertiesAttribute"/> class.
        /// </summary>
        /// <param name="physicalWidth">Width of the physical.</param>
        /// <param name="physicalHeight">Height of the physical.</param>
        /// <param name="nativeResolutionX">The native resolution x.</param>
        /// <param name="nativeResolutionY">The native resolution y.</param>
        public DevicePropertiesAttribute(float physicalWidth, float physicalHeight, float nativeResolutionX, float nativeResolutionY) {
            this.PhysicalWidth=physicalWidth;
            this.PhysicalHeight=physicalHeight;
            this.XNativeResolution=nativeResolutionX;
            this.YNativeResolution=nativeResolutionY;
        }

        /// <summary>
        /// Gets the height of the physical.
        /// </summary>
        /// <value>
        /// The height of the physical.
        /// </value>
        public float PhysicalHeight {
            get;
            private set;
        }

        /// <summary>
        /// Gets the width of the physical.
        /// </summary>
        /// <value>
        /// The width of the physical.
        /// </value>
        public float PhysicalWidth {
            get;
            private set;
        }

        /// <summary>
        /// Gets the a Y native resolution.
        /// </summary>
        /// <value>
        /// The y native resolution.
        /// </value>
        public float YNativeResolution {
            get;
            private set;
        }

        /// <summary>
        /// Gets the a X native resolution.
        /// </summary>
        /// <value>
        /// The x native resolution.
        /// </value>
        public float XNativeResolution {
            get;
            private set;
        }
    }
}
