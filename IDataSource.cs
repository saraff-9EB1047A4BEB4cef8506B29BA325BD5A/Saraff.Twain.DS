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
    /// Provide a Data Source. Traditional device drivers are now included with the
    /// Source software and do not need to be shipped by applications.
    /// </summary>
    public interface IDataSource {

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
        /// <returns></returns>
        TwRC ProcessRequest(TwIdentity appId, TwDG dg, TwDAT dat, TwMSG msg, IntPtr data);
    }
}
