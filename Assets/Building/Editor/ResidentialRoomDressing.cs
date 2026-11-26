using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static void RoomCalendar(Transform room,RoomSpace s,float d)
    {
        var paper=G("Old Calendar",room);paper.localPosition=s.At(d,1.70f,.059f);paper.localRotation=Quaternion.Euler(0,180,0);
        B("Calendar Backing",paper,Vector3.zero,new Vector3(.24f,.35f,.005f),"Paper",false,.001f);
        Q("Calendar Page",paper,new Vector3(0,0,-.003f),new Vector2(.232f,.34f),M("Interior Calendar"),Quaternion.identity);
        Rod("Calendar Clip",paper,new Vector3(-.09f,.17f,-.006f),new Vector3(.09f,.17f,-.006f),.003f,"Cabin Aged Steel");
    }
    static void RoomPartition(Transform room,RoomSpace s,bool family)
    {
        float gapStart=family?.13f:2.18f,gapEnd=family?1.00f:3.02f;
        foreach(var span in new[]{new Vector2(0,gapStart),new Vector2(gapEnd,s.length)})if(span.y-span.x>.03f)
            B("Bedroom Partition",room,s.At(2.19f,1.28f,(span.x+span.y)*.5f),new Vector3(.075f,2.56f,span.y-span.x),"Interior Faded Wall",true,.002f);
        B("Bedroom Door Lintel",room,s.At(2.19f,2.35f,(gapStart+gapEnd)*.5f),new Vector3(.075f,.42f,gapEnd-gapStart),"Household Timber",true,.002f);
        InteriorCloth("Curtain",room,s.At(2.17f,2.13f,gapStart+.01f),new Vector3(.30f,1,1.20f),-90);
    }
    static void HouseholdBooks(Transform room,Vector3 at,int count)
    {
        for(int i=0;i<count;i++)
        {
            B("Schoolbook",room,at+Vector3.up*(.012f+i*.021f),new Vector3(.14f,.020f,.19f),i%2==0?"Interior Notebook Cover":"Household Timber",false,.001f);
            B("Page Edges",room,at+new Vector3(.001f,.012f+i*.021f,-.002f),new Vector3(.138f,.014f,.186f),"Paper",false,.0005f);
        }
    }
    static void TableCrockery(Transform room,Vector3 at)
    {
        foreach(float x in new[]{-.10f,.10f})
        {Rod("Tea Cup",room,at+new Vector3(x,0,0),at+new Vector3(x,.055f,0),.028f,"Interior Pale Enamel");Rod("Tea Surface",room,at+new Vector3(x,.049f,0),at+new Vector3(x,.050f,0),.021f,"Household Timber");}
        B("Tea Tray",room,at+new Vector3(0,-.004f,0),new Vector3(.32f,.008f,.18f),"Household Timber",false,.006f);
    }
    public static string BuildElderlyApartment()
    {
        EnsureApartment();var s=RoomPlan("201");if(GameObject.Find("Room 201")!=null)return "Room 201 already exists";var room=InteriorShell(s);
        RoomPartition(room,s,false);InteriorBed(room,s,3.20f,1.10f,false);InteriorWindow(room,s,1.98f);
        InteriorKitchen(room,s,s.length-.34f);
        var table=InteriorProp("chinese_tea_table",room,s.At(1.21f,0,.80f),90,.42f);TableCrockery(room,s.At(1.21f,.4285f,.80f));
        CommonProp("Household Stool",room,s.At(.56f,0,.78f),-8,.30f);CommonProp("Household Stool",room,s.At(1.80f,0,.73f),7,.30f);
        B("Television Cabinet",room,s.At(1.22f,.29f,.25f),new Vector3(.90f,.58f,.36f),"Household Timber",true,.006f);
        foreach(float d in new[]{1.02f,1.43f}){B("Cabinet Door",room,s.At(d,.30f,.438f),new Vector3(.39f,.48f,.014f),"Household Timber",false,.002f);Rod("Brass Pull",room,s.At(d,.31f,.45f),s.At(d,.36f,.45f),.006f,"Brass");}
        Prop("CRT Monitor",room,s.At(1.22f,.585f,.25f),0,.33f);
        InteriorPhoto(room,s.At(2.145f,1.62f,.94f),Quaternion.Euler(0,-90,0));RoomCalendar(room,s,.55f);
        for(int i=0;i<4;i++)
        {var carton=B("Medicine Box",room,s.At(1.15f+i*.065f,.448f,.91f),new Vector3(.055f,.055f,.095f),"Interior Medicine Carton",false,.002f);Q("Medicine Label",carton,new Vector3(0,.028f,0),new Vector2(.052f,.088f),M("Interior Medicine Label"),Quaternion.Euler(90,0,0));}
        var wardrobe=InteriorProp("wardrobe",room,s.At(3.35f,0,2.80f),180,1.62f);wardrobe.localScale=new Vector3(wardrobe.localScale.x*.75f/BoundsOf(wardrobe).size.x,wardrobe.localScale.y,wardrobe.localScale.z);
        BedroomLamp(room,s,2.55f,.45f);
        HouseholdStreetProp("Potted Succulent",room,s.At(.23f,0,.42f),18,.22f);HouseholdSlippers(room,s.At(.32f,0,1.15f),1,.92f);
        InteriorLight(room,s,s.At(1.30f,2.68f,1.75f),true,4.7f);
        Weather(room,"Water Stain",s.At(2.7f,2.0f,s.length-.052f),new Vector2(.55f,.78f),Quaternion.identity);CommonCeilingWear(room,2.68f,s.At(3.3f,0,0).x,1.15f,3);
        FinishInterior(room,s);return "201: compact elderly resident apartment, kitchen, separate curtained bedroom, vintage furniture, medicine, photographs and warm light.";
    }
    public static string BuildLaundryRoom()
    {
        EnsureApartment();var s=RoomPlan("306");if(GameObject.Find("Room 306")!=null)return "Room 306 already exists";var room=InteriorShell(s);
        var machines=new[]{InteriorProp("washing-machine",room,s.At(2.58f,0,.60f),-90,.83f),InteriorProp("washing-machine",room,s.At(2.58f,0,1.48f),-90,.83f)};
        InteriorProp("kitchen-base-unit-sink",room,s.At(1.60f,0,s.length-.36f),180,.80f);
        InteriorProp("laundry-basket",room,s.At(.72f,0,.40f),18,.38f);InteriorProp("laundry-basket",room,s.At(2.14f,.84f,.60f),-8,.23f);
        foreach(float v in new[]{.60f,1.48f})
        {
            CommonRoute("Washer Supply",room,.011f,"Service Water Steel",s.At(2.85f,2.40f,v),s.At(2.85f,.64f,v),s.At(2.60f,.64f,v));
            CommonRoute("Waste Hose",room,.018f,"Black Rubber",s.At(2.72f,.22f,v),s.At(2.91f,.22f,v),s.At(2.91f,.06f,v));
            Rod("Water Valve",room,s.At(2.80f,1.09f,v),s.At(2.86f,1.09f,v),.023f,"Cabin Aged Steel");
        }
        CommonRoute("Laundry Supply Main",room,.024f,"Service Water Steel",s.At(2.85f,-.07f,.28f),s.At(2.85f,2.4f,.28f),s.At(2.85f,2.4f,s.length-.10f));
        for(int i=0;i<5;i++)B("Pipe Wall Clip",room,s.At(2.91f,.35f+i*.45f,.28f),new Vector3(.08f,.025f,.055f),"Cabin Aged Steel",false,.002f);
        Rod("Drying Rail",room,s.At(.95f,2.48f,.26f),s.At(.95f,2.48f,1.95f),.009f,"Cabin Aged Steel");
        foreach(float v in new[]{.26f,1.95f})Rod("Rail Hanger",room,s.At(.95f,2.48f,v),s.At(.95f,2.675f,v),.006f,"Cabin Aged Steel");
        for(int i=0;i<3;i++){InteriorCloth("Hanging Shirt",room,s.At(.95f,2.43f,.52f+i*.51f),new Vector3(.78f,.8f,.92f),90);Rod("Clothes Hanger",room,s.At(.95f,2.47f,.52f+i*.51f),s.At(.95f,2.40f,.52f+i*.51f),.002f,"Cabin Aged Steel");}
        HouseholdStreetProp("Cleaning Bucket",room,s.At(.30f,0,s.length-.36f),20,.24f);HouseholdStreetProp("Cleaning Bottle",room,s.At(1.42f,.81f,s.length-.34f),0,.22f);
        HouseholdStreetProp("Gully Grate",room,s.At(1.95f,.001f,1.21f),0,.008f);
        Weather(room,"Ground Stain",s.At(2.15f,.003f,1.15f),new Vector2(.55f,1.1f),Quaternion.Euler(90,0,11));
        Weather(room,"Water Stain",s.At(2.99f,1.17f,1.15f),new Vector2(.65f,1.2f),Quaternion.Euler(0,90,0));
        CommonVent(room,s.At(3.015f,2.25f,1.16f),Quaternion.Euler(0,90,0));InteriorLight(room,s,s.At(1.60f,2.68f,1.16f),false,5.2f);
        FinishInterior(room,s);return "306: plumbed washers, shared sink, drain, drying rail, hanging laundry, baskets and cleaning supplies.";
    }
    public static string BuildVacantApartment()
    {
        EnsureApartment();var s=RoomPlan("404");if(GameObject.Find("Room 404")!=null)return "Room 404 already exists";var room=InteriorShell(s);
        var table=InteriorProp("chinese_tea_table",room,s.At(2.20f,0,1.00f),0,.43f);InteriorCloth("Furniture Dust Sheet",room,s.At(2.20f,.45f,1.00f),new Vector3(.65f,1,.52f),90);
        InteriorProp("wardrobe",room,s.At(2.35f,0,2.52f),-90,1.70f);InteriorCloth("Furniture Dust Sheet",room,s.At(2.35f,1.72f,2.52f),new Vector3(.68f,1,.45f));
        Prop("CRT Monitor",room,s.At(1.4f,0,.35f),20,.34f);CommonRoute("Disconnected Flex",room,.003f,"Black Rubber",s.At(1.4f,.1f,.5f),s.At(1.55f,.02f,.65f),s.At(1.12f,.02f,.82f));
        for(int i=0;i<4;i++)HouseholdStreetProp("Delivery Carton",room,s.At(.42f+i%2*.34f,i/2*.22f,2.52f),i*11,.22f);
        InteriorPhoto(room,s.At(.68f,1.55f,s.length-.058f),Quaternion.identity,true);InteriorPhoto(room,s.At(1.30f,.22f,s.length-.10f),Quaternion.Euler(-8,0,0),true);
        InteriorWindow(room,s,1.15f);HouseholdPaper(room,s.At(.35f,0,.42f),4);
        Weather(room,"Water Stain",s.At(1.48f,1.45f,.052f),new Vector2(1.08f,1.65f),Quaternion.Euler(0,180,0));Weather(room,"Ground Stain",s.At(1.85f,.003f,1.68f),new Vector2(.81f,1.5f),Quaternion.Euler(90,0,38));
        InteriorLight(room,s,s.At(1.10f,2.68f,1.4f),false,2.75f);FinishInterior(room,s);return "404: dusty vacant interior, covered furniture, disconnected TV, cartons, empty frames and muted cold light.";
    }
    public static string BuildFamilyApartment()
    {
        EnsureApartment();var s=RoomPlan("503");if(GameObject.Find("Room 503")!=null)return "Room 503 already exists";var room=InteriorShell(s);
        RoomPartition(room,s,true);InteriorBed(room,s,3.22f,2.0f,true);InteriorWindow(room,s,2.1f);InteriorKitchen(room,s,s.length-.34f);
        var table=InteriorProp("chinese_tea_table",room,s.At(1.24f,0,.64f),90,.50f);CommonProp("Household Stool",room,s.At(.57f,0,.65f),4,.34f);CommonProp("Household Stool",room,s.At(1.65f,0,.64f),-10,.29f);TableCrockery(room,s.At(1.24f,.5085f,.65f));
        B("Child Writing Desk",room,s.At(.45f,.49f,.31f),new Vector3(.55f,.035f,.35f),"Household Timber",false,.004f);
        foreach(float d in new[]{.19f,.71f})foreach(float v in new[]{.16f,.46f})B("Desk Leg",room,s.At(d,.24f,v),new Vector3(.028f,.48f,.028f),"Household Timber",false,.002f);
        HouseholdBooks(room,s.At(.45f,.511f,.31f),3);BedroomLamp(room,s,2.55f,1.38f);
        InteriorPhoto(room,s.At(2.145f,1.62f,1.45f),Quaternion.Euler(0,-90,0));RoomCalendar(room,s,1.65f);
        HouseholdSlippers(room,s.At(.30f,0,2.22f),2,.64f);HouseholdSlippers(room,s.At(.57f,0,2.25f),0,.94f);
        var toys=G("Wooden Toy Train",room);toys.localPosition=s.At(1.78f,.005f,1.26f);
        for(int i=0;i<3;i++){B("Toy Carriage",toys,new Vector3(i*.085f,.035f,0),new Vector3(.072f,.045f,.048f),i%2==0?"Old Alarm Enamel":"Interior Notebook Cover",false,.006f);foreach(float x in new[]{-.023f,.023f})foreach(float z in new[]{-.029f,.029f})Rod("Toy Wheel",toys,new Vector3(i*.085f+x,.02f,z),new Vector3(i*.085f+x,.02f,z+Mathf.Sign(z)*.006f),.014f,"Household Rubber");}
        InteriorLight(room,s,s.At(1.33f,2.68f,1.70f),true,4.1f);CommonCeilingWear(room,2.68f,s.At(3.2f,0,0).x,5.45f,2);
        FinishInterior(room,s);return "503: lived-in family apartment, kitchen, curtained sleeping area, dining furniture, child's desk, schoolbooks, slippers, toys and family photographs.";
    }
    public static string BuildCommunalStorage()
    {
        EnsureApartment();var s=RoomPlan("602");if(GameObject.Find("Room 602")!=null)return "Room 602 already exists";var room=InteriorShell(s);
        for(int i=0;i<2;i++)
        {
            var shelf=InteriorProp("Shelf_01",room,s.At(2.66f,0,.65f+i*1.63f),-90,1.86f);var b=BoundsOf(shelf);if(b.size.z>1.3f)shelf.localScale=new Vector3(shelf.localScale.x,shelf.localScale.y,shelf.localScale.z*1.30f/b.size.z);
            for(int j=0;j<3;j++)HouseholdStreetProp("Delivery Carton",room,s.At(2.58f,.32f+j*.45f,.60f+i*1.63f),j*8,.26f);
        }
        InteriorProp("wardrobe",room,s.At(.73f,0,s.length-.39f),180,1.72f);Prop("Desk Fan",room,s.At(.70f,1.722f,s.length-.39f),0,.36f);
        Prop("CRT Monitor",room,s.At(1.78f,.001f,.40f),-12,.35f);Prop("CRT Monitor",room,s.At(1.80f,.001f,1.05f),7,.29f);
        InteriorProp("painted_wooden_chair_02",room,s.At(.57f,0,.40f),23,.86f);
        HouseholdPaper(room,s.At(1.52f,0,s.length-.33f),9);HouseholdStreetProp("Cleaning Bucket",room,s.At(.24f,0,2.15f),-8,.25f);HouseholdStreetProp("Broom",room,s.At(.20f,0,2.55f),0,1.25f);
        var trunk=B("Old Travel Case",room,s.At(1.57f,.23f,2.65f),new Vector3(.62f,.46f,.29f),"Household Timber",true,.035f);
        foreach(float x in new[]{-.20f,.20f})B("Case Strap",trunk,new Vector3(x,0,-.15f),new Vector3(.035f,.44f,.012f),"Household Rubber",false,.002f);
        Rod("Case Handle",trunk,new Vector3(-.09f,.25f,0),new Vector3(.09f,.25f,0),.012f,"Household Rubber");
        InteriorCloth("Furniture Dust Sheet",room,s.At(1.83f,.70f,.43f),new Vector3(.43f,1,.32f));
        InteriorLight(room,s,s.At(1.30f,2.68f,1.65f),false,2.6f);CommonVent(room,s.At(3.015f,2.22f,1.55f),Quaternion.Euler(0,90,0));
        FinishInterior(room,s);return "602: communal storage, loaded shelving, old wardrobe, stacked televisions, electric fan, travel case, documents and cleaning supplies.";
    }
}
