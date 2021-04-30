using UnityEngine;

namespace MyAssets.ScriptableObjects.Variables.ValueReferences
{
    public interface IValue<T>
    {
        string GetName();

        string GetDescription();

        T GetValue(System.Type type);

        void Subscribe(OnUpdate callback);

        void Unsubscribe(OnUpdate callback);

        bool Save(string folderPath, string name = "");
    }
}