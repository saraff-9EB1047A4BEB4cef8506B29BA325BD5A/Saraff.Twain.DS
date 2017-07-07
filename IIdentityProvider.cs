using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Saraff.Twain.DS {

    /// <summary>
    /// Represents identity provider of a Data Source.
    /// </summary>
    public interface IIdentityProvider {

        /// <summary>
        /// Gets the identity.
        /// </summary>
        /// <value>
        /// The identity.
        /// </value>
        Identity Identity {
            get;
        }
    }

    /// <summary>
    /// Identity of a Data Source.
    /// </summary>
    public sealed class Identity {

        /// <summary>
        /// Gets or sets the version.
        /// </summary>
        /// <value>
        /// The version.
        /// </value>
        public Version Version {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the company.
        /// </summary>
        /// <value>
        /// The company.
        /// </value>
        public string Company {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the product family.
        /// </summary>
        /// <value>
        /// The product family.
        /// </value>
        public string ProductFamily {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public string ProductName {
            get;
            set;
        }
    }
}
