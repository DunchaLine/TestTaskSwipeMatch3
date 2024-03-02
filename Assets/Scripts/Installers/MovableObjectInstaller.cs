using UnityEngine;
using Zenject;

[CreateAssetMenu(fileName = "MovableObjectInstaller", menuName = "Installers/MovableObjectInstaller")]
public class MovableObjectInstaller : ScriptableObjectInstaller<MovableObjectInstaller>
{
    [SerializeField]
    private MovableObjectSettings _settings;

    public override void InstallBindings()
    {
        Container.Bind<MovableObjectSettings>().FromInstance(_settings).AsCached().NonLazy();
    }
}