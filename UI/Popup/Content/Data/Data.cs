using System.Collections.Generic;
using UnityEngine;

public class Data
{
}

[System.Serializable]
public class EquipmentItemData : StateItemData
{
    public int maxReinforcement;
    public int currentReinforcement;
    public Enum_EquipmentDetailType detailType;
    public Enum_Class itemClass;

    public EquipmentItemData(int id, string name, string desc, Sprite icon, Enum_Class itemClass, Enum_Grade itemGrade, Enum_ItemType itemType, Enum_EquipmentDetailType detailType,long sellingprice, int level, int attack, int defense
        ,int speed, int attackSpeed, int hp, int mp, int exp, int maxHp, int maxMp, int maxReinforcement, bool durationBool, float duration, int count = 1, int slotNum = -1, int maxCount = 1, int currentReinforcement = 0) : base ( id,  name,  desc,  icon,  itemGrade,  itemType,  sellingprice,  level,  attack,  defense
        ,speed,  attackSpeed,  hp,  mp,  exp,  maxHp,  maxMp,maxCount, durationBool,duration, count, slotNum )
    {
        this.itemClass = itemClass;
        this.maxReinforcement = maxReinforcement;
        this.currentReinforcement = currentReinforcement;
        this.detailType = detailType;
    }

    public override int GetIntType()
    {
        return (int)detailType;
    }

}

public class ConsumptionItemData : StateItemData
{
    public Enum_ConsumptionDetailType detailType;

    public ConsumptionItemData(int id, string name, string desc, Sprite icon, Enum_Grade itemGrade, Enum_ItemType itemType, Enum_ConsumptionDetailType detailType, long sellingprice, int level, int attack, int defense
        , int speed, int attackSpeed, int hp, int mp, int exp, int maxHp, int maxMp, bool durationBool, float duration, int maxCount, int count = 1, int slotNum = -1) : base(id, name, desc, icon,itemGrade, itemType, sellingprice, level, attack, defense
        , speed, attackSpeed, hp, mp, exp, maxHp, maxMp, maxCount, durationBool, duration, count, slotNum)
    {
        this.detailType = detailType;
    }

    public override int GetIntType()
    {
        return (int)detailType;
    }
}


[System.Serializable]
public class StateItemData : ItemData
{
    public int level;
    public int attack;
    public int defense;
    public int speed;
    public int attackSpeed;
    public int exp;
    public int hp;
    public int mp;
    public int maxHp;
    public int maxMp;
    public bool durationBool;
    public float duration;

    public StateItemData(int id, string name, string desc, Sprite icon,  Enum_Grade itemGrade, Enum_ItemType itemType, long sellingprice, int level, int attack, int defense
        , int speed, int attackSpeed, int hp, int mp, int exp, int maxHp, int maxMp, int maxCount, bool durationBool,float duration, int count = 1, int slotNum = -1) : base(id, name, desc, icon, itemType, itemGrade, sellingprice, maxCount, count, slotNum)
    {           
        this.level = level;
        this.attack = attack;
        this.defense = defense;
        this.speed = speed;
        this.attackSpeed = attackSpeed;
        this.exp = exp;
        this.hp = hp;
        this.mp = mp;
        this.maxHp = maxHp;
        this.maxMp = maxMp;
        this.maxCount = maxCount;
        this.durationBool = durationBool;
        this.duration = duration;
    }


}

[System.Serializable]
public class ItemData : Data
{
    public int id;
    public string name;
    public string desc;
    public Sprite icon;
    public Enum_ItemType itemType;
    public Enum_Grade itemGrade; 
    public long sellingprice;
    public int maxCount;
    public int count;
    public int slotNum;
    public ItemData(int id, string name, string desc, Sprite icon, Enum_ItemType itemType, Enum_Grade itemGrade, long sellingprice, int maxCount, int count = 1, int slotNum = -1)
    {
        this.id = id;
        this.name = name;
        this.desc = desc;
        this.icon = icon;
        this.itemType = itemType;
        this.itemGrade = itemGrade;
        this.sellingprice = sellingprice;
        this.maxCount = maxCount;
        this.count = count;
        this.slotNum = slotNum;
    }

    public ItemData(ItemData item, int count)
    {
        this.id = item.id;
        this.name = item.name;
        this.desc = item.desc;
        this.icon = item.icon;
        this.itemType = item.itemType;
        this.itemGrade = item.itemGrade;   
        this.sellingprice = item.sellingprice;
        this.maxCount = item.maxCount;
        this.count = count;
        this.slotNum = item.slotNum;
    }

    public virtual int GetIntType()
    {
        return -1;
    }
}

[System.Serializable]
public class LevelData : Data
{
    public int level;
    public int maxhp;
    public int maxmp;
    public int maxexp;
    public int attack;
    public int defense;

    public LevelData(int level, int maxhp, int maxmp, int maxexp,int attack, int defense)
    {
        this.level = level;
        this.maxhp = maxhp;
        this.maxmp = maxmp;
        this.maxexp = maxexp;
        this.attack = attack;
        this.defense = defense;
    }
}


[System.Serializable]
public class WarriorSkillData : Data
{
    public int id;
    public string skillType;
    public string name;
    public string desc;
    public Sprite icon;
    public WarriorSkill number;
    public float[] skillDuration;
    public int maxLevel;
    public int[] levelCondition;
    public int[] skillPoint;
    public int[] skillMaxHP;
    public int[] skillMaxMP;
    public int[] skillAttack;
    public int[] skillDefense;
    public int[] skillSpeed;
    public int[] skillAttackSpeed;
    public int[] skillMP;
    public float[] skillCool;
    public int[] skillDamage;

    public WarriorSkillData(int id, string skillType, string name, string desc, Sprite icon, WarriorSkill number, float[] skillDuration, int maxLevel, int[] levelCondition,
        int[] skillPoint, int[] skillMaxHP, int[] skillMaxMP,int[] skillAttack, int[] skillDefense, int[] skillSpeed, int[] skillAttackSpeed, int[] skillMP,float[] skillCool, int[] skillDamage)
    {
        this.id = id;
        this.skillType = skillType;
        this.name = name;
        this.desc = desc;
        this.icon = icon;
        this.number = number;
        this.skillDuration = skillDuration;
        this.maxLevel = maxLevel;
        this.levelCondition = levelCondition;
        this.skillPoint = skillPoint;
        this.skillMaxHP = skillMaxHP;
        this.skillMaxMP = skillMaxMP;
        this.skillAttack = skillAttack;
        this.skillSpeed = skillSpeed;
        this.skillDefense = skillDefense;
        this.skillAttackSpeed = skillAttackSpeed;
        this.skillMP = skillMP;
        this.skillCool = skillCool;
        this.skillDamage = skillDamage;
    }
}


[System.Serializable]
public class MonsterData : Data
{
    public int monster_id;
    public string monster_object;
    public string monster_name;
    public Enum_MonsterType monster_type;
    public int monster_level;
    public int monster_exp;
    public int monster_maxhp;
    public int monster_maxmp;
    public int monster_attack;
    public float monster_attackspeed;
    public float monster_delay;
    public float monster_abliltydelay;
    public int monster_defense;
    public int monster_speed;
    public float monster_detectdistance;
    public float monster_attackdistance;
   /* public int[] monster_stateitem;
    public int[] moinster_etcitem;*/
 
    public MonsterData(int monster_id, string monster_object, string monster_name, Enum_MonsterType monster_type, int monster_level,
        int monster_exp, int monster_maxhp, int monster_maxmp, int monster_attack, float monster_attackspeed, float monster_delay, 
        float monster_abliltydelay, int monster_defense, int monster_speed, float monster_detectdistance, float monster_attackdistance/*, 
        int[] monster_stateitem, int[] moinster_etcitem*/)
    {
        this.monster_id = monster_id;
        this.monster_object = monster_object;
        this.monster_name = monster_name;
        this.monster_type = monster_type;
        this.monster_level = monster_level;
        this.monster_exp = monster_exp;
        this.monster_maxhp = monster_maxhp;
        this.monster_maxmp = monster_maxmp;
        this.monster_attack = monster_attack;
        this.monster_attackspeed = monster_attackspeed;
        this.monster_delay = monster_delay;
        this.monster_abliltydelay = monster_abliltydelay;
        this.monster_defense = monster_defense;
        this.monster_speed = monster_speed;
        this.monster_detectdistance = monster_detectdistance;
        this.monster_attackdistance = monster_attackdistance;
        /*this.monster_stateitem = monster_stateitem;
        this.moinster_etcitem = moinster_etcitem;*/
    }
}

[System.Serializable]
public class MonsterItemDropData
{
    public int monster_id;
    public int[] monster_itemdrop;
    public float[] monster_itempercent;
    public int monster_mingold;
    public int monster_maxgold;

    public MonsterItemDropData(int monster_id, int[] monster_itemdrop,float[] monster_itempercent,int monster_mingold, int monster_maxgold)
    {
        this.monster_id = monster_id;
        this.monster_itemdrop = monster_itemdrop;
        this.monster_itempercent = monster_itempercent;
        this.monster_mingold = monster_mingold;
        this.monster_maxgold = monster_maxgold;
    }
}

[System.Serializable]
public class QuestData : Data
{
    // 퀘스트 정보
    public int questID;
    public string title;
    public int npcID;
    public Enum_QuestType questType;
    public string[] conversationText;
    public string summaryText;
    public string ongoingText;
    public string[] completeText;

    // 시작 조건
    public int requiredLevel;
    public int? nextQuestID;

    // 완료 조건
    public List<QuestGoal> goals; // 여러 목표를 담을 리스트

    // 보상
    public int expReward;
    public long goldReward;
    public List<ItemData> itemRewards;

    public QuestData(int questID, string title, int npcID, Enum_QuestType questType, string[] conversationText, string summaryText, string ongoingText, string[] completeText, int requiredLevel, int? nextQuestID,
        List<QuestGoal> goals, int expReward, long goldReward, List<ItemData> itemReward)
    {
        this.questID = questID;
        this.title = title;
        this.npcID = npcID;
        this.questType = questType;
        this.conversationText = conversationText;
        this.summaryText = summaryText;
        this.ongoingText = ongoingText;
        this.completeText = completeText;
        this.requiredLevel = requiredLevel;
        this.nextQuestID = nextQuestID;
        this.goals = goals;
        this.expReward = expReward;
        this.goldReward = goldReward;
        this.itemRewards = itemReward;
    }
}

public abstract class QuestGoal
{
    public abstract bool IsCompleted();
}

public class ObjectGoal : QuestGoal
{
    public int ObjectID { get; set; }
    public string ObjectName { get; set; }
    public int RequiredCount { get; set; }
    int currentCount;

    public ObjectGoal(int objectID, int requiredCount)
    {
        ObjectID = objectID;
        ObjectName = GameManager.Data.itemDatas[objectID].name;
        RequiredCount = requiredCount;
        currentCount = 0; // 초기화
    }

    public void IncrementCount(int amount)
    {
        currentCount += amount;
    }

    public override bool IsCompleted()
    {
        return currentCount >= RequiredCount;
    }
}

public class MonsterGoal : QuestGoal
{
    public int MonsterID { get; set; }
    public string MonsterName { get; set; }
    public int RequiredCount { get; set; }
    int currentCount;

    public MonsterGoal(int monsterID, int requiredCount)
    {
        MonsterID = monsterID;
        MonsterName = GameManager.Data.monsterDatas[monsterID].monster_name;
        RequiredCount = requiredCount;
        currentCount = 0;
    }

    public void IncrementCount(int amount)
    {
        currentCount += amount;
        //Debug.LogError($"{MonsterName} {currentCount}/{RequiredCount}");
    }

    public override bool IsCompleted()
    {
        return currentCount >= RequiredCount;
    }
}
