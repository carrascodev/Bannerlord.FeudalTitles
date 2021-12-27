using TaleWorlds.CampaignSystem;

namespace Bannerlord.FeudalTitles;

public class Land
{
    private string _id;
    public string Id => _id;
    private string _name;
    public string Name => _name;
    private Settlement[] _settlements;
    public Settlement[] Settlements => _settlements;

    public Land(string id, string name, Town town)
    {
        _id = id;
        _name = name;
        _settlements = new Settlement[town.Villages.Count + 1];
        _settlements[0] = town.Settlement;
        for (int i = 1; i < _settlements.Length; i++)
        {
            _settlements[i] = town.Villages[i].Settlement;
        }
    }
}