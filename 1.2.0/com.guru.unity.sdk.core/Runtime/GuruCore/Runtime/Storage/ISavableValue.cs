
using System.Collections;
using UnityEngine;

namespace Guru
{
    using System;
    using System.Collections.Generic;
    
    public interface ISavableValue
    {
        void SaveInt(int value);
        void SaveFloat(float value);
        void SaveBool(bool value);
        void SaveString(string value);
        void SaveVector2(Vector2 value);
        void SaveVector3(Vector3 value);
        void SaveVector4(Vector4 value);
        void SaveArray(string[] value);
        
        int LoadInt(int defaultValue = 0);
        float LoadFloat(float defaultValue = 0);
        bool LoadBool(bool defaultValue = false);
        string LoadString(string defaultValue = "");
        Vector3 LoadVector3(Vector3 defaultValue = new Vector3());
        Vector2 LoadVector2(Vector2 defaultValue = new Vector2());
        Vector4 LoadVector4(Vector4 defaultValue = new Vector4());
        string[] LoadArray(string[] defaultValue = null);

        void ClearValue(string key);
        bool HasKey();
    }
}