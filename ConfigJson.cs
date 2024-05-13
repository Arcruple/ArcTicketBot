using Newtonsoft.Json;

namespace ArcTicketBot {
    public struct ConfigJson {

        //Setting the token and prefix retrieval from the config.json file
        [JsonProperty("token")]
        public string Token { get; private set; }
        [JsonProperty("prefix")]
        public string Prefix { get; private set; }

    }
}
