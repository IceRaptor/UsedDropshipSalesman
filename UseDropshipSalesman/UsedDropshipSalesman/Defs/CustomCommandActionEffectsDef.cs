using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Defs
{
    [JsonObject]
    public class CustomCommandActionEffectsDef
    {
        [JsonRequired]
        public UDSActionDescription Description { get; set; }

        [JsonRequired]
        public JObject[] EffectsData { get; set; }

        public List<EffectData> Effects { get; set; }

        public CustomCommandActionEffectsDef()
        {
            Effects = new();
        }

        public bool Validate()
        {
            bool isValid = true;

            if (this.Description == null || String.IsNullOrEmpty(this.Description.Id))
            {
                Mod.Log.Debug?.Log("CustomCommandActionEffectsDef has no description field or ID!");
                isValid = false;
            }
            if (String.IsNullOrEmpty(this.Description.EffectSuffix))
            {
                Mod.Log.Debug?.Log("CustomCommandActionEffectsDef description is missing an effectSuffix!");
                isValid = false;
            }
            Mod.Log.Debug?.Log($"Validating CustomCommandActionEffectsDef with id: {this.Description.Id}");

            if (this.Effects == null || this.Effects.Count == 0)
            {
                Mod.Log.Debug?.Log("Effects are not defined!");
                isValid = false;
            }

            return isValid;
        }
    }

    public record UDSActionDescription
    {
        public string Id;
        public string Name;
        public string EffectSuffix;
    }
}
