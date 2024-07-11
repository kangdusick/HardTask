using CodeStage.AntiCheat.ObscuredTypes;
using Cysharp.Threading.Tasks;
using SRF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

// 상태 타입 열거형
public enum EStatusType
{
    baseValue = 0, // 기본값, 예: 공격력 +5, 공격력 -5
    multiplier = 1, // 증폭, 예: 공격력 5% 증폭
    divider = 2, // 억제, 예: 쿨타임 5% 억제
    adder = 3, // 추가, 예: 쿨타임 +5%, 쿨타임 -5%
}

/*
  상태 타입에 따른 계산 방식 설명:
  - multiplier: 최종 곱연산
  - divider: 최종 곱연산의 역수
  - adder: 합연산
  - baseValue: 기본값

  예시) 기본 쿨타임 60초
  쿨타임 +100% adder
  쿨타임 -50% adder
  쿨타임 30% 증폭 multiplier
  쿨타임 300% 억제 divider
  최종쿨타임 = (60)*(1 +  (100-50)*0.01 )*1.3/4 = 29.25초
*/

// 상태 딕셔너리 클래스
public class StatusDictionary : ObservableDictionary<(ELanguageTable status, EStatusType statusType), ObscuredFloat>
{
    public static Dictionary<int, StatusDictionary> statusDictionaryDict = new();
    public const string linkKey = "statusDictDesc";
    private static int statusDictIndex;
    private Dictionary<(ELanguageTable status, EStatusType statusType), (StatusDictionary refDict, float refDictMultiplier)> referenceStatusDictionary = new();
    private ObscuredFloat baseValue;
    private ObscuredFloat multiplier;
    private ObscuredFloat divider;
    private ObscuredFloat adder;

    private ObscuredFloat baseValueByReference;
    private ObscuredFloat multiplierByReference;
    private ObscuredFloat dividerByReference;
    private ObscuredFloat adderByReference;

    private Timer randomizeKeyTimer;

    private int dictIndex;
    public ObscuredFloat subValue { get; private set; } // 최종 값: 공격속도, subValue: 공격주기
    public ObscuredFloat FinalValue { get; private set; }
    Func<ObscuredFloat, ObscuredFloat> setSubValueFunc;
    private ELanguageTable subValueDesc;
    public string SubValueDescription
    {
        get
        {
            string replacement = $"<link={$"{linkKey}{dictIndex}"}><u>{subValue.KMBTUnit()}</u></link>";
            return replacement;
        }
    }
    public string FinalValueDescription
    {
        get
        {
            string replacement = $"<link={$"{linkKey}{dictIndex}"}><u>{FinalValue.KMBTUnit()}</u></link>";
            return replacement;
        }
    }

    // 키를 무작위로 변경하는 루틴
    private void RandomizeKeyRoutine()
    {
        baseValue.RandomizeCryptoKey();
        multiplier.RandomizeCryptoKey();
        divider.RandomizeCryptoKey();
        adder.RandomizeCryptoKey();
        baseValueByReference.RandomizeCryptoKey();
        multiplierByReference.RandomizeCryptoKey();
        dividerByReference.RandomizeCryptoKey();
        adderByReference.RandomizeCryptoKey();
    }

    // 설명 문자열을 반환
    public string TmpLinkDesc
    {
        get
        {
            StringBuilder stringBuilder = new StringBuilder();
            var baseList = new List<(ELanguageTable eStatus, float value)>();
            var multiList = new List<(ELanguageTable eStatus, float value)>();
            var dividerList = new List<(ELanguageTable eStatus, float value)>();
            var adderList = new List<(ELanguageTable eStatus, float value)>();
            foreach (var item in dictionary)
            {
                if (Mathf.Abs(item.Value) <= 0.01f)
                {
                    continue;
                }
                switch (item.Key.statusType)
                {
                    case EStatusType.baseValue:
                        baseList.Add((item.Key.status, item.Value));
                        break;
                    case EStatusType.multiplier:
                        multiList.Add((item.Key.status, item.Value));
                        break;
                    case EStatusType.divider:
                        dividerList.Add((item.Key.status, item.Value));
                        break;
                    case EStatusType.adder:
                        adderList.Add((item.Key.status, item.Value));
                        break;
                }
            }
            foreach (var item in referenceStatusDictionary)
            {
                switch (item.Key.statusType)
                {
                    case EStatusType.baseValue:
                        if (Mathf.Abs(item.Value.refDict.FinalValue * item.Value.refDictMultiplier) <= 0.01f)
                        {
                            continue;
                        }
                        baseList.Add((item.Key.status, item.Value.refDict.FinalValue * item.Value.refDictMultiplier));
                        break;
                    case EStatusType.multiplier:
                        if (Mathf.Abs(item.Value.refDict.FinalValue * item.Value.refDictMultiplier - 1f) <= 0.01f)
                        {
                            continue;
                        }
                        multiList.Add((item.Key.status, (item.Value.refDict.FinalValue * item.Value.refDictMultiplier - 1f) * 100f));
                        break;
                    case EStatusType.divider:
                        if (Mathf.Abs(item.Value.refDict.FinalValue * item.Value.refDictMultiplier - 1f) <= 0.01f)
                        {
                            continue;
                        }
                        dividerList.Add((item.Key.status, (item.Value.refDict.FinalValue * item.Value.refDictMultiplier - 1f) * 100f));
                        break;
                    case EStatusType.adder:
                        if (Mathf.Abs(item.Value.refDict.FinalValue * item.Value.refDictMultiplier) <= 0.01f)
                        {
                            continue;
                        }
                        adderList.Add((item.Key.status, item.Value.refDict.FinalValue * item.Value.refDictMultiplier));
                        break;
                }
            }
            var comparison = new Comparison<(ELanguageTable eStatus, float value)>((a, b) => b.value.CompareTo(a.value));
            baseList.Sort(comparison);
            multiList.Sort(comparison);
            dividerList.Sort(comparison);
            adderList.Sort(comparison);
            stringBuilder.Append("<align=left>");

            stringBuilder.Append($"{ELanguageTable.FinalValue.LocalIzeText()}: {FinalValue.KMBTUnit()}\n");
            if (subValueDesc != ELanguageTable.valueTypeDefine)
            {
                stringBuilder.Append($"{subValueDesc.LocalIzeText(subValue.KMBTUnit())}\n");
            }

            stringBuilder.Append($"\n{ELanguageTable.BasicValue.LocalIzeText()}\n");
            if (baseList.Count > 0)
            {
                for (int i = 0; i < baseList.Count; i++)
                {
                    stringBuilder.Append($"{baseList[i].value.KMBTUnit(true)}({Extensions.LocalIzeText(baseList[i].eStatus.ToString())}) ");
                }
                stringBuilder.Append($" = {(baseValue + baseValueByReference).KMBTUnit()}\n");
            }
            stringBuilder.Append($"\n{ELanguageTable.Increase.LocalIzeText()}\n");
            if (adderList.Count > 0)
            {
                for (int i = 0; i < adderList.Count; i++)
                {
                    stringBuilder.Append($"{adderList[i].value.KMBTUnit(true)}%({Extensions.LocalIzeText(adderList[i].eStatus.ToString())}) ");
                }
                stringBuilder.Append($" = {(adder + adderByReference).KMBTUnit(true)}%\n");
            }
            stringBuilder.Append($"\n{ELanguageTable.Amplify.LocalIzeText()}\n");
            if (multiList.Count > 0)
            {
                for (int i = 0; i < multiList.Count; i++)
                {
                    stringBuilder.Append($"x{(1f + multiList[i].value * 0.01f).KMBTUnit()}({Extensions.LocalIzeText(multiList[i].eStatus.ToString())}) ");
                }
                stringBuilder.Append($" = x{(multiplier * multiplierByReference).KMBTUnit()}\n");
            }
            stringBuilder.Append($"\n{ELanguageTable.Suppress.LocalIzeText()}\n");
            if (dividerList.Count > 0)
            {
                for (int i = 0; i < dividerList.Count; i++)
                {
                    stringBuilder.Append($"/{(1f + dividerList[i].value * 0.01f).KMBTUnit()}({Extensions.LocalIzeText(dividerList[i].eStatus.ToString())}) ");
                }
                stringBuilder.Append($" = /{(divider * dividerByReference).KMBTUnit()}");
            }
            stringBuilder.Append("</align>");
            return stringBuilder.ToString();
        }
    }

    // 인덱서를 재정의하여 상태 값을 관리
    public override ObscuredFloat this[(ELanguageTable status, EStatusType statusType) key]
    {
        get
        {
            if (!base.ContainsKey(key))
            {
                base[key] = 0f;
            }
            return base[key];
        }
        set
        {
            var beforeValue = 0f;
            if (dictionary.ContainsKey(key))
            {
                beforeValue = dictionary[key];
            }
            SetFinalValue_Base(key.statusType, beforeValue, value);
            base[key] = value;
        }
    }

    // 상태 딕셔너리 생성자
    public StatusDictionary(Func<ObscuredFloat, ObscuredFloat> setSubValue = null, ELanguageTable subValueDesc = ELanguageTable.valueTypeDefine)
    {
        dictIndex = Interlocked.Increment(ref statusDictIndex);
        statusDictionaryDict.Add(dictIndex, this);
        baseValue = 0f;
        multiplier = 1f;
        divider = 1f;
        adder = 0f;

        baseValueByReference = 0f;
        multiplierByReference = 1f;
        dividerByReference = 1f;
        adderByReference = 0f;
        this.setSubValueFunc = setSubValue;
        this.subValueDesc = subValueDesc;
        SetFinalValue();
        randomizeKeyTimer = new Timer(_ => RandomizeKeyRoutine(), null, 0, 1000);
    }

    // 타이머를 멈추는 메서드
    public void StopRandomizeKeyTimer()
    {
        if (randomizeKeyTimer != null)
        {
            randomizeKeyTimer.Dispose();
            randomizeKeyTimer = null; // 타이머를 삭제한 후 참조를 null로 설정합니다.
        }
    }

    // 상태 딕셔너리를 삭제하는 메서드
    public void DeleteStatusDict()
    {
        foreach (var item in referenceStatusDictionary)
        {
            item.Value.refDict.OnChanged -= SetFinalValue_Reference;
        }
        StopRandomizeKeyTimer();
    }

    // 모든 상태 딕셔너리를 초기화
    public static void AllClearStatusDict()
    {
        try
        {
            var a = statusDictionaryDict.ToArray();
            for (int i = 0; i < a.Length; i++)
            {
                a[i].Value.DeleteStatusDict();
            }
            statusDictionaryDict.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            throw;
        }
    }

    // 참조 상태 딕셔너리를 추가
    public void AddReferenceStatusDict((ELanguageTable status, EStatusType statusType) key, StatusDictionary statusDictionary, float referenceDictionaryMultiflier = 1f)
    {
        referenceStatusDictionary[key] = (statusDictionary, referenceDictionaryMultiflier);
        statusDictionary.OnChanged -= SetFinalValue_Reference;
        statusDictionary.OnChanged += SetFinalValue_Reference;
        SetFinalValue_Reference();
    }

    // 참조 상태 딕셔너리를 제거
    public void RemoveReferenceStatusDict((ELanguageTable status, EStatusType statusType) key, StatusDictionary statusDictionary)
    {
        if (referenceStatusDictionary.ContainsKey(key))
        {
            referenceStatusDictionary.Remove(key);
        }
        statusDictionary.OnChanged -= SetFinalValue_Reference;
        SetFinalValue_Reference();
    }

    // 참조 상태 딕셔너리에 따라 최종 값을 설정
    private void SetFinalValue_Reference()
    {
        baseValueByReference = 0f;
        multiplierByReference = 1f;
        dividerByReference = 1f;
        adderByReference = 0f;

        foreach (var item in referenceStatusDictionary)
        {
            switch (item.Key.statusType)
            {
                case EStatusType.baseValue:
                    baseValueByReference += (item.Value.refDict.FinalValue * item.Value.refDictMultiplier);
                    break;
                case EStatusType.multiplier:
                    multiplierByReference *= (item.Value.refDict.FinalValue * item.Value.refDictMultiplier);
                    break;
                case EStatusType.divider:
                    dividerByReference *= (item.Value.refDict.FinalValue * item.Value.refDictMultiplier);
                    break;
                case EStatusType.adder:
                    adderByReference += (item.Value.refDict.FinalValue * item.Value.refDictMultiplier);
                    break;
            }
        }

        SetFinalValue();
    }

    // 상태 타입에 따른 최종 값을 설정
    private void SetFinalValue_Base(EStatusType statusType, float beforeValue, float afterValue)
    {
        if (beforeValue == afterValue)
        {
            return;
        }
        switch (statusType)
        {
            case EStatusType.baseValue:
                baseValue += afterValue - beforeValue;
                break;
            case EStatusType.multiplier:
                multiplier = multiplier * (1f + afterValue * 0.01f) / (1f + beforeValue * 0.01f);
                break;
            case EStatusType.divider:
                divider = divider * (1f + afterValue * 0.01f) / (1f + beforeValue * 0.01f);
                break;
            case EStatusType.adder:
                adder += afterValue - beforeValue;
                break;
        }

        SetFinalValue();
    }

    // 최종 값을 설정
    private void SetFinalValue()
    {
        FinalValue = (baseValue + baseValueByReference) * (1f + (adder + adderByReference) * 0.01f) * multiplier * multiplierByReference / (divider * dividerByReference);
        if (!ReferenceEquals(setSubValueFunc, null))
        {
            subValue = setSubValueFunc(FinalValue);
        }
        OnChanged.Invoke();
    }

    protected override void Changed()
    {
    }

    // 상태 값에 따른 설명을 반환
    public static string GetDescriptionValue(float finalAmount, int statusType, bool isShowSign = false)
    {
        return GetDescriptionValue(finalAmount, (EStatusType)statusType, isShowSign);
    }

    // 상태 값에 따른 설명을 반환
    public static string GetDescriptionValue(float finalAmount, EStatusType statusType, bool isShowSign = false)
    {
        string sign = isShowSign && finalAmount > 0 ? "+" : "";

        switch (statusType)
        {
            case EStatusType.baseValue:
                return $"{sign}{finalAmount.KMBTUnit()}";
            case EStatusType.multiplier:
                return $"{sign}{finalAmount.KMBTUnit()}% {ELanguageTable.Amplify.LocalIzeText()}";
            case EStatusType.divider:
                return $"{sign}{finalAmount.KMBTUnit()}% {ELanguageTable.Suppress.LocalIzeText()}";
            case EStatusType.adder:
                return $"{sign}{finalAmount.KMBTUnit()}%";
            default:
                return string.Empty;
        }
    }
}
