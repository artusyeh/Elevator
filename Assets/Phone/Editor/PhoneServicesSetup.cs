using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

public static partial class ApartmentBuilder
{
    static Color managementMuted=new Color(.57f,.62f,.65f),managementAccent=new Color(.67f,.74f,.70f);
    static RectTransform ManagementScroll(Transform page,string name,float height)
    {
        var content=ScrollArea(page,name,height);var viewport=(RectTransform)content.parent;
        viewport.sizeDelta=new Vector2(720,940);viewport.anchoredPosition=new Vector2(0,-35);return content;
    }
    static void TopAnchorChildren(RectTransform content){foreach(RectTransform child in content){var at=child.anchoredPosition;child.anchorMin=child.anchorMax=new Vector2(.5f,1);child.anchoredPosition=at;}}
    static RectTransform ManagementPage(Transform screen,string title,List<PhoneApplications.ScreenPage> pages,PhoneApplications.Page page)
    {
        var root=AppPage(screen,"Management · "+title,"Building Services",pages,page);root.Find("App Title").GetComponent<Text>().fontSize=36;
        Copy("Section Title",root,title,new Vector2(680,80),new Vector2(0,495),43,true);return root;
    }
    static void InstallManagementDashboard(PhoneApplications apps,Transform screen,List<PhoneApplications.ScreenPage> pages)
    {
        if(pages.Any(p=>p.page==PhoneApplications.Page.Management))return;
        var dashboard=AppPage(screen,"Management · Overview","Building Services",pages,PhoneApplications.Page.Management);apps.management=dashboard.gameObject;dashboard.Find("App Title").GetComponent<Text>().fontSize=36;
        Copy("Role",dashboard,"Security office",new Vector2(680,60),new Vector2(0,470),30,false,managementMuted);
        string[] titles={"Tasks","Maintenance","Regulations","Records"};var icons=new[]{PhoneIcon.Symbol.Tasks,PhoneIcon.Symbol.Wrench,PhoneIcon.Symbol.Rules,PhoneIcon.Symbol.Report};var destinations=new[]{PhoneApplications.Page.Tasks,PhoneApplications.Page.Maintenance,PhoneApplications.Page.Regulations,PhoneApplications.Page.Records};
        for(int i=0;i<4;i++){var tile=Nav(titles[i],dashboard,new Vector2(328,222),new Vector2(i%2==0?-177:177,235-i/2*255),PhoneNavigationButton.Action.Navigate,destinations[i],phoneTheme.surface);Shape("Section Icon",tile.transform,new Vector2(65,65),new Vector2(-105,51),managementAccent,icons[i]);Copy("Section Name",tile.transform,titles[i],new Vector2(285,78),new Vector2(0,-48),31,true);}
    }
    public static string RestoreManagementDashboard()
    {
        if(Application.isPlaying)throw new InvalidOperationException("Save phone layout changes in Edit Mode.");
        phoneTheme=AssetDatabase.LoadAssetAtPath<PhoneUiTheme>("Assets/Phone/Modern UI Theme.asset");var root=PrefabUtility.LoadPrefabContents(PhonePath);
        try
        {
            var apps=root.GetComponent<PhoneApplications>();var pages=apps.pages.ToList();var screen=root.GetComponent<PhonePresentation>().screen.transform;InstallManagementDashboard(apps,screen,pages);apps.pages=pages.ToArray();
            foreach(var page in pages.Where(p=>p.page==PhoneApplications.Page.Tasks||p.page==PhoneApplications.Page.Maintenance||p.page==PhoneApplications.Page.Regulations||p.page==PhoneApplications.Page.Records))foreach(var child in page.root.transform.Cast<Transform>().ToArray())if(child.name.EndsWith(" Tab")||child.name=="Divider"&&((RectTransform)child).anchoredPosition.y< -500)UnityEngine.Object.DestroyImmediate(child.gameObject);
            var taskPage=pages.First(p=>p.page==PhoneApplications.Page.Tasks).root;foreach(var text in taskPage.GetComponentsInChildren<Text>(true).Where(t=>t.name=="Assigned Time"))text.gameObject.SetActive(false);
            var bridge=root.GetComponent<PhoneNarrativeBridge>();if(bridge!=null)bridge.taskTemplate.time.gameObject.SetActive(false);
            foreach(var image in root.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Row Divider"||i.name=="Divider")){var size=image.rectTransform.sizeDelta;size.y=image.name=="Row Divider"?4:3;image.rectTransform.sizeDelta=size;image.raycastTarget=false;}
            foreach(var button in root.GetComponentsInChildren<PhoneNavigationButton>(true).Where(b=>b.action==PhoneNavigationButton.Action.Conversation))button.GetComponent<Button>().transition=Selectable.Transition.None;
            foreach(string name in new[]{"System Chrome","Flashlight Glance Screen","System Dialog"})screen.Find(name).SetAsLastSibling();foreach(var page in pages)page.root.SetActive(page.page==PhoneApplications.Page.Home);
            PrefabUtility.SaveAsPrefabAsset(root,PhonePath);AssetDatabase.SaveAssets();return "Four existing-style dashboard tiles restored; section pages retained, bottom tabs removed, task times hidden and chat dividers stabilized.";
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    static void InstallManagementUi(GameObject root,PhoneApplications apps,Transform screen,List<PhoneApplications.ScreenPage> pages)
    {
        var manager=root.GetComponent<PhoneManagement>();if(manager==null)manager=root.AddComponent<PhoneManagement>();
        const string path="Assets/Phone/Management Data.asset";var data=AssetDatabase.LoadAssetAtPath<PhoneManagementData>(path);
        if(data==null)
        {
            data=ScriptableObject.CreateInstance<PhoneManagementData>();
            data.assignments=new[]{
                new PhoneManagementData.Assignment{id="handover",time="22:00",title="Review shift handover",location="Security office",description="Read the previous guard's notes and check for unresolved service requests."},
                new PhoneManagementData.Assignment{id="corridors",time="22:15",title="Inspect common areas",location="2F–6F corridors",description="Check corridor lighting and confirm that shared passageways remain clear."},
                new PhoneManagementData.Assignment{id="entrance",time="22:40",title="Check entrance security",location="1F lobby",description="Inspect the entrance gate and note any damage or access problems."},
                new PhoneManagementData.Assignment{id="services",time="23:10",title="Record equipment condition",location="B1 services",description="Observe the electrical and water pump equipment. Record faults without attempting unauthorized repairs."}};
            data.requests=new[]{
                new PhoneManagementData.Request{reference="M-0241",time="20:35",title="Corridor light reported faulty",location="4F corridor",description="Ceiling fixture intermittently fails to start.",status="Reported"},
                new PhoneManagementData.Request{reference="M-0242",time="20:50",title="CCTV connection interruption",location="1F entrance camera",description="Intermittent signal loss reported during the evening shift.",status="Under review"},
                new PhoneManagementData.Request{reference="M-0243",time="21:05",title="Elevator service request",location="Passenger elevator",description="Resident reported delayed response from the landing call button.",status="Awaiting service"},
                new PhoneManagementData.Request{reference="M-0244",time="21:20",title="Water leakage report",location="B1 water pump room",description="Inspect the damp patch beside the supply manifold.",status="Reported"},
                new PhoneManagementData.Request{reference="M-0245",time="21:35",title="Unusual washing machine noise",location="306 · Shared laundry",description="Rattling reported during the final spin cycle.",status="Logged"},
                new PhoneManagementData.Request{reference="M-0246",time="21:45",title="Electrical inspection requested",location="1F utility room",description="Check the local lighting circuit after a reported trip.",status="Under review"}};
            data.records=new[]{
                new PhoneManagementData.Record{time="21:00",title="Entrance inspection",location="1F lobby",description="Gate and emergency access checked by the evening guard."},
                new PhoneManagementData.Record{time="21:15",title="Pump service recorded",location="B1 water pump room",description="Routine pressure reading entered in the maintenance log."},
                new PhoneManagementData.Record{time="21:30",title="Delivery incident recorded",location="3F corridor",description="An unattended parcel was moved clear of the escape route."},
                new PhoneManagementData.Record{time="21:50",title="Shift handover prepared",location="Security office",description="Outstanding requests passed to the night shift."}};
            AssetDatabase.CreateAsset(data,path);
        }
        manager.data=data;
        InstallManagementDashboard(apps,screen,pages);var tasks=ManagementPage(screen,"Tasks",pages,PhoneApplications.Page.Tasks);
        var tc=ManagementScroll(tasks,"Assignment List",data.assignments.Length*230+25);manager.taskList=tc.parent.gameObject;manager.taskStatuses=new Text[data.assignments.Length];
        for(int i=0;i<data.assignments.Length;i++)
        {
            var task=data.assignments[i];var row=Nav("Assignment · "+task.id,tc,new Vector2(710,215),new Vector2(0,-110-i*230),PhoneNavigationButton.Action.Task);row.index=i;
            manager.taskStatuses[i]=Copy("Status",row.transform,PhoneManagement.StatusLabel(task.status),new Vector2(220,45),new Vector2(225,70),25,false,managementAccent,TextAnchor.MiddleRight);
            Copy("Title",row.transform,task.title,new Vector2(680,75),new Vector2(0,5),35,true);Copy("Location",row.transform,task.location,new Vector2(680,55),new Vector2(0,-60),29,false,managementMuted);Divider(row.transform,-105);
        }
        TopAnchorChildren(tc);
        var detail=PhoneRect("Assignment Details",tasks,new Vector2(720,940),new Vector2(0,-35));manager.taskDetail=detail.gameObject;
        manager.detailTitle=Copy("Title",detail,"",new Vector2(680,145),new Vector2(0,335),40,true);manager.detailBody=Copy("Description",detail,"",new Vector2(680,600),new Vector2(0,-60),33,false,managementMuted,TextAnchor.UpperLeft);
        var back=Nav("Back to Assignments",detail,new Vector2(680,90),new Vector2(0,-415),PhoneNavigationButton.Action.Navigate,PhoneApplications.Page.Tasks);Copy("Label",back.transform,"‹  Back to assignments",new Vector2(680,80),Vector2.zero,30,false,phoneTheme.text);detail.gameObject.SetActive(false);
        var maintenance=ManagementPage(screen,"Maintenance",pages,PhoneApplications.Page.Maintenance);var mc=ManagementScroll(maintenance,"Service Requests",data.requests.Length*305+25);manager.requestStatuses=new Text[data.requests.Length];
        for(int i=0;i<data.requests.Length;i++)
        {
            var request=data.requests[i];float y=-35-i*305;Copy("Reference",mc,request.reference+"  ·  "+request.time,new Vector2(440,45),new Vector2(-120,y),25,false,managementMuted);
            manager.requestStatuses[i]=Copy("Status",mc,request.status,new Vector2(240,45),new Vector2(220,y),24,false,managementAccent,TextAnchor.MiddleRight);
            Copy("Problem",mc,request.title,new Vector2(680,80),new Vector2(0,y-65),34,true);Copy("Location",mc,request.location,new Vector2(680,50),new Vector2(0,y-125),28,false,managementMuted);Copy("Description",mc,request.description,new Vector2(680,105),new Vector2(0,y-207),29,false,managementMuted,TextAnchor.UpperLeft);Divider(mc,y-265);
        }
        TopAnchorChildren(mc);
        var rules=ManagementPage(screen,"Rules",pages,PhoneApplications.Page.Regulations);var rc=ManagementScroll(rules,"Night Shift Rules",apps.content.regulations.Length/2*300+20);
        for(int i=0;i<apps.content.regulations.Length/2;i++){float y=-40-i*300;Copy("Rule Title",rc,(i+1).ToString("00")+"  "+apps.content.regulations[i*2],new Vector2(680,75),new Vector2(0,y),34,true);Copy("Rule Text",rc,apps.content.regulations[i*2+1],new Vector2(680,190),new Vector2(0,y-125),32,false,managementMuted,TextAnchor.UpperLeft);Divider(rc,y-235);}TopAnchorChildren(rc);
        var records=ManagementPage(screen,"Records",pages,PhoneApplications.Page.Records);var history=ManagementScroll(records,"Activity Records",data.records.Length*275+20);
        for(int i=0;i<data.records.Length;i++){var record=data.records[i];float y=-35-i*275;Copy("Time",history,record.time+"  ·  "+record.location,new Vector2(680,45),new Vector2(0,y),26,false,managementMuted);Copy("Activity",history,record.title,new Vector2(680,70),new Vector2(0,y-65),35,true);Copy("Record",history,record.description,new Vector2(680,135),new Vector2(0,y-160),30,false,managementMuted,TextAnchor.UpperLeft);Divider(history,y-235);}TopAnchorChildren(history);
    }
    static void InstallBatteryAlerts(GameObject root)
    {
        var previous=root.transform.Find("Battery Notifications");if(previous!=null)UnityEngine.Object.DestroyImmediate(previous.gameObject);
        var canvas=PhoneRect("Battery Notifications",root.transform,new Vector2(1920,1080),Vector2.zero);var ui=canvas.gameObject.AddComponent<Canvas>();ui.renderMode=RenderMode.ScreenSpaceOverlay;ui.sortingOrder=95;
        var scaler=canvas.gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var banner=PhoneImage("Low Battery Notification",canvas,new Vector2(540,62),Vector2.zero,new Color(.035f,.045f,.05f,.95f));banner.rectTransform.anchorMin=banner.rectTransform.anchorMax=new Vector2(.5f,0);banner.rectTransform.anchoredPosition=new Vector2(0,150);
        var alerts=root.GetComponent<PhoneBatteryAlerts>();if(alerts==null)alerts=root.AddComponent<PhoneBatteryAlerts>();alerts.banner=banner.gameObject;alerts.label=Copy("Notification Text",banner.transform,"",new Vector2(510,56),Vector2.zero,26,false,null,TextAnchor.MiddleCenter);banner.gameObject.SetActive(false);
    }
    public static string UpgradePhoneServices()
    {
        phoneTheme=AssetDatabase.LoadAssetAtPath<PhoneUiTheme>("Assets/Phone/Modern UI Theme.asset");var root=PrefabUtility.LoadPrefabContents(PhonePath);
        try
        {
            var apps=root.GetComponent<PhoneApplications>();var pages=new List<PhoneApplications.ScreenPage>();
            foreach(var page in apps.pages){if(page.page==PhoneApplications.Page.Home||page.page==PhoneApplications.Page.WhatsApp||page.page==PhoneApplications.Page.Conversation||page.page==PhoneApplications.Page.Dialer)pages.Add(page);else if(page.root!=null)UnityEngine.Object.DestroyImmediate(page.root);}
            var screen=apps.GetComponent<PhonePresentation>().screen.transform;InstallManagementUi(root,apps,screen,pages);apps.pages=pages.ToArray();
            foreach(var page in pages)page.root.SetActive(page.page==PhoneApplications.Page.Home);
            // Keep system chrome, glance display and dialogs above the new app pages.
            foreach(string name in new[]{"System Chrome","Flashlight Glance Screen","System Dialog"}){var child=screen.Find(name);if(child!=null)child.SetAsLastSibling();}
            InstallBatteryAlerts(root);PrefabUtility.SaveAsPrefabAsset(root,PhonePath);AssetDatabase.SaveAssets();
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
        return "Management rebuilt as four list sections; battery notifications installed. Home and WhatsApp objects preserved.";
    }
    public static string InstallDeskPhoneCharger()
    {
        var office=GameObject.Find("Management Room").transform;var existing=office.Find("Desk Phone Charger");if(existing!=null){FitDeskChargerSupply(existing);PrefabUtility.SaveAsPrefabAsset(existing.gameObject,"Assets/Phone/Desk Phone Charger.prefab");SaveApartment();return "Charger cable connected to the existing desk power outlet.";}
        var station=G("Desk Phone Charger",office);station.position=new Vector3(-3.42f,.779f,2.59f);var charger=station.gameObject.AddComponent<PhoneChargingStation>();
        var hit=station.gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(-.015f,.025f,0);hit.size=new Vector3(.17f,.06f,.23f);
        var placement=G("Phone Placement",station);placement.localPosition=new Vector3(0,.007f,0);placement.localRotation=Quaternion.Euler(90,0,0);charger.phonePlacement=placement;
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(PhonePath);var source=prefab.transform.Find("Presentation Rig/Replaceable Phone Model");var model=UnityEngine.Object.Instantiate(source.gameObject,placement,false);model.name="Docked Phone";charger.dockedPhone=model;
        foreach(var t in model.GetComponentsInChildren<Transform>())t.gameObject.layer=0;foreach(var renderer in model.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;model.SetActive(false);
        PhonePart("USB Charger",station,new Vector3(-.06f,.016f,.13f),new Vector3(.034f,.032f,.044f),prefab.GetComponentsInChildren<Renderer>(true).First(r=>r.name=="Rounded Body").sharedMaterial,.004f);
        CommonRoute("Charging Cable",station,.002f,"Black Rubber",new Vector3(0,.006f,-.082f),new Vector3(-.055f,.006f,-.11f),new Vector3(-.08f,.006f,-.035f),new Vector3(-.08f,.006f,.08f),new Vector3(-.06f,.012f,.11f));
        B("USB Connector",station,new Vector3(0,.006f,-.082f),new Vector3(.011f,.004f,.009f),"Stainless Steel",false,.001f);
        // A short mains lead descends behind the desk to the existing service area.
        FitDeskChargerSupply(station);
        PrefabUtility.SaveAsPrefabAsset(station.gameObject,"Assets/Phone/Desk Phone Charger.prefab");EditorSceneManager.MarkSceneDirty(office.gameObject.scene);EditorSceneManager.SaveScene(office.gameObject.scene);AssetDatabase.SaveAssets();return "Desk charger fitted on the free front-left desk corner; 12% per minute, editable placement and cable.";
    }
    static void FitDeskChargerSupply(Transform station)
    {
        var original=station.Find("Charger Supply Lead");var lead=CommonRoute("Charger Supply Lead",station,.0025f,"Black Rubber",new Vector3(-.06f,.016f,.153f),new Vector3(-.11f,.006f,.18f),new Vector3(-.12f,-.12f,.22f),new Vector3(-.12f,-.62f,.22f),new Vector3(-.12f,-.62f,1.57f),new Vector3(1.14f,-.62f,1.61f),new Vector3(1.19f,-.439f,1.61f));
        if(original!=null){original.GetComponent<MeshFilter>().sharedMesh=lead.GetComponent<MeshFilter>().sharedMesh;UnityEngine.Object.DestroyImmediate(lead.gameObject);}
        if(station.Find("Mains Plug")==null)PhonePart("Mains Plug",station,new Vector3(1.19f,-.439f,1.61f),new Vector3(.032f,.030f,.024f),AssetDatabase.LoadAssetAtPath<Material>("Assets/Phone/Materials/Black Body.mat"),.004f);
        foreach(var part in station.GetComponentsInChildren<Transform>(true))part.gameObject.layer=0;
    }
}
