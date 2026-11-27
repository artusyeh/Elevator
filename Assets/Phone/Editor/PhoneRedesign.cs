using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static PhoneUiTheme phoneTheme;
    static PhoneIcon Shape(string name,Transform parent,Vector2 size,Vector2 at,Color color,PhoneIcon.Symbol symbol=PhoneIcon.Symbol.RoundedRect,float radius=.16f)
    {var r=PhoneRect(name,parent,size,at);var g=r.gameObject.AddComponent<PhoneIcon>();g.symbol=symbol;g.color=color;g.cornerRadius=radius;g.raycastTarget=false;return g;}
    static Text Copy(string name,Transform parent,string value,Vector2 size,Vector2 at,int pixels=34,bool bold=false,Color? tint=null,TextAnchor align=TextAnchor.MiddleLeft)
    {var text=PhoneText(name,parent,value,size,at,pixels,align);text.font=phoneTheme.font;text.fontStyle=bold?FontStyle.Bold:FontStyle.Normal;text.color=tint??phoneTheme.text;text.verticalOverflow=VerticalWrapMode.Truncate;return text;}
    static PhoneNavigationButton Nav(string name,Transform parent,Vector2 size,Vector2 at,PhoneNavigationButton.Action action,PhoneApplications.Page page=PhoneApplications.Page.Home,Color? color=null)
    {
        var graphic=Shape(name,parent,size,at,color??Color.clear);graphic.raycastTarget=true;var button=graphic.gameObject.AddComponent<Button>();button.targetGraphic=graphic;button.navigation=new Navigation{mode=Navigation.Mode.None};
        var c=button.colors;c.highlightedColor=new Color(.8f,.87f,.91f);c.pressedColor=new Color(.60f,.7f,.75f);button.colors=c;
        var nav=graphic.gameObject.AddComponent<PhoneNavigationButton>();nav.action=action;nav.page=page;return nav;
    }
    static void Divider(Transform parent,float y){PhoneImage("Divider",parent,new Vector2(680,1.5f),new Vector2(0,y),new Color(.15f,.19f,.21f));}
    static RectTransform AppPage(Transform screen,string name,string title,List<PhoneApplications.ScreenPage> pages,PhoneApplications.Page page)
    {
        var root=PhoneRect(name,screen,new Vector2(800,1600),Vector2.zero);PhoneImage("App Background",root,new Vector2(800,1600),Vector2.zero,phoneTheme.background);
        var back=Nav("Back",root,new Vector2(80,90),new Vector2(-335,625),PhoneNavigationButton.Action.Back);Shape("Back Icon",back.transform,new Vector2(42,42),Vector2.zero,phoneTheme.accent,PhoneIcon.Symbol.Back);
        Copy("App Title",root,title,new Vector2(585,80),new Vector2(35,625),43,true);Divider(root,565);
        pages.Add(new PhoneApplications.ScreenPage{page=page,root=root.gameObject});return root;
    }
    static RectTransform ScrollArea(Transform root,string name,float contentHeight)
    {
        var viewport=PhoneRect(name,root,new Vector2(720,1120),new Vector2(0,-25));var bg=viewport.gameObject.AddComponent<Image>();bg.color=Color.clear;viewport.gameObject.AddComponent<RectMask2D>();
        var content=PhoneRect("Content",viewport,new Vector2(720,contentHeight),Vector2.zero);content.anchorMin=content.anchorMax=new Vector2(.5f,1);content.pivot=new Vector2(.5f,1);content.anchoredPosition=Vector2.zero;
        var scroll=viewport.gameObject.AddComponent<ScrollRect>();scroll.viewport=viewport;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=45;scroll.inertia=true;return content;
    }
    static InputField Entry(string name,Transform parent,Vector2 size,Vector2 at,string hint,bool multiline=false)
    {
        var bg=Shape(name,parent,size,at,phoneTheme.surface);bg.raycastTarget=true;var field=bg.gameObject.AddComponent<InputField>();field.targetGraphic=bg;
        field.lineType=multiline?InputField.LineType.MultiLineNewline:InputField.LineType.SingleLine;field.characterLimit=multiline?350:110;
        var text=Copy("Editable Text",bg.transform,"",size-new Vector2(40,22),Vector2.zero,32,false,null,multiline?TextAnchor.UpperLeft:TextAnchor.MiddleLeft);text.supportRichText=false;
        var placeholder=Copy("Placeholder",bg.transform,hint,size-new Vector2(40,22),Vector2.zero,32,false,phoneTheme.muted,multiline?TextAnchor.UpperLeft:TextAnchor.MiddleLeft);field.textComponent=text;field.placeholder=placeholder;return field;
    }
    public static string RedesignPhoneUi()
    {
        const string themePath="Assets/Phone/Modern UI Theme.asset",contentPath="Assets/Phone/Prototype Content.asset";
        phoneTheme=AssetDatabase.LoadAssetAtPath<PhoneUiTheme>(themePath);if(phoneTheme==null){phoneTheme=ScriptableObject.CreateInstance<PhoneUiTheme>();phoneTheme.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");AssetDatabase.CreateAsset(phoneTheme,themePath);}
        var content=AssetDatabase.LoadAssetAtPath<PhoneContent>(contentPath);if(content==null)
        {
            content=ScriptableObject.CreateInstance<PhoneContent>();content.directory=AssetDatabase.LoadAssetAtPath<BuildingDirectory>(BuildingBase+"Apartments.asset");
            content.contacts=new[]{
                new PhoneContent.Contact{name="Building Office",preview="The shift log is on the desk.",time="22:41",unread=true,incoming="The shift log is on the desk. Please leave your notes there.",outgoing="Thanks. I'll check it before the next round."},
                new PhoneContent.Contact{name="Maintenance",preview="Received. Thank you.",time="21:18",incoming="Spare parts are stored in the maintenance room.",outgoing="Received. Thank you."},
                new PhoneContent.Contact{name="Resident · 503",preview="Thank you for your help.",time="Yesterday",incoming="Thank you for your help earlier.",outgoing="You're welcome. Have a good evening."}};
            content.tasks=new[]{"Review the shift log","Check shared corridor lights","Inspect the entrance gate","Record equipment condition"};
            content.maintenance=new[]{"Common-area lighting|Routine inspection|Open","Water pump service|Scheduled check|Pending","Entrance intercom|Sound quality check|Reviewed"};
            content.regulations=new[]{"Keep common areas clear","Do not leave deliveries or personal items in escape routes. Keep stair doors and access panels unobstructed.","Respect quiet hours","Avoid unnecessary noise between 22:00 and 07:00. Record repeated complaints in the shift log.","Handle keys responsibly","Building keys remain on the security office board when not in use. Return keys after maintenance work.","Electrical and mechanical access","Only authorized staff may enter B1 service rooms. Isolate equipment before carrying out repairs.","Report safety concerns","Record faults, leaks and damaged fixtures. Use emergency services when there is immediate danger."};
            AssetDatabase.CreateAsset(content,contentPath);
        }
        var root=PrefabUtility.LoadPrefabContents(PhonePath);
        try
        {
            var apps=root.GetComponent<PhoneApplications>();var show=root.GetComponent<PhonePresentation>();var canvas=show.screen.GetComponent<Canvas>();var screen=(RectTransform)canvas.transform;
            foreach(Transform t in screen.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(t.gameObject);
            if(screen.GetComponent<RectMask2D>()==null)screen.gameObject.AddComponent<RectMask2D>();
            apps.content=content;apps.homeButton=apps.backButton=null;
            var pages=new List<PhoneApplications.ScreenPage>();
            var home=PhoneRect("Home Screen",screen,new Vector2(800,1600),Vector2.zero);pages.Add(new PhoneApplications.ScreenPage{page=PhoneApplications.Page.Home,root=home.gameObject});apps.home=home.gameObject;
            var wallpaper=PhoneRect("Atmospheric Wallpaper",home,new Vector2(800,1600),Vector2.zero).gameObject.AddComponent<PhoneWallpaper>();wallpaper.raycastTarget=false;
            apps.largeTime=Copy("Clock",home,"23:14",new Vector2(700,180),new Vector2(0,440),133,false,null,TextAnchor.MiddleCenter);
            apps.dateLabel=Copy("Date",home,"Thursday, 26 November",new Vector2(700,70),new Vector2(0,325),36,false,new Color(.72f,.80f,.84f),TextAnchor.MiddleCenter);
            var waIcon=Nav("WhatsApp",home,new Vector2(140,140),new Vector2(-260,15),PhoneNavigationButton.Action.Navigate,PhoneApplications.Page.WhatsApp,new Color(.06f,.62f,.36f));Shape("Chat Symbol",waIcon.transform,new Vector2(108,108),Vector2.zero,Color.white,PhoneIcon.Symbol.Chat);apps.whatsAppButton=waIcon.GetComponent<Button>();
            Copy("App Name",home,"WhatsApp",new Vector2(180,80),new Vector2(-260,-105),29,false,null,TextAnchor.MiddleCenter);
            var amIcon=Nav("Apartment Management",home,new Vector2(140,140),new Vector2(-65,15),PhoneNavigationButton.Action.Navigate,PhoneApplications.Page.Management,new Color(.22f,.36f,.49f));Shape("Building Symbol",amIcon.transform,new Vector2(100,100),Vector2.zero,Color.white,PhoneIcon.Symbol.Building);apps.managementButton=amIcon.GetComponent<Button>();
            Copy("App Name",home,"Apartment\nManagement",new Vector2(190,95),new Vector2(-65,-111),28,false,null,TextAnchor.MiddleCenter);
            Shape("Page Dot",home,new Vector2(8,8),new Vector2(0,-430),new Color(.75f,.81f,.85f),PhoneIcon.Symbol.RoundedRect,.5f);
            Shape("Translucent Dock",home,new Vector2(680,185),new Vector2(0,-575),new Color(.20f,.28f,.32f,.52f),PhoneIcon.Symbol.RoundedRect,.28f);
            foreach(bool light in new[]{false,true})
            {
                var icon=Nav(light?"Flashlight Shortcut":"Phone Shortcut",home,new Vector2(122,122),new Vector2(light?155:-155,-575),light?PhoneNavigationButton.Action.Flashlight:PhoneNavigationButton.Action.Navigate,PhoneApplications.Page.Dialer,light?new Color(.27f,.32f,.36f):new Color(.10f,.49f,.32f));Shape("Shortcut Icon",icon.transform,new Vector2(78,78),Vector2.zero,Color.white,light?PhoneIcon.Symbol.Flashlight:PhoneIcon.Symbol.Phone);
            }
            var chats=AppPage(screen,"WhatsApp · Conversations","Chats",pages,PhoneApplications.Page.WhatsApp);apps.whatsApp=chats.gameObject;
            Copy("Search Hint",chats,"Messages",new Vector2(650,75),new Vector2(0,492),31,false,phoneTheme.muted);
            for(int i=0;i<content.contacts.Length;i++)
            {
                var c=content.contacts[i];float y=335-i*185;var row=Nav("Conversation · "+c.name,chats,new Vector2(720,174),new Vector2(0,y),PhoneNavigationButton.Action.Conversation);row.index=i;
                Shape("Avatar",row.transform,new Vector2(104,104),new Vector2(-285,4),i==0?new Color(.18f,.40f,.36f):new Color(.22f,.30f,.36f),PhoneIcon.Symbol.RoundedRect,.5f);Shape("Profile",row.transform,new Vector2(75,75),new Vector2(-285,4),new Color(.70f,.80f,.82f),PhoneIcon.Symbol.Person);
                Copy("Contact",row.transform,c.name,new Vector2(390,55),new Vector2(-15,32),35,true);Copy("Preview",row.transform,c.preview,new Vector2(440,65),new Vector2(10,-35),29,false,phoneTheme.muted);
                Copy("Time",row.transform,c.time,new Vector2(155,40),new Vector2(260,42),24,false,c.unread?phoneTheme.accent:phoneTheme.muted,TextAnchor.MiddleRight);
                if(c.unread){Shape("Unread",row.transform,new Vector2(34,34),new Vector2(311,-27),phoneTheme.accent,PhoneIcon.Symbol.RoundedRect,.5f);Copy("Count",row.transform,"1",new Vector2(30,32),new Vector2(311,-27),23,true,phoneTheme.background,TextAnchor.MiddleCenter);}
                PhoneImage("Row Divider",row.transform,new Vector2(535,1),new Vector2(70,-85),new Color(.15f,.19f,.21f));
            }
            var chat=AppPage(screen,"WhatsApp · Conversation","Building Office",pages,PhoneApplications.Page.Conversation);apps.conversationTitle=chat.Find("App Title").GetComponent<Text>();apps.conversationTitle.rectTransform.sizeDelta=new Vector2(500,80);apps.conversationTitle.rectTransform.anchoredPosition=new Vector2(-5,625);apps.conversationTitle.fontSize=36;
            var call=Nav("Call Contact",chat,new Vector2(75,80),new Vector2(320,625),PhoneNavigationButton.Action.Call);Shape("Call Icon",call.transform,new Vector2(48,48),Vector2.zero,phoneTheme.accent,PhoneIcon.Symbol.Phone);
            Shape("Date Pill",chat,new Vector2(175,55),new Vector2(0,475),phoneTheme.surface);Copy("Date Separator",chat,"TODAY",new Vector2(160,50),new Vector2(0,475),24,false,phoneTheme.muted,TextAnchor.MiddleCenter);
            Shape("Incoming Bubble",chat,new Vector2(565,195),new Vector2(-65,290),phoneTheme.surface);apps.incomingText=Copy("Incoming Message",chat,content.contacts[0].incoming,new Vector2(515,140),new Vector2(-65,300),33);Copy("Incoming Time",chat,"22:41",new Vector2(490,35),new Vector2(-65,216),22,false,phoneTheme.muted,TextAnchor.MiddleRight);
            Shape("Outgoing Bubble",chat,new Vector2(550,180),new Vector2(75,55),new Color(.075f,.28f,.24f));apps.outgoingText=Copy("Outgoing Message",chat,content.contacts[0].outgoing,new Vector2(500,128),new Vector2(75,65),33);Copy("Outgoing Time",chat,"22:43  ✓✓",new Vector2(495,32),new Vector2(75,-11),22,false,phoneTheme.muted,TextAnchor.MiddleRight);
            apps.sentText=Copy("Local Draft",chat,"",new Vector2(620,200),new Vector2(20,-190),31,false,phoneTheme.accent);
            apps.messageInput=Entry("Message Input",chat,new Vector2(595,95),new Vector2(-43,-565),"Message");var send=Nav("Send Draft",chat,new Vector2(85,85),new Vector2(305,-565),PhoneNavigationButton.Action.SendMessage,PhoneApplications.Page.Home,new Color(.08f,.38f,.28f));Shape("Send Icon",send.transform,new Vector2(54,54),Vector2.zero,phoneTheme.text,PhoneIcon.Symbol.Send);
            var management=AppPage(screen,"Management · Overview","Aoba · Building Services",pages,PhoneApplications.Page.Management);apps.management=management.gameObject;management.Find("App Title").GetComponent<Text>().fontSize=35;
            Copy("Role",management,"SECURITY OFFICE",new Vector2(680,45),new Vector2(0,485),26,true,phoneTheme.accent);Copy("Greeting",management,"Evening shift",new Vector2(680,80),new Vector2(0,415),53,true);Copy("Shift Range",management,"22:00 – 07:00  ·  Staff workspace",new Vector2(680,55),new Vector2(0,345),29,false,phoneTheme.muted);
            string[] titles={"Today's tasks","Shift progress","Room directory","Maintenance","Regulations","Incident reports"};var symbols=new[]{PhoneIcon.Symbol.Tasks,PhoneIcon.Symbol.Progress,PhoneIcon.Symbol.Directory,PhoneIcon.Symbol.Wrench,PhoneIcon.Symbol.Rules,PhoneIcon.Symbol.Report};var dest=new[]{PhoneApplications.Page.Tasks,PhoneApplications.Page.Shift,PhoneApplications.Page.Directory,PhoneApplications.Page.Maintenance,PhoneApplications.Page.Regulations,PhoneApplications.Page.Reports};
            for(int i=0;i<6;i++){var tile=Nav(titles[i],management,new Vector2(328,222),new Vector2(i%2==0?-177:177,150-i/2*255),PhoneNavigationButton.Action.Navigate,dest[i],phoneTheme.surface);Shape("Section Icon",tile.transform,new Vector2(65,65),new Vector2(-105,51),phoneTheme.accent,symbols[i]);Copy("Section Name",tile.transform,titles[i],new Vector2(285,78),new Vector2(0,-48),31,true);}
            var tasks=AppPage(screen,"Management · Tasks","Today's tasks",pages,PhoneApplications.Page.Tasks);Copy("Date",tasks,"SHIFT CHECKLIST",new Vector2(680,55),new Vector2(0,488),26,true,phoneTheme.muted);
            apps.taskLabels=new Text[content.tasks.Length];apps.taskChecks=new PhoneIcon[content.tasks.Length];
            for(int i=0;i<content.tasks.Length;i++){var row=Nav("Task "+i,tasks,new Vector2(700,175),new Vector2(0,337-i*202),PhoneNavigationButton.Action.Task,PhoneApplications.Page.Home,phoneTheme.surface);row.index=i;Shape("Check Box",row.transform,new Vector2(56,56),new Vector2(-282,0),new Color(.15f,.23f,.25f));apps.taskChecks[i]=Shape("Checked",row.transform,new Vector2(52,52),new Vector2(-282,0),phoneTheme.accent,PhoneIcon.Symbol.Check);Copy("Task Title",row.transform,content.tasks[i],new Vector2(515,82),new Vector2(42,25),33,true);apps.taskLabels[i]=Copy("Task Status",row.transform,"To do",new Vector2(515,45),new Vector2(42,-40),27,false,phoneTheme.muted);}
            var shift=AppPage(screen,"Management · Shift","Shift progress",pages,PhoneApplications.Page.Shift);Copy("Shift Heading",shift,"Evening shift",new Vector2(680,100),new Vector2(0,420),55,true);Copy("Shift Hours",shift,"22:00 – 07:00",new Vector2(680,70),new Vector2(0,330),37,false,phoneTheme.muted);
            apps.progressText=Copy("Completion",shift,"0 of 4 checks completed",new Vector2(680,100),new Vector2(0,130),38,true);PhoneImage("Progress Track",shift,new Vector2(680,16),new Vector2(0,40),phoneTheme.surface);apps.progressFill=PhoneImage("Progress Fill",shift,new Vector2(680,16),new Vector2(0,40),phoneTheme.accent);apps.progressFill.type=Image.Type.Filled;apps.progressFill.fillMethod=Image.FillMethod.Horizontal;apps.progressFill.fillOrigin=0;apps.progressFill.fillAmount=0;
            var checks=Nav("Open Checklist",shift,new Vector2(680,100),new Vector2(0,-300),PhoneNavigationButton.Action.Navigate,PhoneApplications.Page.Tasks,phoneTheme.surface);Copy("Label",checks.transform,"View today's tasks",new Vector2(620,80),Vector2.zero,34,true);
            var directory=AppPage(screen,"Management · Directory","Room directory",pages,PhoneApplications.Page.Directory);var units=content.directory.units;var dc=ScrollArea(directory,"Directory Scroll",units.Length*108+100);
            for(int i=0;i<units.Length;i++){float y=-60-i*108;Copy("Unit "+units[i].number,dc,units[i].number,new Vector2(200,70),new Vector2(-230,y),40,true);Copy("Floor",dc,units[i].floor+"F  ·  "+(string.IsNullOrEmpty(units[i].purpose)?"Residential unit":units[i].purpose),new Vector2(430,70),new Vector2(100,y),29,false,phoneTheme.muted);PhoneImage("Divider",dc,new Vector2(680,1),new Vector2(0,y-53),phoneTheme.surface);}
            var maintenance=AppPage(screen,"Management · Maintenance","Maintenance requests",pages,PhoneApplications.Page.Maintenance);maintenance.Find("App Title").GetComponent<Text>().fontSize=35;
            for(int i=0;i<content.maintenance.Length;i++){var parts=content.maintenance[i].Split('|');float y=330-i*245;Shape("Request Card",maintenance,new Vector2(700,215),new Vector2(0,y),phoneTheme.surface);Copy("Request",maintenance,parts[0],new Vector2(635,72),new Vector2(0,y+50),35,true);Copy("Description",maintenance,parts[1],new Vector2(635,60),new Vector2(0,y-15),30,false,phoneTheme.muted);Copy("Status",maintenance,parts[2],new Vector2(635,45),new Vector2(0,y-72),27,true,phoneTheme.accent);}
            var rules=AppPage(screen,"Management · Regulations","Building regulations",pages,PhoneApplications.Page.Regulations);var rc=ScrollArea(rules,"Regulations Scroll",content.regulations.Length/2*300+20);
            for(int i=0;i<content.regulations.Length/2;i++){float y=-40-i*300;Copy("Rule Title",rc,(i+1).ToString("00")+"  "+content.regulations[i*2],new Vector2(680,75),new Vector2(0,y),34,true);Copy("Rule Text",rc,content.regulations[i*2+1],new Vector2(680,190),new Vector2(0,y-125),32,false,phoneTheme.muted,TextAnchor.UpperLeft);PhoneImage("Divider",rc,new Vector2(680,1),new Vector2(0,y-235),phoneTheme.surface);}
            var reports=AppPage(screen,"Management · Reports","Incident reports",pages,PhoneApplications.Page.Reports);Copy("Report Hint",reports,"New draft",new Vector2(680,80),new Vector2(0,450),48,true);Copy("Report Help",reports,"Record an observation for the next shift.",new Vector2(680,100),new Vector2(0,350),32,false,phoneTheme.muted);apps.reportInput=Entry("Incident Notes",reports,new Vector2(680,410),new Vector2(0,60),"Location and observation…",true);
            var submit=Nav("Save Incident Draft",reports,new Vector2(680,105),new Vector2(0,-250),PhoneNavigationButton.Action.SubmitReport,PhoneApplications.Page.Home,new Color(.08f,.32f,.27f));Copy("Label",submit.transform,"Save draft",new Vector2(600,90),Vector2.zero,35,true,null,TextAnchor.MiddleCenter);apps.reportFeedback=Copy("Feedback",reports,"",new Vector2(680,100),new Vector2(0,-390),29,false,phoneTheme.muted);
            var dialer=AppPage(screen,"Phone · Dialer","Phone",pages,PhoneApplications.Page.Dialer);apps.dialText=Copy("Number",dialer,"",new Vector2(690,100),new Vector2(0,435),48,false,null,TextAnchor.MiddleCenter);
            string[] digits={"1","2","3","4","5","6","7","8","9","*","0","#"};for(int i=0;i<12;i++){var key=Nav("Dial "+digits[i],dialer,new Vector2(150,140),new Vector2(-210+i%3*210,260-i/3*180),PhoneNavigationButton.Action.Dial,PhoneApplications.Page.Home,phoneTheme.surface);key.argument=digits[i];Copy("Digit",key.transform,digits[i],new Vector2(100,100),Vector2.zero,52,false,null,TextAnchor.MiddleCenter);}
            var dial=Nav("Call Number",dialer,new Vector2(140,110),new Vector2(0,-510),PhoneNavigationButton.Action.Call,PhoneApplications.Page.Home,new Color(.08f,.45f,.28f));Shape("Call",dial.transform,new Vector2(62,62),Vector2.zero,phoneTheme.text,PhoneIcon.Symbol.Phone);var erase=Nav("Delete Digit",dialer,new Vector2(140,100),new Vector2(210,-510),PhoneNavigationButton.Action.ClearDial);Shape("Backspace",erase.transform,new Vector2(48,48),Vector2.zero,phoneTheme.muted,PhoneIcon.Symbol.Back);
            // Shared chrome remains above app pages. Nothing is a static screenshot.
            var chrome=PhoneRect("System Chrome",screen,new Vector2(800,1600),Vector2.zero);apps.timeLabel=Copy("Status Time",chrome,"23:14",new Vector2(190,60),new Vector2(-265,729),30,true);
            Shape("Signal",chrome,new Vector2(43,37),new Vector2(165,729),phoneTheme.text,PhoneIcon.Symbol.Signal);Shape("Wi-Fi",chrome,new Vector2(42,39),new Vector2(224,729),phoneTheme.text,PhoneIcon.Symbol.Wifi);apps.batteryLabel=Copy("Battery Percentage",chrome,"100%",new Vector2(90,50),new Vector2(288,729),26,true,null,TextAnchor.MiddleCenter);Shape("Battery Icon",chrome,new Vector2(46,37),new Vector2(355,729),phoneTheme.text,PhoneIcon.Symbol.Battery);
            var homeAction=Nav("Home Indicator",chrome,new Vector2(310,100),new Vector2(0,-727),PhoneNavigationButton.Action.Navigate);Shape("Home Bar",homeAction.transform,new Vector2(235,9),Vector2.zero,new Color(.8f,.85f,.86f),PhoneIcon.Symbol.RoundedRect,.5f);apps.homeButton=homeAction.GetComponent<Button>();
            var glance=PhoneRect("Flashlight Glance Screen",screen,new Vector2(800,1600),Vector2.zero);apps.glance=glance.gameObject;PhoneImage("Glance Background",glance,new Vector2(800,1600),Vector2.zero,new Color(.016f,.035f,.045f));
            Shape("Flashlight",glance,new Vector2(100,100),new Vector2(-235,625),phoneTheme.accent,PhoneIcon.Symbol.Flashlight);apps.glanceBattery=Copy("Glance Battery",glance,"100%\nBATTERY",new Vector2(420,155),new Vector2(55,625),53,true,null,TextAnchor.MiddleCenter);Shape("Battery Outline",glance,new Vector2(68,50),new Vector2(300,690),phoneTheme.text,PhoneIcon.Symbol.Battery);glance.gameObject.SetActive(false);
            var modal=PhoneRect("System Dialog",screen,new Vector2(800,1600),Vector2.zero);apps.modal=modal.gameObject;var shade=PhoneImage("Scrim",modal,new Vector2(800,1600),Vector2.zero,new Color(0,0,0,.78f));shade.raycastTarget=true;Shape("Dialog Card",modal,new Vector2(680,400),Vector2.zero,phoneTheme.surface);apps.modalText=Copy("Dialog Text",modal,"",new Vector2(590,230),new Vector2(0,40),34,false,null,TextAnchor.MiddleCenter);var dismiss=Nav("Dismiss Dialog",modal,new Vector2(590,80),new Vector2(0,-135),PhoneNavigationButton.Action.Back);Copy("Dismiss Text",dismiss.transform,"OK",new Vector2(550,70),Vector2.zero,34,true,phoneTheme.accent,TextAnchor.MiddleCenter);modal.gameObject.SetActive(false);
            foreach(var scroll in root.GetComponentsInChildren<ScrollRect>(true))
                foreach(RectTransform item in scroll.content){var at=item.anchoredPosition;item.anchorMin=item.anchorMax=new Vector2(.5f,1);item.anchoredPosition=at;}
            apps.pages=pages.ToArray();foreach(var entry in pages)entry.root.SetActive(entry.page==PhoneApplications.Page.Home);
            show.flashlightPosition=new Vector3(.16f,-.145f,.24f);show.screen.alpha=0;show.screen.blocksRaycasts=false;show.screen.interactable=false;
            PrefabUtility.SaveAsPrefabAsset(root,PhonePath);AssetDatabase.SaveAssets();return "Redesigned 11 phone pages, editable vector icons/wallpaper, actual 30-unit directory, conversations, local drafts, checklist, dock shortcuts and physical battery glance.";
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
}
