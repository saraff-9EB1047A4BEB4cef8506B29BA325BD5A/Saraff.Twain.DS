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

#if !NETCOREAPP

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Linq;
using System.IO;
using System.Reflection;


namespace Saraff.Twain.DS {

    /// <summary>
    /// The Data Source Installer.
    /// </summary>
    /// <seealso cref="System.Configuration.Install.Installer" />
    public partial class DataSourceInstaller:Installer {
        private const string _x86=@"twain_32";
        private const string _x64=@"twain_64";
        private string _twainDirectory=null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceInstaller"/> class.
        /// </summary>
        public DataSourceInstaller() {
            InitializeComponent();
        }

        /// <summary>
        /// Вызывает событие <see cref="E:System.Configuration.Install.Installer.AfterInstall" />.
        /// </summary>
        /// <param name="savedState"><see cref="T:System.Collections.IDictionary" />, содержащий состояние компьютера после завершения установки всех установщиков из свойства <see cref="P:System.Configuration.Install.Installer.Installers" />.</param>
        protected override void OnAfterInstall(IDictionary savedState) {
            base.OnAfterInstall(savedState);
            Directory.CreateDirectory(this.TwainDirectory);
            foreach(var _file in Directory.GetFiles(this.CurrentDirectory)) {
                File.Copy(_file, Path.Combine(this.TwainDirectory, Path.GetFileName(_file)), true);
            }
            var _ds=Path.Combine(this.TwainDirectory,Path.GetFileName(this.GetType().Assembly.Location));
            File.Move(_ds, Path.ChangeExtension(_ds, ".ds"));
        }

        /// <summary>
        /// Вызывает событие <see cref="E:System.Configuration.Install.Installer.BeforeUninstall" />.
        /// </summary>
        /// <param name="savedState"><see cref="T:System.Collections.IDictionary" />, содержащий состояние компьютера до отмены установок установщиками из свойства <see cref="P:System.Configuration.Install.Installer.Installers" />.</param>
        protected override void OnBeforeUninstall(IDictionary savedState) {
            base.OnBeforeUninstall(savedState);
            foreach(var _file in Directory.GetFiles(this.TwainDirectory, "*.ds")) {
                File.Delete(Path.Combine(this.TwainDirectory, _file));
            }
            Directory.Delete(this.TwainDirectory, true);
        }

        private string TwainDirectory {
            get {
                if(this._twainDirectory==null) {
                    foreach(var _file in Directory.GetFiles(this.CurrentDirectory,"*.dll")) {
                        foreach(DataSourceAttribute _attr in Assembly.LoadFrom(_file).GetCustomAttributes(typeof(DataSourceAttribute), false)) {
                            return this._twainDirectory=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), this.Is64Bit?DataSourceInstaller._x64:DataSourceInstaller._x86, _attr.Type.GUID.ToString().ToUpper());
                        }
                    }
                    throw new InvalidOperationException("Не найден файл сборки, содержащий источник данных.");
                }
                return this._twainDirectory;
            }
        }

        private string CurrentDirectory {
            get {
                return Path.GetDirectoryName(this.GetType().Assembly.Location);
            }
        }

        /// <summary>
        /// Gets a value indicating whether is 64 bit OS.
        /// </summary>
        /// <value>
        ///   <c>true</c> if is 64 bit OS; otherwise, <c>false</c>.
        /// </value>
        protected virtual bool Is64Bit {
            get {
                return Environment.Is64BitProcess;
            }
        }
    }
}

#endif
