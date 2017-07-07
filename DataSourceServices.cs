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
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Controls the TWAIN session. Consumed by both Source Manager and Source. It is always
    /// available, no matter what the current setting of DG_CONTROL / DAT_XFERGROUP.
    /// </summary>
    /// <seealso cref="Saraff.Twain.DS.IDataSource" />
    [SupportedDataCodes(TwDAT.Identity, TwDAT.Status, TwDAT.EntryPoint)]
    public sealed class DataSourceServices:IDataSource {
        private static DataSourceServices _current;
        private Dictionary<string,Type> _handlerType=new Dictionary<string, Type>();
        private Dictionary<uint, _DsmEntry> _dsmEntries=new Dictionary<uint, _DsmEntry>();
        private Dictionary<uint, HandlerIdentity> _handlers=new Dictionary<uint, HandlerIdentity>();

        /// <summary>
        /// Prevents a default instance of the <see cref="DataSourceServices"/> class from being created.
        /// </summary>
        private DataSourceServices() {
        }

        #region IDataSource Members

        /// <summary>
        /// Every Source is required to have a single entry point called DS_Entry. DS_Entry is only called by the Source Manager.
        /// </summary>
        /// <param name="appId">This points to a TwIdentity structure, allocated by the application, that describes the
        /// application making the call. One of the fields in this structure, called Id, is an arbitrary and
        /// unique identifier assigned by the Source Manager to tag the application as a unique TWAIN
        /// entity. The Source Manager maintains a copy of the application’s identity structure, so the
        /// application must not modify that structure unless it first breaks its connection with the Source
        /// Manager,then reconnects to cause the Source Manager to store the new, modified identity.</param>
        /// <param name="dg">The Data Group of the operation triplet. Currently, only DG_CONTROL, DG_IMAGE, and DG_AUDIO are defined.</param>
        /// <param name="dat">The Data Argument Type of the operation triplet.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        public TwRC ProcessRequest(TwIdentity appId, TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
            try {
                if(dg==TwDG.Control) {
                    switch(dat) {
                        case TwDAT.Identity:
                            return this._IdentityControlProcessRequest(appId, msg, data);
                        case TwDAT.Status:
                            return this._StatusControlProcessRequest(appId, msg, data);
                        case TwDAT.EntryPoint:
                            return this._EntryPointControlProcessRequest(appId, msg, Marshal.PtrToStructure(data, typeof(TwEntryPoint)) as TwEntryPoint);
                    }
                }
                this._SetConditionCode(appId, TwCC.Success);
                return this._handlers[appId.Id].ProcessRequest(dg, dat, msg, data);
            } catch(DataSourceException ex) {
                DataSourceServices._ToLog(ex);
                this._SetConditionCode(appId, ex.ConditionCode);
                return ex.ReturnCode;
            } catch(Exception ex) {
                DataSourceServices._ToLog(ex);
            }
            this._SetConditionCode(appId, TwCC.OperationError);
            return TwRC.Failure;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current instance of a Data Source.
        /// </summary>
        /// <value>
        /// The current instance of IDataSource.
        /// </value>
        public static IDataSource Current {
            get {
                if(DataSourceServices._current==null) {
                    DataSourceServices._current=new DataSourceServices();
                }
                return DataSourceServices._current;
            }
        }

        /// <summary>
        /// Get current instance of DSM entry points.
        /// </summary>
        private static Dictionary<uint, _DsmEntry> DsmEntries {
            get {
                return DataSourceServices._current._dsmEntries;
            }
        }

        /// <summary>
        /// Get instance of current handler.
        /// </summary>
        private static Dictionary<uint, HandlerIdentity> Handlers {
            get {
                return DataSourceServices._current._handlers;
            }
        }

        /// <summary>
        /// Get a type of handler.
        /// </summary>
        private Type HandlerType {
            get {
                var _location = this._GetLocation();
                if(!this._handlerType.ContainsKey(_location)) {
                    foreach(var _file in Directory.GetFiles(this._GetLocation(), "*.dll")) {
                        try {
                            var _ds=Attribute.GetCustomAttribute(Assembly.LoadFrom(_file), typeof(DataSourceAttribute), false) as DataSourceAttribute;
                            if(_ds!=null&&_ds.Type!=null&&_ds.Type.GetInterface(typeof(IDataSource).FullName)!=null) {
                                this._handlerType.Add(_location,_ds.Type);
                                this.MaxConnectionCount=_ds.MaxConnectionCount;
                                break;
                            }
                        } catch(Exception ex) {
                            DataSourceServices._ToLog(ex);
                        }
                    }

                }
                return this._handlerType[_location];
            }
        }

        /// <summary>
        /// Get or set a c ondition code of last operation.
        /// </summary>
        private TwCC ConditionCode {
            get;
            set;
        }

        /// <summary>
        /// Get the maximum number of connections.
        /// </summary>
        private int MaxConnectionCount {
            get;
            set;
        }

        #endregion

        private static void _ToLog(Exception ex) {
            try {
                Debug.WriteLine(string.Empty);
                Debug.WriteLine("========== BEGIN EXCEPTION ==========");
                for(var _ex = ex; _ex!=null; _ex=_ex.InnerException) {
                    Debug.WriteLine(string.Empty);
                    Debug.WriteLine("{0}: {1}",_ex.GetType().Name,_ex.Message);
                    Debug.WriteLine(_ex.StackTrace);
                }
                Debug.WriteLine(string.Empty);
                Debug.WriteLine("========== END EXCEPTION ==========");
                Debug.WriteLine(string.Empty);
            } catch {
            }
        }

        internal static TwRC DsmInvoke(TwIdentity appId, TwMSG msg) {
            if(!DataSourceServices.DsmEntries.ContainsKey(appId.Id)) {
                DataSourceServices.DsmEntries.Add(appId.Id, _DsmEntry.Create());
            }
            return DataSourceServices.DsmEntries[appId.Id].DsmRaw(DataSourceServices.Handlers[appId.Id].DS.IndentityPointer, DataSourceServices.Handlers[appId.Id].Application.IndentityPointer, TwDG.Control, TwDAT.Null, msg, IntPtr.Zero);
        }

        /// <summary>
        /// Get location of a data source.
        /// </summary>
        /// <returns>Path to a data source directory.</returns>
        private string _GetLocation() {
            foreach(var _frame in new StackTrace().GetFrames()) {
                var _method=_frame.GetMethod();
                for(var _location=_method.Module.FullyQualifiedName; Path.GetExtension(_location).ToLower()==".ds"&&_method.Name=="DS_Entry"; ) {
                    return Path.GetDirectoryName(_location);
                }
            }
            return Path.GetDirectoryName(this.GetType().Assembly.Location);
        }

        /// <summary>
        /// Get the identity structure for the Source.
        /// </summary>
        /// <returns>Instance of TwIdentity.</returns>
        private TwIdentity _GetDSIdentity() {
            var _asm=this.HandlerType.Assembly;
            var _ds=Attribute.GetCustomAttribute(_asm,typeof(DataSourceAttribute),false) as DataSourceAttribute;
            var _identity = (_ds.IdentityProvider!=null ? Activator.CreateInstance(_ds.IdentityProvider) as IIdentityProvider : null)?.Identity??new _DefaultIdentityProvider(_asm).Identity;
            var _groups = Attribute.GetCustomAttribute(this.HandlerType,typeof(SupportedGroupsAttribute),true) as SupportedGroupsAttribute;
            var _protocol = _groups?.ProtocolVersion??new Version(2,3);

            return new TwIdentity(0,
                new TwVersion((ushort)_identity.Version.Major,(ushort)_identity.Version.Minor,_ds.Language,_ds.Country,_ds.Type.GUID.ToString()),
                (ushort)_protocol.Major,
                (ushort)_protocol.Minor,
                _groups?.SupportedGroups??TwDG.DS2,
                _identity.Company,
                _identity.ProductFamily,
                _identity.ProductName);
        }

        private void _SetConditionCode(TwIdentity appId, TwCC cc) {
            if(this._handlers.ContainsKey(appId.Id)) {
                this._handlers[appId.Id].ConditionCode=cc;
            } else {
                this.ConditionCode=cc;
            }
        }

        /// <summary>
        /// DG_CONTROL / DAT_IDENTITY / MSG_GET
        /// This operation triplet is generated only by the Source Manager and is sent to the Source. It returns
        /// the identity structure for the Source.
        /// 
        /// DG_CONTROL / DAT_IDENTITY / MSG_OPENDS
        /// Opens the Source for operation.
        /// 
        /// DG_CONTROL / DAT_IDENTITY / MSG_CLOSEDS
        /// Closes the Source so it can be unloaded from memory. The Source responds by doing its
        /// shutdown and clean-up activities needed to ensure the heap will be “clean” after the Source is
        /// unloaded.Under Windows, the Source will only be unloaded if the connection with the last
        /// application accessing it is about to be broken.The Source will know this by its internal “connect
        /// count” that should be maintained by any Source that supports multiple application connects.
        /// </summary>
        /// <param name="appId">This points to a TwIdentity structure.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _IdentityControlProcessRequest(TwIdentity appId, TwMSG msg, IntPtr data) {
            TwIdentity _identity=(TwIdentity)Marshal.PtrToStructure(data, typeof(TwIdentity));
            switch(msg) {
                case TwMSG.Get:
                    Marshal.StructureToPtr(this._GetDSIdentity(), data, true);
                    return TwRC.Success;
                case TwMSG.OpenDS:
                    if(this._handlers.Count<this.MaxConnectionCount) {
                        this._handlers.Add(appId.Id, new HandlerIdentity(appId, _identity, Activator.CreateInstance(this.HandlerType) as IDataSource));
                        return TwRC.Success;
                    }
                    throw new DataSourceException(TwRC.Failure, TwCC.MaxConnections);
                case TwMSG.CloseDS:
                    try {
                        this._handlers[appId.Id].Dispose();
                    } finally {
                        this._handlers.Remove(appId.Id);
                        if(this._dsmEntries.ContainsKey(appId.Id)) {
                            this._dsmEntries.Remove(appId.Id);
                        }
                    }
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_STATUS / MSG_GET
        /// Returns the current Condition Code for the specified Source.
        /// </summary>
        /// <param name="appId">This points to a TwIdentity structure.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// <param name="data">The pData parameter is a pointer to the data (a variable or, more
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.</param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _StatusControlProcessRequest(TwIdentity appId, TwMSG msg, IntPtr data) {
            TwStatus _status=(TwStatus)Marshal.PtrToStructure(data, typeof(TwStatus));
            switch(msg) {
                case TwMSG.Get:
                    _status.ConditionCode=this._handlers.ContainsKey(appId.Id)?this._handlers[appId.Id].ConditionCode:this.ConditionCode;
                    Marshal.StructureToPtr(_status, data, true);
                    this._SetConditionCode(appId, TwCC.Success);
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        /// <summary>
        /// DG_CONTROL / DAT_ENTRYPOINT / MSG_SET
        /// The TWAIN 2.0 Source Manager issues this command to Sources (that set DF_DS2) prior of any
        /// other command sent by the Application.In most cases it will immediately precede the call to
        /// DG_CONTROL / DAT_IDENTITY / MSG_OPEN.
        /// TWAIN 1.x Sources must continue to find and load the Source Manager DSM_Entry on
        /// their own.
        /// </summary>
        /// <param name="appId">This points to a TwIdentity structure.</param>
        /// <param name="msg">The Message of the operation triplet.</param>
        /// typically, a structure) that will be used according to the action specified by the operation
        /// triplet.
        /// <param name="entry"></param>
        /// <returns>TWAIN Return Codes.</returns>
        private TwRC _EntryPointControlProcessRequest(TwIdentity appId, TwMSG msg, TwEntryPoint entry) {
            switch(msg) {
                case TwMSG.Set:
                    DataSourceServices.Memory._SetEntryPoints(entry);
                    this._dsmEntries.Add(appId.Id, _DsmEntry.Create(entry.DSM_Entry));
                    return TwRC.Success;
            }
            throw new DataSourceException(TwRC.Failure, TwCC.BadProtocol);
        }

        private sealed class HandlerIdentity:IDisposable {

            public HandlerIdentity(TwIdentity appId, TwIdentity identity, IDataSource handler) {
                this.DS=identity;
                this.Application=appId;
                this.Handler=handler;
                this.ConditionCode=TwCC.Success;
            }

            public TwRC ProcessRequest(TwDG dg, TwDAT dat, TwMSG msg, IntPtr data) {
                return this.Handler.ProcessRequest(this.Application.Identity, dg, dat, msg, data);
            }

            public _TwIdentityCore DS {
                get;
                private set;
            }

            public _TwIdentityCore Application {
                get;
                private set;
            }

            public IDataSource Handler {
                get;
                private set;
            }

            public TwCC ConditionCode {
                get;
                set;
            }

            #region IDisposable Members

            public void Dispose() {
                try {
                    this.DS.Dispose();
                    this.Application.Dispose();
                    for(var _handler=this.Handler as IDisposable; _handler!=null; ) {
                        _handler.Dispose();
                        break;
                    }
                } catch {
                }
            }

            #endregion
        }

        private sealed class _TwIdentityCore:IDisposable {
            private IntPtr _hIdentity=IntPtr.Zero;
            private IntPtr _pIdentity=IntPtr.Zero;

            private _TwIdentityCore() {
            }

            public static implicit operator _TwIdentityCore(TwIdentity value) {
                return new _TwIdentityCore {
                    Identity=value
                };
            }

            #region IDisposable Members

            public void Dispose() {
                try {
                    if(this._hIdentity!=IntPtr.Zero) {
                        DataSourceServices.Memory.Free(this._hIdentity);
                    }
                } catch {
                }
            }

            #endregion

            private IntPtr IdentityHandle {
                get {
                    if(this._hIdentity==IntPtr.Zero) {
                        this._hIdentity=DataSourceServices.Memory.Alloc(Marshal.SizeOf(typeof(TwIdentity)));
                    }
                    return this._hIdentity;
                }
            }

            public IntPtr IndentityPointer {
                get {
                    if(this._pIdentity==IntPtr.Zero) {
                        this._pIdentity=DataSourceServices.Memory.Lock(this.IdentityHandle);
                        DataSourceServices.Memory.Unlock(this.IdentityHandle);
                        Marshal.StructureToPtr(this.Identity, this._pIdentity, true);
                    }
                    return this._pIdentity;
                }
            }

            public TwIdentity Identity {
                get;
                private set;
            }
        }

        private sealed class _DefaultIdentityProvider:IIdentityProvider {
            private Assembly _asm = null;

            public _DefaultIdentityProvider(Assembly assembly) {
                this._asm=assembly;
            }

            public Identity Identity {
                get {
                    return new Identity {
                        Company=(Attribute.GetCustomAttribute(_asm,typeof(AssemblyCompanyAttribute),false) as AssemblyCompanyAttribute)?.Company??"SARAFF SOFTWARE",
                        ProductFamily="TWAIN DS Class Library",
                        ProductName=(Attribute.GetCustomAttribute(_asm,typeof(AssemblyProductAttribute),false) as AssemblyProductAttribute)?.Product??_asm.GetName().Name,
                        Version=new Version((Attribute.GetCustomAttribute(_asm,typeof(AssemblyFileVersionAttribute),false) as AssemblyFileVersionAttribute)?.Version??"1.0.0.0")
                    };
                }
            }
        }

        /// <summary>
        /// Точки входа для работы с DSM.
        /// </summary>
        private sealed class _DsmEntry {

            /// <summary>
            /// Инициализирует новый экземпляр класса <see cref="_DsmEntry"/>.
            /// </summary>
            /// <param name="ptr">Указатель на DSM_Entry.</param>
            private _DsmEntry(IntPtr ptr) {
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        this.DsmRaw=_DsmEntry.CreateDelegate<_DsmCallback>(ptr);
                        break;
                }
            }

            /// <summary>
            /// Создает и возвращает новый экземпляр класса <see cref="_DsmEntry"/>.
            /// </summary>
            /// <returns>Экземпляр класса <see cref="_DsmEntry"/>.</returns>
            public static _DsmEntry Create() {
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        return new _DsmEntry(_DsmEntry.GetProcAddress(_DsmEntry.GetModuleHandle("twain_32.dll"), "DSM_Entry"));
                }
            }

            /// <summary>
            /// Создает и возвращает новый экземпляр класса <see cref="_DsmEntry"/>.
            /// </summary>
            /// <param name="ptr">Указатель на DSM_Entry.</param>
            /// <returns>Экземпляр класса <see cref="_DsmEntry"/>.</returns>
            public static _DsmEntry Create(IntPtr ptr) {
                return new _DsmEntry(ptr);
            }

            /// <summary>
            /// Приводит указатель к требуемомы делегату.
            /// </summary>
            /// <typeparam name="T">Требуемый делегат.</typeparam>
            /// <param name="ptr">Указатель на DSM_Entry.</param>
            /// <returns>Делегат.</returns>
            private static T CreateDelegate<T>(IntPtr ptr) where T:class {
                return Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
            }
            
            #region Properties

            public _DsmCallback DsmRaw {
                get;
                private set;
            }

            #endregion

            #region import kernel32.dll

            [DllImport("kernel32.dll", CharSet=CharSet.Ansi)]
            private static extern IntPtr GetModuleHandle(string moduleName);

            [DllImport("kernel32.dll", CharSet=CharSet.Ansi, ExactSpelling=true)]
            private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            #endregion
        }

        /// <summary>
        /// Точки входа для функций управления памятью.
        /// </summary>
        public sealed class Memory {
            private static TwEntryPoint _entryPoint;

            /// <summary>
            /// Выделяет блок памяти указанного размера.
            /// </summary>
            /// <param name="size">Размер блока памяти.</param>
            /// <returns>Дескриптор памяти.</returns>
            public static IntPtr Alloc(int size) {
                if(Memory._entryPoint!=null&&Memory._entryPoint.MemoryAllocate!=null) {
                    return Memory._entryPoint.MemoryAllocate(size);
                }
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        return Memory.GlobalAlloc(0x42, size);
                }
            }

            /// <summary>
            /// Освобождает память.
            /// </summary>
            /// <param name="handle">Дескриптор памяти.</param>
            public static void Free(IntPtr handle) {
                if(Memory._entryPoint!=null&&Memory._entryPoint.MemoryFree!=null) {
                    Memory._entryPoint.MemoryFree(handle);
                    return;
                }
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        Memory.GlobalFree(handle);
                        break;
                }
            }

            /// <summary>
            /// Выполняет блокировку памяти.
            /// </summary>
            /// <param name="handle">Дескриптор памяти.</param>
            /// <returns>Указатель на блок памяти.</returns>
            public static IntPtr Lock(IntPtr handle) {
                if(Memory._entryPoint!=null&&Memory._entryPoint.MemoryLock!=null) {
                    return Memory._entryPoint.MemoryLock(handle);
                }
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        return Memory.GlobalLock(handle);
                }
            }

            /// <summary>
            /// Выполняет разблокировку памяти.
            /// </summary>
            /// <param name="handle">Дескриптор памяти.</param>
            public static void Unlock(IntPtr handle) {
                if(Memory._entryPoint!=null&&Memory._entryPoint.MemoryUnlock!=null) {
                    Memory._entryPoint.MemoryUnlock(handle);
                    return;
                }
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                    case PlatformID.MacOSX:
                        throw new NotSupportedException();
                    default:
                        Memory.GlobalUnlock(handle);
                        break;
                }
            }

            /// <summary>
            /// Заполняет нулями указанный блок памяти.
            /// </summary>
            /// <param name="dest">Указатель на блок памяти.</param>
            /// <param name="size">Размер блока памяти.</param>
            public static void ZeroMemory(IntPtr dest, IntPtr size) {
                switch(Environment.OSVersion.Platform) {
                    case PlatformID.Unix:
                        byte[] _data=new byte[size.ToInt32()];
                        Marshal.Copy(_data, 0, dest, _data.Length);
                        break;
                    case PlatformID.MacOSX:
                        throw new NotImplementedException();
                    default:
                        Memory._ZeroMemory(dest, size);
                        break;
                }
            }

            /// <summary>
            /// Устаначливает точки входа.
            /// </summary>
            /// <param name="entry">Точки входа.</param>
            internal static void _SetEntryPoints(TwEntryPoint entry) {
                Memory._entryPoint=entry;
            }

            #region import kernel32.dll

            [DllImport("kernel32.dll", ExactSpelling=true)]
            private static extern IntPtr GlobalAlloc(int flags, int size);

            [DllImport("kernel32.dll", ExactSpelling=true)]
            private static extern IntPtr GlobalLock(IntPtr handle);

            [DllImport("kernel32.dll", ExactSpelling=true)]
            private static extern bool GlobalUnlock(IntPtr handle);

            [DllImport("kernel32.dll", ExactSpelling=true)]
            private static extern IntPtr GlobalFree(IntPtr handle);

            [DllImport("kernel32.dll", EntryPoint="RtlZeroMemory", SetLastError=false)]
            private static extern void _ZeroMemory(IntPtr dest, IntPtr size);


            #endregion
        }

        /// <summary>
        /// The Source Manager entry point. Typically called by applications, with the
        /// following exceptions where a Source calls the Source Manager to communicate with an
        /// Application:
        /// DG_CONTROL / DAT_NULL / MSG_XFERREADY
        /// DG_CONTROL / DAT_NULL / MSG_CLOSEDSREQ
        /// DG_CONTROL / DAT_NULL / MSG_CLOSEDSOK
        /// DG_CONTROL / DAT_NULL / MSG_DEVICEEVENT
        /// </summary>
        /// <param name="origin">Source of message.</param>
        /// <param name="dest">Destination of message.</param>
        /// <param name="dg">Data group ID: DG_xxxx.</param>
        /// <param name="dat">Data argument type: DAT_xxxx.</param>
        /// <param name="msg">Message ID: MSG_xxxx.</param>
        /// <param name="arg">Pointer to data.</param>
        /// <returns></returns>
        public delegate TwRC _DsmCallback(IntPtr origin, IntPtr dest, TwDG dg, TwDAT dat, TwMSG msg, IntPtr arg);
    }
}
