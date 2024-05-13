using System.Text.Json;

namespace ArcTicketBot.Configurations {
    public class ConfigurationManager {

        private string filePath = "SetupConfig.json";
        private ConfigData configData;

        public ConfigurationManager() {
            LoadConfig();
        }

        private void LoadConfig() {

            try {
                string jsonString = File.ReadAllText(filePath);
                configData = JsonSerializer.Deserialize<ConfigData>(jsonString);
            }catch(FileNotFoundException) {
                configData = new ConfigData();
                SaveConfig();
            }

        }

        public void SaveConfig() {
            string jsonString = JsonSerializer.Serialize(configData);
            File.WriteAllText(filePath, jsonString);
        }

        public List<string> GetStaffRoles() {
            return configData.StaffRoles;
        }

        public void AddStaffRole(string role) {
            configData.StaffRoles.Add(role);
            SaveConfig();
        }

        public void RemoveStaffRole(string role) {
            configData.StaffRoles.Remove(role);
            SaveConfig();
        }

        public string GetLogChannel() {
            return configData.LogChannel;
        }

        public void SetLogChannel(string channel) {
            configData.LogChannel = channel;
            SaveConfig();
        }

        public string GetTicketCategory() {
            return configData.TicketCategory;
        }

        public void SetTicketCategory(string category) {
            configData.TicketCategory = category;
            SaveConfig();
        }

    }

    public class ConfigData {

        public List<string> StaffRoles { get; set; }
        public string LogChannel { get; set; }
        public string TicketCategory { get; set;}

        public ConfigData() {

            StaffRoles = new List<string>();
            LogChannel = "";
            TicketCategory = "";

        }

    }

}
