using UnityEngine;
public abstract class BaseInstallableModule
{
    public virtual void OnGUI(TSMainManager mainManager){}
    public virtual void OnGUI(){}
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Define { get; }
    public abstract bool Installed { get; }
    public abstract bool Detected { get; }
}