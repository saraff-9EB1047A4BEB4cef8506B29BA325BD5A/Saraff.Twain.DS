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
using System.IO;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Provide information about current transfer environment.
    /// </summary>
    public sealed class XferEnvironment {
        private string _defaultFileName;

        internal XferEnvironment(DataSource ds) {
            this.DS=ds;

            #region MemXfer

            this.MemXferBufferSize=512*1024U;
            foreach(MemXferBufferSizeAttribute _attr in this.DS.GetType().GetCustomAttributes(typeof(MemXferBufferSizeAttribute), false)) {
                this.MemXferBufferSize=_attr.Value;
                break;
            }

            #endregion

            #region FileXfer

            this._FileXferReset();

            #endregion
        }

        internal DataSource DS {
            get;
            private set;
        }

        internal void _FileXferReset() {
            this.FileXferName=this.DefaultFileXferName;
            this.FileXferFormat=this.DefaultFileXferFormat;
            for(var _cap=this.DS[TwCap.ImageFileFormat]; _cap!=null; ) {
                this.FileXferFormat=(TwFF)_cap.Value;
                break;
            }
        }

        /// <summary>
        /// Get or set preferred buffer size for a memory transfer mode.
        /// </summary>
        public uint MemXferBufferSize {
            get;
            set;
        }

        /// <summary>
        /// Get minimum buffer size for a memory transfer mode.
        /// </summary>
        public uint MinMemXferBufferSize {
            get {
                return this.MemXferBufferSize>>1;
            }
        }

        /// <summary>
        /// Get maximum buffer size for a memory transfer mode.
        /// </summary>
        public uint MaxMemXferBufferSize {
            get {
                return this.MemXferBufferSize<<1;
            }
        }

        /// <summary>
        /// Get or set file name for a file transfer mode.
        /// </summary>
        public string FileXferName {
            get;
            set;
        }

        /// <summary>
        /// Get or set image file format for a file transfer mode.
        /// </summary>
        public TwFF FileXferFormat {
            get;
            set;
        }

        /// <summary>
        /// Get default file name for a file transfer mode.
        /// </summary>
        internal string DefaultFileXferName {
            get {
                if(this._defaultFileName==null) {
                    this._defaultFileName=Path.GetTempFileName();
                }
                return this._defaultFileName;
            }
        }

        /// <summary>
        /// Get default image file format for a file transfer mode.
        /// </summary>
        internal TwFF DefaultFileXferFormat {
            get {
                return TwFF.Tiff;
            }
        }

        /// <summary>
        /// Get the number of transfers the Source is ready to supply to the application, upon demand.
        /// </summary>
        public ushort PendingXfers {
            get;
            set;
        }

        /// <summary>
        /// Get or set information about the current image that has just been transferred.
        /// </summary>
        public ImageInfo ImageInfo {
            get;
            set;
        }
    }
}
