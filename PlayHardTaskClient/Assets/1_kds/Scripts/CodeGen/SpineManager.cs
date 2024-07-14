//ToolSpineManagerGen 의해 자동으로 생성된 스크립트입니다..
using System.ComponentModel;
using Spine;
using Spine.Unity;
using System.Collections.Generic;
using System;
public static class SpineManager
{
    public static void SetSkin(this SkeletonAnimation SpineCharacter, string eGangSkin)
    {
        SetSkin(SpineCharacter, eGangSkin.ParseEnum<EGangSkin>());
    }
    public static void SetSkin(this SkeletonAnimation SpineCharacter, EGangSkin eGangSkin)
    {
        SpineCharacter.Skeleton.SetSkin(SpineCharacter.skeletonDataAsset.GetSkeletonData(true).FindSkin(eGangSkin.OriginName()));
        SpineCharacter.Skeleton.SetSlotsToSetupPose();
    }
    public static void AddSkin(this SkeletonAnimation SpineCharacter, EGangSkin eGangSkin)
    {
        Skin currentSkin = SpineCharacter.Skeleton.Skin;
        Skin addSkin = SpineCharacter.skeletonDataAsset.GetSkeletonData(true).FindSkin(eGangSkin.OriginName());
        Skin temporarySkin = new Skin("temporarySkin");
        // addSkin.Attachments의 항목을 임시 리스트에 복사
        var entries = new List<Skin.SkinEntry>(currentSkin.Attachments);

        foreach (var entry in entries)
        {
            int slotIndex = entry.SlotIndex;
            string attachmentName = entry.Name;
            Attachment brownhairAttachment = entry.Attachment;

            // 현재 스킨의 해당 슬롯에 brownhairSkin의 어태치먼트 설정
            temporarySkin.SetAttachment(slotIndex, attachmentName, brownhairAttachment);
        }

        entries = new List<Skin.SkinEntry>(addSkin.Attachments);
        // 임시 리스트를 순회
        foreach (var entry in entries)
        {
            int slotIndex = entry.SlotIndex;
            string attachmentName = entry.Name;
            Attachment brownhairAttachment = entry.Attachment;

            // 현재 스킨의 해당 슬롯에 brownhairSkin의 어태치먼트 설정
            temporarySkin.SetAttachment(slotIndex, attachmentName, brownhairAttachment);
        }

        // 변경된 스킨 적용하기
        SpineCharacter.Skeleton.SetSkin(temporarySkin);
        SpineCharacter.Skeleton.SetSlotsToSetupPose();

    }
}
public enum EGangSkin
{
    [Description("BodySkins/GirlLv1/GirlLv1White")]
	BodySkins_GirlLv1_GirlLv1White = 563822418,
	[Description("BodySkins/GirlLv2/GirlLv2MoreBlack")]
	BodySkins_GirlLv2_GirlLv2MoreBlack = 1907291181,
	[Description("BodySkins/GirlLv2/GirlLv2White")]
	BodySkins_GirlLv2_GirlLv2White = 1457105098,
	[Description("BodySkins/GirlLv3/GirlLv3Red")]
	BodySkins_GirlLv3_GirlLv3Red = -539614232,
	[Description("BodySkins/GirlLv3/GirlLv3White")]
	BodySkins_GirlLv3_GirlLv3White = 1111530714,
	[Description("BodySkins/ManLv1/ManLv1Black")]
	BodySkins_ManLv1_ManLv1Black = -921352772,
	[Description("BodySkins/ManLv1/ManLv1Brown")]
	BodySkins_ManLv1_ManLv1Brown = -920818819,
	[Description("BodySkins/ManLv1/ManLv1Pink")]
	BodySkins_ManLv1_ManLv1Pink = 1355344191,
	[Description("BodySkins/ManLv1/ManLv1White")]
	BodySkins_ManLv1_ManLv1White = -931325538,
	[Description("BodySkins/ManLv1/ManLv1Yellow")]
	BodySkins_ManLv1_ManLv1Yellow = 852766427,
	[Description("BodySkins/ManLv2/ManLv2Black")]
	BodySkins_ManLv2_ManLv2Black = -787501050,
	[Description("BodySkins/ManLv2/ManLv2Brown")]
	BodySkins_ManLv2_ManLv2Brown = -787583193,
	[Description("BodySkins/ManLv2/ManLv2Pink")]
	BodySkins_ManLv2_ManLv2Pink = -718690863,
	[Description("BodySkins/ManLv2/ManLv2White")]
	BodySkins_ManLv2_ManLv2White = -805576088,
	[Description("BodySkins/ManLv2/ManLv2Yellow")]
	BodySkins_ManLv2_ManLv2Yellow = 631601541,
	[Description("BodySkins/ManLv3/ManLv3Black")]
	BodySkins_ManLv3_ManLv3Black = 1018930672,
	[Description("BodySkins/ManLv3/ManLv3Brown")]
	BodySkins_ManLv3_ManLv3Brown = 1019472049,
	[Description("BodySkins/ManLv3/ManLv3BrownBlack")]
	BodySkins_ManLv3_ManLv3BrownBlack = 1403918390,
	[Description("BodySkins/ManLv3/ManLv3Pink")]
	BodySkins_ManLv3_ManLv3Pink = 447848395,
	[Description("BodySkins/ManLv3/ManLv3White")]
	BodySkins_ManLv3_ManLv3White = 993697234,
	[Description("BodySkins/ManLv3/ManLv3WhitePink")]
	BodySkins_ManLv3_ManLv3WhitePink = -1105259986,
	[Description("BodySkins/ManLv3/ManLv3WhiteTatoo")]
	BodySkins_ManLv3_ManLv3WhiteTatoo = 92733163,
	[Description("BodySkins/ManLv3/ManLv3Yellow")]
	BodySkins_ManLv3_ManLv3Yellow = 1146851223,
	[Description("DungeonProject/Anubis")]
	DungeonProject_Anubis = -1492206235,
	
}


public enum EGangAnimation
{
    [Description("None")]
	None = 4467689,
	[Description("GangDungeon/AnubisCastDone")]
	GangDungeon_AnubisCastDone = 849873946,
	[Description("GangDungeon/AnubisCasting")]
	GangDungeon_AnubisCasting = 1274385510,
	[Description("GangDungeon/AnubisIdle")]
	GangDungeon_AnubisIdle = -1125618467,
	[Description("GangDungeon/AnubisIdle2")]
	GangDungeon_AnubisIdle2 = -534434063,
	[Description("GangDungeon/AnubisIdle3")]
	GangDungeon_AnubisIdle3 = -534434064,
	[Description("GangDungeon/AnubisIdle4")]
	GangDungeon_AnubisIdle4 = -534434057,
	[Description("GangDungeon/Die")]
	GangDungeon_Die = 987669603,
	[Description("GangDungeon/Idle")]
	GangDungeon_Idle = 552592181,
	[Description("GangDungeon/knockdown")]
	GangDungeon_knockdown = 726143269,
	[Description("GangDungeon/Knuckback")]
	GangDungeon_Knuckback = -2108464458,
	[Description("GangDungeon/PickDown")]
	GangDungeon_PickDown = 1188296924,
	[Description("GangDungeon/PickUp")]
	GangDungeon_PickUp = -812170937,
	[Description("GangDungeon/Run")]
	GangDungeon_Run = 987687166,
	[Description("GangDungeon/Walk")]
	GangDungeon_Walk = 553421444,
	[Description("GangDungeon/WalkSlapped0")]
	GangDungeon_WalkSlapped0 = 1626759599,
	
}


public enum EGangSlot
{
    [Description("Default")]
	Default = 409710486,
	[Description("BackHair")]
	BackHair = -464147554,
	[Description("Cape")]
	Cape = 4622034,
	[Description("BackArmUnder")]
	BackArmUnder = -99296714,
	[Description("BackArmUnderTatoo")]
	BackArmUnderTatoo = -1820536953,
	[Description("BackArmUpper")]
	BackArmUpper = -99967368,
	[Description("BackArmUpperTatoo")]
	BackArmUpperTatoo = 727433997,
	[Description("BackHand")]
	BackHand = -464147543,
	[Description("BackWeapon")]
	BackWeapon = -378039098,
	[Description("BackFinger")]
	BackFinger = 119660121,
	[Description("BackArmUpperCloth")]
	BackArmUpperCloth = 713803484,
	[Description("BackArmUnderCloth")]
	BackArmUnderCloth = -1837849898,
	[Description("BackThigh")]
	BackThigh = -1529732338,
	[Description("BackShine")]
	BackShine = -1532685589,
	[Description("BackFoot")]
	BackFoot = -464669562,
	[Description("BackThighCloth")]
	BackThighCloth = 1668395758,
	[Description("BackFootCloth_UnderShine")]
	BackFootCloth_UnderShine = 1645973454,
	[Description("BackShineCloth")]
	BackShineCloth = -1083540183,
	[Description("BackFootCloth")]
	BackFootCloth = -732574106,
	[Description("Body")]
	Body = 4594487,
	[Description("BodyTatoo")]
	BodyTatoo = -1327927896,
	[Description("Top_UnderBottom")]
	Top_UnderBottom = 559053542,
	[Description("Bottom")]
	Bottom = 119833290,
	[Description("FrontThigh")]
	FrontThigh = -1418936856,
	[Description("FrontShine")]
	FrontShine = -1410084383,
	[Description("FrontFoot")]
	FrontFoot = -45100452,
	[Description("FrontThighCloth")]
	FrontThighCloth = -1413230260,
	[Description("Top")]
	Top = 138868,
	[Description("FrontFootCloth_UnderShine")]
	FrontFootCloth_UnderShine = -184229640,
	[Description("FrontShineCloth")]
	FrontShineCloth = -62677653,
	[Description("FrontFootCloth")]
	FrontFootCloth = -163229584,
	[Description("Skirt")]
	Skirt = 128586216,
	[Description("Top_UpperSkirt")]
	Top_UpperSkirt = 133606786,
	[Description("Head")]
	Head = 4406221,
	[Description("FrontArmLower")]
	FrontArmLower = 1901151633,
	[Description("FrontArmLowerTatoo")]
	FrontArmLowerTatoo = 1937497862,
	[Description("FrontArmUpper")]
	FrontArmUpper = 1892635938,
	[Description("FrontShoulderTatoo")]
	FrontShoulderTatoo = -980368459,
	[Description("FrontFinger")]
	FrontFinger = -389819117,
	[Description("FrontWeapon")]
	FrontWeapon = -887856228,
	[Description("Rifle")]
	Rifle = 127594539,
	[Description("FrontHand")]
	FrontHand = -45407769,
	[Description("Equipment")]
	Equipment = -399111371,
	[Description("FrontArmLowerCloth")]
	FrontArmLowerCloth = 1921843931,
	[Description("FrontArmUpperCloth")]
	FrontArmUpperCloth = 32629058,
	[Description("BackSkirt")]
	BackSkirt = -1532814497,
	[Description("FrontSkirt")]
	FrontSkirt = -1410118967,
	[Description("MiddleSkirt")]
	MiddleSkirt = -1068405439,
	[Description("ThrowWeapon")]
	ThrowWeapon = 1586962857,
	[Description("FrontHandCloth")]
	FrontHandCloth = 1885744037,
	[Description("BackHandCloth")]
	BackHandCloth = 1914362131,
	
}


public enum EGangEvent
{
    [Description("Attack")]
	Attack = -16114689,
	
}


public enum EGangType
{
    
}


