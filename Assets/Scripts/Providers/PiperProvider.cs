using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Services/Piper")]
public class PiperProvider : TTSserviceProvider
{

    public override TTService CreateTypedService()
        => PiperManager.GetInstance();

    public override IEnumerable<OptionBase> GetTTSOptions()
    {
        throw new System.NotImplementedException();
    }
}


