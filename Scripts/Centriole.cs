using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Centriole : MonoBehaviour
{
    [Header("Centriole")]
    public int centrioleNum;

    [Header("Spindle Fibers")]
    [SerializeField] private float spindleFiberWidth = 0.15f;
    [SerializeField] private float spindleFiberStopDistance = 0.1f;
    [SerializeField] private float spindleFiberDrawSpeed = 3;
    [SerializeField] private float spindleFiberFadeSpeed = 0.005f;

    [Header("Chromatids")]
    [SerializeField] private float distanceToStopPullingChromatidsIn = 2;
    [SerializeField] private float chromatidPullSpeed = 3f;
    [SerializeField] private Vector3 chromatidPullOffset = new Vector3();
    [SerializeField] private float centromereFadeSpeed = 0.002f;

    public event EventHandler OnChromsomePullComplete;

    [HideInInspector]
    public float linesDrawnM1;
    private bool a1triggered = false;
    private bool a2triggered = false;
    private int chromosomesPulledIntoNuclei;
    private bool T1triggered = false;
    private List<GameObject> centromeres = new List<GameObject>();
    private List<Chromosome> Chromosomes = new List<Chromosome>();
    private LineRenderer lr;
    private List<LineRenderer> spindleFibers = new List<LineRenderer>();

    private int chromatidsPulled = 0;
    private bool t2triggered = false;

    private MeiosisManager mm;

    public event EventHandler OnAnaphase2Done;

    public bool lockAllLinePos = false;

    private List<Centriole> centrioles = new List<Centriole>();

    private void Awake()
    {
        centrioles = FindObjectsOfType<Centriole>().ToList();
        mm = GameObject.FindObjectOfType<MeiosisManager>();
        lr = GetComponent<LineRenderer>();
        GameObject[] chromosomes = GameObject.FindGameObjectsWithTag("Chromosome");
        foreach (GameObject chromo in chromosomes)
        {
            Chromosomes.Add(chromo.GetComponent<Chromosome>());
            centromeres.Add(chromo.GetComponent<Chromosome>().Centromere);
        }
        //Have this line triggered at start
        GameObject.FindObjectOfType<MeiosisManager>().OnProphase1Initiation += DrawSpindleFibersToCentromeresM1;
        OnChromsomePullComplete += FadeAllLinesOut;
    }

    public void DrawSpindleFibersToCentromeresM1(System.Object obj, EventArgs e)
    {
        if (centrioleNum == 1)
        {
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(DrawLineOverTime(centromeres[i].transform.position, Chromosomes[i].gameObject));
            }
        }
        else if (centrioleNum == 2)
        {
            for (int i = 3; i < 6; i++)
            {
                StartCoroutine(DrawLineOverTime(centromeres[i].transform.position, Chromosomes[i].gameObject));
            }
        }
    }

    public void DrawSpindleFibersToCentromeresM2(System.Object obj, EventArgs e)
    {
        for (int i = 0; i < Chromosomes.Count; i++)
        {
            StartCoroutine(DrawLineOverTime(centromeres[i].transform.position, Chromosomes[i].gameObject));
        }
    }

    public void Update()
    {
        if (mm.Meiosis1 == true)
        {

            if (linesDrawnM1 == 3 && a1triggered == false && centrioles.All(i=>i.linesDrawnM1 == 3))
            {
                if (centrioleNum == 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        PullChromosome(Chromosomes[i].gameObject, i);
                    }
                }
                else if (centrioleNum == 2)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        PullChromosome(Chromosomes[j + 3].gameObject, j);
                    }
                }

                a1triggered = true;
            }

            if (chromosomesPulledIntoNuclei == 3 && T1triggered == false)
            {
                OnChromsomePullComplete?.Invoke(this, EventArgs.Empty);
                T1triggered = true;
            }
            if (lockAllLinePos)
            {
                if (centrioleNum == 1)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        List<Vector3> POINTS = new List<Vector3>();
                        POINTS.Add(gameObject.transform.position);
                        POINTS.Add(Chromosomes[i].transform.position);
                        spindleFibers[i].positionCount = POINTS.Count;
                        spindleFibers[i].SetPositions(POINTS.ToArray());
                        //PullChromosome(Chromosomes[i].gameObject, i);
                    }
                }
                else if (centrioleNum == 2)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        List<Vector3> POINTS = new List<Vector3>();
                        POINTS.Add(gameObject.transform.position);
                        POINTS.Add(Chromosomes[j + 3].transform.position);
                        spindleFibers[j].positionCount = POINTS.Count;
                        spindleFibers[j].SetPositions(POINTS.ToArray());
                        //PullChromosome(Chromosomes[j + 3].gameObject, j);
                    }
                }
            }
        }
        else
        {
            if (linesDrawnM1 == 6 && a2triggered == false && mm.currentState == MeiosisManager.MeiosisState.Anaphase2)
            {
                StartCoroutine(PullSingleChromatid());
                a2triggered = true;
            }
            if (t2triggered == false &&chromatidsPulled ==6)
            {
                OnAnaphase2Done?.Invoke(this, EventArgs.Empty);
                FadeAllLinesOut(this, EventArgs.Empty);
                t2triggered = true;
            }
            if (lockAllLinePos == true)
            {
                for (int i=0; i<Chromosomes.Count; i++)
                {
                    List<Vector3> POINTS = new List<Vector3>();
                    POINTS.Add(gameObject.transform.position);
                    POINTS.Add(Chromosomes[i].transform.position);
                    spindleFibers[i].positionCount = POINTS.Count;
                    spindleFibers[i].SetPositions(POINTS.ToArray());

                }
            }
        }
    }

    public IEnumerator PullSingleChromatid()
    {
        yield return new WaitForSeconds(mm.timeToWaitAfterMetaphase);
        if (centrioleNum == 1)
        {
            mm.startAnaphase2Dialogue();
        }
        for (int i = 0; i < Chromosomes.Count; i++)
        {
            float distanceToChromatid1 = Vector3.Distance(gameObject.transform.position, Chromosomes[i].chromatids[0].transform.position);
            float distanceToChromatid2 = Vector3.Distance(gameObject.transform.position, Chromosomes[i].chromatids[1].transform.position);
            GameObject targetChromatid = null;
            if (distanceToChromatid2 < distanceToChromatid1)
            {
                targetChromatid = Chromosomes[i].chromatids[1].gameObject;
            }
            else if (distanceToChromatid1 <= distanceToChromatid2)
            {
                targetChromatid = Chromosomes[i].chromatids[0].gameObject;
            }
            StartCoroutine(MoveChromatidTowardCentriole(targetChromatid, i));
            StartCoroutine(FadeOutCentromere(Chromosomes[i].Centromere));
        }
    }

    private IEnumerator FadeOutCentromere(GameObject centro)
    {
        MeshRenderer mr = centro.GetComponent<MeshRenderer>();
        while (mr.material.color.a > 0.001f)
        {
            Utilities.AssignMaterialTransparency(mr.material, mr.material.color.a - centromereFadeSpeed);
            yield return null;
        }
        centro.SetActive(false);
    }

    public void PullChromosome(GameObject targetChromosome, int chromosomeNum)
    {
        StartCoroutine(MoveChromosomeTowardCentriole(targetChromosome, chromosomeNum));
    }

    private IEnumerator MoveChromatidTowardCentriole(GameObject targetChromatid, int chromosomeNum)
    {
        while (Vector3.Distance(gameObject.transform.position, targetChromatid.transform.position) > distanceToStopPullingChromatidsIn)
        {
            targetChromatid.transform.position = Vector3.MoveTowards(targetChromatid.transform.position, gameObject.transform.position, chromatidPullSpeed * Time.deltaTime);
            //Maintain line position
            List<Vector3> points = new List<Vector3>();
            points.Add(gameObject.transform.position);
            points.Add(targetChromatid.transform.position+chromatidPullOffset);
            spindleFibers[chromosomeNum].SetPositions(points.ToArray());

            yield return null;
        }
        chromatidsPulled += 1;
    }

    private IEnumerator MoveChromosomeTowardCentriole(GameObject targetChromosome, int chromosomeNum)
    {
        while (Vector3.Distance(gameObject.transform.position, targetChromosome.transform.position) > distanceToStopPullingChromatidsIn)
        {
            targetChromosome.transform.position = Vector3.MoveTowards(targetChromosome.transform.position, gameObject.transform.position, chromatidPullSpeed * Time.deltaTime);
            //Maintain line position
            List<Vector3> points = new List<Vector3>();
            points.Add(gameObject.transform.position);
            points.Add(targetChromosome.transform.position + chromatidPullOffset);
            spindleFibers[chromosomeNum].SetPositions(points.ToArray());

            yield return null;
        }
        chromosomesPulledIntoNuclei += 1;
    }

    private IEnumerator DrawLineOverTime(Vector3 point, GameObject chromo)
    {
        GameObject GOLN = new GameObject();
        GOLN.transform.parent = gameObject.transform;
        LineRenderer LN = GOLN.AddComponent<LineRenderer>();
        spindleFibers.Add(LN);
        LN.material = lr.material;
        LN.startWidth = spindleFiberWidth;
        LN.endWidth = spindleFiberWidth;

        List<Vector3> points = new List<Vector3>();
        points.Add(gameObject.transform.position);
        points.Add(gameObject.transform.position);

        Vector3 targetPoint = point;

        while (Vector3.Distance(targetPoint, points[1]) > spindleFiberStopDistance)
        {
            targetPoint = chromo.transform.position+chromatidPullOffset;
            points[1] = Vector3.MoveTowards(points[1], targetPoint, spindleFiberDrawSpeed * Time.deltaTime);
            LN.positionCount = points.Count;
            LN.SetPositions(points.ToArray());
            yield return null;
        }
        linesDrawnM1 += 1;
    }

    public void FadeAllLinesOut(System.Object obj, EventArgs e)
    {
        foreach (LineRenderer lr in spindleFibers)
        {
            StartCoroutine(FadeLine(lr));
        }
    }

    private IEnumerator FadeLine(LineRenderer LR)
    {
        while (LR.material.color.a > 0.001)
        { 
            Utilities.AssignMaterialTransparency(LR.material, LR.material.color.a - spindleFiberFadeSpeed);
            yield return null;
        }
        LR.gameObject.SetActive(false);
    }

}
