using TaleWorlds.CampaignSystem;

namespace Bannerlord.FeudalTitles;

public class Title
{
    private string _name;
    public string Name => _name;

    private string _landId;
    public string LandId => _landId;

    public Title(string name, string landId)
    {
        _name = name;
        _landId = landId;
    }
}