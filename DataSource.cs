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

        public virtual TwRC ProcessRequest(TwIdentity appId, TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
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

        private TwRC _XferGroupProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr((uint)this.OnGetXferGroup(), data, true);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

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

        private TwRC _SetupMemXferProcessRequest(TwMSG msg, IntPtr data) {
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr(new TwSetupMemXfer {
                        MinBufSize=this.XferEnvironment.MinMemXferBufferSize,
                        MaxBufSize=this.XferEnvironment.MaxMemXferBufferSize,
                        Preferred=this.XferEnvironment.MemXferBufferSize
                    }, data, true);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

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

        protected virtual TwRC OnProcessRequest(TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        protected virtual bool OnProcessEvent(Message msg) {
            return false;
        }

        protected virtual void OnEnableDS(bool showUI, bool modalUI, IntPtr hwnd) {
        }

        protected virtual void OnDisableDS(IntPtr hwnd) {
        }

        protected abstract TwDG OnGetXferGroup();

        protected virtual void OnCapabilityChanged(CapabilityEventArgs e) {
        }

        protected virtual void OnCapabilityValueNeeded(CapabilityEventArgs e) {
        }

        protected virtual void OnSetupFileXfer(string fileName,TwFF format) {
        }

        protected virtual void OnResetXfer() {
        }

        protected virtual void OnEndXfer() {
        }

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

        protected virtual void OnCloseDSReq() {
            for(var _rc=DataSourceServices.DsmInvoke(this.AppIdentity, TwMSG.CloseDSReq); _rc!=TwRC.Success; ) {
                throw new DataSourceException(TwRC.Failure, TwCC.OperationError);
            }
        }

        #endregion

        #region properties

        public TwIdentity AppIdentity {
            get;
            private set;
        }

        public DataSourceCapability this[TwCap cap] {
            get {
                return this._Capabilities.ContainsKey(cap)?this._Capabilities[cap]:null;
            }
        }

        public IEnumerable<TwCap> SupportedCapabilities {
            get {
                return this._Capabilities.Keys;
            }
        }

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

        public XferEnvironment XferEnvironment {
            get {
                if(this._xferEnvironment==null) {
                    this._xferEnvironment=new XferEnvironment(this);
                }
                return this._xferEnvironment;
            }
        }

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

    [Flags]
    public enum DataSourceState {
        Enabled=0x01,
        Ready=0x02,
        Transferring=0x04
    }
}
