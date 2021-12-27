using System.Collections.Generic;

namespace Bannerlord.FeudalTitles;

public class LandManager : GenericSingleton<LandManager>
{
    public List<Land> Lands;

    public LandManager()
    {
        Lands = new List<Land>();
        Load();
    }

    private void Load()
    {
        
    }
}