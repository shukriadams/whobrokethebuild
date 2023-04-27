using System;
using System.Text.Json;

namespace Wbtb.Core.Common
{
    /// <summary>
    /// A utility class to serialize and deserialize JSON. Internal JSON handling can change.
    /// </summary>
    public static class JsonConvert
    {
        /// <summary>
        /// Serializes the specified object. Supports anonymous and dynamic types.
        /// </summary>
        /// <param name="value">The object to serialize.</param>
        /// <param name="options">Options to use for serialization.</param>
        /// <returns>
        /// A JSON representation of the serialized object.
        /// </returns>
        public static string SerializeObject(object value)
        {
            return JsonSerializer.Serialize(value);
        }

        /// <summary>
        /// Deserializes an object from the specified text.
        /// </summary>
        /// <param name="text">The text text.</param>
        /// <param name="targetType">The required target type.</param>
        /// <param name="options">Options to use for deserialization.</param>
        /// <returns>
        /// An instance of an object representing the input data.
        /// </returns>
        public static object DeserializeObject(string text, Type targetType = null)
        {
            return JsonSerializer.Deserialize(text, targetType, new JsonSerializerOptions { });
        }

        public static T DeserializeObject<T>(string text)
        {
            return (T)JsonSerializer.Deserialize(text, typeof(T), new JsonSerializerOptions { });
        }
    }
}