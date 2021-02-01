using BehaviorDesigner.Runtime.Tasks;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    public interface IFloatValue : IValue<float>
    {
        float GetFloat();
    }
}