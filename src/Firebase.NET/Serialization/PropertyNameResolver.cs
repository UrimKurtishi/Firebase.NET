using System.Text;
using System.Globalization;
using Newtonsoft.Json.Serialization;

namespace Firebase.NET.Serialization
{
    class PropertyNameResolver : DefaultContractResolver
    {
        private readonly string _separator;

        public PropertyNameResolver(string separator)
        {
            _separator = separator;
        }

        public PropertyNameResolver()
        {
            _separator = "_";
        }

        /// <summary>
        /// Converts every uppercase letter (not the first char) to lowercase 
        /// and inserts an underscore between that and its previous character
        /// </summary>
        /// <returns></returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return propertyName;

            var sb = new StringBuilder();
            for (var i = 0; i < propertyName.Length; i++)
            {
                var flag = i + 1 < propertyName.Length;
                if (char.IsUpper(propertyName[i]) || !flag || char.IsUpper(propertyName[i + 1]))
                {
                    var ch = char.ToLower(propertyName[i], CultureInfo.InvariantCulture);
                    sb.Append(ch);
                    if (flag && char.IsUpper(propertyName[i + 1]))
                        sb.Append(_separator);
                }
                else
                    sb.Append(propertyName[i]);
            }

            return sb.ToString();
        }
    }
}
