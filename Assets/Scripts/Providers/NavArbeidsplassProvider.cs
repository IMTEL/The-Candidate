using UnityEngine;


[CreateAssetMenu(menuName = "Services/NavWorkProvider")]
public class NavArbeidsPlassProvider : JobSelection
{

    NavArbeidsPlass instance;
    public override IJobSelectionService CreateTypedService()
    {
        instance ??= new NavArbeidsPlass();
        return instance; 
    }
}