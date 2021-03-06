﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GlobalDefines;
using SimpleJSON;

class LinkedPair
{
    public int indexBegin;
    public int indexEnd;
    public GameObject line;
    public LinkedPair(int b, int e, GameObject l)
    {
        if (b > e) {
            indexBegin = b;
            indexEnd = e;
        }
        else if (e > b) {
            indexBegin = e;
            indexEnd = b;
        }
        else {
            return;
        }
        line = l;
    }
    public bool EqualPair(int s, int e)
    {
        if ((indexBegin == s && indexEnd == e) || (indexBegin == e && indexEnd == s)) {
            return true;
        }
        return false;
    }
};

public class LevelPlayModel : MonoBehaviour {
    private List<LinkedPair> _linkedLineList;  // 已经连接的线表
    private Dictionary<int, int> _starDictionary; // 星星所连接线的hash表，之前用于显示颜色用
    private List<int> _correctAnswerList; // 正确答案
    private List<int> _answerList; // 当前答案
    private string _levelName;
    private GameObject _readyStar; // 选中的星星
    private bool _isWin;
    private bool _isFin;
    private LevelGuideModel _levelGuide;

    public GameDirector gameDirector;
    public LevelPlayView levelPlayView;
    public AudioPlayerModel audioPlayer;

    public string levelName {
        get {
            return _levelName;
        }
    }

    void Awake() {
        _linkedLineList = new List<LinkedPair>();
        _starDictionary = new Dictionary<int, int>();
        _correctAnswerList = new List<int>();
        _answerList = new List<int>(); 
        _readyStar = null;
        _levelGuide = this.gameObject.GetComponent<LevelGuideModel>();
    }
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    //////////////////////////////////////////////////////////////////////////
    /////                        Outter Interface                        /////
    //////////////////////////////////////////////////////////////////////////
    #region OutterInterface
    public void LoadLevel(string name)
    {
        // Clear data and UI of pre level
        this.Clear();
        levelPlayView.ClearUI();

        // Load data and UI of this level
        this.Load(name);
        levelPlayView.LoadLevelUI(name);

        int guideLevel = PlayerPrefs.GetInt(PlayerPrefsKey.GuideLevel);
        if (guideLevel <= 0 && _levelName == "Triangulum") {
            _levelGuide.TriggerLevelGuide(1);
        }
        if (guideLevel <= 1 && _levelName == "Aries") {
            _levelGuide.TriggerLevelGuide(7);
        }
    }

    void Clear() 
    {
        _linkedLineList.Clear();
        _starDictionary.Clear();
        _correctAnswerList.Clear();
        _answerList.Clear();
        _readyStar = null;
        audioPlayer.ClearLittleStarIndex();
    }

    void Load(string name)
    {
        // Load template data
        JSONNode jo = TemplateMgr.Instance.GetTemplateString(ConfigKey.LevelInfo, name); 
        _levelName = jo["name"];
        // Load answer
        JSONArray answerList = jo["answer"] as JSONArray;
        foreach (JSONNode answerObject in answerList) {
            int tmpAnswer = answerObject.AsInt;
            _correctAnswerList.Add(tmpAnswer);
        }
        _isWin = false;
    }

    // Click star
    public void TriggerStar(GameObject goStar)
    {
        int index = goStar.GetComponent<Star>().index;
        //Check whether ready
        if (_readyStar == null) {
            //first one
            _readyStar = goStar;
            RefreshStar(goStar);
            audioPlayer.PlayLittleStar();
        }
        else {
            if (_readyStar.GetComponent<Star>().index == index) {
                _readyStar = null;
                RefreshStar(goStar);
                audioPlayer.PlayUnChoseSound();
            }
            else {
                TryLinkStar(_readyStar, goStar);
            }
        }
    }

    // Click Card
    public void TriggerCard(GameObject go)
    {
       levelPlayView.FlopCard(go);
    }

    public bool IsFin()
    {
        return _isFin;
    }

    public string GetNextLevel()
    {
        return gameDirector.GetNextLevel();
    }

    // Win
    public void GameWin()
    {
        _isFin = gameDirector.FinishLevel();
        _isWin = true;
        levelPlayView.ShowGameWin();
        audioPlayer.PlayWinSound();
    }

    // Click tips
    public void OnTips()
    {
        // Check if have coin
        if(gameDirector.GetCoin() >= DefineNumber.TipCost) {
            this.DoTip();
        } 
        else {
            gameDirector.ShowPurchaseConfirm();
        }
    }

    public void FlopToCardBack()
    {
        levelPlayView.ShowMenu();
    }

    public void AfterWinTween()
    {
        levelPlayView.ShowComplete();
    }

    public int GetCoin()
    {
        return gameDirector.GetCoin();
    }

    public void ShowPreview()
    {
        if(_levelName == null || _isWin) {
            return;
        }
        levelPlayView.ShowPreview(_levelName.ToLower());
        audioPlayer.PlayClickSound();
    }

    IEnumerator TriggerLevelGuideStep(float waitTime, int step)
    {
        yield return new WaitForSeconds(waitTime);
        _levelGuide.TriggerLevelGuide(step);
    }    

    public void OnLevelGuide(int step)
    {
        switch(step) {
            case 1:
                GameObject goStep1 = this.gameObject.transform.Find("TriangulumContainer(Clone)/Sky/StarContainer/Star3").gameObject;
                this.TriggerStar(goStep1);
                audioPlayer.PlayLittleStar();
                StartCoroutine(TriggerLevelGuideStep(0.1f, 2));
                break;
            case 2:
                GameObject goStep2 = this.gameObject.transform.Find("TriangulumContainer(Clone)/Sky/StarContainer/Star1").gameObject;
                this.TriggerStar(goStep2);
                audioPlayer.PlayLittleStar();
                StartCoroutine(TriggerLevelGuideStep(0.1f, 3));
                break;
            case 3:
                audioPlayer.PlayClickSound();
                this.ShowPreview();
                StartCoroutine(TriggerLevelGuideStep(0.1f, 4));
                break;
            case 4:
                audioPlayer.PlayClickSound();
                this.DoTip();
                StartCoroutine(TriggerLevelGuideStep(0.1f, 5));
                break;
            case 5:
                audioPlayer.PlayLittleStar();
                GameObject goStep5 = this.gameObject.transform.Find("TriangulumContainer(Clone)/Sky/StarContainer/Star3").gameObject;
                this.TriggerStar(goStep5);
                StartCoroutine(TriggerLevelGuideStep(0.1f, 6));
                break;
            case 6:
                audioPlayer.PlayLittleStar();
                GameObject goStep6 = this.gameObject.transform.Find("TriangulumContainer(Clone)/Sky/StarContainer/Star2").gameObject;
                _levelGuide.StopGuide();
                PlayerPrefs.SetInt(PlayerPrefsKey.GuideLevel, 1);
                this.TriggerStar(goStep6);
                break;
            case 7:
                _levelGuide.StopGuide();
                PlayerPrefs.SetInt(PlayerPrefsKey.GuideLevel, 2);
                break;
            default:
                break;
        }
    }

    public int GetLinkedLineNum()
    {
        return _answerList.Count;
    }

    public int GetAllLineNum()
    {
        return _correctAnswerList.Count;
    }
    #endregion

    //////////////////////////////////////////////////////////////////////////
    /////                        Inner Function                          /////
    //////////////////////////////////////////////////////////////////////////
    // Check whether answer is correct
    private bool CheckAnswer()
    {
        return _correctAnswerList.TrueForAll(_answerList.Contains) && _answerList.TrueForAll(_correctAnswerList.Contains);
    }

    // Add Answer to list
    private void AddAnswerToList(int indexBegin, int indexEnd)
    {
        // Add Answer to list
        int answerMark = 0;
        if (indexBegin > indexEnd) {
            answerMark = indexEnd * 100 + indexBegin;
        }
        else {
            answerMark = indexBegin * 100 + indexEnd;
        }
        if (_answerList.Contains(answerMark)) {
            return;
        }
        _answerList.Add(answerMark);
        // Check whether sucess
        if (CheckAnswer()) {
            // Win
            this.GameWin();
        }
        else {
            audioPlayer.PlayLittleStar();
        }
    }

    // Remove Answer from list
    private void RemoveAnswerFromList(int indexBegin, int indexEnd)
    {
        // Just remove from _answerList
        int answerMark = 0;
        if (indexBegin > indexEnd) {
            answerMark = indexEnd * 100 + indexBegin;
        }
        else {
            answerMark = indexBegin * 100 + indexEnd;
        }
        _answerList.Remove(answerMark);

        // Check whether sucess
        if (CheckAnswer()) {
            // Win
            this.GameWin();
        }
        else {
            audioPlayer.PlayLittleStar();  
        }
    }

    // Check whether line(b,e) is already existed
    private bool HaveLinkedLine(int b, int e)
    {
        foreach (LinkedPair lp in _linkedLineList) {
            if (lp.EqualPair(b, e)) {
                return true;
            }
        }
        return false;
    }

    // Get the reference of line(b,e)
    private int GetLinkedLine(int b, int e)
    {
        for (int i = 0; i < _linkedLineList.Count; ++i) {
            if (_linkedLineList[i].EqualPair(b, e)) {
                return i;
            }
        }
        return -1;
    }

    // Check whether star(s) is already have be connected
    private bool IsStarLinked(int s)
    {
        if (_starDictionary.ContainsKey(s) && _starDictionary[s] > 0) {
            return true;
        }
        return false;
    }

    // Refresh Stars state
    private void RefreshStar(GameObject goStar)
    {
        Star starComponent = goStar.GetComponent<Star>();
        int indexStar = starComponent.index;
        // Check whether is ready
        if (_readyStar != null && _readyStar.GetComponent<Star>().index == indexStar) {
            starComponent.SetChosen();
        }
        else {
            // Not ready
            if (IsStarLinked(indexStar)) {
                starComponent.SetLinked();
            }
            else {
                starComponent.SetNormal();
            }
        }
    }

    // Add a line, this function is also the key logic of linked
    private void TryLinkStar(GameObject goBegin, GameObject goEnd)
    {
        if (goBegin == null || goEnd == null) {
            return;
        }
        int indexB = goBegin.GetComponent<Star>().index;
        int indexE = goEnd.GetComponent<Star>().index;

        //Check whether already linke
        int index = this.GetLinkedLine(indexB, indexE);
        if (index >= 0) {
            //Already linked, try to delete it
            GameObject goLine = _linkedLineList[index].line;
            //delte from list
            _linkedLineList.RemoveAt(index);
            //modify hash table
            if (IsStarLinked(indexB)) {
                _starDictionary[indexB]--;
            }
            if (IsStarLinked(indexE)) {
                _starDictionary[indexE]--;
            }
            //delete the line
            Destroy(goLine);
            //remove two star from anwser
            RemoveAnswerFromList(indexB, indexE);
        }
        else {
            // Try to link two stars
            Transform beginTransform = goBegin.transform;
            Transform endTransform = goEnd.transform;
            // Instantiate line
            GameObject linkedLine = levelPlayView.AddLine(beginTransform, endTransform);
            // Add record to List
            LinkedPair lp = new LinkedPair(indexB, indexE, linkedLine);
            _linkedLineList.Add(lp);
            // Add record to Hash
            if (_starDictionary.ContainsKey(indexB)) {
                _starDictionary[indexB]++;
            }
            else {
                _starDictionary[indexB] = 1;
            }
            if (_starDictionary.ContainsKey(indexE)) {
                _starDictionary[indexE]++;
            }
            else {
                _starDictionary[indexE] = 1;
            }
            //Add two star to answerList
            this.AddAnswerToList(indexB, indexE);
        }

        // Reset ready starts
        _readyStar = null;
        // Refresh the stars state
        this.RefreshStar(goBegin);
        this.RefreshStar(goEnd);
        levelPlayView.RefreshLabelNum();
    }

    private void TipLinkLine(int indexBegin, int indexEnd) 
    {
        GameObject goBegin = levelPlayView.GetStarGo(indexBegin);
        GameObject goEnd = levelPlayView.GetStarGo(indexEnd);
        TryLinkStar(goBegin, goEnd);
        gameDirector.SubCoin(DefineNumber.TipCost);
        levelPlayView.UpdateCoinLabel();
    }

    private void DoTip()
    {
        // Restore the ready one
        if(_readyStar) {
            GameObject tmp = _readyStar;
            _readyStar = null;
            RefreshStar(tmp);
        }
        
        // Check if already
        if(CheckAnswer()) {
            return;
        }
        // Traverse the answer list for error one
        foreach(int ans in _answerList) {
            // Check ans whether is correct
            if(!_correctAnswerList.Contains(ans)) {
                // Wrong ans, try to remove it 
                TipLinkLine(ans % 100, ans / 100);
                return;
            }
        }

        // Traverse correct answer to find a new line
        foreach(int cans in _correctAnswerList) {
            if(!_answerList.Contains(cans)) {
                // Right ans, try to link it 
                TipLinkLine(cans % 100, cans / 100);
                return;
            }
        }
    }

}
