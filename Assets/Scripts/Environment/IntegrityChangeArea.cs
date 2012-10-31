using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Collider))]
public class IntegrityChangeArea: MonoBehaviour {
    
    public float changePerSecond = 0.1f;
    public float chargeTime = 0.5f; // half a second
    
    // keep track of how long each avatar is in the area.
    private Dictionary<string, int> counters = new Dictionary<string, int>();
    
    // A visual effect that has a SetTarget for when an avatar gets close...
    public GameObject visualEffect;

    // Use this for initialization
    void Start () {
        
    }
    
    // Update is called once per frame
    void Update () {
    
    }
    
    OCPhysiologicalModel getModelFromCollider(Collider o) {
        Detector d = o.GetComponent<Detector>();
        if (d == null) return null;
        // if it has a detector it might be an avatar.
        OCPhysiologicalModel pm = o.transform.parent.GetComponent<OCPhysiologicalModel>();
        return pm;
    }
    
    void OnTriggerExit (Collider other) {
        OCPhysiologicalModel pm = getModelFromCollider(other);
        if (pm == null) return;         
        if (counters.ContainsKey(other.name)) counters.Remove(other.name);
        
    }
    
    void OnTriggerEnter (Collider other) {
        OCPhysiologicalModel pm = getModelFromCollider(other);
        if (pm == null) return;          
        counters[other.name] = 0;
    }
    
    void OnTriggerStay (Collider other) {
        OCPhysiologicalModel pm = getModelFromCollider(other);
        // we can only have a physiological effect if there is a model present.
        if (pm == null) return;
        if (counters[other.name] > (chargeTime / Time.fixedDeltaTime)) {
            PhysiologicalEffect pe = new PhysiologicalEffect(PhysiologicalEffect.CostLevel.NONE);
            pe.fitnessChange = changePerSecond * chargeTime;
            
            Debug.LogWarning("Applying change of " + pe.fitnessChange + " to avatar " + other.name);
            pm.processPhysiologicalEffect(pe);
            if (visualEffect != null)
            {
                visualEffect.active = true;
                visualEffect.SendMessage("SetTarget", pm.gameObject);
            }
            counters[other.name] = 0;
        } else {
            if (visualEffect != null && visualEffect.active)
            {
                // This also will deactivate the visual effect
                visualEffect.SendMessage("NoTarget");
            }
            counters[other.name] += 1;
        }
        
    }

}
