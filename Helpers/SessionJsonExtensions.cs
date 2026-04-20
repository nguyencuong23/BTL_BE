using System.Text.Json;

namespace QuanLyThuVienTruongHoc.Helpers
{
    public static class SessionJsonExtensions
    {
        private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

        public static void SetJson<T>(this ISession session, string key, T value)
        {
            session.SetString(key, JsonSerializer.Serialize(value, Options));
        }

        public static T? GetJson<T>(this ISession session, string key)
        {
            var str = session.GetString(key);
            return string.IsNullOrWhiteSpace(str) ? default : JsonSerializer.Deserialize<T>(str, Options);
        }
    }
}

