using AbcWarehouse.Plugin.Widgets.UniFi.Models;
using Nop.Core.Configuration;

namespace AbcWarehouse.Plugin.Widgets.UniFi
{
    public class UniFiSettings : ISettings
    {
        public string PartnerId { get; private set; }

        public static UniFiSettings FromModel(ConfigModel model)
        {
            return new UniFiSettings()
            {
                PartnerId = model.PartnerId,
            };
        }

        public ConfigModel ToModel()
        {
            return new ConfigModel
            {
                PartnerId = PartnerId,
            };
        }
    }
}