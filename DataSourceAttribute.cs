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
    /// Define a Data Source.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Assembly,AllowMultiple=false,Inherited=false)]
    public sealed class DataSourceAttribute:Attribute {

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceAttribute"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DataSourceAttribute(Type type) {
            this.Type=type;
            this.Language=TwLanguage.RUSSIAN;
            this.Country=TwCountry.BELARUS;
            this.MaxConnectionCount=1;
        }

        /// <summary>
        /// Gets the type of a Data Source.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public Type Type {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the language.
        /// </summary>
        /// <value>
        /// The language.
        /// </value>
        public TwLanguage Language {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
        public TwCountry Country {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the maximum connection count.
        /// </summary>
        /// <value>
        /// The maximum connection count.
        /// </value>
        public int MaxConnectionCount {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the identity provider of a Data Source.
        /// </summary>
        /// <value>
        /// The identity provider of a Data Source.
        /// </value>
        public Type IdentityProvider {
            get;
            set;
        }
    }
}
