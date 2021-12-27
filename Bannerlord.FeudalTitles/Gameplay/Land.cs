using TaleWorlds.CampaignSystem;

namespace Bannerlord.FeudalTitles;

public class Land
{
    private string _id;
    public string Id => _id;
    private string _name;
    public string Name => _name;
    
    private string[] _settlements;
    public Settlement[] Settlements { get; }

    public Land()
    {
        Settlements = new Settlement[_settlements.Length];
        for (int i = 0; i < _settlements.Length; i++)
        {
            Settlements[i] = Settlement.Find(_settlements[i]);
        }
    }
}