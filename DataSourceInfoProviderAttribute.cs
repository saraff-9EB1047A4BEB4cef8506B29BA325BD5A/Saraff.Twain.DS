using System;

namespace Saraff.Twain.DS
{
    public interface IDataSourceInfo
    {
        Version Version { get; }
        string Company { get; }
        string ProductFamily { get; }
        string ProductName { get; }
    }

    /// <summary>
    /// Specifies type that is responsible to provide custom data source information.
    /// The given type should implement <see cref="IDataSourceInfo"/> interface.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DataSourceInfoProviderAttribute : Attribute
    {
        public Type Type { get; }

        public DataSourceInfoProviderAttribute(Type type)
        {
            Type = type;
        }
    }

    public class DataSourceInfo : IDataSourceInfo
    {
        public Version Version { get; set; }
        public string Company { get; set; }
        public string ProductFamily { get; set; }
        public string ProductName { get; set; }
    }
}
