using System.Collections.Generic;
using System.IO;
using System.Linq;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.FeudalTitles;

public class LandManager : GenericSingleton<LandManager>
{
    private List<Land> _lands;
    private string _path;

    public LandManager()
    {
        _lands = new List<Land>();
        _path = Path.Combine(BasePath.Name, "Modules", 
            "Bannerlord.FeudalTitles", "assets", "Lands.json");
    }

    public void Initialize()
    {
        if (Campaign.Current != null)
        {
            LoadLands();
        }
    }

    public void LoadLands()
    {
        
        string file = File.ReadAllText(_path);
        var json = JObject.Parse(file);
        if (json != null)
        {
            _lands = json["Lands"].ToObject<List<Land>>();
        }
    }

    public void SaveLands()
    {
        var jObject = JObject.FromObject(new {Lands = _lands});
        File.WriteAllText(_path, jObject.ToString());
    }

    public Land GetLand(string id)
    {
        return _lands.FirstOrDefault(l => l.Id == id);
    }

    public void AddLand(Land land)
    {
        _lands.Add(land);
    }

    public void GenerateLands()
    {
        foreach (var settlement in Campaign.Current.Settlements.Where(t => t.IsTown || t.IsCastle))
        {
            var land = new Land()
            {
                Id = "land_of_" + settlement.StringId,
                Name = "Land of " + settlement.Name.ToString().Replace(" Castle",""),
            };

            land.SettlementIds = new string[settlement.BoundVillages.Count + 1];
            land.SettlementIds[0] = settlement.StringId;
            land.LandOwner = settlement.Owner.StringId;

            for (int i = 1; i < settlement.BoundVillages.Count; i++)
            {
                var village = settlement.BoundVillages[i];
                land.SettlementIds[i] = village.Settlement.StringId;
            }

            _lands.Add(land);
        }
        
        SaveLands();
    }
}