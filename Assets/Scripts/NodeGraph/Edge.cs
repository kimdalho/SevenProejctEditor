using System.Collections.Generic;
using UnityEngine;

public class Edge : MonoBehaviour
{
    public BaseNode MyNode;
    public BaseNode prevNode;
    public BaseNode nextNode;

    public GameObject prefab;

    public GameObject targetDot;
    public List<LinkButton> ban = new();
    public BaseNode data;

    public void SetNameless(Vector3 start, Vector3 end)
    {
        var dir = end - start;
        float len = dir.magnitude;
        var mid = (start + end) * 0.5f;
        mid.y = -20f;
        transform.position = mid;

        float angleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, angleY, 90);

        var scale = transform.localScale;
        scale.x = len;
        transform.localScale = scale;
        targetDot.transform.position = end;
    }

    public void SetDrawLine(Vector3 start, Vector3 end)
    {
        if(targetDot == null)
        {
            targetDot = Instantiate(prefab);
        }

        gameObject.SetActive(true);
        targetDot.gameObject.SetActive(true);

        //#######################1.)
        var dir = end - start;
        float len = dir.magnitude;
        var mid = (start + end) * 0.5f;
        mid.y = -20f;
        transform.position = mid;

        float angleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(90, angleY, 90);

        var scale = transform.localScale;
        scale.x = len;
        transform.localScale = scale;
        //#######################1.)
        var nearest = FindNearestDotExcludingSelf(end);
        


        if (nearest && Vector3.Distance(end, nearest.transform.position) <= 0.5f)
        {

            Debug.Log(nearest.gameObject.name);

            targetDot.transform.position = nearest.transform.position;
            dir = targetDot.transform.position - start;
            float lenX = dir.magnitude;

            mid = (start + targetDot.transform.position) * 0.5f;
            mid.y = -20f;
            transform.position = mid;

            angleY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(90, angleY, 90);
            scale = transform.localScale;
            transform.localScale = scale;

            nextNode = nearest.MyNode;
            nextNode.PrevNode = MyNode;
            nextNode.prevEdge = this;

            Debug.Log("1");
        }
        else
        {
            Debug.Log("2");

            if(nextNode != null)
            {
                nextNode.PrevNode = null;
                nextNode.prevEdge = null;
                nextNode = null;
            }
            
            

            targetDot.transform.position = end;
        }              
    }

    public BaseNode GetNextNode()
    {        
        if(nextNode == null)
        {
            this.gameObject.SetActive(false);
            targetDot.SetActive(false);
        }

        return nextNode;
    }

    private LinkButton FindNearestDotExcludingSelf(Vector3 from)
    {
        LinkButton best = null;
        float bestDist = float.MaxValue;
        var all = FindObjectsOfType<LinkButton>();
        for (int i = 0; i < all.Length; i++)
        {
            var d = all[i];
            if (d == this) continue;
            if (d.isPrev is false) continue;
            if (ban.Contains(d)) continue;
            float dist = Vector3.Distance(from, d.transform.position);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = d;
            }
        }
        return best;
    }
}
