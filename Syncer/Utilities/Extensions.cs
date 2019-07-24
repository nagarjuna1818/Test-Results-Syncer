namespace Syncer.Utilities
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using System.Net.Http;
    using System.Threading.Tasks;

    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Extension method.
        /// </summary>
        /// <param name="source">Source.</param>
        /// <param name="formatting">Formatting.</param>
        /// <param name="jsonSerializerSettings">Settings.</param>
        /// <param name="camelizePropertyNames">CamelizePropertyNames.</param>
        /// <returns>String.</returns>
        public static string ToJson(this object source, Formatting formatting = Formatting.Indented, JsonSerializerSettings jsonSerializerSettings = null, bool camelizePropertyNames = false)
        {
            if (jsonSerializerSettings == null)
            {
                jsonSerializerSettings = new JsonSerializerSettings
                {
                    Error = (o, e) =>
                    {
                        var currentError = e.ErrorContext.Error.Message;
                        e.ErrorContext.Handled = true;
                    },
                    ContractResolver = camelizePropertyNames ? new CamelCasePropertyNamesContractResolver() : new DefaultContractResolver(),
                };
            }

            return source != null ? JsonConvert.SerializeObject(source, formatting, jsonSerializerSettings) : string.Empty;
        }

        /// <summary>
        /// Extension method.
        /// </summary>
        /// <typeparam name="T">DataType.</typeparam>
        /// <param name="content">Content.</param>
        /// <returns>Value of Input DataType.</returns>
        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = await content.ReadAsStringAsync().ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(dataAsString);
        }
    }
}