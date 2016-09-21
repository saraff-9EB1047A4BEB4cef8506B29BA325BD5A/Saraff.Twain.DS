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
using System.Runtime.InteropServices;
using System.Reflection;
using System.Windows.Forms;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Provide a Data Source. Traditional device drivers are now included with the
    /// Source software and do not need to be shipped by applications.
    /// </summary>
    [Capability(typeof(Capabilities.DeviceOnlineDataSourceCapability))]
    [Capability(typeof(Capabilities.SupportedCapsDataSourceCapability))]
    [Capability(typeof(Capabilities.SupportedDatsDataSourceCapability))]
    [Capability(typeof(Capabilities.UIControllableDataSourceCapability))]
    [Capability(typeof(Capabilities.XferCountDataSourceCapability))]
    [Capability(typeof(Capabilities.FeederEnabledDataSourceCapability))]
    [SupportedDataCodes(TwDAT.Event, TwDAT.UserInterface, TwDAT.XferGroup, TwDAT.Capability, TwDAT.PendingXfers, TwDAT.SetupMemXfer, TwDAT.SetupFileXfer)]
    public abstract class DataSource:IDataSource {
        private Dictionary<TwCap, DataSourceCapability> _capabilities;
        private ReadOnlyCollection<TwDAT> _dats;
        private ReadOnlyCollection<TwSX> _xferMechs;
        private XferEnvironment _xferEnvironment;

        #region IDataSource Members

        /// <summary>
        /// Every Source is required to have a single entry point called DS_Entry. DS_Entry is only called by the Source Manager.
        /// </summary>
        /// <param name="appId">
        /// This points to a TwIdentity structure, allocated by the application, that describes the
        /// application making the call. One of the fields in this structure, called Id, is an arbitrary and
        /// unique identifier assigned by the Source Manager to tag the application as a unique TWAIN
        /// entity. The Source Manager maintains a copy of the application’s identity structure, so the
        /// application must not modify that structure unless it first breaks its connection with the Source
        /// Manager,then reconnects to cause the Source Manager to store the new, modified identity.
        /// </param>
        /// <param name="dg">The Data Group of the operation triplet. Currently, only DG_CONTROL, DG_IMAGE, and DG_AUDIO are defined.</param>
        /// <param name="dat">The Data Argument Type of the operation triplet.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">
        /// The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.
        /// </param>
        /// <returns>TWAIN Return Codes.</returns>
        public TwRC ProcessRequest(TwIdentity appId, TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
            if(this.AppIdentity==null) {
                this.AppIdentity=appId;
            }
            if(this.AppIdentity.Id!=appId.Id) {
                throw new DataSourceException(TwRC.Failure, TwCC.OperationError);
            }
            if(!this.SupportedDataCodes.Contains(dat)) {
                throw new DataSourceException(TwRC.Failure,TwCC.OperationError);
            }
            if(dg==TwDG.Control) {
                switch(dat) {
                    case TwDAT.Event:
                        return this._EventProcessRequest(msg, data);
                    case TwDAT.UserInterface:
                        return this._UserInterfaceProcessRequest(msg, data);
                    case TwDAT.XferGroup:
                        return this._XferGroupProcessRequest(msg, data);
                    case TwDAT.Capability:
                        return this._CapabilityProcessRequest(msg, data);
                    case TwDAT.PendingXfers:
                        return this._PendingXfersProcessRequest(msg, data);
                    case TwDAT.SetupMemXfer:
                        return this._SetupMemXferProcessRequest(msg, data);
                    case TwDAT.SetupFileXfer:
                        return this._SetupFileXferProcessRequest(msg, data);
                }
            }
            return this.OnProcessRequest(dg, dat, msg, data);
        }

        #endregion

        #region Process Request

        /// <summary>
        /// DG_CONTROL / DAT_EVENT / MSG_PROCESSEVENT
        /// This operation supports the distribution of events from the application to Sources so that the
        /// Source can maintain its user interface and return messages to the application.Once the
        /// application has enabled the Source, it must immediately begin sending to the Source all events
        /// that enter the application’s main event loop. This allows the Source to update its user interface in
        /// real-time and to return messages to the application which cause state transitions. Even if the
        /// application overrides the Source’s user interface, it must forward all events once the Source has
        /// been enabled. The Source will tell the application whether or not each event belongs to the Source.
        /// 
        /// The Source should be structured such that identification of the event’s “owner” is handled before
        /// doing anything else. Further, the Source should return immediately if the Source isn’t the owner.
        /// This convention should minimize performance concerns for the application (remember, these
        /// events are only sent while a Source is enabled—that is, just before and just after the transfer is
        /// taking place).
        /// </summary>
        /// <remarks>Events only need to be forwarded to the Source while it is enabled.</remarks>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _EventProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.ProcessEvent:
                    var _event=Marshal.PtrToStructure(data,typeof(TwEvent)) as TwEvent;
                    var _msg=(WINMSG)Marshal.PtrToStructure(_event.EventPtr,typeof(WINMSG));
                    try {
                        return this.OnProcessEvent(Message.Create(_msg.hwnd, _msg.message, _msg.wParam, _msg.lParam))?TwRC.DSEvent:TwRC.NotDSEvent;
                    } finally {
                        if(_event.Message!=TwMSG.Null) {
                            _event.Message=TwMSG.Null;
                            Marshal.StructureToPtr(_event, data, true);
                        }
                    }
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_USERINTERFACE / MSG_ENABLEDS
        /// This operation causes three responses in the Source:
        /// - Places the Source into a “ready to acquire” condition. If the application raises the Source’s user
        /// interface (see #2, below), the Source will wait to assert MSG_XFERREADY until the “GO” button
        /// in its user interface or on the device is clicked. If the application bypasses the Source’s user
        /// interface, this operation causes the Source to become immediately “armed”. That is, the Source
        /// should assert MSG_XFERREADY as soon as it has data to transfer.
        /// - The application can choose to raise the Source’s built-in user interface, or not, using this
        /// operation. The application signals the Source’s user interface should be displayed by setting
        /// pUserInterface->ShowUI to TRUE. If the application does not want the Source’s user interface
        /// to be displayed, or wants to replace the Source’s user interface with one of its own, it sets
        /// pUserInterface->ShowUI to FALSE. If activated, the Source’s user interface will remain
        /// displayed until it is closed by the user or explicitly disabled by the application(see Note).
        /// - Terminates Source’s acceptance of “set capability” requests from the application. Capabilities
        /// can only be negotiated in State 4 (unless special arrangements are made using the
        /// CAP_EXTENDEDCAPS capability). Values of capabilities can still be inquired in States 5 through 7.
        /// Note: Once the Source is enabled, the application must begin sending the Source every event
        /// that enters the application’s main event loop.The application must continue to send the
        /// Source events until it disables(MSG_DISABLEDS) the Source. This is true even if the
        /// application chooses not to use the Source’s built-in user interface.
        /// 
        /// DG_CONTROL / DAT_USERINTERFACE / MSG_DISABLEDS
        /// This operation causes the Source’s user interface, if displayed during the 
        /// DG_CONTROL / DAT_USERINTERFACE / MSG_ENABLEDS operation, to be lowered. The Source is returned to
        /// State 4,where capability negotiation can again occur. The application can invoke this operation
        /// either because it wants to shut down the current session,or in response to the Source “posting” a
        /// MSG_CLOSEDSREQ event to it. Rarely, the application may need to close the Source because an
        /// error condition was detected.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _UserInterfaceProcessRequest(TwMSG msg, IntPtr data) {
            var _ui=Marshal.PtrToStructure(data, typeof(TwUserInterface)) as TwUserInterface;
            switch(msg) {
                case TwMSG.EnableDS:
                    if((this.State&DataSourceState.Enabled)!=0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    this.State|=DataSourceState.Enabled;
                    try {
                        this.OnEnableDS(_ui.ShowUI, _ui.ModalUI, _ui.ParentHand);
                    } catch(Exception) {
                        this.State&=~DataSourceState.Enabled;
                        throw;
                    }
                    return TwRC.Success;
                case TwMSG.DisableDS:
                    if((this.State&DataSourceState.Enabled)==0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    this.OnDisableDS(_ui.ParentHand);
                    this.State&=~DataSourceState.Enabled;
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_XFERGROUP / MSG_GET
        /// Returns the Data Group (the type of data) for the upcoming transfer. The Source is required to
        /// only supply one of the DGs specified in the SupportedGroups field of a AppIdentity.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _XferGroupProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr((uint)this.OnGetXferGroup(), data, true);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_CAPABILITY / MSG_GET
        /// Returns the Source’s Current, Default and Available Values for a specified capability.
        /// If the application requests this operation on a capability your Source does not recognize (and
        /// you’re sure you’ve implemented all the capabilities that you’re required to), disregard the
        /// operation, but return TWRC_FAILURE with TWCC_BADCAP.
        /// If you support the capability, fill in the fields listed below and allocate the container structure and
        /// place its handle into pCapability->hContainer.The container should be referenced by a
        /// “handle” of type TW_HANDLE.
        /// Set ConType to the container type your Source uses for this capability. For the container type of
        /// TWON_ONEVALUE provide the Current Value.For the container type of TWON_ARRAY provide the
        /// Available Values.For container types TWON_ENUMERATION and TWON_RANGE provide the
        /// Current,Default and Available Values.
        /// This is a memory allocation operation. It is possible for this operation to fail due to a low memory
        /// condition.Be sure to verify that the allocation is successful.If it is not, attempt to reduce the
        /// amount of memory occupied by the Source.If the allocation cannot be made, return
        /// TWRC_FAILURE with TWCC_LOWMEMORY to the application and set the
        /// pCapability->hContainer handle to NULL.
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_GETCURRENT
        /// Returns the Source’s Current Value for the specified capability.
        /// The Current Value reflects previous MSG_SET operations on the capability, or Source’s automatic
        /// changes. (See MSG_SET).
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_GETDEFAULT
        /// Returns the Source’s Default Value. This is the Source’s preferred default value.
        /// The Source’s Default Value cannot be changed.
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_SET
        /// Changes the Current Value of the capability to that specified by the application. As of TWAIN 2.2
        /// MSG_SET only modifies the Current Value of the specified capability, constraints cannot be
        /// changed with MSG_SET.The original functionality of MSG_SET has been addressed in
        /// MSG_SETCONSTRAINT for TWAIN 2.2 Sources and higher. (Please refer to DG_CONTROL / DAT_CAPABILITY / MSG_SETCONSTRAINT)
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_RESET
        /// Change the Current Value of the specified capability back to its power-on value and return the
        /// new Current Value.
        /// The power-on value is the Current Value the Source started with when it entered State 4 after a
        /// DG_CONTROL / DAT_IDENTITY / MSG_OPENDS.These values are listed as TWAIN defaults. If “no default” 
        /// is specified, the Source uses it preferred default value (returned from MSG_GETDEFAULT).
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_RESETALL
        /// This command resets all current values back to original power-on defaults. All current values are
        /// set to their default value except is the where mandatory values are required.All constraints are
        /// removed for all of the negotiable capabilities supported by the driver.
        /// 
        /// DG_CONTROL / DAT_CAPABILITY / MSG_QUERYSUPPORT
        /// Returns the Source’s support status of this capability.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _CapabilityProcessRequest(TwMSG msg, IntPtr data) {
            var _cap=Marshal.PtrToStructure(data, typeof(TwCapability)) as TwCapability;
            var _handler=this[_cap.Cap];
            if(_handler==null&&msg!=TwMSG.QuerySupport&&msg!=TwMSG.ResetAll) {
                throw new DataSourceException(TwRC.Failure, TwCC.CapUnsupported);
            }
            switch(msg) {
                case TwMSG.QuerySupport:
                    Marshal.StructureToPtr(new TwCapability(_cap.Cap, _handler!=null?(uint)_handler.SupportedOperations:0U, TwType.Int32), data, true);
                    return TwRC.Success;
                case TwMSG.Get:
                    if((_handler.SupportedOperations&TwQC.Get)!=0) {
                        Marshal.StructureToPtr(_handler.Get(), data, true);
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.CapBadOperation);
                case TwMSG.GetCurrent:
                    if((_handler.SupportedOperations&TwQC.GetCurrent)!=0) {
                        Marshal.StructureToPtr(_handler.GetCurrent(), data, true);
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.CapBadOperation);
                case TwMSG.GetDefault:
                    if((_handler.SupportedOperations&TwQC.GetDefault)!=0) {
                        Marshal.StructureToPtr(_handler.GetDefault(), data, true);
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.CapBadOperation);
                case TwMSG.Reset:
                    if((this.State&DataSourceState.Enabled)!=0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    if((_handler.SupportedOperations&TwQC.Reset)!=0) {
                        _handler.Reset();
                        Marshal.StructureToPtr(_handler.GetCurrent(), data, true);
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.CapBadOperation);
                case TwMSG.Set:
                    if((this.State&DataSourceState.Enabled)!=0) {
                        throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                    }
                    if((_handler.SupportedOperations&TwQC.Set)!=0) {
                        _handler.Set(_cap);
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.CapBadOperation);
                case TwMSG.ResetAll:
                    foreach(TwCap _item in this.SupportedCapabilities) {
                        for(var _hcap=this[_item]; (_hcap.SupportedOperations&TwQC.Reset)!=0; ) {
                            _hcap.Reset();
                            break;
                        }
                    }
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_SETUPMEMXFER / MSG_GET
        /// Returns the Source’s preferred, minimum, and maximum allocation sizes for transfer memory
        /// buffers.The application using buffered memory transfers must use a buffer size between
        /// MinBufSize and MaxBufSize in their TW_IMAGEMEMXFER.Memory.Length when using the
        /// DG_IMAGE / DAT_IMAGEMEMXFER / MSG_GET operation. Sources may return a more efficient
        /// preferred value in State 6 after the image size, etc. has been specified.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _SetupMemXferProcessRequest(TwMSG msg,IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr(new TwSetupMemXfer {
                        MinBufSize=this.XferEnvironment.MinMemXferBufferSize,
                        MaxBufSize=this.XferEnvironment.MaxMemXferBufferSize,
                        Preferred=this.XferEnvironment.MemXferBufferSize
                    },data,true);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure,TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_SETUPFILEXFER / MSG_GET
        /// Returns information about the file into which the Source has or will put the acquired DG_IMAGE or DG_AUDIO data.
        /// 
        /// DG_CONTROL / DAT_SETUPFILEXFER / MSG_GETDEFAULT
        /// Returns information for the default DG_IMAGE or DG_AUDIO file.
        /// 
        /// DG_CONTROL / DAT_SETUPFILEXFER / MSG_RESET
        /// Resets the current file information to the DG_IMAGE or DG_AUDIO default file information and
        /// returns that default information.
        /// 
        /// DG_CONTROL / DAT_SETUPFILEXFER / MSG_SET
        /// Sets the file transfer information for the next file transfer. The application is responsible for
        /// verifying that the specified file name is valid and that the file either does not currently exist(in
        /// which case, the Source is to create the file), or that the existing file is available for opening and
        /// read/write operations. The application should also assure that the file format it is requesting can
        /// be provided by the Source (otherwise,the Source will generate a TWRC_FAILURE /
        /// TWCC_BADVALUE error).
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _SetupFileXferProcessRequest(TwMSG msg, IntPtr data) {
            if(this.SupportedXferMechs.Contains(TwSX.File)) {
                switch(msg) {
                    case TwMSG.Get:
                        Marshal.StructureToPtr(new TwSetupFileXfer {FileName=this.XferEnvironment.FileXferName,Format=this.XferEnvironment.FileXferFormat}, data, true);
                        return TwRC.Success;
                    case TwMSG.GetDefault:
                        Marshal.StructureToPtr(new TwSetupFileXfer {FileName=this.XferEnvironment.DefaultFileXferName,Format=this.XferEnvironment.DefaultFileXferFormat}, data, true);
                        return TwRC.Success;
                    case TwMSG.Set:
                        var _setupFileXfer=Marshal.PtrToStructure(data, typeof(TwSetupFileXfer)) as TwSetupFileXfer;
                        this.OnSetupFileXfer(_setupFileXfer.FileName, _setupFileXfer.Format);
                        this.XferEnvironment.FileXferName=_setupFileXfer.FileName;
                        this.XferEnvironment.FileXferFormat=_setupFileXfer.Format;
                        return TwRC.Success;
                    case TwMSG.Reset:
                        this.XferEnvironment._FileXferReset();
                        return TwRC.Success;
                }
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_PENDINGXFERS / MSG_ENDXFER
        /// This triplet is used to cancel or terminate a transfer. Issued in state 6, this triplet cancels the next
        /// pending transfer, discards the transfer data, and decrements the pending transfers count. In state 7,
        /// this triplet terminates the current transfer. If any data has not been transferred (this is only
        /// possible during a memory transfer) that data is discarded.
        /// The application can use this operation to cancel the next pending transfer (Source writers take
        /// note of this). For example, after the application checks TW_IMAGEINFO(or TW_AUDIOINFO, if
        /// transferring audio snippets), it may decide to not transfer the next image. The operation must be
        /// sent prior to the beginning of the transfer, otherwise the Source will simply abort the current
        /// transfer. The Source decrements the number of pending transfers.
        /// 
        /// DG_CONTROL / DAT_PENDINGXFERS / MSG_GET
        /// Returns the number of transfers the Source is ready to supply to the application, upon demand. If
        /// DAT_XFERGROUP is set to DG_IMAGE, this is the number of images.If DAT_XFERGROUP is set to
        /// DG_AUDIO, this is the number of audio snippets for the current image. If there is no current
        /// image, this call must return TWRC_FAILURE / TWCC_SEQERROR.
        /// 
        /// DG_CONTROL / DAT_PENDINGXFERS / MSG_RESET
        /// Sets the number of pending transfers in the Source to zero.
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The data parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _PendingXfersProcessRequest(TwMSG msg, IntPtr data) {
            try {
                switch(msg) {
                    case TwMSG.EndXfer:
                        if((this.State&DataSourceState.Ready)==0) {
                            throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                        }
                        if(this.XferEnvironment.PendingXfers==0) {
                            throw new DataSourceException(TwRC.Failure, TwCC.OperationError);
                        }
                        this.XferEnvironment.PendingXfers--;
                        this.OnEndXfer();
                        break;
                    case TwMSG.Get:
                        break;
                    case TwMSG.Reset:
                        if((this.State&DataSourceState.Ready)==0||(this.State&DataSourceState.Transferring)!=0) {
                            throw new DataSourceException(TwRC.Failure, TwCC.SeqError);
                        }
                        this.XferEnvironment.PendingXfers=0;
                        this.OnResetXfer();
                        break;
                    default:
                        throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
                }
                var _pendingXfers=new TwPendingXfers {Count=this.XferEnvironment.PendingXfers};
                for(var _cap=this[TwCap.JobControl]; _cap!=null; ) {
                    _pendingXfers.EOJ=(uint)(TwJC)_cap.Value;
                    break;
                }
                Marshal.StructureToPtr(_pendingXfers, data, true);
                return TwRC.Success;
            } finally {
                if(this.XferEnvironment.PendingXfers>=0) {
                    this.State&=~DataSourceState.Transferring;
                }
                if(this.XferEnvironment.PendingXfers==0) {
                    this.State&=~DataSourceState.Ready;
                    this.XferEnvironment.ImageInfo=null;
                }
            }
        }

        #endregion

        #region protected virtual

        /// <summary>
        /// Invoked to processing a TWAIN operations (Triplets).
        /// </summary>
        /// <param name="dg">The Data Group of the operation triplet. Currently, only DG_CONTROL, DG_IMAGE, and DG_AUDIO are defined.</param>
        /// <param name="dat">The Data Argument Type of the operation triplet.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">
        /// The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.
        /// </param>
        /// <returns>TWAIN Return Codes.</returns>
        /// <exception cref="DataSourceException"></exception>
        protected virtual TwRC OnProcessRequest(TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// This operation supports the distribution of events from the application to Sources so that the
        /// Source can maintain its user interface and return messages to the application.Once the
        /// application has enabled the Source, it must immediately begin sending to the Source all events
        /// that enter the application’s main event loop. This allows the Source to update its user interface in
        /// real-time and to return messages to the application which cause state transitions. Even if the
        /// application overrides the Source’s user interface, it must forward all events once the Source has
        /// been enabled. The Source will tell the application whether or not each event belongs to the Source.
        /// 
        /// The Source should be structured such that identification of the event’s “owner” is handled before
        /// doing anything else. Further, the Source should return immediately if the Source isn’t the owner.
        /// This convention should minimize performance concerns for the application (remember, these
        /// events are only sent while a Source is enabled—that is, just before and just after the transfer is
        /// taking place).
        /// </summary>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <returns><c>true</c> if a source consumed event--application should not process it; <c>false</c> if Event belongs to application - process as usual.</returns>
        protected virtual bool OnProcessEvent(Message msg) {
            return false;
        }

        /// <summary>
        /// If showUI is <c>true</c>, the Source should display its user interface and wait for
        /// the user to initiate an acquisition. If showUI is <c>false</c>,the Source should
        /// immediately begin acquiring data based on its current configuration (a device that requires the
        /// user to push a button on the device,such as a hand-scanner,will be “armed” by this operation and
        /// will assert MSG_XFERREADY as soon as the Source has data ready for transfer). The Source should
        /// fail any attempt to set a capability value (TWRC_FAILURE / TWCC_SEQERROR) until it returns to
        /// State 4 (unless an exception approval exists via a CAP_EXTENDEDCAPS agreement).
        /// 
        /// Note: If the application has set showUI or CAP_INDICATORS to <c>true</c>, then the Source is
        /// responsible for presenting the user with appropriate progress indicators regarding the
        /// acquisition and transfer process. If showUI is set to <c>true</c>, CAP_INDICATORS is ignored
        /// and progress and errors are always shown.
        /// 
        /// Note: It is strongly recommended that all Sources support being enabled without their User
        /// Interface if the application requests (showUI = <c>false</c>). But if your
        /// Source cannot be used without its User Interface, it should enable showing the Source
        /// User Interface (just as if showUI = <c>true</c>) but return TWRC_CHECKSTATUS. All Sources,
        /// however, must support the CAP_UICONTROLLABLE. This capability reports whether or
        /// not a Source allows showUI = <c>false</c>. An application can use this capability to know
        /// whether the Source-supplied user interface can be suppressed before it is displayed.
        /// </summary>
        /// <param name="showUI"><c>true</c> if DS should bring up its UI.</param>
        /// <param name="modalUI"><c>true</c> if the DS's UI is modal.</param>
        /// <param name="hwnd">For windows only - Application window handle.</param>
        protected virtual void OnEnableDS(bool showUI, bool modalUI, IntPtr hwnd) {
        }

        /// <summary>
        /// If the Source’s user interface is displayed, it should be lowered. The Source returns to State 4 and
        /// is again available for capability negotiation.
        /// </summary>
        /// <param name="hwnd">For windows only - Application window handle.</param>
        protected virtual void OnDisableDS(IntPtr hwnd) {
        }

        /// <summary>
        /// Returns the Data Group (the type of data) for the upcoming transfer. The Source is required to
        /// only supply one of the DGs specified in the SupportedGroups field of a AppIdentity.
        /// </summary>
        /// <returns>The DG_xxxx constant that identifies the type of data that is ready for transfer
        /// from the Source</returns>
        protected abstract TwDG OnGetXferGroup();

        /// <summary>
        /// Invoked when the capability value changed.
        /// </summary>
        /// <param name="e">Information about the capability that was changed.</param>
        protected virtual void OnCapabilityChanged(CapabilityEventArgs e) {
        }

        /// <summary>
        /// Invoked when the capability value need.
        /// </summary>
        /// <param name="e">Information about the capability that was changed.</param>
        protected virtual void OnCapabilityValueNeeded(CapabilityEventArgs e) {
        }

        /// <summary>
        /// Invoked when a aplication sets the file transfer information for the next file transfer. The application is responsible for
        /// verifying that the specified file name is valid and that the file either does not currently exist (in
        /// which case, the Source is to create the file), or that the existing file is available for opening and
        /// read/write operations. The application should also assure that the file format it is requesting can
        /// be provided by the Source (otherwise,the Source will generate a TWRC_FAILURE / TWCC_BADVALUE error).
        /// </summary>
        /// <param name="fileName">File to contain entry.</param>
        /// <param name="format">A TWFF_xxxx constant.</param>
        protected virtual void OnSetupFileXfer(string fileName,TwFF format) {
        }

        /// <summary>
        /// Invoked when the pending transfers discarded.
        /// </summary>
        protected virtual void OnResetXfer() {
        }

        /// <summary>
        /// Invoked at the end of every transfer to signal that the application has received all the data it expected.
        /// </summary>
        protected virtual void OnEndXfer() {
        }

        /// <summary>
        /// Invoked to indicate that the Source has data that is ready to be transferred.
        /// </summary>
        /// <exception cref="DataSourceException"></exception>
        protected virtual void OnXferReady() {
            if(this.XferEnvironment.PendingXfers==0) {
                this.XferEnvironment.PendingXfers=(ushort)Math.Abs((short)this[TwCap.XferCount].Value);
            }
            this.State|=DataSourceState.Ready;
            for(var _rc=DataSourceServices.DsmInvoke(this.AppIdentity, TwMSG.XFerReady); _rc!=TwRC.Success; ) {
                this.State&=~DataSourceState.Ready;
                this.XferEnvironment.PendingXfers=0;
                throw new DataSourceException(TwRC.Failure, TwCC.OperationError);
            }
        }

        /// <summary>
        /// Invoked to indicate that the Source needs to be closed.
        /// </summary>
        /// <exception cref="DataSourceException"></exception>
        protected virtual void OnCloseDSReq() {
            for(var _rc=DataSourceServices.DsmInvoke(this.AppIdentity, TwMSG.CloseDSReq); _rc!=TwRC.Success; ) {
                throw new DataSourceException(TwRC.Failure, TwCC.OperationError);
            }
        }

        #endregion

        #region properties

        /// <summary>
        /// Gets a TwIdentity instance, that describes the application making the call.
        /// </summary>
        /// <value>
        /// Instance of TwIdentity.
        /// </value>
        public TwIdentity AppIdentity {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="DataSourceCapability"/> for the specified capability.
        /// </summary>
        /// <value>
        /// The <see cref="DataSourceCapability"/>.
        /// </value>
        /// <param name="cap">The capability.</param>
        /// <returns>Instance of <see cref="DataSourceCapability"/>.</returns>
        public DataSourceCapability this[TwCap cap] {
            get {
                return this._Capabilities.ContainsKey(cap)?this._Capabilities[cap]:null;
            }
        }

        /// <summary>
        /// Gets the supported capabilities.
        /// </summary>
        /// <value>
        /// The supported capabilities.
        /// </value>
        public IEnumerable<TwCap> SupportedCapabilities {
            get {
                return this._Capabilities.Keys;
            }
        }

        /// <summary>
        /// Gets the supported data codes.
        /// </summary>
        /// <value>
        /// The supported data codes.
        /// </value>
        public IEnumerable<TwDAT> SupportedDataCodes {
            get {
                if(this._dats==null) {
                    var _result=new List<TwDAT>();
                    for(var _type=this.GetType(); _type!=null; _type=_type.BaseType) {
                        foreach(SupportedDataCodesAttribute _attr in _type.GetCustomAttributes(typeof(SupportedDataCodesAttribute), false)) {
                            foreach(var _dat in _attr.DataCodes) {
                                if(!_result.Contains(_dat)) {
                                    _result.Add(_dat);
                                }
                            }
                        }
                    }
                    this._dats=_result.AsReadOnly();
                }
                return this._dats;
            }
        }

        /// <summary>
        /// Gets the supported transfer mechanisms.
        /// </summary>
        /// <value>
        /// The supported transfer mechanisms.
        /// </value>
        public IEnumerable<TwSX> SupportedXferMechs {
            get {
                if(this._xferMechs==null) {
                    var _result=new List<TwSX> { TwSX.Native, TwSX.Memory };
                    foreach(Capabilities.XferMechAttribute _attr in this.GetType().GetCustomAttributes(typeof(Capabilities.XferMechAttribute), false)) {
                        if(_attr.MemFile) {
                            _result.Add(TwSX.MemFile);
                        }
                        if(_attr.File) {
                            _result.Add(TwSX.File);
                        }
                        break;
                    }
                    this._xferMechs=_result.AsReadOnly();
                }
                return this._xferMechs;
            }
        }

        /// <summary>
        /// Gets the a transfer environment.
        /// </summary>
        /// <value>
        /// Instance of <see cref="XferEnvironment"/>.
        /// </value>
        public XferEnvironment XferEnvironment {
            get {
                if(this._xferEnvironment==null) {
                    this._xferEnvironment=new XferEnvironment(this);
                }
                return this._xferEnvironment;
            }
        }

        /// <summary>
        /// Gets or sets the state of a Source.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        protected DataSourceState State {
            get;
            set;
        }

        private Dictionary<TwCap, DataSourceCapability> _Capabilities {
            get {
                if(this._capabilities==null) {
                    this._capabilities=new Dictionary<TwCap, DataSourceCapability>();
                    for(var _type=this.GetType(); _type!=null; _type=_type.BaseType) {
                        foreach(CapabilityAttribute _attr in _type.GetCustomAttributes(typeof(CapabilityAttribute), false)) {
                            var _cap=DataSourceCapability.Create(_attr.Type, this);
                            if(_cap==null) {
                                throw new InvalidOperationException(string.Format("Тип \"{0}\" должен быть производным от \"{1}\".", _attr.Type.FullName, typeof(DataSourceCapability)));
                            }
                            if(!this._capabilities.ContainsKey(_cap.CapabilityInfo.Capability)) {
                                _cap.CapabilityChanged+=(sender, e) => this.OnCapabilityChanged(e);
                                _cap.CapabilityValueNeeded+=(sender, e) => this.OnCapabilityValueNeeded(e);
                                this._capabilities.Add(_cap.CapabilityInfo.Capability, _cap);
                            }
                        }
                    }
                }
                return this._capabilities;
            }
        }

        #endregion

        [StructLayout(LayoutKind.Sequential, Pack=2)]
        private struct WINMSG {
            public IntPtr hwnd;
            public int message;
            public IntPtr wParam;
            public IntPtr lParam;
        }
    }

    /// <summary>
    /// States of a data source.
    /// </summary>
    [Flags]
    public enum DataSourceState {

        /// <summary>
        /// DS is enabled.
        /// </summary>
        Enabled = 0x01,

        /// <summary>
        /// DS is ready to transfer.
        /// </summary>
        Ready = 0x02,

        /// <summary>
        /// DS transferring data to aplication.
        /// </summary>
        Transferring = 0x04
    }
}
