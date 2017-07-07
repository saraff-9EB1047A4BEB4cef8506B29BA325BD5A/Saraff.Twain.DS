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
    /// Information of a image. Описание изображения.
    /// </summary>
    public sealed class ImageInfo {

        /// <summary>
        /// Get or set resolution in the horizontal.
        /// </summary>
        public float XResolution {
            get;
            set;
        }

        /// <summary>
        /// Get or set resolution in the vertical.
        /// </summary>
        public float YResolution {
            get;
            set;
        }

        /// <summary>
        /// Get or set columns in the image, -1 if unknown by DS.
        /// </summary>
        public int ImageWidth {
            get;
            set;
        }

        /// <summary>
        /// Get or set rows in the image, -1 if unknown by DS.
        /// </summary>
        public int ImageLength {
            get;
            set;
        }

        /// <summary>
        /// Get or set number of bits for each sample.
        /// </summary>
        public short[] BitsPerSample {
            get;
            set;
        }

        /// <summary>
        /// Get or set number of bits for each padded pixel.
        /// </summary>
        public short BitsPerPixel {
            get;
            set;
        }

        /// <summary>
        /// Get or set <c>true</c> if Planar, <c>false</c> if chunky.
        /// </summary>
        public bool Planar {
            get;
            set;
        }

        /// <summary>
        /// Get or set pixel type of image.
        /// </summary>
        public TwPixelType PixelType {
            get;
            set;
        }

        /// <summary>
        /// Get or set how the data is compressed.
        /// </summary>
        public TwCompression Compression {
            get;
            set;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Saraff.Twain.DS.ImageInfo"/> to <see cref="Saraff.Twain.DS.TwImageInfo"/>.
        /// </summary>
        /// <returns>The TwImageInfo.</returns>
        internal TwImageInfo ToTwImageInfo() {
            return new TwImageInfo {
                BitsPerPixel=this.BitsPerPixel,
                BitsPerSample=new Func<short[]>(() => {
                    var _result=new short[8];
                    for(var i=0; i<8&&i<this.BitsPerSample.Length; i++) {
                        _result[i]=this.BitsPerSample[i];
                    }
                    return _result;
                })(),
                Compression=this.Compression,
                ImageLength=this.ImageLength,
                ImageWidth=this.ImageWidth,
                PixelType=this.PixelType,
                Planar=this.Planar,
                SamplesPerPixel=(short)this.BitsPerSample.Length,
                XResolution=this.XResolution,
                YResolution=this.YResolution
            };
        }
    }
}
