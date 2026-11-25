using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static void MailboxContents(Transform root,BuildingDirectory directory,Material face,Material housing)
    {
        foreach(Transform child in root.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        var rows=directory.units.GroupBy(u=>u.floor).OrderBy(g=>g.Key).ToArray();int columns=rows.Max(g=>g.Count());
        float width=columns*.34f+.08f,height=rows.Length*.215f+.10f;var p=root.localPosition;p.x=1.755f;root.localPosition=p;
        var body=B("Mailbox Housing",root,new Vector3(0,.66f,.11f),new Vector3(width,height,.27f),"Elevator Frame Metal",true,.007f);body.GetComponent<Renderer>().sharedMaterial=housing;
        for(int row=0;row<rows.Length;row++)
        {
            int column=0;foreach(var unit in rows[row].OrderBy(u=>u.number))
            {
                var box=G("Mailbox "+unit.number,root);box.localPosition=new Vector3((column-(columns-1)*.5f)*.34f,.66f+(row-(rows.Length-1)*.5f)*.215f,-.043f);column++;
                string finishPath=Base+"Materials/Mailbox "+unit.number+" Finish.mat";var finish=AssetDatabase.LoadAssetAtPath<Material>(finishPath);if(finish==null){finish=new Material(face);AssetDatabase.CreateAsset(finish,finishPath);}else finish.CopyPropertiesFromMaterial(face);
                int number=int.Parse(unit.number);float tone=.90f+(number%7)*.025f;finish.SetColor("_BaseColor",face.GetColor("_BaseColor")*tone);var offset=new Vector2((number%7)*.131f,(number%5)*.173f);foreach(string property in new[]{"_BaseMap","_BumpMap","_MetallicGlossMap"})finish.SetTextureOffset(property,offset);EditorUtility.SetDirty(finish);
                var data=box.gameObject.AddComponent<MailboxUnit>();data.apartmentNumber=unit.number;data.directory=directory;
                var lower=B("Door Face",box,new Vector3(0,-.028f,0),new Vector3(.328f,.145f,.018f),"Elevator Frame Metal",false,.0015f);lower.GetComponent<Renderer>().sharedMaterial=finish;
                var upper=B("Slot Hood",box,new Vector3(0,.079f,0),new Vector3(.328f,.043f,.018f),"Elevator Frame Metal",false,.0015f);upper.GetComponent<Renderer>().sharedMaterial=finish;
                foreach(float side in new[]{-1f,1f}){var strip=B("Slot Return",box,new Vector3(side*.137f,.052f,0),new Vector3(.054f,.014f,.018f),"Elevator Frame Metal",false,.001f);strip.GetComponent<Renderer>().sharedMaterial=finish;}
                B("Letter Flap",box,new Vector3(0,.052f,.010f),new Vector3(.219f,.019f,.006f),"Black Rubber",false,.001f);
                B("Label Holder",box,new Vector3(-.027f,-.038f,-.011f),new Vector3(.151f,.050f,.004f),"Elevator Frame Metal",false,.001f);
                var material=Plain("Mailbox "+unit.number+" Insert",Color.white,.06f);material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/Mailboxes/"+unit.number+".png"));
                Q("Number Insert",box,new Vector3(-.027f,-.038f,-.0135f),new Vector2(.143f,.043f),material,Quaternion.identity);
                var wear=Plain("Mailbox "+unit.number+" Wear",Color.white,.15f,.2f);wear.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(BuildingBase+"Textures/Mailboxes/"+unit.number+" Wear.png"));
                wear.SetFloat("_Surface",1);wear.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);wear.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);wear.SetFloat("_ZWrite",0);wear.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");wear.SetOverrideTag("RenderType","Transparent");wear.renderQueue=3000;
                Q("Surface Wear",box,new Vector3(0,0,-.0096f),new Vector2(.328f,.201f),wear,Quaternion.identity);
                Rod("Cylinder Lock",box,new Vector3(.122f,-.062f,-.009f),new Vector3(.122f,-.062f,-.017f),.010f,"Elevator Frame Metal");
                B("Key Slot",box,new Vector3(.122f,-.062f,-.0176f),new Vector3(.002f,.008f,.001f),"Black Rubber",false,.0001f);
                foreach(float y in new[]{-.063f,.063f})Rod("Door Hinge",box,new Vector3(-.158f,y-.015f,-.005f),new Vector3(-.158f,y+.015f,-.005f),.003f,"Elevator Frame Metal");
            }
        }
        foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>()){renderer.lightmapIndex=-1;renderer.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;}
    }
    public static string RepairMailboxBank()
    {
        EnsureApartment();var root=GameObject.Find("Apartment").transform;
        var units=root.GetComponentsInChildren<TextMesh>().Where(t=>t.GetComponentsInParent<Transform>().Any(p=>p.name=="Apartment Door")&&int.TryParse(t.text,out int n)&&n>=200&&n<700).Select(t=>t.text).Distinct().OrderBy(n=>n).Select(n=>new BuildingDirectory.Unit{floor=int.Parse(n)/100,number=n}).ToArray();
        if(units.Length==0)throw new Exception("No residential unit labels found; existing bank retained.");
        string path=BuildingBase+"Apartments.asset";var directory=AssetDatabase.LoadAssetAtPath<BuildingDirectory>(path);if(directory==null){directory=ScriptableObject.CreateInstance<BuildingDirectory>();AssetDatabase.CreateAsset(directory,path);}directory.units=units;EditorUtility.SetDirty(directory);
        string folder=BuildingBase+"Textures/Mailboxes/";
        foreach(var id in AssetDatabase.FindAssets("t:Texture2D",new[]{folder.TrimEnd('/')}))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(id));bool normal=importer.assetPath.Contains("Normal"),mask=importer.assetPath.Contains("Mask");
            importer.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;importer.sRGBTexture=!normal&&!mask;importer.anisoLevel=4;importer.SaveAndReimport();
        }
        var face=Plain("Mailbox Steel",new Color(.82f,.85f,.81f),.34f,1);face.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Brushed Steel Color.png"));face.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Brushed Steel Normal.png"));face.SetFloat("_BumpScale",.15f);face.EnableKeyword("_NORMALMAP");face.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"Brushed Steel Mask.png"));face.EnableKeyword("_METALLICSPECGLOSSMAP");
        var housing=Plain("Mailbox Housing",new Color(.24f,.28f,.25f),.24f,.65f);housing.SetTexture("_BaseMap",face.GetTexture("_BaseMap"));
        string canonical=Base+"Prefabs/Mailboxes.prefab";var prefab=PrefabUtility.LoadPrefabContents(canonical);try{MailboxContents(prefab.transform,directory,face,housing);PrefabUtility.SaveAsPrefabAsset(prefab,canonical);}finally{PrefabUtility.UnloadPrefabContents(prefab);}
        MailboxContents(root.Find("Ground Floor/Lobby/Mailboxes"),directory,face,housing);SaveApartment();
        string previewPath=Base+"Prefabs/Entrance Interior.prefab";var preview=PrefabUtility.LoadPrefabContents(previewPath);
        try{foreach(var bank in preview.GetComponentsInChildren<Transform>().Where(t=>t.name=="Mailboxes").ToArray()){if(!PrefabUtility.IsAnyPrefabInstanceRoot(bank.gameObject))MailboxContents(bank,directory,face,housing);else{var p=bank.localPosition;p.x=1.755f;bank.localPosition=p;PrefabUtility.RecordPrefabInstancePropertyModifications(bank);}}PrefabUtility.SaveAsPrefabAsset(preview,previewPath);}finally{PrefabUtility.UnloadPrefabContents(preview);}
        SaveApartment();return units.Length+" mailboxes match the actual units: "+string.Join("; ",units.GroupBy(u=>u.floor).Select(g=>"floor "+g.Key+" = "+g.Count()))+". Subdued brushed steel and printed inserts replace the rusty high-contrast bank.";
    }
}
