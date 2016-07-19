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
    /// Exception of a Data Source.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public sealed class DataSourceException:Exception {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="rc">The TWRC_xxxx.</param>
        /// <param name="cc">The TWCC_xxxx.</param>
        public DataSourceException(TwRC rc, TwCC cc) : this(rc, cc, string.Empty) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="rc">The TWRC_xxxx.</param>
        /// <param name="cc">The TWCC_xxxx.</param>
        /// <param name="message">The message.</param>
        public DataSourceException(TwRC rc, TwCC cc, string message) : base(message) {
            this.ReturnCode=rc;
            this.ConditionCode=cc;
        }

        /// <summary>
        /// Возвращает код результата операции. Get return code.
        /// </summary>
        public TwRC ReturnCode {
            get;
            private set;
        }

        /// <summary>
        /// Возвращает код состояния операции. Get condition code.
        /// </summary>
        public TwCC ConditionCode {
            get;
            private set;
        }
    }
}
