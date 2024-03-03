using UnityEngine;
using Zenject;

/// <summary>
/// Инсталлер ScriptableObject'a для настроек пула объектов бэкграунда
/// </summary>
[CreateAssetMenu(fileName = "BackgroundInstaller", menuName = "Installers/BackgroundInstaller")]
public class BackgroundInstaller : ScriptableObjectInstaller<BackgroundInstaller>
{
    [SerializeField]
    private BackgroundPoolSettings _settings;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<IBackgroundPoolSettings>().FromInstance(_settings).AsCached();
    }
}