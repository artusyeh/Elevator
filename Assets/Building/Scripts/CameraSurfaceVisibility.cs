using UnityEngine;

// Camera work is needed only when some part of its screen or mirror can be seen.
public static class CameraSurfaceVisibility
{
    public static bool CanSee(Camera viewer,Renderer surface,Transform owner,float distance,Plane[] planes,RaycastHit[] hits,Vector3 outwardNormal=default)
    {
        if(viewer==null||surface==null||!surface.enabled||!surface.gameObject.activeInHierarchy)return false;
        var bounds=surface.bounds;var origin=viewer.transform.position;
        if((bounds.ClosestPoint(origin)-origin).sqrMagnitude>distance*distance||!GeometryUtility.TestPlanesAABB(planes,bounds))return false;
        if(outwardNormal.sqrMagnitude<.01f)outwardNormal=-surface.transform.forward;
        if(Vector3.Dot(outwardNormal,origin-bounds.center)<=0)return false;
        var ext=bounds.extents*.80f;
        for(int sample=0;sample<5;sample++)
        {
            var point=bounds.center;if(sample>0)point+=new Vector3(sample%2==0?ext.x:-ext.x,sample<3?ext.y:-ext.y,sample%2==0?ext.z:-ext.z);
            var delta=point-origin;float length=delta.magnitude;if(length<.05f)return true;
            int count=Physics.RaycastNonAlloc(origin,delta/length,hits,length-.005f,~0,QueryTriggerInteraction.Ignore);
            bool blocked=count==hits.Length;
            for(int i=0;i<count;i++)
            {
                var collider=hits[i].collider;if(collider==null||collider is CharacterController||collider.transform.IsChildOf(owner))continue;
                var renderer=collider.GetComponent<Renderer>();var material=renderer!=null?renderer.sharedMaterial:null;
                if(material!=null&&(material.renderQueue>=3000||material.HasProperty("_Surface")&&material.GetFloat("_Surface")>0))continue;
                blocked=true;break;
            }
            if(!blocked)return true;
        }
        return false;
    }
}
