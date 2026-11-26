using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static Transform HouseholdStreetProp(string name,Transform parent,Vector3 at,float yaw,float height)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Alley/Prefabs/Street/"+name+".prefab");
        var root=((GameObject)PrefabUtility.InstantiatePrefab(asset,parent)).transform;root.localRotation=Quaternion.Euler(0,yaw,0);
        root.localScale*=height/BoundsOf(root).size.y;var bounds=BoundsOf(root);root.position+=parent.TransformPoint(at)-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);CommonDynamic(root);return root;
    }
    static void HouseholdSlippers(Transform parent,Vector3 at,int variant,float size=1)
    {
        var pair=G(size<.8f?"Small Slippers":"House Slippers",parent);pair.localPosition=at;
        foreach(int side in new[]{-1,1})
        {
            var shoe=CommonProp("House Slipper",pair,new Vector3(side*.054f*size,0,side*.014f*size),side<0?-7:5,.060f*size);
            foreach(var r in shoe.GetComponentsInChildren<Renderer>())if(!r.name.Contains("Sole"))r.sharedMaterial=M(new[]{"House Slipper Blue","House Slipper Ochre","House Slipper Plum"}[variant%3]);
        }
    }
    static void HouseholdRack(Transform root,bool cabinet,int variant)
    {
        var rack=G(cabinet?"Shoe Cabinet":"Shoe Rack",root);rack.localPosition=new Vector3(0,0,-.18f);
        if(cabinet)
        {
            B("Cabinet Back",rack,new Vector3(0,.30f,.14f),new Vector3(.52f,.54f,.014f),"Household Timber",false,.002f);
            foreach(float x in new[]{-.254f,.254f})B("Cabinet Side",rack,new Vector3(x,.30f,0),new Vector3(.018f,.54f,.29f),"Household Timber",false,.002f);
            foreach(float y in new[]{.042f,.575f})B("Cabinet Board",rack,new Vector3(0,y,0),new Vector3(.53f,.025f,.30f),"Household Timber",false,.003f);
            foreach(float x in new[]{-.128f,.128f}){B("Slatted Door",rack,new Vector3(x,.31f,-.153f),new Vector3(.245f,.49f,.017f),"Household Timber",false,.002f);Rod("Inset Pull",rack,new Vector3(x+Mathf.Sign(x)*-.06f,.33f,-.164f),new Vector3(x+Mathf.Sign(x)*-.06f,.28f,-.164f),.005f,"Cabin Aged Steel");}
            foreach(float x in new[]{-.22f,.22f})foreach(float z in new[]{-.10f,.10f})B("Cabinet Foot",rack,new Vector3(x,.018f,z),new Vector3(.024f,.036f,.024f),"Cabin Aged Steel",false,.003f);
        }
        else
        {
            foreach(float x in new[]{-.255f,.255f})foreach(float z in new[]{-.13f,.13f}){Rod("Rack Leg",rack,new Vector3(x,.006f,z),new Vector3(x,.43f,z),.008f,"Service Conduit Paint");B("Rubber Foot",rack,new Vector3(x,.003f,z),new Vector3(.022f,.006f,.022f),"Household Rubber",false,.002f);}
            foreach(float y in new[]{.055f,.245f,.434f})B("Rack Shelf",rack,new Vector3(0,y,0),new Vector3(.54f,.014f,.29f),"Household Timber",false,.002f);
            HouseholdSlippers(rack,new Vector3(-.105f,.064f,-.005f),variant,.84f);HouseholdSlippers(rack,new Vector3(.105f,.254f,-.005f),variant+1,.80f);
            Rod("Rear Cross Brace",rack,new Vector3(-.255f,.06f,.132f),new Vector3(.255f,.43f,.132f),.004f,"Service Conduit Paint");
        }
        var collider=rack.gameObject.AddComponent<BoxCollider>();collider.center=new Vector3(0,cabinet?.3f:.22f,0);collider.size=new Vector3(.54f,cabinet?.6f:.44f,.30f);
    }
    static void HouseholdPaper(Transform parent,Vector3 at,int count)
    {
        var papers=G("Folded Neighborhood Papers",parent);papers.localPosition=at;papers.localRotation=Quaternion.Euler(0,8,0);
        for(int i=0;i<count;i++)
        {
            var sheet=B("Folded Sheet",papers,new Vector3(i%2*.003f,.003f+i*.003f,0),new Vector3(.25f,.003f,.17f),"Paper",false,.0005f);
            if(i==count-1)Q("Printed Front",sheet,new Vector3(0,.0018f,0),new Vector2(.248f,.168f),M("Neighborhood Paper"),Quaternion.Euler(90,0,0));
        }
    }
    static void HouseholdUmbrella(Transform root,int variant)
    {
        var holder=G("Umbrella Holder",root);holder.localPosition=new Vector3(.26f,0,-.10f);
        // Open slatted basket, with a solid base so the umbrellas are supported.
        B("Drip Tray",holder,new Vector3(0,.013f,0),new Vector3(.16f,.026f,.16f),"Household Rubber",false,.012f);
        foreach(float y in new[]{.04f,.26f})foreach(float side in new[]{-1f,1f})
        {Rod("Basket Rim",holder,new Vector3(-.075f,y,side*.075f),new Vector3(.075f,y,side*.075f),.0035f,"Service Conduit Paint");Rod("Basket Rim",holder,new Vector3(side*.075f,y,-.075f),new Vector3(side*.075f,y,.075f),.0035f,"Service Conduit Paint");}
        for(int i=0;i<5;i++)foreach(float side in new[]{-1f,1f})
        {Rod("Basket Wire",holder,new Vector3(-.06f+i*.03f,.04f,side*.075f),new Vector3(-.06f+i*.03f,.26f,side*.075f),.002f,"Service Conduit Paint");Rod("Basket Wire",holder,new Vector3(side*.075f,.04f,-.06f+i*.03f),new Vector3(side*.075f,.26f,-.06f+i*.03f),.002f,"Service Conduit Paint");}
        HouseholdStreetProp("Folded Umbrella",holder,new Vector3(-.027f,.027f,0),18+variant*7,.72f);
    }
    static void HouseholdBoots(Transform parent,bool child=false)
    {
        var pair=G(child?"Child Rain Boots":"Worn Work Boots",parent);pair.localPosition=new Vector3(-.055f,0,-.16f);
        float height=child?.215f:.35f;CommonProp("Rubber Boot Left",pair,new Vector3(-.054f,0,0),174,height);CommonProp("Rubber Boot Right",pair,new Vector3(.054f,0,.025f),184,height);
    }
    static void CommonHouseholds(Transform room,int floor)
    {
        var root=CommonGroup(room,"Households");
        // Existing repeated planters/cartons are folded into the new household groups.
        var old=root.parent.Cast<Transform>().Where(t=>t.name=="Delivery Carton"||t.name=="Potted Fern").ToArray();foreach(var t in old)t.gameObject.SetActive(false);
        var profiles=new[]{new[]{1,0,2,3,0,4},new[]{0,5,6,7,8,0},new[]{9,1,10,0,11,5},new[]{2,0,12,3,1,7},new[]{5,8,0,6,3,0}}[floor-2];
        for(int unit=0;unit<6;unit++)
        {
            int profile=profiles[unit];int side=unit%2==0?-1:1;float z=1.90f+(unit/2)*3.15f;
            // Preserve some intentionally sparse, bare thresholds rather than evenly filling every doorway.
            var mat=room.Cast<Transform>().Where(t=>t.name=="Doormat").OrderBy(t=>Mathf.Abs(t.localPosition.z-z)+Mathf.Abs(t.localPosition.x-side*1.35f)).First();
            mat.gameObject.SetActive(profile!=0||unit%3!=0);
            if(profile==0)continue;
            var household=G("Entrance "+floor+(unit+1).ToString("00"),root);household.localPosition=new Vector3(side*1.985f,0,z+(unit/2==2?-1.13f:1.13f));household.localRotation=Quaternion.Euler(0,side>0?90:-90,0);
            switch(profile)
            {
                case 1:
                    CommonProp("Household Stool",household,new Vector3(-.035f,0,-.20f),12,.34f);HouseholdPaper(household,new Vector3(-.025f,.341f,-.20f),3);HouseholdSlippers(household,new Vector3(.32f,0,-.19f),1,.92f);break;
                case 2:
                    HouseholdRack(household,false,unit);break;
                case 3:
                    HouseholdStreetProp("Delivery Carton",household,new Vector3(-.07f,0,-.17f),-8,.24f);HouseholdStreetProp("Delivery Carton",household,new Vector3(-.07f,.241f,-.16f),4,.13f);HouseholdUmbrella(household,unit);break;
                case 4:
                    HouseholdSlippers(household,new Vector3(-.07f,0,-.15f),0);HouseholdSlippers(household,new Vector3(.20f,0,-.16f),2,.62f);HouseholdStreetProp("Cleaning Bucket",household,new Vector3(.42f,0,-.13f),5,.17f);break;
                case 5:
                    HouseholdStreetProp("Potted Foliage",household,new Vector3(-.08f,0,-.18f),31,.42f);HouseholdStreetProp("Potted Succulent",household,new Vector3(.22f,0,-.14f),12,.23f);HouseholdStreetProp("Watering Can",household,new Vector3(.42f,0,-.12f),42,.16f);break;
                case 6:
                    HouseholdBoots(household);HouseholdStreetProp("Cleaning Bucket",household,new Vector3(.28f,0,-.17f),-20,.21f);break;
                case 7:
                    HouseholdSlippers(household,new Vector3(0,0,-.17f),unit,.94f);break;
                case 8:
                    HouseholdRack(household,true,unit);HouseholdStreetProp("Potted Succulent",household,new Vector3(.03f,.59f,-.14f),20,.17f);break;
                case 9:
                    HouseholdUmbrella(household,unit);HouseholdSlippers(household,new Vector3(-.16f,0,-.17f),0,.91f);break;
                case 10:
                    HouseholdSlippers(household,new Vector3(.03f,0,-.16f),2);break;
                case 11:
                    HouseholdBoots(household,true);HouseholdSlippers(household,new Vector3(.20f,0,-.16f),1,.62f);break;
                case 12:
                    HouseholdStreetProp("Potted Succulent",household,new Vector3(-.12f,0,-.12f),1,.25f);HouseholdPaper(household,new Vector3(.13f,0,-.17f),2);break;
            }
            // One household retains faded festival paper, applied directly to its door leaf.
            if(profile==10)
            {
                var door=room.Cast<Transform>().Where(t=>t.name=="Apartment Door").OrderBy(t=>Mathf.Abs(t.localPosition.z-z)+Mathf.Abs(t.localPosition.x-side*1.84f)).First();
                var decoration=CommonGroup(door,"Household Decoration");
                Q("Faded Couplet Left",decoration,new Vector3(-.335f,1.12f,.0217f),new Vector2(.10f,.72f),M("Couplet Left"),Quaternion.identity);
                Q("Faded Couplet Right",decoration,new Vector3(.335f,1.12f,.0217f),new Vector2(.10f,.72f),M("Couplet Right"),Quaternion.identity);
            }
            CommonDynamic(household);PrefabUtility.SaveAsPrefabAsset(household.gameObject,CommonPrefabs+"Entrance "+floor+(unit+1).ToString("00")+".prefab");
        }
        CommonDynamic(root);
    }
}
