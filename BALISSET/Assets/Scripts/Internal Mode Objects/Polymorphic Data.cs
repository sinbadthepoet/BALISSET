using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public abstract class PolymorphicData
{
    // Finds all subclasses of the provided base class type
    public static List<Type> GetAllDerivedTypes<T>() where T : PolymorphicData
    {
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(T)))
            .ToList();
    }
}