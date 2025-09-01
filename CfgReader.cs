using System;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class CfgReaderConfigAttribute : Attribute
{

}

public partial class CfgReader
{
    public static void LoadAllConfigs()
    {
        try
        {
            LoadAllJson();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
}
