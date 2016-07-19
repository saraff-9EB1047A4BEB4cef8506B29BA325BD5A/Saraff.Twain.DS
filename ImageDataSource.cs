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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.IO;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Provide a Data Source that controls the image acquisition device and is written by the device developer to
    /// comply with TWAIN specifications. Traditional device drivers are now included with the
    /// Source software and do not need to be shipped by applications.
    /// </summary>
    [SupportedGroups(TwDG.Image|TwDG.DS2)]
    [Capability(typeof(Capabilities.BitDepthDataSourceCapability))]
    [Capability(typeof(Capabilities.BitOrderDataSourceCapability))]
    [Capability(typeof(Capabilities.CompressionDataSourceCapability))]
    [Capability(typeof(Capabilities.PhysicalHeightDataSourceCapability))]
    [Capability(typeof(Capabilities.PhysicalWidthDataSourceCapability))]
    [Capability(typeof(Capabilities.PixelFlavorDataSourceCapability))]
    [Capability(typeof(Capabilities.PixelTypeDataSourceCapability))]
    [Capability(typeof(Capabilities.PlanarChunkyDataSourceCapability))]
    [Capability(typeof(Capabilities.UnitsDataSourceCapability))]
    [Capability(typeof(Capabilities.XNativeResolutionDataSourceCapability))]
    [Capability(typeof(Capabilities.XResolutionDataSourceCapability))]
    [Capability(typeof(Capabilities.YNativeResolutionDataSourceCapability))]
    [Capability(typeof(Capabilities.YResolutionDataSourceCapability))]
    [Capability(typeof(Capabilities.ImageFileFormatDataSourceCapability))]
    [Capability(typeof(Capabilities.BitDepthReductionDataSourceCapability))]
    [Capability(typeof(Capabilities.ThresholdDataSourceCapabilitiy))]
    [Capability(typeof(Capabilities.HalftonesDataSourceCapability))]
    [Capability(typeof(Capabilities.CustHalftoneDataSourceCapability))]
    [Capability(typeof(Capabilities.XferMechDataSourceCapability))]
    [Capability(typeof(Capabilities.IndicatorsDataSourceCapability))]
    [Capability(typeof(Capabilities.IndicatorsModeDataSourceCapability))]
    [SupportedDataCodes(TwDAT.ImageLayout, TwDAT.ImageInfo, TwDAT.ImageNativeXfer, TwDAT.ImageMemXfer, TwDAT.ImageFileXfer)]
    public abstract class ImageDataSource:DataSource {

        /// <summary>
        /// Invoked to processing a TWAIN operations (Triplets).
        /// </summary>
        /// <param name="dg">The Data Group of the operation triplet. Currently, only DG_CONTROL, DG_IMAGE, and DG_AUDIO are defined.</param>
        /// <param name="dat">The Data Argument Type of the operation triplet.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>
        /// TWAIN Return Codes.
        /// </returns>
        /// <exception cref="DataSourceException">
        /// </exception>
        protected override TwRC OnProcessRequest(TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
            if(dg==TwDG.Image) {
                switch(dat) {
                    case TwDAT.ImageLayout:
                        return this._ImageLayoutProcessRequest(msg, data);
                    case TwDAT.ImageInfo:
                        if((this.State&DataSourceState.Ready)!=0) {
                            return this._ImageInfoProcessRequest(msg, data);
                        }
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    case TwDAT.ImageNativeXfer:
                        if((this.State&DataSourceState.Ready)!=0) {
                            return this._ImageNativeXferProcessRequest(msg, data);
                        }
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    case TwDAT.ImageMemXfer:
                        if((this.State&DataSourceState.Ready)!=0) {
                            return this._ImageMemXferProcessRequest(msg, data, false);
                        }
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    case TwDAT.ImageMemFileXfer:
                        if((this.State&DataSourceState.Ready)!=0) {
                            return this._ImageMemXferProcessRequest(msg, data, true);
                        }
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    case TwDAT.ImageFileXfer:
                        if((this.State&DataSourceState.Ready)!=0) {
                            return this._ImageFileXferProcessRequest(msg);
                        }
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                }
            }
            return base.OnProcessRequest(dg, dat, msg, data);
        }

        /// <summary>
        /// Returns the Data Group (the type of data) for the upcoming transfer. The Source is required to
        /// only supply one of the DGs specified in the SupportedGroups field of a AppIdentity.
        /// </summary>
        /// <returns>
        /// The DG_xxxx constant that identifies the type of data that is ready for transfer
        /// from the Source
        /// </returns>
        protected override TwDG OnGetXferGroup() {
            return TwDG.Image;
        }

        #region Process Request

        /// <summary>
        /// DG_IMAGE / DAT_IMAGELAYOUT / MSG_GET
        /// The DAT_IMAGELAYOUT operations control information on the physical layout of the image on the
        /// acquisition platform of the Source(e.g.the glass of a flatbed scanner,the size of a photograph,
        /// etc.).
        /// The MSG_GET operation describes both the size and placement of the image on the scanner.The
        /// coordinates on the scanner and the extents of the image are expressed in the unit of measure
        /// currently negotiated for ICAP_UNITS(default is inches).
        /// 
        /// DG_IMAGE / DAT_IMAGELAYOUT / MSG_GETDEFAULT
        /// This operation returns the default information on the layout of an image. This is the size and
        /// position of the image that will be acquired from the Source if the acquisition is started with the
        /// Source(and the device it is controlling) in its power-on state(for instance,most flatbed scanners
        /// will capture the entire bed).
        /// 
        /// DG_IMAGE / DAT_IMAGELAYOUT / MSG_RESET
        /// This operation sets the image layout information for the next transfer to its default settings.
        /// 
        /// DG_IMAGE / DAT_IMAGELAYOUT / MSG_SET
        /// This operation sets the layout for the next image transfer. This allows the application to specify
        /// the physical area to be acquired during the next image transfer(for instance,a frame-based
        /// application would pass to the Source the size of the frame the user selected within the
        /// application—the helpful Source would present a selection region already sized to match the
        /// layout frame size).
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException">
        /// </exception>
        private TwRC _ImageLayoutProcessRequest(TwMSG msg, IntPtr data) {
            var _layout=Marshal.PtrToStructure(data, typeof(TwImageLayout)) as TwImageLayout;
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr(new TwImageLayout {
                        Frame=this.CurrentImageLayout,
                        DocumentNumber=1,
                        PageNumber=1,
                        FrameNumber=1
                    }, data, true);
                    return TwRC.Success;
                case TwMSG.GetDefault:
                    Marshal.StructureToPtr(new TwImageLayout {
                        Frame=this.DefaultImageLayout,
                        DocumentNumber=1,
                        PageNumber=1,
                        FrameNumber=1
                    }, data, true);
                    return TwRC.Success;
                case TwMSG.Set:
                    if((this.State&DataSourceState.Enabled)!=0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    this.CurrentImageLayout=_layout.Frame;
                    return TwRC.Success;
                case TwMSG.Reset:
                    if((this.State&DataSourceState.Enabled)!=0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    this.CurrentImageLayout=this.DefaultImageLayout;
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_IMAGE / DAT_IMAGEINFO / MSG_GET
        /// This operation provides the Application with specific image description
        /// information about the current image that has just been transferred.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException"></exception>
        private TwRC _ImageInfoProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr(this.XferEnvironment.ImageInfo.ToTwImageInfo(), data, true);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_IMAGE / DAT_IMAGENATIVEXFER / MSG_GET
        /// Causes the transfer of an image’s data from the Source to the application, via the Native transfer
        /// mechanism, to begin.The resulting data is stored in main memory in a single block.The data is
        /// stored in the Operating Systems native image format. The size of the image that can be transferred
        /// is limited to the size of the memory block that can be allocated by the Source.If the image is too
        /// large to fit,the Source may resize or crop the image.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException"></exception>
        private TwRC _ImageNativeXferProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    using(var _stream=new MemoryStream()) {
                        using(var _image=this.OnImageNativeXfer()) {
                            _image.Save(_stream, ImageFormat.Bmp);
                        }
                        _stream.Seek(14,SeekOrigin.Begin);
                        var _dib=new byte[_stream.Length-14];
                        _stream.Read(_dib, 0, _dib.Length);

                        var _hData=DataSourceServices.Memory.Alloc(_dib.Length);
                        var _pData=DataSourceServices.Memory.Lock(_hData);
                        DataSourceServices.Memory.Unlock(_hData);
                        Marshal.Copy(_dib, 0, _pData, _dib.Length);
                        Marshal.WriteIntPtr(data, _pData);
                    }
                    this.State|=DataSourceState.Transferring;
                    return TwRC.XferDone;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_IMAGE / DAT_IMAGEMEMXFER / MSG_GET
        /// This operation is used to initiate the transfer of an image from the Source to the application via the
        /// Buffered Memory transfer mechanism.
        /// This operation supports the transfer of successive blocks of image data (in strips or, optionally,
        /// tiles) from the Source into one or more main memory transfer buffers. These buffers(for strips)
        /// are allocated and owned by the application. For tiled transfers, the source allocates the buffers.
        /// The application should repeatedly invoke this operation while TWRC_SUCCESS is returned by the Source.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <param name="isMemFile">If set to <c>true</c> that transfer a MemFile.</param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException">
        /// </exception>
        private TwRC _ImageMemXferProcessRequest(TwMSG msg, IntPtr data, bool isMemFile) {
            switch(msg) {
                case TwMSG.Get:
                    var _memXfer=Marshal.PtrToStructure(data, typeof(TwImageMemXfer)) as TwImageMemXfer;

                    if(_memXfer.Memory.Length>this.XferEnvironment.MaxMemXferBufferSize||_memXfer.Memory.Length<this.XferEnvironment.MinMemXferBufferSize) {
                        throw new DataSourceException(TwRC.Failure, TwCC.BadValue);
                    }

                    var _image=this.OnImageMemXfer(_memXfer.Memory.Length, isMemFile);
                    if(_image==null) {
                        return TwRC.Cancel;
                    }

                    Marshal.StructureToPtr(
                        new TwImageMemXfer {
                            BytesPerRow=_image.BytesPerRow,
                            BytesWritten=(uint)_image.ImageData.Length,
                            Columns=_image.Columns,
                            Compression=_image.Compression,
                            Rows=_image.Rows,
                            XOffset=_image.XOffset,
                            YOffset=_image.YOffset,
                            Memory=_memXfer.Memory
                        },
                        data,
                        true);

                    var _pImageData=IntPtr.Zero;
                    switch(_memXfer.Memory.Flags&(TwMF.Handle|TwMF.Pointer)) {
                        case TwMF.Handle:
                            _pImageData=DataSourceServices.Memory.Lock(_memXfer.Memory.TheMem);
                            DataSourceServices.Memory.Unlock(_memXfer.Memory.TheMem);
                            break;
                        case TwMF.Pointer:
                            _pImageData=_memXfer.Memory.TheMem;
                            break;
                        default:
                            throw new DataSourceException(TwRC.Failure, TwCC.BadValue);
                    }
                    Marshal.Copy(_image.ImageData, 0, _pImageData, _image.ImageData.Length);

                    return _image.IsXferDone?TwRC.XferDone:TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_IMAGE / DAT_IMAGEFILEXFER / MSG_GET
        /// This operation is used to initiate the transfer of an image from the Source to the application via the
        /// disk-file transfer mechanism. It causes the transfer to begin.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException"></exception>
        private TwRC _ImageFileXferProcessRequest(TwMSG msg) {
            switch(msg) {
                case TwMSG.Get:
                    this.OnImageFileXfer();
                    return TwRC.XferDone;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        #endregion

        #region protected virtual

        /// <summary>
        /// Causes the transfer of an image’s data from the Source to the application, via the Native transfer
        /// mechanism, to begin. The resulting data is stored in main memory in a single block. The data is
        /// stored in the Operating Systems native image format. The size of the image that can be transferred
        /// is limited to the size of the memory block that can be allocated by the Source. If the image is too
        /// large to fit, the Source may resize or crop the image.
        /// </summary>
        /// <returns>A image to transfer.</returns>
        protected abstract Image OnImageNativeXfer();

        /// <summary>
        /// This operation is used to initiate the transfer of an image from the Source to the application via the
        /// Buffered Memory transfer mechanism.
        /// This operation supports the transfer of successive blocks of image data (in strips or,optionally,
        /// tiles) from the Source into one or more main memory transfer buffers. These buffers(for strips)
        /// are allocated and owned by the application. For tiled transfers, the source allocates the buffers.
        /// The application should repeatedly invoke this operation while TWRC_SUCCESS is returned by the Source.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="isMemFile">If set to <c>true</c> that transfer a MemFile.</param>
        /// <returns>Information about transmitting data.</returns>
        protected abstract ImageMemXfer OnImageMemXfer(long length, bool isMemFile);

        /// <summary>
        /// This operation is used to initiate the transfer of an image from the Source to the application via the
        /// disk-file transfer mechanism. It causes the transfer to begin.
        /// </summary>
        /// <exception cref="DataSourceException"></exception>
        protected virtual void OnImageFileXfer() {
            throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
        }

        /// <summary>
        /// Gets or sets the current image layout.
        /// </summary>
        /// <value>
        /// The current image layout.
        /// </value>
        protected virtual RectangleF CurrentImageLayout {
            get;
            set;
        }

        /// <summary>
        /// Gets the default image layout.
        /// </summary>
        /// <value>
        /// The default image layout.
        /// </value>
        /// <exception cref="System.InvalidOperationException">Не указан ImageLayoutAttribute.</exception>
        protected virtual RectangleF DefaultImageLayout {
            get {
                foreach(ImageLayoutAttribute _attr in this.GetType().GetCustomAttributes(typeof(ImageLayoutAttribute), false)) {
                    return _attr.Frame;
                }
                throw new InvalidOperationException("Не указан ImageLayoutAttribute.");
            }
        }

        #endregion
    }
}
