using System.Text.Json;

namespace CarConfigPROJECTmvc.Infrastructure
{

    public static class SessionExtensions
    {
        public static void SetJson<T>(this ISession session, string key, T value)
            => session.SetString(key, JsonSerializer.Serialize(value));

        public static T? GetJson<T>(this ISession session, string key)
        {
            var s = session.GetString(key);
            return s == null ? default : JsonSerializer.Deserialize<T>(s);
        }
    }
}
//Ova klasa omogućuje da u Session spremaš i čitaš kompleksne objekte (poput lista) tako da ih automatski pretvara u JSON i nazad