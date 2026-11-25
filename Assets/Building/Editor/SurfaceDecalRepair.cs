using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static partial class ApartmentBuilder
{
    static bool DecalSupport(Vector3 point,Vector3 normal,out Vector3 surface)
    {
        foreach(var hit in Physics.RaycastAll(point+normal*.025f,-normal,.15f,~0,QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))
        {
            var r=hit.collider.GetComponent<Renderer>();if(r==null||!r.enabled||Vector3.Dot(hit.normal,normal)<.94f)continue;var size=hit.collider.bounds.size;var dimensions=new[]{size.x,size.y,size.z}.OrderByDescending(v=>v).ToArray();if(dimensions[1]<.20f)continue;
            surface=hit.point;return true;
        }
        surface=point;return false;
    }
    static string ClipSurfaceDecals(Transform scope)
    {
        if(!AssetDatabase.IsValidFolder(BuildingBase+"Models/Surface Decals"))AssetDatabase.CreateFolder(BuildingBase+"Models","Surface Decals");Physics.SyncTransforms();int fitted=0,hidden=0,triangles=0;
        foreach(var renderer in scope.GetComponentsInChildren<MeshRenderer>(true).Where(r=>r.enabled&&r.sharedMaterial!=null&&AssetDatabase.GetAssetPath(r.sharedMaterial).StartsWith("Assets/Alley/Materials/Weathering/")).ToArray())
        {
            var filter=renderer.GetComponent<MeshFilter>();if(filter==null)continue;var t=renderer.transform;var normal=t.rotation*Vector3.back;int nx=Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(t.lossyScale.x)/.025f),8,64),ny=Mathf.Clamp(Mathf.CeilToInt(Mathf.Abs(t.lossyScale.y)/.025f),8,64),n=(nx+1)*(ny+1);var vertices=new Vector3[n];var uv=new Vector2[n];var valid=new bool[n];var depth=new float[n];var depths=new List<float>();
            for(int y=0;y<=ny;y++)for(int x=0;x<=nx;x++)
            {int i=y*(nx+1)+x;uv[i]=new Vector2(x/(float)nx,y/(float)ny);var p=t.TransformPoint(new Vector3(uv[i].x-.5f,uv[i].y-.5f,0));Vector3 support;valid[i]=DecalSupport(p,normal,out support);depth[i]=Vector3.Dot(support-p,normal);if(valid[i])depths.Add(depth[i]);vertices[i]=t.InverseTransformPoint(support+normal*.0009f);}
            if(depths.Count==0){renderer.enabled=false;hidden++;continue;}depths.Sort();float plane=depths[depths.Count/2];for(int i=0;i<n;i++)valid[i]&=Mathf.Abs(depth[i]-plane)<.004f;
            var indices=new List<int>();var full=new bool[nx*ny];var used=new bool[nx*ny];for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){int a=y*(nx+1)+x,b=a+nx+1;full[y*nx+x]=valid[a]&&valid[b]&&valid[b+1]&&valid[a+1];}
            for(int y=0;y<ny;y++)for(int x=0;x<nx;x++)
            {
                int cell=y*nx+x;if(!full[cell]||used[cell])continue;int width=1;while(x+width<nx&&full[cell+width]&&!used[cell+width])width++;int height=1;bool grow=true;while(y+height<ny&&grow){for(int i=0;i<width;i++)if(!full[(y+height)*nx+x+i]||used[(y+height)*nx+x+i]){grow=false;break;}if(grow)height++;}
                for(int j=0;j<height;j++)for(int i=0;i<width;i++)used[(y+j)*nx+x+i]=true;int a=y*(nx+1)+x,b=(y+height)*(nx+1)+x,c=b+width,d=a+width;indices.AddRange(new[]{a,b,c,a,c,d});
            }
            for(int y=0;y<ny;y++)for(int x=0;x<nx;x++){if(full[y*nx+x])continue;int a=y*(nx+1)+x,b=a+nx+1,c=b+1,d=a+1;if(valid[a]&&valid[b]&&valid[c])indices.AddRange(new[]{a,b,c});if(valid[a]&&valid[c]&&valid[d])indices.AddRange(new[]{a,c,d});}
            if(indices.Count==0){renderer.enabled=false;hidden++;continue;}
            var kept=indices.Distinct().ToArray();var remap=kept.Select((value,index)=>new{value,index}).ToDictionary(p=>p.value,p=>p.index);var mesh=new Mesh{name="Mounted Surface"};mesh.vertices=kept.Select(i=>vertices[i]).ToArray();mesh.uv=kept.Select(i=>uv[i]).ToArray();mesh.triangles=indices.Select(i=>remap[i]).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();string path=AssetDatabase.GetAssetPath(filter.sharedMesh);
            if(path.StartsWith(BuildingBase+"Models/Surface Decals/")){EditorUtility.CopySerialized(mesh,filter.sharedMesh);UnityEngine.Object.DestroyImmediate(mesh);}else{string location=string.Join(" ",t.GetComponentsInParent<Transform>().Reverse().Select(p=>p.name).ToArray());location=System.Text.RegularExpressions.Regex.Replace(location,@"[^\w\- ]"," ");path=AssetDatabase.GenerateUniqueAssetPath(BuildingBase+"Models/Surface Decals/"+location+".asset");AssetDatabase.CreateAsset(mesh,path);filter.sharedMesh=mesh;}
            renderer.lightmapIndex=-1;triangles+=indices.Count/3;fitted++;PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);PrefabUtility.RecordPrefabInstancePropertyModifications(filter);
        }
        return "Fitted surface overlays="+fitted+"; unsupported overlays hidden="+hidden+"; supported triangles="+triangles;
    }
    public static string RepairInteriorSurfaceDecals()
    {
        EnsureApartment();Directory.CreateDirectory("References/Surface Mounting");string result=string.Join("\n",new[]{"Apartment","Aoba Exterior","Side Streets"}.Where(n=>GameObject.Find(n)!=null).Select(n=>n+": "+ClipSurfaceDecals(GameObject.Find(n).transform)).ToArray());SaveApartment();Capture("Elevator Wall Surfaces",new Vector3(.05f,1.65f,9.35f),new Vector3(3.13f,1.6f,10.8f));File.Copy("References/Apartment/Captures/Elevator Wall Surfaces.png","References/Surface Mounting/Elevator Wall Surfaces.png",true);File.WriteAllText("References/Surface Mounting/Checks.txt",result);return result;
    }
}
