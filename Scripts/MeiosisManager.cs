using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class MeiosisManager : MonoBehaviour
{
    public List<Chromosome> chromosomes;
    public List<GameObject> MetaphasePositions;
    public float chromosomeMoveSpeed;
    public float chromosomeStopDistance;
    public event EventHandler OnProphase1Initiation;
    public event EventHandler OnMetaphase1Initiation;
    public event EventHandler OnAnaphase1Initiation;

    [Header("Prophase 1")]
    public List<GameObject> P1Positions;
    public float yOffset;
    public int genesToChange;
    private float ChromosomesReachedP1 = 0;
    private List<GameObject> chromosomesRP1 = new List<GameObject>();
    private float ChromosomesReachedM1 = 0;
    public float timeToWaitAfterCrossingOver = 1;


    [Header("Meiosis Part 2")]
    public float timeToWaitAfterMetaphase = 1;

    public enum MeiosisState {Prophase1, Metaphase1, Anaphase1, Telophase1, Prophase2, Metaphase2, Anaphase2, Telophase2};
    public MeiosisState currentState = MeiosisState.Prophase1;

    public DaughterCell dc1;
    public DaughterCell dc2;
    public DaughterCell parentCell;
    private float membranesDissolved;
    private bool m2triggered = false;
    private bool a2triggered = false;
    [HideInInspector]
    public bool Meiosis1;
    private List<Centriole> centris = new List<Centriole>();
    private DialogueManager dm;

    [Header("General Dialogue")]

    [TextArea(3, 10)]
    public string endDialogue;
    public float timeToWaitBeforeEndDialogue;
    public float timeToWaitAfterEndDialgoue = 8;

    [Header("Dialgoues for Meiosis 1")]
    [TextArea(3, 10)]
    public string startDialogue;

    [TextArea(3, 10)]
    public string onHalfDoneCrossingOver;
    private bool halfReached = false;

    [TextArea(3, 10)]
    public string onMetaphase1Starts;

    [TextArea(3, 10)]
    public string onAnaphase1Starts;

    [TextArea(3, 10)]
    public string onTelophase1Starts;
    private bool t1d = false;

    [Header("Dialogues for Meiosis 2")]
    [TextArea(3, 10)]
    public string onProphase2Starts;

    [TextArea(3, 10)]
    public string onMetaphase2Starts;

    [TextArea(3,10)]
    public string onAnapahse2Starts;
    private bool a2d = false;

    [TextArea(3, 10)]
    public string onTelophase2Starts;


    private void Awake()
    {
        dm = GetComponent<DialogueManager>();
        centris = FindObjectsOfType<Centriole>().ToList();
        if (currentState == MeiosisState.Prophase1)
        {
            OnProphase1Initiation += InitiateP1;
            OnMetaphase1Initiation += InitiateM1;
            Meiosis1 = true;
            dm.StartDialogue(startDialogue);

        }
        if (currentState == MeiosisState.Prophase2)
        {
            Meiosis1 = false;
            dc1.OnMembraneDissolved += Dc1_OnMembraneDissolved;
            dc2.OnMembraneDissolved += Dc1_OnMembraneDissolved;
            parentCell.OnMembraneDissolved += Dc1_OnMembraneDissolved;
            dm.StartDialogue(onProphase2Starts);
        }
    }

    private void Dc1_OnMembraneDissolved(object sender, EventArgs e)
    {
        membranesDissolved += 1;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (currentState == MeiosisState.Prophase1)
        {
            OnProphase1Initiation?.Invoke(this, EventArgs.Empty);
            parentCell.DissolveMembraneMethod();
        }
        if (currentState == MeiosisState.Prophase2)
        {
            //Dissolve membrane & draw spindle fibers
            
            foreach (Centriole cent in centris)
            {
                cent.DrawSpindleFibersToCentromeresM2(this, EventArgs.Empty);
            }
            parentCell.DissolveMembraneMethod();
        }
    }

    // Update is called once per frame
    void Update()
    {
        IfMeiosisPart1();
        IfMeiosisPart2();
    }

    public void IfMeiosisPart1()
    {
        if (ChromosomesReachedP1 == 5 && currentState == MeiosisState.Prophase1 && halfReached == false)
        {
            dm.StartDialogue(onHalfDoneCrossingOver);
            halfReached = true;
        }
        if (ChromosomesReachedP1 == chromosomes.Count && currentState == MeiosisState.Prophase1)
        {
            foreach (Centriole cent in centris)
            {
                cent.lockAllLinePos = true;
            }
            OnMetaphase1Initiation?.Invoke(this, EventArgs.Empty);
            currentState = MeiosisState.Metaphase1;
        }

        if (ChromosomesReachedM1 == chromosomes.Count && currentState == MeiosisState.Metaphase1)
        {
            OnAnaphase1Initiation?.Invoke(this, EventArgs.Empty);
            currentState = MeiosisState.Anaphase1;
            foreach (Centriole cent in centris)
            {
                cent.lockAllLinePos = false;
            }
            dm.StartDialogue(onAnaphase1Starts);
        }
        if (currentState == MeiosisState.Anaphase1 && t1d == false && dc1.currentTransparency>0 && dc2.currentTransparency>0)
        {
            dm.StartDialogue(onTelophase1Starts);
            t1d = true;
            currentState = MeiosisState.Telophase1;
            StartCoroutine(endTheDialogue());
        }
    }

    private IEnumerator endTheDialogue()
    {
        yield return new WaitForSeconds(timeToWaitBeforeEndDialogue);
        dm.StartDialogue(endDialogue);
        yield return new WaitForSeconds(timeToWaitAfterEndDialgoue);
        dm.loadNextScene();
    }

    public void IfMeiosisPart2()
    {
        if (currentState == MeiosisState.Prophase2)
        {
            if (membranesDissolved == 1 && m2triggered == false && centris.All(i=>i.linesDrawnM1==6))
            {
                InitiateM1(this, EventArgs.Empty);
                m2triggered = true;
                currentState = MeiosisState.Metaphase2;
                dm.StartDialogue(onMetaphase2Starts);
                foreach (Centriole cent in centris)
                {
                    cent.lockAllLinePos = true;
                }
            }
        }
        if (currentState == MeiosisState.Metaphase2 && a2triggered == false && ChromosomesReachedM1 == 6)
        {
            currentState = MeiosisState.Anaphase2;
            a2triggered = true;
            foreach (Centriole cent in centris)
            {
                cent.lockAllLinePos = false;
            }
        }
        if (currentState == MeiosisState.Anaphase2 && dc1.currentTransparency>0 && dc2.currentTransparency>0 && a2d == false)
        {
            StartCoroutine(t2dialogue());
            a2d = true;
            currentState = MeiosisState.Telophase2;
        }
    }

    private IEnumerator t2dialogue()
    {
        yield return new WaitForSeconds(2);
        dm.StartDialogue(onTelophase2Starts);
        yield return new WaitForSeconds(timeToWaitBeforeEndDialogue);
        dm.StartDialogue(endDialogue);
        yield return new WaitForSeconds(timeToWaitAfterEndDialgoue);
        dm.loadNextScene();
    }

    public void startAnaphase2Dialogue()
    {
        dm.StartDialogue(onAnapahse2Starts);
    }

    public void InitiateP1(System.Object sender, EventArgs E)
    {
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[0].gameObject, P1Positions[0].transform.position + new Vector3(0, yOffset, 0)));
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[1].gameObject, P1Positions[0].transform.position - new Vector3(0, yOffset, 0)));
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[2].gameObject, P1Positions[1].transform.position + new Vector3(0, yOffset, 0)));
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[3].gameObject, P1Positions[1].transform.position - new Vector3(0, yOffset, 0)));
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[4].gameObject, P1Positions[2].transform.position + new Vector3(0, yOffset, 0)));
        StartCoroutine(MoveChromosomeToP1Pos(chromosomes[5].gameObject, P1Positions[2].transform.position - new Vector3(0, yOffset, 0)));

        StartCoroutine(AlterChromosomeAlleles(chromosomes[0].gameObject, chromosomes[1].gameObject));
        StartCoroutine(AlterChromosomeAlleles(chromosomes[2].gameObject, chromosomes[3].gameObject));
        StartCoroutine(AlterChromosomeAlleles(chromosomes[4].gameObject, chromosomes[5].gameObject));
    }

    private IEnumerator AlterChromosomeAlleles(GameObject c1, GameObject c2)
    {
        while (chromosomesRP1.Contains(c1) == false || chromosomesRP1.Contains(c2) == false)
        {
            yield return null;
        }
        c1.GetComponent<Chromosome>().AlterGeneColors(genesToChange);
        c2.GetComponent<Chromosome>().AlterGeneColors(genesToChange);
    }

    private IEnumerator MoveChromosomeToP1Pos(GameObject chromo, Vector3 targetPos)
    {
        while (Vector3.Distance(chromo.transform.position, targetPos) > chromosomeStopDistance)
        {
            chromo.transform.position = Vector3.MoveTowards(chromo.transform.position, targetPos, chromosomeMoveSpeed * Time.deltaTime);
            yield return null;
        }
        ChromosomesReachedP1 += 1;
        chromosomesRP1.Add(chromo);
    }

    public void InitiateM1(System.Object sender, EventArgs e)
    {
        StartCoroutine(Metaphase1Process());
    }

    private IEnumerator Metaphase1Process()
    {
        yield return new WaitForSeconds(timeToWaitAfterCrossingOver);
        if (Meiosis1 == true)
        {
            dm.StartDialogue(onMetaphase1Starts);
        }

        //This is an attempt at random orientation
        List<int> chosenInts = new List<int>();
        chosenInts.Add(0);
        chosenInts.Add(1);
        chosenInts.Add(2);
        chosenInts.Add(3);
        chosenInts.Add(4);
        chosenInts.Add(5);

        chosenInts = chosenInts.OrderBy(x => UnityEngine.Random.value).ToList();

        for (var i = 0; i < chromosomes.Count; i++)
        {
            int index = chosenInts[i];
            StartCoroutine(MoveChromosomeToM1Pos(chromosomes[i].gameObject, MetaphasePositions[index].transform.position));
        }
    }

    private IEnumerator MoveChromosomeToM1Pos(GameObject chromo, Vector3 targetPos)
    {
        while (Vector3.Distance(chromo.transform.position, targetPos) > chromosomeStopDistance)
        {
            chromo.transform.position = Vector3.MoveTowards(chromo.transform.position, targetPos, chromosomeMoveSpeed * Time.deltaTime);
            yield return null;
        }
        ChromosomesReachedM1 += 1;
    }


}
