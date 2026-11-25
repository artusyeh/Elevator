using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class CabinWorkBridge
{
    const string Folder="Library/CabinWork";
    static double next;
    static CabinWorkBridge(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(EditorApplication.timeSinceStartup<next||EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        next=EditorApplication.timeSinceStartup+.25;
        string path=Folder+"/request.txt";if(!File.Exists(path))return;
        string method=File.ReadAllText(path).Trim();File.Delete(path);
        try
        {
            if(method=="Refresh"){AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);File.WriteAllText(Folder+"/result.txt","OK\nRefresh requested");return;}
            var entry=typeof(ApartmentBuilder).GetMethod(method,BindingFlags.Public|BindingFlags.Static,null,Type.EmptyTypes,null);
            if(entry==null)throw new Exception("Unknown cabin work method: "+method);
            var result=entry.Invoke(null,null);File.WriteAllText(Folder+"/result.txt","OK\n"+(result==null?"Done":result.ToString()));
        }
        catch(Exception e){File.WriteAllText(Folder+"/result.txt","ERROR\n"+e);Debug.LogException(e);}
    }
}
