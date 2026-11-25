using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static partial class ApartmentBuilder
{
    static void Fan(Transform room,Vector3 at,float yaw=0)
    {
        var root=G("Desk Fan",room);root.localPosition=at;root.localRotation=Quaternion.Euler(0,yaw,0);
        B("Fan Base",root,new Vector3(0,.025f,0),new Vector3(.27f,.05f,.19f),"Aged Plastic",false,.02f);Rod("Fan Stem",root,new Vector3(0,.04f,0),new Vector3(0,.26f,0),.025f,"Aged Plastic");
        Rod("Motor",root,new Vector3(0,.38f,-.02f),new Vector3(0,.38f,.12f),.072f,"Green Steel");
        for(int ring=0;ring<4;ring++){float radius=.055f+ring*.038f;for(int i=0;i<40;i++){float a=i*Mathf.PI/20,b=(i+1)*Mathf.PI/20;Rod("Guard Ring",root,new Vector3(Mathf.Cos(a)*radius,.38f+Mathf.Sin(a)*radius,-.06f),new Vector3(Mathf.Cos(b)*radius,.38f+Mathf.Sin(b)*radius,-.06f),.0018f,"Stainless Steel");}}
        for(int i=0;i<24;i++){float a=i*Mathf.PI/12;Rod("Guard Spoke",root,new Vector3(0,.38f,-.075f),new Vector3(Mathf.Cos(a)*.172f,.38f+Mathf.Sin(a)*.172f,-.04f),.0015f,"Stainless Steel");}
        for(int i=0;i<3;i++){var blade=B("Fan Blade",root,new Vector3(0,.38f,-.026f),new Vector3(.08f,.27f,.012f),"Green Steel",false,.012f);blade.localRotation=Quaternion.Euler(0,0,i*120);}
        Rod("Fan Hub",root,new Vector3(0,.38f,-.08f),new Vector3(0,.38f,-.055f),.035f,"Aged Plastic");PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Desk Fan.prefab");
    }
    static void Phone(Transform desk,Vector3 at)
    {
        var root=G("Telephone",desk);root.localPosition=at;
        B("Telephone Base",root,new Vector3(0,.035f,0),new Vector3(.22f,.07f,.235f),"Aged Plastic",false,.024f);
        B("Receiver",root,new Vector3(-.065f,.105f,0),new Vector3(.063f,.06f,.23f),"Aged Plastic",false,.02f);
        foreach(float z in new[]{-.088f,.088f})B("Receiver Earpiece",root,new Vector3(-.065f,.08f,z),new Vector3(.08f,.06f,.07f),"Aged Plastic",false,.022f);
        for(int row=0;row<4;row++)for(int col=0;col<3;col++)B("Telephone Key",root,new Vector3(.008f+col*.025f,.076f,-.05f+row*.027f),new Vector3(.019f,.012f,.019f),"Black Rubber",false,.003f);
        for(int i=0;i<45;i++){float a=i*.7f,b=(i+1)*.7f;Rod("Coiled Cord",root,new Vector3(-.15f+Mathf.Cos(a)*.007f,.06f-i*.001f,.1f+i*.002f),new Vector3(-.15f+Mathf.Cos(b)*.007f,.06f-(i+1)*.001f,.1f+(i+1)*.002f),.002f,"Black Rubber");}
        PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Telephone.prefab");
    }
    static void Office(Transform floor)
    {
        var room=G("Management Room",floor);
        B("Office Floor",room,new Vector3(-4.1f,-.12f,4.1f),new Vector3(3.8f,.24f,5.1f),"Terrazzo");B("Office Ceiling",room,new Vector3(-4.1f,2.88f,4.1f),new Vector3(4,.2f,5.3f),"Concrete");
        Wall(room,new Vector3(-6.09f,0,4.1f),5.3f,2.78f,true);Wall(room,new Vector3(-4.1f,0,1.46f),4.16f,2.78f);Wall(room,new Vector3(-4.1f,0,6.74f),4.16f,2.78f);
        var desk=Prop("Management Desk",room,new Vector3(-3.10f,0,3.4f),90,.77f);var db=BoundsOf(desk);float top=db.max.y;
        var furniture=G("Desk Items",room);
        var phone=G("Work Surface",furniture);phone.localPosition=new Vector3(-3.05f,top,2.88f);phone.localRotation=Quaternion.Euler(0,-90,0);Phone(phone,Vector3.zero);
        Prop("Desk Lamp",furniture,new Vector3(-2.75f,top,2.66f),-65,.46f);
        Lamp(furniture,new Vector3(-3.1f,top+.36f,2.8f),1.2f,2.4f,2850,Quaternion.identity);
        for(int row=0;row<2;row++)for(int col=0;col<2;col++)
        {
            var tv=Prop("CRT Monitor",furniture,new Vector3(-2.98f,top+row*.335f,3.55f+col*.48f),-90,.32f);
            var screen=Plain("Surveillance Screen "+(row*2+col+1),new Color(.18f,.22f,.2f));screen.SetColor("_EmissionColor",new Color(.16f,.2f,.18f));screen.EnableKeyword("_EMISSION");
            // Independent display material, ready to take a camera RenderTexture.
            Q("Surveillance Screen",tv,new Vector3(0,.19f,.18f),new Vector2(.32f,.23f),screen,Quaternion.Euler(0,180,0));
        }
        var console=B("CCTV Console",furniture,new Vector3(-3.26f,top+.028f,3.78f),new Vector3(.24f,.055f,.65f),"Green Steel",false);
        for(int i=0;i<10;i++)B("Console Button",console,new Vector3(-.035f,.035f,(i-4.5f)*.05f),new Vector3(.055f,.012f,.02f),i%4==0?"Red Enamel":"Black Rubber",false);
        for(int stack=0;stack<3;stack++)
        {
            var papers=G("Document Stack",furniture);papers.localPosition=new Vector3(-2.85f-stack*.12f,top,2.25f+stack*.18f);
            for(int sheet=0;sheet<7;sheet++){var paper=B("Paper Sheet",papers,new Vector3((sheet%3-1)*.005f,.003f+sheet*.003f,0),new Vector3(.21f,.002f,.29f),"Paper",false,.0002f);paper.localRotation=Quaternion.Euler(0,(sheet%3-1)*3,0);}
        }
        var book=Prop("Notebook",furniture,new Vector3(-3.17f,top,3.08f),0,.035f);book.localScale=new Vector3(1,1,1)*.7f;PrefabUtility.RecordPrefabInstancePropertyModifications(book);
        Notice("Maintenance Log",furniture,new Vector3(-3.22f,top+.024f,3.1f),new Vector2(.31f,.25f),Quaternion.Euler(90,0,90));
        Rod("Pen",furniture,new Vector3(-3.3f,top+.038f,3.1f),new Vector3(-3.3f,top+.038f,3.25f),.004f,"Black Rubber");
        Rod("Thermos Body",furniture,new Vector3(-2.78f,top,4.22f),new Vector3(-2.78f,top+.26f,4.22f),.041f,"Stainless Steel");Rod("Thermos Cap",furniture,new Vector3(-2.78f,top+.26f,4.22f),new Vector3(-2.78f,top+.31f,4.22f),.044f,"Black Rubber");
        Rod("Noodle Cup",furniture,new Vector3(-3.3f,top,2.47f),new Vector3(-3.3f,top+.10f,2.47f),.04f,"Paper");Rod("Chopstick",furniture,new Vector3(-3.31f,top+.03f,2.47f),new Vector3(-3.34f,top+.22f,2.47f),.002f,"Old Wood");
        B("Walkie Talkie",furniture,new Vector3(-3.4f,top+.05f,4.27f),new Vector3(.055f,.1f,.035f),"Black Rubber",false);Rod("Radio Aerial",furniture,new Vector3(-3.4f,top+.1f,4.27f),new Vector3(-3.4f,top+.24f,4.27f),.004f,"Black Rubber");
        Prop("Office Chair",room,new Vector3(-4.08f,.005f,3.65f),-90,1.08f);
        var cabinet=G("Filing Cabinet",room);cabinet.localPosition=new Vector3(-5.45f,0,5.85f);
        B("Cabinet Body",cabinet,new Vector3(0,.62f,0),new Vector3(.65f,1.24f,.55f),"Green Steel");
        for(int i=0;i<4;i++){B("File Drawer",cabinet,new Vector3(0,.17f+i*.30f,-.287f),new Vector3(.59f,.275f,.035f),"Green Steel",false);B("Drawer Label",cabinet,new Vector3(0,.22f+i*.3f,-.308f),new Vector3(.15f,.045f,.008f),"Paper",false);Rod("Drawer Pull",cabinet,new Vector3(-.09f,.13f+i*.3f,-.33f),new Vector3(.09f,.13f+i*.3f,-.33f),.01f,"Stainless Steel");}
        Fan(room,new Vector3(-5.45f,1.25f,5.85f),10);
        var keys=G("Key Cabinet",room);keys.localPosition=new Vector3(-3.8f,1.65f,6.60f);
        B("Key Board",keys,Vector3.zero,new Vector3(1.1f,.9f,.11f),"Old Wood");
        for(int row=0;row<3;row++)for(int col=0;col<7;col++)
        {float x=(col-3)*.135f,y=.28f-row*.27f;Rod("Key Hook",keys,new Vector3(x,y,-.07f),new Vector3(x,y,-.09f),.006f,"Brass");Rod("Key",keys,new Vector3(x,y-.02f,-.095f),new Vector3(x,y-.105f,-.095f),.008f,"Brass");B("Key Tooth",keys,new Vector3(x+.009f,y-.092f,-.095f),new Vector3(.022f,.017f,.01f),"Brass",false);B("Key Tag",keys,new Vector3(x,y-.145f,-.09f),new Vector3(.045f,.044f,.006f),"Aged Plastic",false);}
        Q("Key Cabinet Glass",keys,new Vector3(0,0,-.078f),new Vector2(1.02f,.82f),M("Dirty Glass"),Quaternion.identity);
        Prop("Wall Clock",room,new Vector3(-5.18f,2.12f,6.63f),180,.43f);
        Notice("Building Plan",room,new Vector3(-3.8f,2.33f,6.64f),new Vector2(.8f,.48f),Quaternion.identity);
        Notice("Calendar",room,new Vector3(-2.66f,1.8f,6.64f),new Vector2(.44f,.61f),Quaternion.identity);
        var shelf=B("Bookshelf",room,new Vector3(-5.78f,.66f,2.3f),new Vector3(.45f,1.32f,1.15f),"Old Wood");Prop("Books",room,new Vector3(-5.75f,1.33f,2.3f),90,.25f);
        Prop("Storage Crate",room,new Vector3(-5.4f,.01f,1.9f),5,.3f,false);
        PipeRun(room,new Vector3(-5.92f,2.57f,1.6f),new Vector3(-5.92f,2.57f,6.55f));PipeRun(room,new Vector3(-5.92f,.12f,4.5f),new Vector3(-5.92f,2.57f,4.5f),.018f);
        Fluorescent(room,new Vector3(-4.5f,2.77f,4.55f),5.2f,4400);
        Weather(room,"Peeling Paint",new Vector3(-6,1.85f,4),new Vector2(2.2f,1.55f),Quaternion.Euler(0,-90,0));Weather(room,"Water Stain",new Vector3(-4.2f,2,6.64f),new Vector2(1.3f,1.5f),Quaternion.identity);
    }
    static void Elevator(Transform room,int level)
    {
        float y=level*2.9f;var root=G("Elevator Landing",room);root.localPosition=new Vector3(3.2f,y,10.8f);root.localRotation=Quaternion.Euler(0,90,0);
        foreach(float side in new[]{-1,1})
        {B("Door Jamb",root,new Vector3(side*.63f,1.13f,-.035f),new Vector3(.12f,2.26f,.16f),"Stainless Steel");B("Shaft Wall",root,new Vector3(side*.87f,1.4f,.83f),new Vector3(.18f,2.8f,1.85f),"Concrete");}
        B("Door Header",root,new Vector3(0,2.29f,-.035f),new Vector3(1.38f,.12f,.16f),"Stainless Steel");
        B("Indicator Housing",root,new Vector3(0,2.55f,-.04f),new Vector3(.65f,.21f,.065f),"Stainless Steel",false);B("Indicator Glass",root,new Vector3(0,2.55f,-.078f),new Vector3(.55f,.14f,.01f),"Black Rubber",false);Text((level+1).ToString(),root,new Vector3(.15f,2.55f,-.086f),.15f,Quaternion.identity,new Color(1,.035f,.006f));
        var left=B("Left Elevator Door",root,new Vector3(-.275f,1.06f,.015f),new Vector3(.55f,2.12f,.05f),"Green Steel");var right=B("Right Elevator Door",root,new Vector3(.275f,1.06f,.015f),new Vector3(.55f,2.12f,.05f),"Green Steel");
        B("Threshold",root,new Vector3(0,.008f,0),new Vector3(1.18f,.016f,.17f),"Stainless Steel",false);
        B("Call Panel",root,new Vector3(.94f,1.19f,-.12f),new Vector3(.12f,.30f,.045f),"Stainless Steel",false);Text("▲\n▼",root,new Vector3(.94f,1.19f,-.15f),.09f,Quaternion.identity,new Color(.55f,.49f,.36f));
        Notice("Inspection Certificate",root,new Vector3(1.22f,1.79f,-.107f),new Vector2(.32f,.45f),Quaternion.identity);
        Weather(root,"Rust",new Vector3(-.25f,1.0f,-.013f),new Vector2(.42f,1.8f),Quaternion.identity);Weather(root,"Rust",new Vector3(.27f,.55f,-.014f),new Vector2(.4f,1.05f),Quaternion.identity);
        if(level==0)
        {
            var control=root.gameObject.AddComponent<SlidingDoors>();control.left=left;control.right=right;control.open=true;left.localPosition=new Vector3(-.96f,1.06f,.015f);right.localPosition=new Vector3(.96f,1.06f,.015f);
            Cabin(root);
        }
        else
        {B("Shaft Back Wall",root,new Vector3(0,1.4f,1.76f),new Vector3(1.92f,2.8f,.18f),"Concrete");B("Shaft Floor",root,new Vector3(0,-.13f,.83f),new Vector3(1.92f,.24f,1.85f),"Concrete");B("Shaft Ceiling",root,new Vector3(0,2.78f,.83f),new Vector3(1.92f,.18f,1.85f),"Concrete");}
        if(level==0)PrefabUtility.SaveAsPrefabAsset(root.gameObject,Base+"Prefabs/Elevator Landing.prefab");
    }
    static void Cabin(Transform landing)
    {
        var cabin=G("Elevator Cabin",landing);cabin.localPosition=new Vector3(0,0,.11f);
        B("Cabin Floor",cabin,new Vector3(0,-.055f,.8f),new Vector3(1.5f,.11f,1.6f),"Green Floor");B("Cabin Ceiling",cabin,new Vector3(0,2.33f,.8f),new Vector3(1.6f,.12f,1.7f),"Pale Green Panels");
        foreach(float side in new[]{-1,1})
        {B("Side Panel",cabin,new Vector3(side*.79f,1.15f,.8f),new Vector3(.08f,2.3f,1.7f),"Pale Green Panels");Rod("Handrail",cabin,new Vector3(side*.66f,.94f,.25f),new Vector3(side*.66f,.94f,1.42f),.024f,"Stainless Steel");foreach(float z in new[]{.3f,1.35f})Rod("Rail Bracket",cabin,new Vector3(side*.74f,.94f,z),new Vector3(side*.66f,.94f,z),.014f,"Stainless Steel");for(int i=0;i<3;i++)B("Panel Joint",cabin,new Vector3(side*.744f,1.15f,.15f+i*.65f),new Vector3(.01f,2.28f,.008f),"Black Rubber",false);}
        B("Rear Panel",cabin,new Vector3(0,1.15f,1.65f),new Vector3(1.65f,2.3f,.1f),"Pale Green Panels");Rod("Rear Handrail",cabin,new Vector3(-.6f,.94f,1.5f),new Vector3(.6f,.94f,1.5f),.024f,"Stainless Steel");
        var material=new Material(Shader.Find("Apartment/Planar Mirror"));AssetDatabase.CreateAsset(material,Base+"Materials/Cabin Mirror.mat");material.SetTexture("_Dirt",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Alley/Textures/Weathering/Discoloration.png"));
        B("Mirror Frame",cabin,new Vector3(0,1.6f,1.586f),new Vector3(1.26f,.88f,.025f),"Stainless Steel",false);
        var mirror=Q("Mirror",cabin,new Vector3(0,1.6f,1.57f),new Vector2(1.17f,.79f),material,Quaternion.identity);
        var rt=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGBHalf);rt.name="Cabin Reflection";AssetDatabase.CreateAsset(rt,Base+"Materials/Cabin Reflection.renderTexture");
        var camera=G("Mirror Camera",cabin).gameObject.AddComponent<Camera>();camera.enabled=false;camera.gameObject.AddComponent<UniversalAdditionalCameraData>().renderPostProcessing=true;
        var component=mirror.gameObject.AddComponent<ApartmentMirror>();component.reflectionCamera=camera;component.mirrorSurface=mirror.GetComponent<Renderer>();component.texture=rt;
        var panel=G("Control Panel",cabin);panel.localPosition=new Vector3(.733f,0,.37f);panel.localRotation=Quaternion.Euler(0,90,0);
        B("Panel Face",panel,new Vector3(0,1.2f,0),new Vector3(.23f,1.43f,.025f),"Stainless Steel",false);
        for(int i=1;i<=6;i++){float y=.73f+(i-1)*.145f;Rod("Floor Button",panel,new Vector3(0,y,-.024f),new Vector3(0,y,-.04f),.028f,"Brass");Text(i.ToString(),panel,new Vector3(0,y,-.043f),.042f,Quaternion.identity,new Color(.1f,.09f,.065f));}
        Text("1",panel,new Vector3(0,1.83f,-.018f),.16f,Quaternion.identity,new Color(1,.025f,.005f));
        foreach(float x in new[]{-.055f,.055f})Rod("Door Button",panel,new Vector3(x,.52f,-.025f),new Vector3(x,.52f,-.038f),.024f,"Stainless Steel");Rod("Emergency Button",panel,new Vector3(0,.39f,-.025f),new Vector3(0,.39f,-.04f),.028f,"Red Enamel");
        Fluorescent(cabin,new Vector3(0,2.26f,.7f),3.2f,3900);
        for(int i=0;i<9;i++)B("Ceiling Vent",cabin,new Vector3(.55f,2.265f,.7f+i*.035f),new Vector3(.22f,.012f,.018f),"Black Rubber",false);
        Weather(cabin,"Rust",new Vector3(0,.35f,1.595f),new Vector2(1.3f,.65f),Quaternion.identity);
        PrefabUtility.SaveAsPrefabAsset(cabin.gameObject,Base+"Prefabs/Elevator Cabin.prefab");
    }
}
