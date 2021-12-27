using TaleWorlds.CampaignSystem;

namespace Bannerlord.FeudalTitles;

public class Title
{
    private string _name;
    public string Name => _name;

    private string _landId;
    public string LandId => _landId;

    private Hero _owner;
    public Hero Owner => _owner;

    public Title(string name, string landId, Hero owner = null)
    {
        _name = name;
        _landId = landId;
        _owner = owner;
    }
}