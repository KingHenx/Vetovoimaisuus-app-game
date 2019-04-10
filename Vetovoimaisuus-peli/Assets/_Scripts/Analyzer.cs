using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Analyzer : MonoBehaviour {

    //Erazethis later
    public Text debug;

    public GameObject analyzeFolder, analyzeReadyScreen,resultFolder, slide1, slide2, slide3, whatToDo; 
    public enum State { opening, idle, analyze, results}
    public State state;

    public Text binaryText;
    public GameObject ring;
    public List<GameObject> fingerImages;
    public float ringRotateSpeed = 1;
    float randomRot;

    public float analyzeTime = 3f;
    bool released;

    public struct HandData
    {
        public int fingerAmount;
        public float handSize;
        public float handTakeOffTime;

        public HandData(int fingerAmount, 
                        float handSize, 
                        float handTakeOffTime)
        {
            this.fingerAmount = fingerAmount;
            this.handSize = handSize;
            this.handTakeOffTime = handTakeOffTime;
        }
    }

    List<HandData> lastHands;

    float handTouchTime;
    HandData currenthand;

    public ResultList resultList;

    float timer;

    private void Awake()
    {
        lastHands = new List<HandData>();

        ring.SetActive(false);
        binaryText.gameObject.SetActive(false);

        Vector2 reso;
        reso.x = Screen.currentResolution.width;
        reso.y = Screen.currentResolution.height;

        debug.text += reso;

        analyzeFolder.SetActive(false);
        resultFolder.SetActive(false);
        analyzeReadyScreen.SetActive(false);
        slide1.SetActive(false);
        slide2.SetActive(false);
        slide3.SetActive(false);
        whatToDo.SetActive(false);

        StartCoroutine("OpeningLogos");
    }

    void Update () 
    {
        switch (state) 
        {
            case State.idle:

                analyzeFolder.SetActive(true);
                resultFolder.SetActive(false);


                if(Time.time > timer + 2 && Input.touchCount == 0)
                {
                    whatToDo.SetActive(true);
                }
                else
                {
                    whatToDo.SetActive(false);
                }

                break;


            case State.analyze:
                break;


            case State.results:
                analyzeFolder.SetActive(false);
                resultFolder.SetActive(true);

                break;
        }


        if (Input.touchCount != 0 && state == State.idle)
        {
            state = State.analyze;

            debug.text += " Start Analysis";

            StartCoroutine("Timer");
            handTouchTime = Time.time;
            StartCoroutine("BinaryLoremIpsum");
            StartCoroutine("RotateRing");
            released = false;
        }

        if(state == State.analyze)
        {
            ring.SetActive(true);
            whatToDo.SetActive(false);
            binaryText.gameObject.SetActive(true);

            if (!released)
            {
                GetHandData();
            }
     
            ring.GetComponent<RectTransform>().Rotate(0, 0, randomRot * ringRotateSpeed * Time.deltaTime);
        }
        else
        {
            ring.SetActive(false);
            binaryText.gameObject.SetActive(false);
        }



        //Move fingerIndicators
        if(Input.touchCount != 0)
        {
            List<Touch> coveredTouches = new List<Touch>();

            foreach(Touch finger in Input.touches)
            {
                if (coveredTouches.Contains(finger))
                    coveredTouches.Add(finger);
            }

            for (int i = 0; i < coveredTouches.Count; i++)
            {
                fingerImages[i].transform.position = coveredTouches[i].position;
            }

        }
	}

    void GetHandData()
    {
        if (Input.touchCount >= currenthand.fingerAmount)
        {
            Touch[] allTouches = Input.touches;

            currenthand.fingerAmount = Input.touchCount;

            float distance = 0;

            for (int fingerNmbr = 0; fingerNmbr < allTouches.Length; fingerNmbr++)
            {

                if (fingerNmbr == 0)
                {
                    distance += Vector2.Distance(allTouches[fingerNmbr].position, allTouches[allTouches.Length - 1].position);
                }
                else
                {
                    distance += Vector2.Distance(allTouches[fingerNmbr].position, allTouches[fingerNmbr - 1].position);
                }
            }

            distance = distance / allTouches.Length;

            currenthand.handSize = distance / Screen.currentResolution.width;
        }
        else
        {
            float newTakeOffTime = Time.time - handTouchTime;
            currenthand.handTakeOffTime = newTakeOffTime / analyzeTime;
            debug.text += " Release!! ";

            released = true;
        }



    }

    HandData CompareWithOtherHandData(HandData newData)
    {
        //True if handdata is new and presumably belongs to same user
        // if data is not new erase new data
        // if data is new erase oldest data


        //foreach (HandData lastData in lastHands)
        //{
        //    if (Mathf.Abs(newData.handSize - lastData.handSize) > lastData.handSize * 0.05f)
        //    {
        //        debug.text += " This is familiar hand";
        //        return lastData;
        //    }
        //}

        debug.text += " New hand detected";
        return currenthand;
    }

    IEnumerator Timer()
    {
        yield return new WaitForSeconds(analyzeTime);

        if (!released) 
        {
            float newTakeOffTime = Time.time - handTouchTime;
            currenthand.handTakeOffTime = newTakeOffTime / analyzeTime;
            debug.text += " Release!! ";

            released = true;
        }

        state = State.results;

        lastHands.Add(CompareWithOtherHandData(currenthand));

        currenthand = new HandData();

        debug.text += " Analysis complete :: HandData: -> ";

        GiveAnalysis(lastHands[lastHands.Count - 1]);

        analyzeReadyScreen.SetActive(true);

        yield return new WaitForSeconds(1f);

        analyzeReadyScreen.SetActive(false);
    }



    void GiveAnalysis(HandData data)
    {
        //show results
        debug.text += data.fingerAmount + " : " + data.handSize + " : " + data.handTakeOffTime;

        Result bestMatch = new Result();
        float differenceAmount = 99;

        foreach(Result result in resultList.allResults)
        {
            float newMatchValue = 0;

            newMatchValue += Mathf.Abs(data.handSize - result.requiredHandSize * 0.75f);
            newMatchValue += Mathf.Abs(data.handTakeOffTime - result.requiredSpeed * 0.50f);

            if(newMatchValue < differenceAmount)
            {
                debug.text += " next best = " + result.resultName;

                differenceAmount = newMatchValue;
                bestMatch = result;
            }
        }

        debug.text += bestMatch.resultName;

        resultFolder.GetComponentInChildren<Text>().text = bestMatch.resultName;

        Invoke("BackToIdle", 5);
    }



    IEnumerator BinaryLoremIpsum()
    {
        string newBinary = "";

        int randomLenght = Random.Range(100, 400);

        for (int i = 0; i < (int)randomLenght; i++)
        {
            newBinary = newBinary + (int)Random.Range(0, 2);

            if(Random.Range(0, 50) == 0)
            {
                newBinary = newBinary + "\n";
            }
        }

        binaryText.text = newBinary;

        yield return new WaitForSeconds(0.1f);

        if(state == State.analyze)
        StartCoroutine("BinaryLoremIpsum");
    }

    IEnumerator RotateRing()
    {
        float randomTime = Random.Range(0.1f, 2);

        randomRot = Random.Range(-180, 180);

        yield return new WaitForSeconds(randomTime);

        if (state == State.analyze)
            StartCoroutine("RotateRing");
    }


    public void TestTouch()
    {
        if (Application.isEditor)
        {
            debug.text += " TEST TOUCH NOT REAL TOUCH";

            StartCoroutine("Timer");
            handTouchTime = Time.time;
            StartCoroutine("BinaryLoremIpsum");
            StartCoroutine("RotateRing");
            state = State.analyze;
        }
    }

    public void BackToIdleButton(float timer)
    {
        Invoke("BackToIdle", timer);
    }

    void BackToIdle()
    {
        state = State.idle;
        timer = Time.time;
    }



    IEnumerator OpeningLogos() 
    {
        slide1.SetActive(true);

        yield return new WaitForSeconds(2);

        slide1.SetActive(false);
        slide2.SetActive(true);

        yield return new WaitForSeconds(2);

        slide2.SetActive(false);
        slide3.SetActive(true);

        yield return new WaitForSeconds(2);

        slide3.GetComponentInChildren<Button>().enabled = true;

        yield return new WaitForSeconds(2);

        foreach(Text text in slide3.GetComponentsInChildren<Text>())
        {
            if(text.gameObject.name == "Press")
            {
                text.enabled = true;
            }
        }

    }
}
