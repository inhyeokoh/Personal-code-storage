using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// 플레이어의 인벤토리와 장비 + 인벤토리 - 상점 상호작용 일부
public class InventoryManager : SubClass<GameManager>
{
    public int totalSlotCount = 30;
    public List<ItemData> items; // slot index에 따른 아이템 리스트

    public int equipSlotCount = 9;
    public List<ItemData> equips;

    public long gold = 100000L;
    UI_Inventory _inven;
    UI_PlayerInfo _playerInfo;

    enum Enum_Sort // 아이템 정렬 방법
    {
        Grade,
        DetailType,
        ID
    }

    protected override void _Clear()
    {        
    }

    protected override void _Excute()
    {        
    }

    protected override void _Init()
    {
    }

    //로컬로부터 내 인벤 아이템 정보 가져와서 인벤 리스트에 집어넣음
    public void ConnectInvenUI()
    {
        items = new List<ItemData>(new ItemData[totalSlotCount]);
        equips = new List<ItemData>(new ItemData[equipSlotCount]);

        _inven = GameObject.Find("PopupCanvas").GetComponentInChildren<UI_Inventory>();
        _playerInfo = GameObject.Find("PopupCanvas").GetComponentInChildren<UI_PlayerInfo>();
        _GetInvenDataFromTable();
    }

    void _GetInvenDataFromTable()
    {
        var item = CSVReader.Read("Data/InvenItems");
        for (int i = 0; i < item.Count; i++)
        {
            int id = int.Parse(item[i]["id"]);
            int count = int.Parse(item[i]["count"]);
            int slotNum = int.Parse(item[i]["slotNum"]);

            // id,count,slotNum받고 해당하는 id로 아이템 생성
            items[slotNum] = ItemParsing.StateItemDataReader(id);
            items[slotNum].count = count;

            // TODO 장비아이템은 고유번호
        }
    }

    public void ExtendItemList()
    {
        while (items.Count < totalSlotCount)
        {
            items.Add(null);
        }
    }

    public void DragAndDropItems(int oldPos, int newPos)
    {
        if (oldPos == newPos) return;

        // newPos가 비어있는 슬롯이면 옮기기
        if (items[newPos] == null)
        {
            _ChangeSlotNum(oldPos, newPos);
        }
        // 아이템 id가 같은 아이템이고 장비 아이템이 아니면 수량 합치기
        else if (_CheckSameAndCountable(oldPos, newPos))
        {
            _AddUpItems(oldPos, newPos);
        }
        // 다른 아이템이면 위치 교환
        else
        {
            _ExchangeSlotNum(oldPos, newPos);
        }
        // 이미지 갱신
        _inven.UpdateInvenUI(oldPos);
        _inven.UpdateInvenUI(newPos);

        // TODO 바뀐 내용 서버로 전송
    }

    void _ChangeSlotNum(int oldPos, int newPos)
    {
        items[newPos] = items[oldPos];
        items[oldPos] = null;
    }

    void _ExchangeSlotNum(int oldPos, int newPos)
    {
        ItemData temp = items[oldPos];
        items[oldPos] = items[newPos];
        items[newPos] = temp;
    }

    bool _CheckSameAndCountable(int a, int b)
    {
        return items[a].id == items[b].id && (items[a].itemType != Enum_ItemType.Equipment);
    }

    void _AddUpItems(int a, int b)
    {
        if (items[b].count == items[b].maxCount) // 바꿀 위치에 이미 꽉 차있는 경우, 수량만 교환
        {
            int temp = items[b].count;
            items[b].count = items[a].count;
            items[a].count = temp;
        }
        else
        {
            items[b].count = items[a].count + items[b].count;
            if (items[b].count > items[b].maxCount) // 합쳤을 때 수가 MaxCount보다 크면
            {
                items[a].count = items[b].count - items[b].maxCount;
                items[b].count = items[b].maxCount;
            }
            else
            {
                items[a] = null;
            }
        }
    }

    public void InvenToEquipSlot(int invenPos, int equipPos)
    {
        // 맞는 장착 아이템 아니면 반환
        if (!_CheckSameEquipType(equipPos, invenPos)) return;

        ItemData temp = equips[equipPos]; 
        equips[equipPos] = items[invenPos];
        items[invenPos] = temp;

        _inven.UpdateInvenUI(invenPos);
        _playerInfo.UpdateEquipUI(equipPos);
    }

    public void EquipSlotToInven(int equipPos, int invenPos)
    {
        if (items[invenPos] == null) // 드롭한 인벤칸이 비어 있으면 옮기기
        {
            items[invenPos] = equips[equipPos];
            equips[equipPos] = null;
        }
        // 같은 타입의 장비 아이템이면 교환
        else if (_CheckSameEquipType(equipPos, invenPos))
        {
            ItemData temp = items[invenPos];
            items[invenPos] = equips[equipPos];
            equips[equipPos] = temp;
        }

        _inven.UpdateInvenUI(invenPos);
        _playerInfo.UpdateEquipUI(equipPos);
    }

    bool _CheckSameEquipType(int equipPos, int invenPos)
    {
        int equipType = -1;

        StateItemData sid = items[invenPos] as StateItemData;
        switch (sid.detailType)
        {
            case Enum_DetailType.Head:
                equipType = 0;
                break;
            case Enum_DetailType.Body:
                equipType = 1;
                break;
            case Enum_DetailType.Hand:
                equipType = 2;
                break;
            case Enum_DetailType.Foot:
                equipType = 3;
                break;
            case Enum_DetailType.Weapon:
                equipType = 4;
                break;
            case Enum_DetailType.Default:
                equipType = 5;
                break;
            default:
                equipType = -1;
                break;
        }

        return equipPos == equipType;
    }

    int GetEmptySlotIndex()
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                return i;
            }
        }

        // 비어있는 슬롯 없음
        return -1;
    }

    public void GetItem(ItemData acquired)
    {
        // 획득한 아이템이 없는 경우
        if (acquired == null) return;        

        // 수량이 합산되지 않는 장비 아이템 처리
        if (acquired.itemType == Enum_ItemType.Equipment)
        {
            int emptySlotIndex = GetEmptySlotIndex();
            if (emptySlotIndex != -1)
            {
                items[emptySlotIndex] = new ItemData(acquired, acquired.count);
            }
            else
            {
                // 인벤토리가 가득 찬 경우 처리 (공간 부족 알림 팝업)
            }
        }
        else
        {
            // 획득 수량 전부 처리할때까지 반복
            while (acquired.count > 0) 
            {
                for (int i = 0; i < items.Count; i++)
                {           
                    // 동일 아이템 확인
                    if (items[i] != null && items[i].id == acquired.id)
                    {
                        // 칸에 최대 수량이 아닌 경우
                        if (items[i].count < items[i].maxCount)
                        {
                            int remainSpace = items[i].maxCount - items[i].count; // 잔여공간 크기
                            // 획득한 수량이 잔여 공간에 들어갈 수 있는 경우
                            if (acquired.count <= remainSpace)
                            {
                                items[i].count += acquired.count;
                                acquired.count = 0;
                                break;
                            }
                            // 획득한 수량이 잔여 공간보다 큰 경우
                            else
                            {
                                items[i].count = items[i].maxCount;
                                acquired.count -= remainSpace;
                            }
                        }
                        // 칸에 이미 꽉 찬 경우
                        else
                        {
                            continue;
                        }
                    }

                    // 마지막 칸까지 동일 아이템 안 보이면 새로운 슬롯에 추가
                    if (i == items.Count - 1)
                    {
                        int emptySlotIndex = GetEmptySlotIndex();
                        if (emptySlotIndex != -1)
                        {
                            // 획득 수량이 최대 수량 이하인 경우
                            if (acquired.count <= acquired.maxCount)
                            {
                                items[emptySlotIndex] = new ItemData(acquired, acquired.count);
                                acquired.count = 0;
                            }
                            // 획득 수량이 최대 수량 보다 클 경우
                            else
                            {
                                items[emptySlotIndex] = new ItemData(acquired, acquired.count);
                                items[emptySlotIndex].count = acquired.maxCount;
                                acquired.count -= acquired.maxCount;
                            }
                        }
                    }
                }
            }
        }

        // UI 갱신
        for (int i = 0; i < items.Count; i++)
        {
            // Debug.Log(items[0].id, items[0].icon);
            _inven.UpdateInvenUI(i);
        }
    }

    public void DropInvenItem(int index, int dropCount = 1)
    {
        items[index].count -= dropCount;
        ItemData droppedItem = new ItemData(items[index], dropCount);
        Vector3 playerTr = GameObject.FindWithTag("Player").transform.position;
        Vector3 dropPos = new Vector3(playerTr.x, playerTr.y, playerTr.z);
        ItemManager._item.ItemInstance(droppedItem, dropPos, Quaternion.identity);

        if (items[index].count <= 0)
        {
            items[index] = null;
        }

        _inven.UpdateInvenUI(index);
    }

    public void DropEquipItem(int index)
    {
        ItemData droppedItem = new ItemData(equips[index], 1);
        Vector3 playerTr = GameObject.FindWithTag("Player").transform.position;
        Vector3 dropPos = new Vector3(playerTr.x, playerTr.y, playerTr.z);
        ItemManager._item.ItemInstance(droppedItem, dropPos, Quaternion.identity);
        equips[index] = null;

        _playerInfo.UpdateEquipUI(index);
    }

    public void EquipItem(int index)
    {
        int equipType = -1;

        StateItemData sid = items[index] as StateItemData;
        switch (sid.detailType)
        {
            case Enum_DetailType.Head:
                equipType = 0;
                break;
            case Enum_DetailType.Body:
                equipType = 1;
                break;
            case Enum_DetailType.Hand:
                equipType = 2;
                break;
            case Enum_DetailType.Foot:
                equipType = 3;
                break;
            case Enum_DetailType.Weapon:
                equipType = 4;
                break;
            case Enum_DetailType.Default:
                equipType = 5;
                break;
            default:
                equipType = -1;
                break;
        }

        // 장착 아이템 아니면 반환
        if (equipType == -1) return;

        ItemData temp = null;
        // 장착하고 있던 아이템이 있다면 잠시 보관
        if (equips[equipType] != null)
        {
            temp = equips[equipType];
        }
        equips[equipType] = items[index];
        items[index] = null;

        if (temp != null)
        {
            items[index] = temp;
        }

        // 장비창 UI 갱신
        if (_playerInfo.gameObject.activeSelf)
        {
            _playerInfo.UpdateEquipUI(equipType);
        }

        // 인벤토리 UI 갱신
        _inven.UpdateInvenUI(index);
    }

    public void UnEquipItem(int index)
    {
        // 인벤토리 비어 있는 칸 찾아서 넣기
        int invenEmptySlot = GetEmptySlotIndex();
        items[invenEmptySlot] = equips[index];
        equips[index] = null;

        _playerInfo.UpdateEquipUI(index);
        _inven.UpdateInvenUI(invenEmptySlot);

        // TODO : 인벤토리 꽉 찬 경우 해제 불가 팝업
    }

    // 아이템 번호에 따라서 리스트 재정렬 + 앞부터 비어 있는 칸 채워야함 + 같은 아이템이면 합쳐줌 + slotNum 변경
    // 한번이라도 정렬 버튼 누른적 있을거고 정리 되어 있는 상태가 많기 때문에 병합 정렬로 가는게 가장 괜찮다고 판단
    public void SortItems()
    {
        items.RemoveAll(item => item == null);
        List<ItemData> result = MergeSort(items); // 병합 정렬
        items.Clear();
        items.AddRange(result);

        _CombineQuantities(items);
        ExtendItemList(); // 비어있는 칸 다시 null로 채우기
        for (int i = 0; i < items.Count; i++) // 아이템의 슬롯 번호 변경
        {
            if (items[i] != null)
            {
                items[i].slotNum = i;
            }
        }

        // UI 갱신
        for (int i = 0; i < items.Count; i++)
        {
            _inven.UpdateInvenUI(i);
        }
    }

    List<ItemData> MergeSort(List<ItemData> unsorted)
    {
        if (unsorted.Count <= 1)
            return unsorted;

        int middle = unsorted.Count / 2;
        List<ItemData> left = new List<ItemData>();
        List<ItemData> right = new List<ItemData>();

        for (int i = 0; i < middle; i++)
            left.Add(unsorted[i]);

        for (int i = middle; i < unsorted.Count; i++)
            right.Add(unsorted[i]);

        left = MergeSort(left);
        right = MergeSort(right);

        return Merge(left, right);
    }

    List<ItemData> Merge(List<ItemData> left, List<ItemData> right)
    {
        List<ItemData> result = new List<ItemData>();

        while (left.Count > 0 || right.Count > 0)
        {
            if (left.Count > 0 && right.Count > 0)
            {
                if (CompareLeftIsSmall(left[0],right[0]))
                {
                    result.Add(left[0]);
                    left.RemoveAt(0);
                }
                else
                {
                    result.Add(right[0]);
                    right.RemoveAt(0);
                }
            }
            else if (left.Count > 0)
            {
                result.Add(left[0]);
                left.RemoveAt(0);
            }
            else if (right.Count > 0)
            {
                result.Add(right[0]);
                right.RemoveAt(0);
            }
        }

        return result;
    }

    // 왼쪽에 올 아이템에 대해 true 반환
    bool CompareLeftIsSmall(ItemData left, ItemData right)
    {
        if (left.itemType < right.itemType)
            return true;
        if (left.itemType > right.itemType)
            return false;
        if (left.itemGrade < right.itemGrade)
            return true;
        if (left.itemGrade > right.itemGrade)
            return false;
        // 기타아이템에는 상세타입 없으니 제외
        if (left.itemType != Enum_ItemType.ETC)
        {
            StateItemData leftItem = left as StateItemData;
            StateItemData rightItem = right as StateItemData;
            if (leftItem != null && rightItem != null)
            {
                if (leftItem != null && leftItem.detailType < rightItem.detailType)
                    return true;
                if (leftItem.detailType > rightItem.detailType)
                    return false;
            }
        }
        if (left.id < right.id)
            return true;
        if (left.id > right.id)
            return false;
        return true;
    }

    void _CombineQuantities(List<ItemData> itemList)
    {
        if (itemList.Count < 2)
        {
            return;
        }

        int i = 1;
        while (i < itemList.Count)
        {
            // 같은 ItemID이고 더 앞쪽에 있는 아이템의 수량이 최대가 아닐 경우 앞에다가 합치기
            if (itemList[i].id == itemList[i - 1].id && itemList[i - 1].count != itemList[i - 1].maxCount)
            {
                _AddUpItems(i, i - 1);
                if (itemList[i] == null)
                {
                    itemList.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
            else
            {
                i++;
            }
        }
    }

    public void Purchase(long totalPurchaseGold)
    {
        gold -= totalPurchaseGold;
        _inven.UpdateGoldPanel(gold);
    }

    public void Sell(long totalSellGold)
    {
        gold += totalSellGold;
        _inven.UpdateGoldPanel(gold);
    }

    // 인벤 우클릭으로 아이템을 상점 품목으로 이동
    public void InvenToShop(int index)
    {
        UI_ShopSell shopSell = GameManager.UI.Shop.GetComponentInChildren<UI_ShopSell>();
        int emptyIndex = shopSell.GetEmptySlotIndex();
        shopSell.shopItems[emptyIndex] = items[index];
        items[index] = null;

        _inven.UpdateInvenUI(index);
        shopSell.UpdateGoldPanel();
        shopSell.shopItemCount++;
        shopSell.transform.GetChild(0).GetChild(emptyIndex).GetComponent<UI_ShopSlot>().ItemRender();
    }

    // 상점 품목 우클릭으로 인벤토리로 되돌리기
    public void ShopToInven(UI_ShopSlot.Enum_ShopSlotTypes slotType, int index)
    {
        int emptyIndex = GetEmptySlotIndex();
        switch (slotType)
        {
            case UI_ShopSlot.Enum_ShopSlotTypes.Sell:
                UI_ShopSell shopSell = GameManager.UI.Shop.GetComponentInChildren<UI_ShopSell>();
                items[emptyIndex] = shopSell.shopItems[index];
                shopSell.shopItems[index] = null;

                _inven.UpdateInvenUI(emptyIndex);
                shopSell.UpdateGoldPanel();
                shopSell.transform.GetChild(0).GetChild(index).GetComponent<UI_ShopSlot>().ItemRender();
                break;
            case UI_ShopSlot.Enum_ShopSlotTypes.Repurchase:
                UI_ShopRepurchase shopRepurchase = GameManager.UI.Shop.GetComponentInChildren<UI_ShopRepurchase>();
                items[emptyIndex] = shopRepurchase.tempSoldItems[index];
                Purchase(shopRepurchase.tempSoldItems[index].sellingprice); // 구매 가격 아니고 판매 가격으로 재구매임. 
                shopRepurchase.tempSoldItems[index] = null;

                _inven.UpdateInvenUI(emptyIndex);
                _inven.UpdateGoldPanel(gold);
                shopRepurchase.transform.GetChild(0).GetChild(index).GetComponent<UI_ShopSlot>().ItemRender();
                break;
            default:
                break;
        }
    }
}
