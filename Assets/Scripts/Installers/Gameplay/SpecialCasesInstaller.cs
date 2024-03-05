using SwipeMatch3.Gameplay.Settings;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace SwipeMatch3.Gameplay.Installers
{
    [CreateAssetMenu(fileName = "SpecialCasesInstaller", menuName = "Installers/SpecialCasesInstaller")]
    public class SpecialCasesInstaller : ScriptableObjectInstaller<SpecialCasesInstaller>
    {
        [SerializeField]
        private List<SpecialCase> _cases = new List<SpecialCase>();

        public override void InstallBindings()
        {
            Container.Bind<List<SpecialCase>>().FromInstance(_cases).AsSingle();
            /*foreach (var specialCase in _cases)
                Container.Bind<SpecialCase>().FromInstance(specialCase).AsSingle();*/
            //Container.Bind<SpecialCase>().FromInstance();
        }
    }
}