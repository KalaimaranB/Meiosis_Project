using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Chromosome : MonoBehaviour
{
    public GameObject Centromere;
    public List<Chromatid> chromatids;

    private List<MeshRenderer> genes = new List<MeshRenderer>();

    public List<Material> possibleAlleles;

    private void Awake()
    {
        genes = gameObject.GetComponentsInChildren<MeshRenderer>().ToList();
        //Remove the last item, as its the centromere
        genes.RemoveAt(genes.Count - 1);
    }


    public void AlterGeneColors(int numOfGenes)
    {
        for (int i = 0; i < numOfGenes; i++)
        {
            int targetGene = Random.Range(0, genes.Count - 1);
            int targetColor = Random.Range(0, possibleAlleles.Count - 1);

            genes[targetGene].material = possibleAlleles[targetColor];
        }
    }
}
