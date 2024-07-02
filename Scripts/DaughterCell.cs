using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DaughterCell : MonoBehaviour
{
    public MeshRenderer nuclearMembrane;
    public Centriole centriole;
    [Range(0,1)]
    public float targetTransparency=0.8f;
    public float transparencyIncreaseRate=0.001f;

    public event EventHandler OnMembraneDissolved;
    private MeiosisManager mm;

    public float currentTransparency;
    private void Awake()
    {
        mm = GameObject.FindObjectOfType<MeiosisManager>();    
    }

    // Start is called before the first frame update
    void Start()
    {
        if (centriole!=null)
        {
            centriole.OnChromsomePullComplete += Centriole_OnChromsomePullComplete;
            centriole.OnAnaphase2Done += Centriole_OnAnaphase2Done;
        }
        //nuclearMembrane.gameObject.SetActive(false);
    }

    private void Update()
    {
        currentTransparency = nuclearMembrane.material.color.a;
    }

    private void Centriole_OnAnaphase2Done(object sender, EventArgs e)
    {
        StartCoroutine(CreateMembrane());
    }

    private void Centriole_OnChromsomePullComplete(object sender, System.EventArgs e)
    {
        StartCoroutine(CreateMembrane());
    }

    public void DissolveMembraneMethod()
    {
        StartCoroutine(DissolveMembrane());
    }

    private IEnumerator CreateMembrane()
    {
        nuclearMembrane.gameObject.SetActive(true);
        while (nuclearMembrane.material.color.a<targetTransparency)
        {
            Utilities.AssignMaterialTransparency(nuclearMembrane.material, nuclearMembrane.material.color.a + transparencyIncreaseRate);
            yield return null;
        }
    }

    private IEnumerator DissolveMembrane()
    {
        nuclearMembrane.gameObject.SetActive(true);
        Utilities.AssignMaterialTransparency(nuclearMembrane.material, targetTransparency);
        while (nuclearMembrane.material.color.a > 0.001f)
        {
            Utilities.AssignMaterialTransparency(nuclearMembrane.material, nuclearMembrane.material.color.a - transparencyIncreaseRate);
            yield return null;
        }
        nuclearMembrane.gameObject.SetActive(false);
        OnMembraneDissolved?.Invoke(this, EventArgs.Empty);
    }
}
