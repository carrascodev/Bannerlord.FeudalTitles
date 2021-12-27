using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace Bannerlord.FeudalTitles;

public class LandManager : GenericSingleton<LandManager>
{
    public List<Land> Lands;

    public LandManager()
    {
        Lands = new List<Land>();
    }

    public void Initialize()
    {
        if (MBGameManager.Current.IsLoaded)
        {
            LoadLands();
        }
    }

    public void LoadLands()
    {
        string path = Path.Combine(BasePath.Name, "Modules", 
            "Bannerlord.FeudalTitles", "assets", "Lands.json");
        string file = File.ReadAllText(path);
        var json = JObject.Parse(file);
        Lands = json["Lands"].ToObject<List<Land>>();
    }
}