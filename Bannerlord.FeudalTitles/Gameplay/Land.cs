using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;

namespace Bannerlord.FeudalTitles;

public interface ILand
{
    string Id { get; set; }
    string Name { get; set; }
    string[] SettlementIds { get; set; }
    Settlement[] Settlements { get; }
}

public class Land : ILand
{
    [JsonProperty]
    public string Id { get; set; }
    [JsonProperty]
    public string Name { get; set; }
    [JsonProperty]
    public string[] SettlementIds { get; set; }

    [JsonProperty]
    public string LandOwner { get; set; }
    
    [JsonIgnore]
    public Settlement[] Settlements { get; }

    public Land()
    {
        if (SettlementIds != null)
        {
            Settlements = new Settlement[SettlementIds.Length];
            for (int i = 0; i < SettlementIds.Length; i++)
            {
                Settlements[i] = Settlement.Find(SettlementIds[i]);
                if (Settlements[i].IsTown || Settlements[i].IsCastle)
                {
                    LandOwner = Settlements[i].Owner.StringId;
                }
            }
        }
    }
}