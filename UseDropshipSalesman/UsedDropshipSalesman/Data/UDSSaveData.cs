using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UsedDropshipSalesman.Data
{
    public class UDSSaveData
    {
        public Dictionary<String, List<String>> PurchasedUpgrades = new Dictionary<String, List<String>>();
        public String CurrentDropshipId = ModConsts.FALLBACK_DROPSHIP_ID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append($"CurrentDropship: {CurrentDropshipId}");
            foreach (KeyValuePair<String, List<String>> kvp in PurchasedUpgrades)
            {
                sb.Append($"Upgrades for dropship: {kvp.Value}: '{String.Join(", ", kvp.Value)}'");
            }
            return sb.ToString();
        }

        public void Reset()
        {
            PurchasedUpgrades = new Dictionary<string, List<string>>();
            CurrentDropshipId = ModConsts.FALLBACK_DROPSHIP_ID;
        }
    }
}
