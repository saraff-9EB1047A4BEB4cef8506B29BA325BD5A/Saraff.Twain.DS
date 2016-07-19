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
    /// Used to pass image entry (e.g. in strips) from DS to appId.
    /// </summary>
    public sealed class ImageMemXfer {

        /// <summary>
        /// Get or set how the entry is compressed.
        /// </summary>
        public TwCompression Compression {
            get;
            set;
        }

        /// <summary>
        /// Get or set number of bytes in a row of entry.
        /// </summary>
        public uint BytesPerRow {
            get;
            set;
        }

        /// <summary>
        /// Get or set number of columns.
        /// </summary>
        public uint Columns {
            get;
            set;
        }

        /// <summary>
        /// Get or set number of rows.
        /// </summary>
        public uint Rows {
            get;
            set;
        }

        /// <summary>
        /// Get or set offset from the side of the image.
        /// </summary>
        public uint XOffset {
            get;
            set;
        }

        /// <summary>
        /// Get or set offset from the top of the image.
        /// </summary>
        public uint YOffset {
            get;
            set;
        }

        /// <summary>
        /// Get or set a image data.
        /// </summary>
        public byte[] ImageData {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating that a Source done transferring the specified image.
        /// </summary>
        /// <value>
        /// <c>true</c> if a Source done transferring the specified image; otherwise, <c>false</c>.
        /// </value>
        public bool IsXferDone {
            get;
            set;
        }
    }
}
