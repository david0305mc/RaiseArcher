using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerCmd
{
    public const int AUTH_USER_LOGIN = 1000;            // ���� �α���
    public const int AUTH_GAME_LOGIN = 1001;            // ���� �α���
    public const int AUTH_EXISTS_USER = 1010;           // ���� ���� ����
    public const int AUTH_GAME_LEAVE = 9901;            // ���� ����/Ż��
    public const int AUTH_USER_RESTORE = 9902;

    public const int USER_BASIC_INFO = 2100;                // �⺻���� ��û
    public const int USER_HEART_PRODUCTION_INFO = 2101;     // ����� ���� ���� ��û
    public const int USER_CRYSTAL_PRODUCTION_INFO = 2102;   // ���� ���� ���� ��û
    public const int USER_UNLOCK_LIST = 2103;               // ��� ���� - �������� �����ϴ� �Ϻ� ��� ����
    public const int USER_OFFLINE_REWARD = 2104;            // �������� ���� ȹ��
    public const int SAVE_TUTORIAL = 2105;                  // Ʃ�丮�� ���� ��û
    public const int USER_RAW_DATA = 2106;                  // Ŭ���̾�Ʈ �뵥����
    public const int PAGE_TUTORIAL_IDX = 2107;              // �Ϸ�� ������ Ʃ�丮�� �ε���.

    public const int USER_DATA_SAVE = 2110;                 // ���� ������ ����
    public const int USER_DATA_LOAD = 2111;                 // ���� ������ �ε�

    public const int CORAL_LIST = 2200;         // ��ȣ ����Ʈ
    public const int CORAL_BUY = 2201;          // ��ȣ ����
    public const int CORAL_LEVEL_UP = 2202;     // ��ȣ ������
    public const int STONE_LEVEL_UP = 2210;     // ��ȣ�� ������
    public const int HEART_HARVEST = 2211;      // ����� ��Ȯ
    public const int HEART_TOUCH = 2220;        // ��ġ�Ͽ� ����� ���� �ֱ� ���� ����

    public const int FOOD_LIST = 2250;          //�Ҷ�� ���� ����Ʈ
    public const int FOOD_USE = 2251;           //�Ҷ�� �� ���̱�
    public const int CRYSTAL_HARVEST = 2252;    //���� ��Ȯ

    public const int FISH_STORAGE_LIST = 2300;          // ����� ������ ����
    //const FISH_TANK_LIST  = 2301;
    public const int FISH_BUY = 2302;                   // ����� ���� ��û
    public const int FISH_MOVE_TANK = 2303;             // ����� ������ �̵� ��û (������ ��ġ)
    public const int FISH_MOVE_STORAGE = 2304;          // ����� ���������� �̵� ��û
    public const int FISH_RANDOM_MOVE_TANK = 2305;      // ����� ������ ���� �̵� ��û (������ ���� ��ġ)
    public const int FISH_TOTAL_MOVE_STORAGE = 2306;    // ����� ��� ���������� �̵� ��û
    public const int FISH_NOW_SEASON_SETTING = 2307;    // �̹� ���� ����� ��ġ ��û
    public const int FISH_CHANGE_ARRANGE_SETTING = 2310;    // ����� ��ġ ���� ��û
    public const int FISH_BUBBLE_LIST = 2320;           // ����� ���� ����Ʈ
    public const int FISH_BUBBLE_REWARD = 2321;         // ����� ���� ���� ��û

    //public const int WEMIX_GET_TRADE_INFO = 2400;       // ��ū ��ȯ ����
    //public const int WEMIX_INGAME_TO_FT = 2401;         // �ΰ�����ȭ -> FT
    //public const int WEMIX_FT_TO_INGAME = 2402;         // FT -> �ΰ�����ȭ
    //public const int WEMIX_CONNECT = 2403;              // ���ͽ�����
    //public const int WEMIX_NFT_ANIPANG_ACTIVE = 2404;   // �ִ��� NFT Ȱ��ȭ

    public const int WEMIX_TRADE_INFO = 2400;           // ��ū ��ȯ ����
    public const int WEMIX_TRADE_HASH_TO_FT = 2401;     // �ΰ�����ȭ -> FT sign hash ��û
    public const int WEMIX_TRADE_HASH_TO_INGAME = 2402; // FT -> �ΰ�����ȭ sign hash ��û
    public const int WEMIX_CONNECT = 2403;              // ���ͽ�����    
    public const int WEMIX_TRADE = 2404;
    public const int WEMIX_APPROVE_HASH = 2405;
    public const int WEMIX_APPROVE = 2406;              // FT ��ȯ �����㰡 ����Ʈ������    

    public const int CARD_PACK_LIST = 2500;     // ī���� ����Ʈ.
    public const int CARD_PACK_OPEN = 2501;     // ī���� ����.
    public const int CARD_LIST = 2502;          // ī�� ����Ʈ.
    public const int CARD_BAG_EXTEND = 2503;    // ī�� ���� Ȯ�� ����.
    public const int CARD_SYNTHESIS = 2504;     // ī�� ���� �ռ� ����.
    public const int CARD_CHANGE_LOCK = 2505;   // ī�� ��� ���� ����.
    public const int CARD_USE = 2506;           // ī�� ���.
    public const int CARD_SELL = 2507;           // ī�� �Ǹ�.

    public const int WELCOME_CARD_INFO = 2510;     // ����ī�� ����
    public const int WELCOME_CARD_REWARD = 2511;     // ����ī�� ����ޱ�



    public const int SKILL_LIST = 2600;         // ��ų ����Ʈ
    public const int SKILL_LEVEL_UP = 2601;     // ��ų ������
    public const int SKILL_USE = 2602;          // ��ų ���

    public const int ARTIFACT_LIST = 2610;      // ��Ƽ��Ʈ ����Ʈ
    public const int ARTIFACT_BUY = 2611;       // ��Ƽ��Ʈ ����
    public const int ARTIFACT_LEVEL_UP = 2612;  // ��Ƽ��Ʈ ������

    public const int DAILY_MISSION_LIST = 2650;     // ���� �̼� ����Ʈ
    public const int DAILY_MISSION_REWARD = 2651;   // ���� �̼� ���� ��û

    public const int AD_LIST = 2660;            // ���� ����Ʈ
    public const int AD_REWARD = 2661;          // ���� ���� ��û

    public const int THEME_DYE_COSTUME_LIST = 2670; // �׸�, ����, �ڽ�Ƭ ����Ʈ.
    public const int THEME_DYE_COSTUME_BUY = 2671; // �׸�, ����, �ڽ�Ƭ ����.
    public const int THEME_DYE_COSTUME_EQUIP = 2672; // �׸�, ����, �ڽ�Ƭ ����.

    public const int COLLECTION_LIST = 2680;        // �÷��� ����Ʈ
    public const int COLLECTION_REWARD = 2681;   // �÷��� ���� ��û

    public const int EVENT_INFO = 2700;                     // �̺�Ʈ ����
    public const int EVENT_SEASONPASS_REWARD = 2701;        // �̺�Ʈ �����н� ����ޱ�
    public const int EVENT_SEASONPASS_CHANGE = 2702;        // �̺�Ʈ �����н� ��ȭ ġȯ
    public const int EVENT_SEASONPASS_LEVEL_REWARD = 2703;        // �̺�Ʈ �����н� ���� ����

    public const int FARM_DATA = 2800;        // ����, ���幰 ����
    public const int FARM_BUY = 2801;        // ���� ����.
    public const int FARM_COLLECT = 2802;        // ���幰 ��Ȯ.

    public const int EXPAND_DATA = 2900;            // Ȯ�幰 ����.
    public const int EXPAND_BUY = 2901;             // Ȯ�幰 ����.
    public const int TILE_BUY = 2902;               // Ÿ�� ����.
    public const int EXPAND_EDIT = 2903;            // Ȯ�幰 ��ġ, ����, ����.
    public const int EXPAND_LOCATE_INIT = 2904;     // Ȯ�幰 ��ġ ��ü �ʱ�ȭ.
    public const int EXPAND_LOCATE_DATA = 2905;     // Ȯ�幰 ��ġ ����.


    public const int ATTENDANCE_INFO = 6000;    // �⼮ ����
    public const int ATTENDANCE_REWARD = 6001;   // �⼮ ����

    public const int SHOP_LIST = 7001;                  // ���� ���� ��ȸ
    public const int SHOP_TERM_ITEM_LIST = 7002;        // �Ⱓ�� ��ǰ ��ȸ
    public const int SHOP_BUY = 7003;                   // ��ǰ ����

    public const int SUBSCRIPTION_STATE = 7105;     // ���� ���� ��û
    public const int SUBSCRIPTION_COUPON = 7030;    //���� ���� �߱� ��û    

    public const int COUPON_REWAED = 7010;      // ���� ���� ��û
    public const int COUPON_CREATE = 7040;    // ���� �߱� ��û

    public const int BILLING_INIT = 7100;       //  ���� �غ� ��û
    public const int BILLING_RECEIPT = 7101;    // ������ ���� ��û
    public const int BILLING_END = 7102;

    public const int SENDBOX_GET_LIST = 8000;           // ���� ��� ��û
    public const int SENDBOX_RECEIVE_REWARD = 8001;     // ���� �Ѱ� �ޱ�
    public const int SENDBOX_RECEIVE_REWARD_ALL = 8002; // ���� ��ü �ޱ�

}